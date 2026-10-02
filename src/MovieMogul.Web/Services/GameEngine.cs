using MovieMogul.Web.Data;
using MovieMogul.Web.Models;

namespace MovieMogul.Web.Services;

/// <summary>
/// Orchestrates one step of the game using SeedData + randomness. Balance numbers live in
/// GameConstants/GAME_DESIGN.md; anything here that's deterministic given a roll delegates to
/// the testable GameLogic class.
/// </summary>
public class GameEngine
{
    public List<Movie> DrawScriptChoices() =>
        [.. SeedData.Movies.OrderBy(_ => Random.Shared.Next()).Take(GameConstants.ScriptChoiceCount)];

    public List<CastMember> DrawCastingPool()
    {
        var half = GameConstants.CastingPoolSize / 2;
        var actors = SeedData.Actors.OrderBy(_ => Random.Shared.Next()).Take(half);
        var actresses = SeedData.Actresses.OrderBy(_ => Random.Shared.Next()).Take(half);
        return [.. actors.Concat(actresses).OrderBy(_ => Random.Shared.Next())];
    }

    public (string Text, decimal Cost) RollRandomEvent(IReadOnlyList<CastAssignment> cast)
    {
        var evt = SeedData.RandomEvents[Random.Shared.Next(SeedData.RandomEvents.Count)];
        var text = evt.TextTemplate;
        if (evt.NeedsStarName && cast.Count > 0)
        {
            var star = cast[Random.Shared.Next(cast.Count)].Member.Name;
            text = text.Replace("{star}", star);
        }
        return (text, evt.Cost);
    }

    public (decimal Percent, decimal Amount) RollBudgetOverrun(decimal baseCost) =>
        GameLogic.RollOverrun(baseCost, Random.Shared.NextDouble());

    public string RollMpaaRating() => SeedData.MpaaRatings[Random.Shared.Next(SeedData.MpaaRatings.Count)];

    public decimal ComputeQualityScore(Movie movie, decimal spend, IReadOnlyList<CastAssignment> cast,
        out decimal talentFactor, out decimal popularityFactor, out decimal budgetFactor)
    {
        var avgTalent = cast.Average(c => c.Member.Talent);
        var avgPopularity = cast.Average(c => c.Member.Popularity);

        talentFactor = (decimal)avgTalent / 100m;
        popularityFactor = (decimal)avgPopularity / 100m;

        var ceilingRange = GameConstants.ProductionSpendCeiling - movie.MinBudget;
        budgetFactor = ceilingRange <= 0
            ? 1m
            : Math.Clamp((spend - movie.MinBudget) / ceilingRange, 0m, 1m);

        var noise = (Random.Shared.NextDouble() * 2 - 1) * GameConstants.QualityNoiseRange;
        var raw = 100m * ((decimal)GameConstants.QualityBudgetWeight * budgetFactor
                           + (decimal)GameConstants.QualityPopularityWeight * popularityFactor
                           + (decimal)GameConstants.QualityTalentWeight * talentFactor)
                  + (decimal)noise;

        return Math.Clamp(raw, 0m, 100m);
    }

    public List<ReviewResult> RollReviews(decimal qualityScore)
    {
        var pLoved = Math.Clamp((double)qualityScore / 100 - GameConstants.ReviewLovedItOffset,
            GameConstants.ReviewLovedItFloor, GameConstants.ReviewLovedItCeiling);
        var pLiked = GameConstants.ReviewLikedItShare;
        var pDidnt = GameConstants.ReviewDidntLikeShare;
        var pHated = Math.Max(0, 1 - pLoved - pLiked - pDidnt);

        var total = pLoved + pLiked + pDidnt + pHated;
        pLoved /= total; pLiked /= total; pDidnt /= total; pHated /= total;

        var results = new List<ReviewResult>();
        foreach (var reviewer in SeedData.Reviewers)
        {
            var roll = Random.Shared.NextDouble();
            string verdict;
            if (roll < pLoved) verdict = SeedData.ReviewVerdicts[0];
            else if (roll < pLoved + pLiked) verdict = SeedData.ReviewVerdicts[1];
            else if (roll < pLoved + pLiked + pDidnt) verdict = SeedData.ReviewVerdicts[2];
            else verdict = SeedData.ReviewVerdicts[3];

            results.Add(new ReviewResult { Reviewer = reviewer, Verdict = verdict });
        }
        return results;
    }

