using MovieMogul.Web.Models;

namespace MovieMogul.Web.Data;

/// <summary>
/// Movie titles, summaries and role names are verbatim from MOVIES.DAT. Per-role sex
/// restrictions, min budgets and scale factors are documented in GAME_DESIGN.md.
/// </summary>
public static partial class SeedData
{
    public static readonly IReadOnlyList<Movie> Movies =
    [
        new Movie
        {
            Title = "SPACE WARS",
            SummaryLine1 = "A sci-fi spectacular set in a galaxy",
            SummaryLine2 = "far away.",
            Roles =
            [
                new Role { Name = "Space Hero", Restriction = RoleRestriction.Either },
                new Role { Name = "Princess", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Alien Sidekick", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 5_000_000m,
            ScaleFactor = 30_000_000m,
        },
        new Movie
        {
            Title = "SLASHER NIGHTS",
            SummaryLine1 = "Chilling horror story about an escaped",
            SummaryLine2 = "psycho in a small town.",
            Roles =
            [
                new Role { Name = "Threatened Independent Woman", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Boyfriend", Restriction = RoleRestriction.MaleOnly },
                new Role { Name = "Psycho", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 500_000m,
            ScaleFactor = 12_000_000m,
        },
        new Movie
        {
            Title = "DEMON DUSTERS",
            SummaryLine1 = "Funny adventures of a trio of demon",
            SummaryLine2 = "fighters.",
            Roles =
            [
                new Role { Name = "Demon Duster #1", Restriction = RoleRestriction.Either },
                new Role { Name = "Demon Duster #2", Restriction = RoleRestriction.Either },
                new Role { Name = "Demon Duster #3", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 1_500_000m,
            ScaleFactor = 26_000_000m,
        },
        new Movie
        {
            Title = "THE LAST BATTLE",
            SummaryLine1 = "The story of brave Americans fighting",
            SummaryLine2 = "in Europe during World War II.",
            Roles =
            [
                new Role { Name = "Compassionate Lieutenant", Restriction = RoleRestriction.Either },
                new Role { Name = "French Farm Girl", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Tough Sergeant", Restriction = RoleRestriction.MaleOnly },
            ],
            MinBudget = 2_500_000m,
            ScaleFactor = 19_000_000m,
        },
        new Movie
        {
            Title = "GUNS & RIFLES",
            SummaryLine1 = "A rancher fights off cattle rustlers,",
            SummaryLine2 = "indians, and bankers in the old west.",
            Roles =
            [
                new Role { Name = "Rancher", Restriction = RoleRestriction.MaleOnly },
                new Role { Name = "Wife", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Greedy Banker", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 2_000_000m,
            ScaleFactor = 17_000_000m,
        },
        new Movie
        {
            Title = "FINAL REUNION",
            SummaryLine1 = "A dramatic story of a family facing",
            SummaryLine2 = "an old dark secret.",
            Roles =
            [
                new Role { Name = "Widowed Matriarch", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Older Son", Restriction = RoleRestriction.MaleOnly },
                new Role { Name = "Younger Son", Restriction = RoleRestriction.MaleOnly },
            ],
            MinBudget = 1_000_000m,
            ScaleFactor = 15_000_000m,
        },
        new Movie
        {
            Title = "BONKERS!",
            SummaryLine1 = "Teenagers go wild over surfing, video",
            SummaryLine2 = "games, and sex.",
            Roles =
            [
                new Role { Name = "Innocent Hero", Restriction = RoleRestriction.Either },
                new Role { Name = "Girlfriend", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Funny Slob", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 250_000m,
            ScaleFactor = 7_000_000m,
        },
        new Movie
        {
            Title = "QUEST FOR HONOR",
            SummaryLine1 = "An adventurer must rescue a friend's",
            SummaryLine2 = "daughter from an evil warlord.",
            Roles =
            [
                new Role { Name = "Archeologist Hero", Restriction = RoleRestriction.Either },
                new Role { Name = "Kidnapped Daughter", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Villainous Warlord", Restriction = RoleRestriction.MaleOnly },
            ],
            MinBudget = 3_000_000m,
            ScaleFactor = 27_000_000m,
        },
        new Movie
        {
            Title = "I'VE GOT MUSIC",
            SummaryLine1 = "A young song writer tries to make it",
            SummaryLine2 = "big in this old-fashioned musical.",
            Roles =
            [
                new Role { Name = "Song Writer", Restriction = RoleRestriction.Either },
                new Role { Name = "Big Producer", Restriction = RoleRestriction.Either },
                new Role { Name = "Femme Fatale", Restriction = RoleRestriction.FemaleOnly },
            ],
            MinBudget = 1_200_000m,
            ScaleFactor = 16_000_000m,
        },
        new Movie
        {
            Title = "CONSENT TO KILL",
            SummaryLine1 = "Story of a detective investigating a",
            SummaryLine2 = "strange double murder.",
            Roles =
            [
                new Role { Name = "Detective", Restriction = RoleRestriction.Either },
                new Role { Name = "Rich Female Socialite", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Partner", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 750_000m,
            ScaleFactor = 18_000_000m,
        },
        new Movie
        {
            Title = "EXECUTIVE DECISIONS",
            SummaryLine1 = "A middle-aged business woman must deal",
            SummaryLine2 = "with divorce and corporate takeover.",
            Roles =
            [
                new Role { Name = "Business Woman", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Ex-Husband", Restriction = RoleRestriction.MaleOnly },
                new Role { Name = "Young Stud", Restriction = RoleRestriction.MaleOnly },
            ],
            MinBudget = 1_000_000m,
            ScaleFactor = 15_000_000m,
        },
        new Movie
        {
            Title = "STRANGE BEDFELLOWS",
            SummaryLine1 = "Based on the hilarious play about a",
            SummaryLine2 = "marriage in mid-life crisis.",
            Roles =
            [
                new Role { Name = "Husband", Restriction = RoleRestriction.MaleOnly },
                new Role { Name = "Wife", Restriction = RoleRestriction.FemaleOnly },
                new Role { Name = "Best Friend", Restriction = RoleRestriction.Either },
            ],
            MinBudget = 1_000_000m,
            ScaleFactor = 15_000_000m,
        },
    ];
}
