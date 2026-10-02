namespace MovieMogul.Web.Data;

/// <summary>
/// All invented (non-sourced) game-balance numbers, in one place. See GAME_DESIGN.md for the
/// rationale behind each of these; nothing here is pulled from the original .DAT files.
/// </summary>
public static class GameConstants
{
    public const decimal ProductionSpendCeiling = 30_000_000m;
    public const decimal BoxOfficeFloor = 200_000m;
    public const decimal BoxOfficeWeeklyCutoff = 50_000m;
    public const int BoxOfficeMaxWeeks = 20;
    public const int CastingPoolSize = 12;
    public const int ScriptChoiceCount = 3;

    // Budget overrun bands: (percent, cumulative probability weight)
    public static readonly (decimal Percent, double Weight)[] OverrunBands =
    [
        (0.00m, 0.30),
        (0.02m, 0.25),
        (0.05m, 0.20),
        (0.10m, 0.15),
        (0.20m, 0.07),
        (0.30m, 0.03),
    ];

    // Quality score weights
    public const double QualityBudgetWeight = 0.45;
    public const double QualityPopularityWeight = 0.35;
    public const double QualityTalentWeight = 0.20;
    public const double QualityNoiseRange = 8.0;

    // Reviews
    public const double ReviewLovedItFloor = 0.05;
    public const double ReviewLovedItCeiling = 0.60;
    public const double ReviewLovedItOffset = 0.10;
    public const double ReviewLikedItShare = 0.35;
    public const double ReviewDidntLikeShare = 0.35;

    // Box office
    public const double LegsChance = 0.30;
    public const double LegsDecayMin = 0.80;
    public const double LegsDecayMax = 0.93;
    public const double CrashDecayMin = 0.35;
    public const double CrashDecayMax = 0.60;
    public const double OpeningWeekVarianceMin = 0.8;
    public const double OpeningWeekVarianceMax = 1.2;

    // Opening week gross = spend * qualityMultiplier * scaleAdjustment * variance.
    // qualityMultiplier ranges from "bomb" (a sliver of spend back) to "blockbuster" (multiples of spend).
    public const double OpeningWeekQualityMultiplierMin = 0.05;
    public const double OpeningWeekQualityMultiplierMax = 2.50;
    // scaleAdjustment is a mild per-script flavor factor (not the dominant driver of magnitude).
    public const double ScaleAdjustmentBase = 0.75;
    public const double ScaleAdjustmentRange = 0.25;

    // Awards
    public const double AwardWinBaseChance = 0.15;
    public const double AwardWinTalentWeight = 0.5;
    public const double AwardWinQualityWeight = 0.1;
    public const double AwardWinFloor = 0.05;
    public const double AwardWinCeiling = 0.65;
    public const double ReReleaseRevenueShare = 0.15;
    public const double ReReleaseVarianceMin = 0.7;
    public const double ReReleaseVarianceMax = 1.3;

    // High scores
    public const int HighScoreTopCount = 10;
    public const int HighScoreBottomCount = 5;
}
