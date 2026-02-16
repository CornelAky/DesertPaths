using System.ComponentModel.DataAnnotations;

namespace DesertPaths.Models.Entities;

public class JourneyItinerary
{
    public int Id { get; set; }

    public int JourneyId { get; set; }

    public int DayNumber { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Highlights { get; set; }

    // Navigation
    public virtual Journey Journey { get; set; } = null!;
}
