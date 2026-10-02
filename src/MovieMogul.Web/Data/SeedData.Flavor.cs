using MovieMogul.Web.Models;

namespace MovieMogul.Web.Data;

public static partial class SeedData
{
    /// <summary>Verbatim from extracted strings.</summary>
    public static readonly IReadOnlyList<string> Reviewers =
    [
        "The NY Times",
        "Entertainment Tonight",
        "Gene Siskel",
        "Roger Ebert",
        "Sneak Previews",
        "Rex Reed",
        "Time Magazine",
        "Newsweek",
        "LA Times",
    ];

    /// <summary>Verbatim from extracted strings.</summary>
    public static readonly IReadOnlyList<string> ReviewVerdicts =
    [
        "Loved it!",
        "Liked it.",
        "Didn't like it.",
        "Hated it!",
    ];

    /// <summary>Verbatim from extracted strings.</summary>
    public static readonly IReadOnlyList<string> MpaaRatings = ["PG", "PG-13", "R"];

    /// <summary>Verbatim event flavor text and hard costs, from extracted strings.</summary>
    public static readonly IReadOnlyList<RandomEventDef> RandomEvents =
    [
        new RandomEventDef
        {
            Id = "arrest",
            TextTemplate = "{star} has been arrested for possession of cocaine. The bad publicity could hurt the movie.",
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "sues-tabloid",
            TextTemplate = "{star} is suing the National Enquirer. The publicity could be good for the movie.",
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "stuntman-killed",
            TextTemplate = "A stunt man is killed while filming. The publicity could be bad.",
            NeedsStarName = false,
        },
        new RandomEventDef
        {
            Id = "car-accident",
            TextTemplate = "{star} is injured in a car accident. The delay will cost you $200,000.",
            Cost = 200_000m,
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "hates-director",
            TextTemplate = "{star} hates the director. Getting a new one will cost $450,000.",
            Cost = 450_000m,
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "dating-athlete",
            TextTemplate = "{star} has started dating a famous athlete. The publicity could be good for the movie.",
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "dating-singer",
            TextTemplate = "{star} has started dating a famous singer. The publicity could be good for the movie.",
            NeedsStarName = true,
        },
        new RandomEventDef
        {
            Id = "autobiography",
            TextTemplate = "{star} has just written an autobiography. The publicity could be good for the movie.",
            NeedsStarName = true,
        },
    ];

    /// <summary>Invented (see GAME_DESIGN.md). Presenter names for the 3 Oscar categories.</summary>
    public static readonly IReadOnlyList<string> AwardPresenters =
    [
        "Marla Chevalier",
        "Dexter Vance",
        "last year's Best Picture director",
    ];
}
