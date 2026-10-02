using MovieMogul.Web.Data;

namespace MovieMogul.Web.Services;

/// <summary>
/// Pure, deterministic game-balance calculations with no dependency on Random or session state,
/// so they can be unit tested directly. Anything that needs randomness takes the random draw
/// (a [0,1) roll, or similar) as a parameter instead of generating it internally.
/// </summary>
public static class GameLogic
{
    /// <summary>Picks a budget overrun band from a [0,1) roll and returns (percent, dollar amount).</summary>
    public static (decimal Percent, decimal Amount) RollOverrun(decimal baseCost, double roll)
    {
        double cumulative = 0;
        foreach (var band in GameConstants.OverrunBands)
        {
            cumulative += band.Weight;
            if (roll < cumulative)
                return (band.Percent, Math.Round(baseCost * band.Percent, 2));
        }

        var last = GameConstants.OverrunBands[^1];
        return (last.Percent, Math.Round(baseCost * last.Percent, 2));
    }

    /// <summary>A film can never gross less than the lifetime floor.</summary>
    public static decimal EnforceBoxOfficeFloor(decimal rawLifetimeTotal)
        => Math.Max(rawLifetimeTotal, GameConstants.BoxOfficeFloor);

    /// <summary>
    /// A game qualifies for the shared high score list if it would land in the current top-10
    /// by net profit, or the bottom-5 (biggest bombs), matching the manual's "high or notably
    /// low" framing.
    /// </summary>
    public static bool QualifiesForHighScore(decimal netProfit, IReadOnlyList<decimal> existingProfits)
    {
        var descending = existingProfits.OrderByDescending(p => p).ToList();
        bool topQualifies = descending.Count < GameConstants.HighScoreTopCount
            || netProfit > descending[GameConstants.HighScoreTopCount - 1];

        var ascending = existingProfits.OrderBy(p => p).ToList();
        bool bottomQualifies = ascending.Count < GameConstants.HighScoreBottomCount
            || netProfit < ascending[GameConstants.HighScoreBottomCount - 1];

        return topQualifies || bottomQualifies;
    }

    /// <summary>
    /// Disambiguation marker for a title+initials pair that already appears on the list
    /// ("", then "II", "III", ...), per the manual's own note about identical titles/initials.
    /// </summary>
    public static string ComputeMarker(string title, string initials, IReadOnlyList<(string Title, string Initials)> existing)
    {
        var matchCount = existing.Count(e => e.Title == title && e.Initials == initials);
        return matchCount == 0 ? "" : ToRoman(matchCount + 1);
    }

    private static readonly string[] RomanOnes = ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    private static string ToRoman(int n) => n >= 0 && n < RomanOnes.Length ? RomanOnes[n] : n.ToString();
}
