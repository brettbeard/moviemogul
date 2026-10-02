using System.ComponentModel.DataAnnotations;

namespace MovieMogul.Web.Models;

public class HighScoreEntry
{
    public int Id { get; set; }

    [MaxLength(40)]
    public required string MovieTitle { get; set; }

    [MaxLength(3)]
    public required string Initials { get; set; }

    /// <summary>Disambiguation marker ("", "II", "III"...) for repeat title+initials pairs.</summary>
    [MaxLength(8)]
    public string Marker { get; set; } = "";

    public decimal NetProfit { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime PlayedAtUtc { get; set; }

    public string DisplayName => Marker.Length == 0 ? Initials : $"{Initials} {Marker}";
}
