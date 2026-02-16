using System.ComponentModel.DataAnnotations;

namespace DesertPaths.Models.Entities;

public class JourneyImage
{
    public int Id { get; set; }

    public int JourneyId { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    // Navigation
    public virtual Journey Journey { get; set; } = null!;
}
