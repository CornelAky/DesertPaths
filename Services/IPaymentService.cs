namespace DesertPaths.Services;

/// <summary>
/// Interface for payment processing - allows easy switching between providers
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a payment and returns the redirect URL for the payment page
    /// </summary>
    Task<PaymentResult> CreatePaymentAsync(PaymentRequest request);

    /// <summary>
    /// Queries the status of a payment by transaction reference
    /// </summary>
    Task<PaymentQueryResult> QueryPaymentAsync(string transactionReference);

    /// <summary>
    /// Validates a callback/webhook from the payment provider
    /// </summary>
    bool ValidateCallback(string signature, string payload);

    /// <summary>
    /// Name of the payment provider (for logging/display)
    /// </summary>
    string ProviderName { get; }
}

#region Common Payment Models

public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string CallbackUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionReference { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentResult Failed(string error) => new() { Success = false, ErrorMessage = error };
    public static PaymentResult Succeeded(string transactionRef, string redirectUrl) => new() 
    { 
        Success = true, 
        TransactionReference = transactionRef, 
        RedirectUrl = redirectUrl 
    };
}

public class PaymentQueryResult
{
    public bool Success { get; set; }
    public PaymentTransactionStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public decimal? Amount { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}

public enum PaymentTransactionStatus
{
    Unknown,
    Pending,
    Authorized,  // Success
    Declined,
    Cancelled,
    Refunded
}

#endregion
