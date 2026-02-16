namespace DesertPaths.Services;

/// <summary>
/// Mock payment service for development and testing.
/// Simulates payment processing without actual payment gateway.
/// </summary>
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;
    private static readonly Dictionary<string, MockPaymentData> _payments = new();

    public string ProviderName => "Mock Payment (Development)";

    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
    }

    public Task<PaymentResult> CreatePaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("🧪 MOCK: Creating payment for order {OrderId}, Amount: {Amount} {Currency}", 
            request.OrderId, request.Amount, request.Currency);

        // Generate a mock transaction reference
        var transactionRef = $"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // Store payment data for later verification
        _payments[transactionRef] = new MockPaymentData
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            CustomerEmail = request.CustomerEmail,
            CreatedAt = DateTime.UtcNow,
            ReturnUrl = request.ReturnUrl
        };

        // Create a mock payment URL that redirects to our mock payment page
        var redirectUrl = $"/Payment/MockCheckout?transRef={transactionRef}";

        _logger.LogInformation("🧪 MOCK: Payment created with reference {TransRef}", transactionRef);

        return Task.FromResult(PaymentResult.Succeeded(transactionRef, redirectUrl));
    }

    public Task<PaymentQueryResult> QueryPaymentAsync(string transactionReference)
    {
        _logger.LogInformation("🧪 MOCK: Querying payment {TransRef}", transactionReference);

        if (_payments.TryGetValue(transactionReference, out var payment))
        {
            return Task.FromResult(new PaymentQueryResult
            {
                Success = true,
                Status = payment.IsCompleted ? PaymentTransactionStatus.Authorized : PaymentTransactionStatus.Pending,
                TransactionReference = transactionReference,
                Amount = payment.Amount,
                ResponseCode = payment.IsCompleted ? "000" : "P01",
                ResponseMessage = payment.IsCompleted ? "Payment Successful" : "Payment Pending"
            });
        }

        return Task.FromResult(new PaymentQueryResult
        {
            Success = false,
            Status = PaymentTransactionStatus.Unknown,
            ResponseMessage = "Transaction not found"
        });
    }

    public bool ValidateCallback(string signature, string payload)
    {
        // In mock mode, always return true
        return true;
    }

    /// <summary>
    /// Simulates completing a payment (called from mock checkout page)
    /// </summary>
    public static bool CompletePayment(string transactionReference, bool success)
    {
        if (_payments.TryGetValue(transactionReference, out var payment))
        {
            payment.IsCompleted = success;
            payment.CompletedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets mock payment data for display
    /// </summary>
    public static MockPaymentData? GetPaymentData(string transactionReference)
    {
        return _payments.TryGetValue(transactionReference, out var payment) ? payment : null;
    }

    public class MockPaymentData
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