    public decimal ComputeOpeningWeekGross(Movie movie, decimal spend, decimal qualityScore)
    {
        var qualityMultiplier = GameConstants.OpeningWeekQualityMultiplierMin
            + (double)(qualityScore / 100m) * (GameConstants.OpeningWeekQualityMultiplierMax - GameConstants.OpeningWeekQualityMultiplierMin);

        var scaleAdjustment = GameConstants.ScaleAdjustmentBase
            + GameConstants.ScaleAdjustmentRange * (double)(movie.ScaleFactor / GameConstants.ProductionSpendCeiling);

        var variance = GameConstants.OpeningWeekVarianceMin
            + Random.Shared.NextDouble() * (GameConstants.OpeningWeekVarianceMax - GameConstants.OpeningWeekVarianceMin);

        var raw = spend * (decimal)(qualityMultiplier * scaleAdjustment * variance);
        return Math.Round(raw, 0);
    }

    public (string Profile, double Multiplier) PickDecayProfile()
    {
        if (Random.Shared.NextDouble() < GameConstants.LegsChance)
        {
            var m = GameConstants.LegsDecayMin + Random.Shared.NextDouble() * (GameConstants.LegsDecayMax - GameConstants.LegsDecayMin);
            return ("Legs", m);
        }
        else
        {
            var m = GameConstants.CrashDecayMin + Random.Shared.NextDouble() * (GameConstants.CrashDecayMax - GameConstants.CrashDecayMin);
            return ("Spike-and-crash", m);
        }
    }

    public decimal NextWeekGross(decimal currentWeekGross, double decayMultiplier) =>
        Math.Round(currentWeekGross * (decimal)decayMultiplier, 0);

    public List<AwardResult> RunAwards(decimal talentFactor, decimal qualityScore, IReadOnlyList<CastAssignment> cast)
    {
        var winChance = Math.Clamp(
            GameConstants.AwardWinBaseChance
                + GameConstants.AwardWinTalentWeight * (double)talentFactor
                + GameConstants.AwardWinQualityWeight * (double)(qualityScore / 100m),
            GameConstants.AwardWinFloor, GameConstants.AwardWinCeiling);

        var actress = cast.FirstOrDefault(c => c.Member.Sex == Sex.Female)?.Member.Name;
        var actor = cast.FirstOrDefault(c => c.Member.Sex == Sex.Male)?.Member.Name;

        var categories = new (string Category, string? Winner)[]
        {
            ("Best Actress", actress),
            ("Best Actor", actor),
            ("Best Picture", null),
        };

        var results = new List<AwardResult>();
        for (int i = 0; i < categories.Length; i++)
        {
            var (category, winnerName) = categories[i];
            var won = Random.Shared.NextDouble() < winChance;
            results.Add(new AwardResult
            {
                Category = category,
                Presenter = SeedData.AwardPresenters[i % SeedData.AwardPresenters.Count],
                Won = won,
                WinnerName = won ? winnerName : null,
            });
        }
        return results;
    }

    public decimal ComputeReReleaseRevenue(decimal lifetimeGross)
    {
        var variance = GameConstants.ReReleaseVarianceMin
            + Random.Shared.NextDouble() * (GameConstants.ReReleaseVarianceMax - GameConstants.ReReleaseVarianceMin);
        return Math.Round(lifetimeGross * (decimal)GameConstants.ReReleaseRevenueShare * (decimal)variance, 0);
    }
}
