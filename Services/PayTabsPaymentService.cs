using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesertPaths.Configuration;
using Microsoft.Extensions.Options;

namespace DesertPaths.Services;

/// <summary>
/// PayTabs payment service implementation
/// </summary>
public class PayTabsPaymentService : IPaymentService
{
    private readonly PayTabsSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PayTabsPaymentService> _logger;

    public string ProviderName => "PayTabs";

    public PayTabsPaymentService(
        IOptions<PayTabsSettings> settings, 
        HttpClient httpClient,
        ILogger<PayTabsPaymentService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", _settings.ServerKey);
    }

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            var payTabsRequest = new PayTabsPaymentRequest
            {
                ProfileId = _settings.ProfileId,
                CartId = request.OrderId,
                CartDescription = request.Description,
                CartCurrency = request.Currency,
                CartAmount = request.Amount,
                CallbackUrl = request.CallbackUrl,
                ReturnUrl = request.ReturnUrl,
                CustomerDetails = new PayTabsCustomerDetails
                {
                    Name = request.CustomerName,
                    Email = request.CustomerEmail,
                    Phone = request.CustomerPhone,
                    Country = "SA"
                },
                HideShipping = true
            };

            var json = JsonSerializer.Serialize(payTabsRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogInformation("PayTabs: Creating payment for order {OrderId}", request.OrderId);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/payment/request", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var payTabsResponse = JsonSerializer.Deserialize<PayTabsPaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (payTabsResponse?.RedirectUrl != null)
                {
                    return PaymentResult.Succeeded(payTabsResponse.TransactionReference!, payTabsResponse.RedirectUrl);
                }
            }

            _logger.LogError("PayTabs payment creation failed: {Response}", responseContent);
            return PaymentResult.Failed("Payment creation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayTabs payment");
            return PaymentResult.Failed(ex.Message);
        }
    }

    public async Task<PaymentQueryResult> QueryPaymentAsync(string transactionReference)
    {
        try
        {
            var request = new { profile_id = _settings.ProfileId, tran_ref = transactionReference };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/payment/query", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var queryResponse = JsonSerializer.Deserialize<PayTabsQueryResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var status = queryResponse?.PaymentResult?.ResponseStatus?.ToUpper() switch
                {
                    "A" => PaymentTransactionStatus.Authorized,
                    "D" => PaymentTransactionStatus.Declined,
                    "C" => PaymentTransactionStatus.Cancelled,
                    "P" => PaymentTransactionStatus.Pending,
                    _ => PaymentTransactionStatus.Unknown
                };

                return new PaymentQueryResult
                {
                    Success = true,
                    Status = status,
                    TransactionReference = transactionReference,
                    Amount = decimal.TryParse(queryResponse?.CartAmount, out var amt) ? amt : null,
                    ResponseCode = queryResponse?.PaymentResult?.ResponseCode,
                    ResponseMessage = queryResponse?.PaymentResult?.ResponseMessage
                };
            }

            return new PaymentQueryResult { Success = false, Status = PaymentTransactionStatus.Unknown };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying PayTabs payment");
            return new PaymentQueryResult { Success = false, Status = PaymentTransactionStatus.Unknown };
        }
    }

    public bool ValidateCallback(string signature, string payload)
    {
        // In production, implement HMAC validation with server key
        return !string.IsNullOrEmpty(signature);
    }
}

#region PayTabs Specific Models

internal class PayTabsPaymentRequest
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = string.Empty;
    
    [JsonPropertyName("tran_type")]
    public string TransactionType { get; set; } = "sale";
    
    [JsonPropertyName("tran_class")]
    public string TransactionClass { get; set; } = "ecom";
    
    [JsonPropertyName("cart_id")]
    public string CartId { get; set; } = string.Empty;
    
    [JsonPropertyName("cart_description")]
    public string CartDescription { get; set; } = string.Empty;
    
    [JsonPropertyName("cart_currency")]
    public string CartCurrency { get; set; } = "SAR";
    
    [JsonPropertyName("cart_amount")]
    public decimal CartAmount { get; set; }
    
    [JsonPropertyName("callback")]
    public string? CallbackUrl { get; set; }
    
    [JsonPropertyName("return")]
    public string? ReturnUrl { get; set; }
    
    [JsonPropertyName("customer_details")]
    public PayTabsCustomerDetails? CustomerDetails { get; set; }
    
    [JsonPropertyName("hide_shipping")]
    public bool HideShipping { get; set; } = true;
}

internal class PayTabsCustomerDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = "SA";
}

internal class PayTabsPaymentResponse
{
    [JsonPropertyName("tran_ref")]
    public string? TransactionReference { get; set; }
    
    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }
}

internal class PayTabsQueryResponse
{
    [JsonPropertyName("tran_ref")]
    public string? TransactionReference { get; set; }
    
    [JsonPropertyName("cart_amount")]
    public string? CartAmount { get; set; }
    
    [JsonPropertyName("payment_result")]
    public PayTabsPaymentResult? PaymentResult { get; set; }
}

internal class PayTabsPaymentResult
{
    [JsonPropertyName("response_status")]
    public string? ResponseStatus { get; set; }
    
    [JsonPropertyName("response_code")]
    public string? ResponseCode { get; set; }
    
    [JsonPropertyName("response_message")]
    public string? ResponseMessage { get; set; }
}

/// <summary>
/// Callback model for PayTabs webhook
/// </summary>
public class PayTabsCallback
{
    [JsonPropertyName("tran_ref")]
    public string? TransactionReference { get; set; }
    
    [JsonPropertyName("cart_id")]
    public string? CartId { get; set; }
    
    [JsonPropertyName("cart_amount")]
    public string? CartAmount { get; set; }
    
    [JsonPropertyName("respStatus")]
    public string? ResponseStatus { get; set; }
    
    [JsonPropertyName("respCode")]
    public string? ResponseCode { get; set; }
    
    [JsonPropertyName("respMessage")]
    public string? ResponseMessage { get; set; }
    
    public bool IsSuccess => ResponseStatus?.ToUpper() == "A";
}

#endregion
