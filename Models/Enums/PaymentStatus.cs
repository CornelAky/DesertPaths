namespace DesertPaths.Models.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,  // Renamed from Success for clarity
    Failed = 2,
    Refunded = 3,
    Cancelled = 4
}
