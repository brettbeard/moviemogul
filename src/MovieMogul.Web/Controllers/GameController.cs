using Microsoft.AspNetCore.Mvc;
using MovieMogul.Web.Data;
using MovieMogul.Web.Models;
using MovieMogul.Web.Services;

namespace MovieMogul.Web.Controllers;

public class GameController(GameEngine engine, HighScoreService highScores) : Controller
{
    private const string SessionKey = "GameState";

    private GameState? Load() => HttpContext.Session.GetObject<GameState>(SessionKey);
    private void Save(GameState state) => HttpContext.Session.SetObject(SessionKey, state);

    [HttpPost]
    public IActionResult Start()
    {
        var state = new GameState
        {
            ScriptChoices = engine.DrawScriptChoices(),
            Stage = GameStage.ScriptSelection,
        };
        Save(state);
        return RedirectToAction(nameof(Script));
    }

    [HttpGet]
    public IActionResult Script()
    {
        var state = Load();
        if (state is null) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    public IActionResult Script(int movieIndex)
    {
        var state = Load();
        if (state is null) return RedirectToAction("Index", "Home");
        if (movieIndex < 0 || movieIndex >= state.ScriptChoices.Count)
        {
            ModelState.AddModelError("", "Pick one of the three scripts shown.");
            return View(state);
        }

        state.SelectedMovieIndex = movieIndex;
        state.CastingPool = engine.DrawCastingPool();
        state.Stage = GameStage.Casting;
        Save(state);
        return RedirectToAction(nameof(Casting));
    }

    [HttpGet]
    public IActionResult Casting()
    {
        var state = Load();
        if (state?.SelectedMovieIndex is null) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    public IActionResult Casting([FromForm] string?[] roleSelections)
    {
        var state = Load();
        if (state?.SelectedMovieIndex is null) return RedirectToAction("Index", "Home");

        var movie = state.SelectedMovie;
        roleSelections ??= [];

        if (roleSelections.Length != movie.Roles.Count || roleSelections.Any(string.IsNullOrWhiteSpace))
        {
            ModelState.AddModelError("", "You must cast all three roles.");
            return View(state);
        }

        if (roleSelections.Distinct().Count() != roleSelections.Length)
        {
            ModelState.AddModelError("", "Each star can only be cast in one role.");
            return View(state);
        }

        var cast = new List<CastAssignment>();
        for (int i = 0; i < movie.Roles.Count; i++)
        {
            var name = roleSelections[i]!;
            var member = state.CastingPool.FirstOrDefault(c => c.Name == name);
            if (member is null)
            {
                ModelState.AddModelError("", $"'{name}' isn't in your casting pool.");
                return View(state);
            }
            if (!movie.Roles[i].Allows(member.Sex))
            {
                ModelState.AddModelError("", $"{member.Name} can't play {movie.Roles[i].Name}.");
                return View(state);
            }
            cast.Add(new CastAssignment { RoleIndex = i, Member = member });
        }

        state.Cast = cast;
        state.Stage = GameStage.Budget;
        Save(state);
        return RedirectToAction(nameof(Budget));
    }

    [HttpGet]
    public IActionResult Budget()
    {
        var state = Load();
        if (state is null || state.Cast.Count == 0) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    public IActionResult Budget(decimal spend)
    {
        var state = Load();
        if (state is null || state.Cast.Count == 0) return RedirectToAction("Index", "Home");

        var floor = state.SelectedMovie.MinBudget;
        var ceiling = GameConstants.ProductionSpendCeiling;
        if (spend < floor || spend > ceiling)
        {
            ModelState.AddModelError("", $"Production spend must be between {floor:C0} and {ceiling:C0}.");
            return View(state);
        }

        state.ProductionSpend = spend;
        var (text, cost) = engine.RollRandomEvent(state.Cast);
        state.EventText = text;
        state.EventCost = cost;
        state.Stage = GameStage.EventReveal;
        Save(state);
        return RedirectToAction(nameof(Event));
    }

    [HttpGet]
    public IActionResult Event()
    {
        var state = Load();
        if (state?.EventText is null) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    [ActionName("Event")]
    public IActionResult EventPost()
    {
        var state = Load();
        if (state?.EventText is null) return RedirectToAction("Index", "Home");

        var baseCost = state.ProductionSpend + state.TotalSalaries + state.EventCost;
        var (percent, amount) = engine.RollBudgetOverrun(baseCost);
        state.OverrunPercent = percent;
        state.OverrunAmount = amount;
        state.TotalCost = baseCost + amount;
        state.Stage = GameStage.OverrunReveal;
        Save(state);
        return RedirectToAction(nameof(Overrun));
    }

    [HttpGet]
    public IActionResult Overrun()
    {
        var state = Load();
        if (state is null || state.TotalCost == 0) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    [ActionName("Overrun")]
    public IActionResult OverrunPost()
    {
        var state = Load();
        if (state is null || state.TotalCost == 0) return RedirectToAction("Index", "Home");

        state.MpaaRating = engine.RollMpaaRating();
        state.Stage = GameStage.Preview;
        Save(state);
        return RedirectToAction(nameof(Preview));
    }

    [HttpGet]
    public IActionResult Preview()
    {
        var state = Load();
        if (state?.MpaaRating is null) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    [ActionName("Preview")]
    public IActionResult PreviewPost()
    {
        var state = Load();
        if (state?.MpaaRating is null) return RedirectToAction("Index", "Home");

        state.QualityScore = engine.ComputeQualityScore(state.SelectedMovie, state.ProductionSpend, state.Cast,
            out var talentFactor, out _, out _);
        state.TalentFactor = talentFactor;
        state.Reviews = engine.RollReviews(state.QualityScore);
        state.Stage = GameStage.Reviews;
        Save(state);
        return RedirectToAction(nameof(Reviews));
    }

    [HttpGet]
    public IActionResult Reviews()
    {
        var state = Load();
        if (state is null || state.Reviews.Count == 0) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    [ActionName("Reviews")]
    public IActionResult ReviewsPost()
    {
        var state = Load();
        if (state is null || state.Reviews.Count == 0) return RedirectToAction("Index", "Home");

        state.CurrentWeekGross = engine.ComputeOpeningWeekGross(state.SelectedMovie, state.ProductionSpend, state.QualityScore);
        var (profile, multiplier) = engine.PickDecayProfile();
        state.DecayProfile = profile;
        HttpContext.Session.SetObject("DecayMultiplier", multiplier);

        state.NextWeek = 2;
        state.LifetimeGross = state.CurrentWeekGross;
        state.BoxOfficeFinished = state.CurrentWeekGross < GameConstants.BoxOfficeWeeklyCutoff;
        if (state.BoxOfficeFinished) state.LifetimeGross = GameLogic.EnforceBoxOfficeFloor(state.LifetimeGross);
        state.BoxOfficeWeeks = [new BoxOfficeWeek { Week = 1, WeeklyGross = state.CurrentWeekGross, RunningTotal = state.LifetimeGross }];
        state.Stage = GameStage.BoxOffice;
        Save(state);
        return RedirectToAction(nameof(BoxOffice));
    }

    [HttpGet]
    public IActionResult BoxOffice()
    {
        var state = Load();
        if (state is null || state.BoxOfficeWeeks.Count == 0) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    public IActionResult NextWeek()
    {
        var state = Load();
        if (state is null || state.BoxOfficeWeeks.Count == 0) return RedirectToAction("Index", "Home");
        if (state.BoxOfficeFinished) return RedirectToAction(nameof(BoxOffice));

        var multiplier = HttpContext.Session.GetObject<double>("DecayMultiplier");
        state.CurrentWeekGross = engine.NextWeekGross(state.CurrentWeekGross, multiplier);
        state.LifetimeGross += state.CurrentWeekGross;

        var finishedByCutoff = state.CurrentWeekGross < GameConstants.BoxOfficeWeeklyCutoff;
        var finishedByCap = state.NextWeek >= GameConstants.BoxOfficeMaxWeeks;
        if (finishedByCutoff || finishedByCap)
        {
            state.LifetimeGross = GameLogic.EnforceBoxOfficeFloor(state.LifetimeGross);
            state.BoxOfficeFinished = true;
        }

        state.BoxOfficeWeeks.Add(new BoxOfficeWeek
        {
            Week = state.NextWeek,
            WeeklyGross = state.CurrentWeekGross,
            RunningTotal = state.LifetimeGross,
        });
        state.NextWeek++;

        Save(state);
        return RedirectToAction(nameof(BoxOffice));
    }

    [HttpGet]
    public IActionResult Awards()
    {
        var state = Load();
        if (state is null || !state.BoxOfficeFinished) return RedirectToAction("Index", "Home");

        if (state.AwardResults.Count == 0)
        {
            state.AwardResults = engine.RunAwards(state.TalentFactor, state.QualityScore, state.Cast);
            state.ReReleaseRevenue = state.AwardResults.Any(a => a.Won)
                ? engine.ComputeReReleaseRevenue(state.LifetimeGross)
                : 0m;
            state.Stage = GameStage.Awards;
            Save(state);
        }
        return View(state);
    }

    [HttpPost]
    [ActionName("Results")]
    public async Task<IActionResult> ResultsPost()
    {
        var state = Load();
        if (state is null || state.AwardResults.Count == 0) return RedirectToAction("Index", "Home");

        state.TotalRevenue = state.LifetimeGross + state.ReReleaseRevenue;
        state.NetProfit = state.TotalRevenue - state.TotalCost;
        state.QualifiesForHighScore = await highScores.QualifiesAsync(state.NetProfit);
        state.Stage = GameStage.Results;
        Save(state);
        return RedirectToAction(nameof(Results));
    }

    [HttpGet]
    public IActionResult Results()
    {
        var state = Load();
        if (state is null || state.Stage < GameStage.Results) return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpGet]
    public IActionResult HighScoreEntry()
    {
        var state = Load();
        if (state is null || !state.QualifiesForHighScore || state.HighScoreSubmitted)
            return RedirectToAction("Index", "Home");
        return View(state);
    }

    [HttpPost]
    public async Task<IActionResult> HighScoreEntry(string initials)
    {
        var state = Load();
        if (state is null || !state.QualifiesForHighScore) return RedirectToAction("Index", "Home");

        if (string.IsNullOrWhiteSpace(initials))
        {
            ModelState.AddModelError("", "Enter your initials.");
            return View(state);
        }

        await highScores.AddAsync(state.SelectedMovie.Title, initials, state.NetProfit, state.TotalRevenue, state.TotalCost);
        state.HighScoreSubmitted = true;
        state.Stage = GameStage.Done;
        Save(state);
        return RedirectToAction(nameof(HighScores));
    }

    [HttpGet]
    public async Task<IActionResult> HighScores()
    {
        ViewBag.Top = await highScores.GetTopAsync(10);
        ViewBag.Bottom = await highScores.GetBottomAsync(5);
        return View();
    }
}
