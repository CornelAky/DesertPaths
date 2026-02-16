namespace DesertPaths.Services;

public interface IBookingService
{
    string GenerateBookingReference();
    decimal CalculateTotalPrice(decimal basePrice, decimal styleMultiplier, int numberOfGuests);
}

public class BookingService : IBookingService
{
    public string GenerateBookingReference()
    {
        // Format: DP-YYYYMMDD-XXXX (e.g., DP-20260211-A3F7)
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpper();
        return $"DP-{datePart}-{randomPart}";
    }

    public decimal CalculateTotalPrice(decimal basePrice, decimal styleMultiplier, int numberOfGuests)
    {
        return basePrice * styleMultiplier * numberOfGuests;
    }
}
