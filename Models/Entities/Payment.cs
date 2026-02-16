using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DesertPaths.Models.Enums;

namespace DesertPaths.Models.Entities;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    [MaxLength(100)]
    public string TransactionReference { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PayTabsTransactionId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "SAR";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(50)]
    public string? ResponseCode { get; set; }

    [MaxLength(500)]
    public string? ResponseMessage { get; set; }

    public string? PayTabsResponseJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    // Navigation
    public virtual Booking Booking { get; set; } = null!;
}
