using MovieMogul.Web.Data;
using MovieMogul.Web.Services;

namespace MovieMogul.Tests;

public class GameLogicTests
{
    [Theory]
    [InlineData(0.0, 0.00)]      // first band: on budget
    [InlineData(0.29, 0.00)]
    [InlineData(0.30, 0.02)]     // second band starts at cumulative 0.30
    [InlineData(0.54, 0.02)]
    [InlineData(0.55, 0.05)]     // third band starts at cumulative 0.55
    [InlineData(0.90, 0.20)]     // fifth band starts at cumulative 0.90
    [InlineData(0.97, 0.30)]     // sixth band starts at cumulative 0.97
    [InlineData(0.999, 0.30)]
    public void RollOverrun_PicksBandFromCumulativeWeights(double roll, decimal expectedPercent)
    {
        var (percent, _) = GameLogic.RollOverrun(1_000_000m, roll);
        Assert.Equal(expectedPercent, percent);
    }

    [Fact]
    public void RollOverrun_ComputesDollarAmountFromPercent()
    {
        var (percent, amount) = GameLogic.RollOverrun(1_000_000m, 0.60); // 5% band
        Assert.Equal(0.05m, percent);
        Assert.Equal(50_000m, amount);
    }

    [Fact]
    public void RollOverrun_OnBudgetBandHasZeroAmount()
    {
        var (percent, amount) = GameLogic.RollOverrun(2_500_000m, 0.0);
        Assert.Equal(0m, percent);
        Assert.Equal(0m, amount);
    }

    [Theory]
    [InlineData(500_000)]
    [InlineData(199_999.99)]
    [InlineData(0)]
    [InlineData(-1_000_000)]
    public void EnforceBoxOfficeFloor_NeverGoesBelowFloor(decimal rawTotal)
    {
        var result = GameLogic.EnforceBoxOfficeFloor(rawTotal);
        Assert.True(result >= GameConstants.BoxOfficeFloor);
    }

    [Fact]
    public void EnforceBoxOfficeFloor_LeavesTotalsAboveFloorUnchanged()
    {
        var result = GameLogic.EnforceBoxOfficeFloor(5_000_000m);
        Assert.Equal(5_000_000m, result);
    }

    [Fact]
    public void EnforceBoxOfficeFloor_ExactlyAtFloorIsUnchanged()
    {
        var result = GameLogic.EnforceBoxOfficeFloor(GameConstants.BoxOfficeFloor);
        Assert.Equal(GameConstants.BoxOfficeFloor, result);
    }

    [Fact]
    public void QualifiesForHighScore_AlwaysTrueWhenListsNotFull()
    {
        // Fewer than 10 entries total -> always qualifies for "top", regardless of value.
        var existing = new List<decimal> { 100, 200, 300 };
        Assert.True(GameLogic.QualifiesForHighScore(-999_999m, existing));
    }

    [Fact]
    public void QualifiesForHighScore_TopBeatsLowestOfFullTop10()
    {
        var existing = Enumerable.Range(1, 15).Select(i => (decimal)(i * 100_000)).ToList(); // 100k..1.5M, 15 entries
        // Full top-10 threshold is the 10th highest = 600,000 (values 700k..1.5M are the current top 10).
        Assert.True(GameLogic.QualifiesForHighScore(650_000m, existing));
        // 550,000 is between the bottom-5 ceiling (500k) and the top-10 floor (600k) -> qualifies for neither.
        Assert.False(GameLogic.QualifiesForHighScore(550_000m, existing));
    }

    [Fact]
    public void QualifiesForHighScore_BottomBeatsHighestOfFullBottom5()
    {
        var existing = Enumerable.Range(1, 15).Select(i => (decimal)(i * 100_000)).ToList();
        // Bottom-5 threshold is the 5th smallest = 500,000.
        Assert.True(GameLogic.QualifiesForHighScore(50_000m, existing));
        Assert.False(GameLogic.QualifiesForHighScore(550_000m, existing));
    }

    [Fact]
    public void ComputeMarker_FirstEntryHasNoMarker()
    {
        var existing = new List<(string Title, string Initials)>();
        var marker = GameLogic.ComputeMarker("SPACE WARS", "BSB", existing);
        Assert.Equal("", marker);
    }

    [Fact]
    public void ComputeMarker_SecondCollisionGetsRomanTwo()
    {
        var existing = new List<(string Title, string Initials)> { ("SPACE WARS", "BSB") };
        var marker = GameLogic.ComputeMarker("SPACE WARS", "BSB", existing);
        Assert.Equal("II", marker);
    }

    [Fact]
    public void ComputeMarker_ThirdCollisionGetsRomanThree()
    {
        var existing = new List<(string Title, string Initials)>
        {
            ("SPACE WARS", "BSB"),
            ("SPACE WARS", "BSB"),
        };
        var marker = GameLogic.ComputeMarker("SPACE WARS", "BSB", existing);
        Assert.Equal("III", marker);
    }

    [Fact]
    public void ComputeMarker_DifferentTitleSameInitialsDoesNotCollide()
    {
        var existing = new List<(string Title, string Initials)> { ("SPACE WARS", "BSB") };
        var marker = GameLogic.ComputeMarker("BONKERS!", "BSB", existing);
        Assert.Equal("", marker);
    }

    [Fact]
    public void ComputeMarker_DifferentInitialsSameTitleDoesNotCollide()
    {
        var existing = new List<(string Title, string Initials)> { ("SPACE WARS", "BSB") };
        var marker = GameLogic.ComputeMarker("SPACE WARS", "ARC", existing);
        Assert.Equal("", marker);
    }
}
