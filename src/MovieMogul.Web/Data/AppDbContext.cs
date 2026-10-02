using Microsoft.EntityFrameworkCore;
using MovieMogul.Web.Models;

namespace MovieMogul.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<HighScoreEntry> HighScores => Set<HighScoreEntry>();
}
