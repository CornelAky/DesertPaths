using System.ComponentModel.DataAnnotations;

namespace DesertPaths.Models.Entities;

public class Land
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? HeroImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();
}
