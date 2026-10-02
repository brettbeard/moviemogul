using System.Text.Json.Serialization;

namespace MovieMogul.Web.Models;

public enum Sex
{
    Male,
    Female
}

public class CastMember
{
    public required string Name { get; init; }
    public required Sex Sex { get; init; }

    /// <summary>2/4/6/8 = young adult / adult / mature / senior, from ACTOR.DAT/ACTRESS.DAT.</summary>
    public required int AgeBucket { get; init; }

    public required int Talent { get; init; }
    public required int Popularity { get; init; }
    public required decimal Salary { get; init; }

    [JsonIgnore]
    public string AgeLabel => AgeBucket switch
    {
        2 => "Young",
        4 => "Adult",
        6 => "Mature",
        8 => "Senior",
        _ => "Unknown"
    };
}
