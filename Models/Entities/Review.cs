using System.ComponentModel.DataAnnotations;

namespace DesertPaths.Models.Entities;

public class Review
{
    public int Id { get; set; }

    public int JourneyId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Journey Journey { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}
