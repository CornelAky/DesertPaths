using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DesertPaths.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Country { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsBlocked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";

    // Navigation Properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
