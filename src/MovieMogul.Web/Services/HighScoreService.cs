using Microsoft.EntityFrameworkCore;
using MovieMogul.Web.Data;
using MovieMogul.Web.Models;

namespace MovieMogul.Web.Services;

public class HighScoreService(AppDbContext db)
{
    public async Task<bool> QualifiesAsync(decimal netProfit)
    {
        var existingProfits = await db.HighScores.Select(h => h.NetProfit).ToListAsync();
        return GameLogic.QualifiesForHighScore(netProfit, existingProfits);
    }

    public async Task<HighScoreEntry> AddAsync(string movieTitle, string initials, decimal netProfit,
        decimal totalRevenue, decimal totalCost)
    {
        initials = initials.Trim().ToUpperInvariant();
        if (initials.Length == 0) initials = "???";
        if (initials.Length > 3) initials = initials[..3];

        var existing = await db.HighScores
            .Where(h => h.MovieTitle == movieTitle && h.Initials == initials)
            .Select(h => new { h.MovieTitle, h.Initials })
            .ToListAsync();

        var marker = GameLogic.ComputeMarker(movieTitle, initials,
            [.. existing.Select(e => (e.MovieTitle, e.Initials))]);

        var entry = new HighScoreEntry
        {
            MovieTitle = movieTitle,
            Initials = initials,
            Marker = marker,
            NetProfit = netProfit,
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            PlayedAtUtc = DateTime.UtcNow,
        };

        db.HighScores.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<List<HighScoreEntry>> GetTopAsync(int count) =>
        await db.HighScores.OrderByDescending(h => h.NetProfit).Take(count).ToListAsync();

    public async Task<List<HighScoreEntry>> GetBottomAsync(int count) =>
        await db.HighScores.OrderBy(h => h.NetProfit).Take(count).ToListAsync();
}
