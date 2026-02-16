using System.ComponentModel.DataAnnotations;
using DesertPaths.Models.Enums;

namespace DesertPaths.ViewModels;

public class CreateBookingViewModel
{
    public int JourneyId { get; set; }
    public string JourneyTitle { get; set; } = string.Empty;
    public string JourneySlug { get; set; } = string.Empty;
    public string LandName { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int DurationNights { get; set; }
    public decimal BasePrice { get; set; }
    public string? HeroImageUrl { get; set; }

    [Required(ErrorMessage = "Please select a travel style")]
    public int StyleId { get; set; }

    [Required(ErrorMessage = "Please select a travel date")]
    [DataType(DataType.Date)]
    public DateTime TravelDate { get; set; } = DateTime.Today.AddDays(14);

    [Required]
    [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
    public int NumberOfGuests { get; set; } = 1;

    /// <summary>
    /// Payment method: Online or OnArrival
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;

    /// <summary>
    /// Contact phone number
    /// </summary>
    [Required(ErrorMessage = "Please enter a contact phone number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Contact email (pre-filled from user)
    /// </summary>
    [Required(ErrorMessage = "Please enter a contact email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    // For display
    public List<StyleOption> AvailableStyles { get; set; } = new();
    public int MaxGroupSize { get; set; }
}

public class StyleOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceMultiplier { get; set; }
    public decimal CalculatedPrice { get; set; }
}

public class BookingConfirmationViewModel
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string JourneyTitle { get; set; } = string.Empty;
    public string LandName { get; set; } = string.Empty;
    public string StyleName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string? ContactPhone { get; set; }
}

public class MyBookingsViewModel
{
    public List<BookingListItem> Bookings { get; set; } = new();
}

public class BookingListItem
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string JourneyTitle { get; set; } = string.Empty;
    public string? JourneyImageUrl { get; set; }
    public string LandName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanCancel { get; set; }
    public bool CanReview { get; set; }
    public bool CanPay { get; set; }
    public bool HasReviewed { get; set; }
    public int JourneyId { get; set; }
}

public class CreateReviewViewModel
{
    public int BookingId { get; set; }
    public int JourneyId { get; set; }
    public string JourneyTitle { get; set; } = string.Empty;
    public string? JourneyImageUrl { get; set; }
    public string LandName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }

    [Required(ErrorMessage = "Please select a rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }
}
