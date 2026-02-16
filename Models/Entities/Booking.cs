using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DesertPaths.Models.Enums;

namespace DesertPaths.Models.Entities;

public class Booking
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string BookingReference { get; set; } = string.Empty;

    public int JourneyId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int StyleId { get; set; }

    public DateTime TravelDate { get; set; }

    public int NumberOfGuests { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    /// <summary>
    /// Payment method: Online or OnArrival
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;

    /// <summary>
    /// Has the booking been paid?
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Contact phone number for the booking
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email for the booking (can be different from user email)
    /// </summary>
    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    /// <summary>
    /// Admin notes (internal)
    /// </summary>
    [MaxLength(1000)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual Journey Journey { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual JourneyStyle Style { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
