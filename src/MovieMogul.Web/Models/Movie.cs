using System.Text.Json.Serialization;

namespace MovieMogul.Web.Models;

public enum RoleRestriction
{
    MaleOnly,
    FemaleOnly,
    Either
}

public class Role
{
    public required string Name { get; init; }
    public required RoleRestriction Restriction { get; init; }

    public bool Allows(Sex sex) => Restriction switch
    {
        RoleRestriction.MaleOnly => sex == Sex.Male,
        RoleRestriction.FemaleOnly => sex == Sex.Female,
        _ => true
    };
}

public class Movie
{
    public required string Title { get; init; }
    public required string SummaryLine1 { get; init; }
    public required string SummaryLine2 { get; init; }
    public required IReadOnlyList<Role> Roles { get; init; }

    /// <summary>Minimum production spend for this script, in dollars.</summary>
    public required decimal MinBudget { get; init; }

    /// <summary>Interpreted box-office scale factor for this script (see GAME_DESIGN.md).</summary>
    public required decimal ScaleFactor { get; init; }

    [JsonIgnore]
    public string Summary => $"{SummaryLine1} {SummaryLine2}";
}
