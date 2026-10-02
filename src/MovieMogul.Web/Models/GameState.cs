using System.Text.Json.Serialization;

namespace MovieMogul.Web.Models;

public enum GameStage
{
    ScriptSelection,
    Casting,
    Budget,
    EventReveal,
    OverrunReveal,
    Preview,
    Reviews,
    BoxOffice,
    Awards,
    Results,
    HighScoreEntry,
    Done
}

public class CastAssignment
{
    public required int RoleIndex { get; init; }
    public required CastMember Member { get; init; }
}

public class ReviewResult
{
    public required string Reviewer { get; init; }
    public required string Verdict { get; init; }
}

public class BoxOfficeWeek
{
    public required int Week { get; init; }
    public required decimal WeeklyGross { get; init; }
    public required decimal RunningTotal { get; init; }
}

public class AwardResult
{
    public required string Category { get; init; }
    public required string Presenter { get; init; }
    public required bool Won { get; init; }
    public string? WinnerName { get; init; }
}

/// <summary>
/// The full in-progress game, serialized into session between steps.
/// </summary>
public class GameState
{
    public GameStage Stage { get; set; } = GameStage.ScriptSelection;

    // Script selection
    public List<Movie> ScriptChoices { get; set; } = [];
    public int? SelectedMovieIndex { get; set; }

    [JsonIgnore]
    public Movie SelectedMovie => ScriptChoices[SelectedMovieIndex!.Value];

    // Casting
    public List<CastMember> CastingPool { get; set; } = [];
    public List<CastAssignment> Cast { get; set; } = [];

    // Budget
    public decimal ProductionSpend { get; set; }

    [JsonIgnore]
    public decimal TotalSalaries => Cast.Sum(c => c.Member.Salary);

    // Random event
    public string? EventText { get; set; }
    public decimal EventCost { get; set; }

    // Overrun
    public decimal OverrunPercent { get; set; }
    public decimal OverrunAmount { get; set; }
    public decimal TotalCost { get; set; }

    // Preview / reviews
    public string? MpaaRating { get; set; }
    public List<ReviewResult> Reviews { get; set; } = [];

    // Internal scoring (kept for the awards step + display)
    public decimal QualityScore { get; set; }
    public decimal TalentFactor { get; set; }

    // Box office
    public List<BoxOfficeWeek> BoxOfficeWeeks { get; set; } = [];
    public int NextWeek { get; set; } = 1;
    public decimal CurrentWeekGross { get; set; }
    public string DecayProfile { get; set; } = "";
    public decimal LifetimeGross { get; set; }
    public bool BoxOfficeFinished { get; set; }

    // Awards
    public List<AwardResult> AwardResults { get; set; } = [];
    public decimal ReReleaseRevenue { get; set; }

    // Results
    public decimal TotalRevenue { get; set; }
    public decimal NetProfit { get; set; }

    // High score
    public bool QualifiesForHighScore { get; set; }
    public bool HighScoreSubmitted { get; set; }
}
