using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesertPaths.Models.Entities;

public class JourneyStyle
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal PriceMultiplier { get; set; } = 1.0m;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
