using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DesertPaths.Models.Enums;

namespace DesertPaths.Models.Entities;

public class Journey
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public int LandId { get; set; }

    public int DefaultStyleId { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public int DurationDays { get; set; }

    public int DurationNights { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceFrom { get; set; }

    public int MaxGroupSize { get; set; } = 20;

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Moderate;

    public string? HeroImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Land Land { get; set; } = null!;
    public virtual JourneyStyle DefaultStyle { get; set; } = null!;
    public virtual ICollection<JourneyItinerary> Itineraries { get; set; } = new List<JourneyItinerary>();
    public virtual ICollection<JourneyImage> Images { get; set; } = new List<JourneyImage>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
