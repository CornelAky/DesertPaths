namespace DesertPaths.Configuration;

public class PayTabsSettings
{
    public const string SectionName = "PayTabs";
    
    /// <summary>
    /// Your PayTabs Profile ID (Merchant ID)
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;
    
    /// <summary>
    /// Server Key from PayTabs dashboard
    /// </summary>
    public string ServerKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Region: SAU (Saudi Arabia), ARE (UAE), EGY (Egypt), OMN (Oman), JOR (Jordan), GLOBAL
    /// </summary>
    public string Region { get; set; } = "SAU";
    
    /// <summary>
    /// Currency code (SAR, AED, USD, etc.)
    /// </summary>
    public string Currency { get; set; } = "SAR";
    
    /// <summary>
    /// Use sandbox/test environment
    /// </summary>
    public bool UseSandbox { get; set; } = true;
    
    /// <summary>
    /// Gets the API base URL based on region and sandbox mode
    /// </summary>
    public string BaseUrl => GetBaseUrl();
    
    private string GetBaseUrl()
    {
        // PayTabs regional endpoints
        var regionUrls = new Dictionary<string, string>
        {
            { "SAU", "https://secure.paytabs.sa" },
            { "ARE", "https://secure.paytabs.com" },
            { "EGY", "https://secure-egypt.paytabs.com" },
            { "OMN", "https://secure-oman.paytabs.com" },
            { "JOR", "https://secure-jordan.paytabs.com" },
            { "GLOBAL", "https://secure-global.paytabs.com" }
        };
        
        return regionUrls.TryGetValue(Region.ToUpper(), out var url) 
            ? url 
            : regionUrls["GLOBAL"];
    }
}
