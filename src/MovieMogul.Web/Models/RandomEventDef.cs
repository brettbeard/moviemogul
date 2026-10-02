namespace MovieMogul.Web.Models;

public class RandomEventDef
{
    public required string Id { get; init; }

    /// <summary>Display text. May contain "{star}" for the affected cast member's name.</summary>
    public required string TextTemplate { get; init; }

    /// <summary>Hard dollar cost this event adds to production cost, or 0 for flavor-only events.</summary>
    public decimal Cost { get; init; }

    public bool NeedsStarName { get; init; }
}
