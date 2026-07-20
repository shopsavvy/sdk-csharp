using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShopSavvy.DataApi
{
    /// <summary>
    /// SDK version
    /// </summary>
    public static class ShopSavvySdk
    {
        public const string Version = "1.1.0";
    }

    /// <summary>
    /// Configuration for the ShopSavvy API client
    /// </summary>
    public class ShopSavvyConfig
    {
        /// <summary>
        /// Your ShopSavvy API key
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for the API
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.shopsavvy.com/v1";

        /// <summary>
        /// Request timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// API response metadata containing credit usage info
    /// </summary>
    public class ApiMeta
    {
        /// <summary>
        /// Credits used for this request
        /// </summary>
        [JsonProperty("credits_used")]
        public int CreditsUsed { get; set; }

        /// <summary>
        /// Remaining credits
        /// </summary>
        [JsonProperty("credits_remaining")]
        public int CreditsRemaining { get; set; }

        /// <summary>
        /// Remaining rate limit
        /// </summary>
        [JsonProperty("rate_limit_remaining")]
        public int? RateLimitRemaining { get; set; }
    }

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Response data
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; } = default!;

        /// <summary>
        /// Optional message
        /// </summary>
        [JsonProperty("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Response metadata
        /// </summary>
        [JsonProperty("meta")]
        public ApiMeta? Meta { get; set; }

        /// <summary>
        /// Get credits used from meta object
        /// </summary>
        public int CreditsUsed() => Meta?.CreditsUsed ?? 0;

        /// <summary>
        /// Get credits remaining from meta object
        /// </summary>
        public int CreditsRemaining() => Meta?.CreditsRemaining ?? 0;
    }

    /// <summary>
    /// Pagination info for search results
    /// </summary>
    public class PaginationInfo
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("returned")]
        public int Returned { get; set; }
    }

    /// <summary>
    /// Product search result with pagination
    /// </summary>
    public class ProductSearchResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public ProductDetails[] Data { get; set; } = Array.Empty<ProductDetails>();

        [JsonProperty("pagination")]
        public PaginationInfo? Pagination { get; set; }

        [JsonProperty("meta")]
        public ApiMeta? Meta { get; set; }

        /// <summary>
        /// Get credits used from meta object
        /// </summary>
        public int CreditsUsed() => Meta?.CreditsUsed ?? 0;

        /// <summary>
        /// Get credits remaining from meta object
        /// </summary>
        public int CreditsRemaining() => Meta?.CreditsRemaining ?? 0;
    }

    /// <summary>
    /// Product details information
    /// </summary>
    public class ProductDetails
    {
        /// <summary>
        /// Product title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// ShopSavvy product identifier
        /// </summary>
        [JsonProperty("shopsavvy")]
        public string Shopsavvy { get; set; } = string.Empty;

        /// <summary>
        /// Product brand
        /// </summary>
        [JsonProperty("brand")]
        public string? Brand { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        [JsonProperty("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Product images
        /// </summary>
        [JsonProperty("images")]
        public string[]? Images { get; set; }

        /// <summary>
        /// Product barcode
        /// </summary>
        [JsonProperty("barcode")]
        public string? Barcode { get; set; }

        /// <summary>
        /// Amazon ASIN
        /// </summary>
        [JsonProperty("amazon")]
        public string? Amazon { get; set; }

        /// <summary>
        /// Product model number
        /// </summary>
        [JsonProperty("model")]
        public string? Model { get; set; }

        /// <summary>
        /// Manufacturer part number
        /// </summary>
        [JsonProperty("mpn")]
        public string? Mpn { get; set; }

        /// <summary>
        /// Product color
        /// </summary>
        [JsonProperty("color")]
        public string? Color { get; set; }

        [JsonProperty("title_short")]
        public string? TitleShort { get; set; }

        [JsonProperty("slug")]
        public string? Slug { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("categories")]
        public string[]? Categories { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, string>? Attributes { get; set; }

        [JsonProperty("rating")]
        public Dictionary<string, object>? Rating { get; set; }

        /// <summary>
        /// Expert quality scores on a 0-1 scale (multiply by 10 or 100 for
        /// display): "overall", "customer", "professional", plus an "aspects"
        /// map keyed by free-form aspect names from the product's
        /// professional reviews.
        /// </summary>
        [JsonProperty("score")]
        public Dictionary<string, object>? Score { get; set; }

        [JsonProperty("keywords")]
        public string[]? Keywords { get; set; }

        [JsonProperty("identifiers")]
        public Dictionary<string, object>? Identifiers { get; set; }

        // Backward-compatible aliases

        /// <summary>
        /// Product name (deprecated, use Title instead)
        /// </summary>
        [Obsolete("Use Title instead")]
        public string Name => Title;

        /// <summary>
        /// Product ID (deprecated, use Shopsavvy instead)
        /// </summary>
        [Obsolete("Use Shopsavvy instead")]
        public string ProductId => Shopsavvy;

        /// <summary>
        /// ASIN (deprecated, use Amazon instead)
        /// </summary>
        [Obsolete("Use Amazon instead")]
        public string? Asin => Amazon;

        /// <summary>
        /// Image URL (deprecated, use Images instead)
        /// </summary>
        [Obsolete("Use Images instead")]
        public string? ImageUrl => Images?.Length > 0 ? Images[0] : null;
    }

    /// <summary>
    /// Product with nested offers (returned by offers endpoint)
    /// </summary>
    public class ProductWithOffers
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("shopsavvy")]
        public string Shopsavvy { get; set; } = string.Empty;

        [JsonProperty("brand")]
        public string? Brand { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("images")]
        public string[]? Images { get; set; }

        [JsonProperty("barcode")]
        public string? Barcode { get; set; }

        [JsonProperty("amazon")]
        public string? Amazon { get; set; }

        [JsonProperty("model")]
        public string? Model { get; set; }

        [JsonProperty("mpn")]
        public string? Mpn { get; set; }

        [JsonProperty("color")]
        public string? Color { get; set; }

        [JsonProperty("offers")]
        public Offer[] Offers { get; set; } = Array.Empty<Offer>();
    }

    /// <summary>
    /// Single price point in history
    /// </summary>
    public class PriceHistoryEntry
    {
        /// <summary>
        /// Date of price point
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Price at this date
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Availability at this date
        /// </summary>
        [JsonProperty("availability")]
        public string Availability { get; set; } = string.Empty;
    }

    /// <summary>
    /// Product offer from a retailer
    /// </summary>
    public class Offer
    {
        /// <summary>
        /// Unique offer identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Retailer name
        /// </summary>
        [JsonProperty("retailer")]
        public string? Retailer { get; set; }

        /// <summary>
        /// Product price
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        /// <summary>
        /// Availability status
        /// </summary>
        [JsonProperty("availability")]
        public string? Availability { get; set; }

        /// <summary>
        /// Product condition
        /// </summary>
        [JsonProperty("condition")]
        public string? Condition { get; set; }

        /// <summary>
        /// Product URL at retailer
        /// </summary>
        [JsonProperty("URL")]
        public string? Url { get; set; }

        /// <summary>
        /// Seller name
        /// </summary>
        [JsonProperty("seller")]
        public string? Seller { get; set; }

        /// <summary>
        /// Timestamp of last update
        /// </summary>
        [JsonProperty("timestamp")]
        public string? Timestamp { get; set; }

        /// <summary>
        /// Price history (when included)
        /// </summary>
        [JsonProperty("history")]
        public PriceHistoryEntry[]? History { get; set; }

        // Backward-compatible aliases

        /// <summary>
        /// Offer ID (deprecated, use Id instead)
        /// </summary>
        [Obsolete("Use Id instead")]
        public string OfferId => Id;

        /// <summary>
        /// Offer URL (deprecated, use Url instead)
        /// </summary>
        [Obsolete("Use Url instead")]
        public string? OfferUrl => Url;

        /// <summary>
        /// Last updated (deprecated, use Timestamp instead)
        /// </summary>
        [Obsolete("Use Timestamp instead")]
        public string? LastUpdated => Timestamp;
    }

    /// <summary>
    /// Offer with historical price data
    /// </summary>
    public class OfferWithHistory
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("retailer")]
        public string? Retailer { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("availability")]
        public string? Availability { get; set; }

        [JsonProperty("condition")]
        public string? Condition { get; set; }

        [JsonProperty("URL")]
        public string? Url { get; set; }

        [JsonProperty("seller")]
        public string? Seller { get; set; }

        [JsonProperty("timestamp")]
        public string? Timestamp { get; set; }

        /// <summary>
        /// Historical price data
        /// </summary>
        [JsonProperty("price_history")]
        public PriceHistoryEntry[] PriceHistory { get; set; } = Array.Empty<PriceHistoryEntry>();
    }

    /// <summary>
    /// Current billing period details
    /// </summary>
    public class UsagePeriod
    {
        [JsonProperty("start_date")]
        public string StartDate { get; set; } = string.Empty;

        [JsonProperty("end_date")]
        public string EndDate { get; set; } = string.Empty;

        [JsonProperty("credits_used")]
        public int CreditsUsed { get; set; }

        [JsonProperty("credits_limit")]
        public int CreditsLimit { get; set; }

        [JsonProperty("credits_remaining")]
        public int CreditsRemaining { get; set; }

        [JsonProperty("requests_made")]
        public int RequestsMade { get; set; }
    }

    /// <summary>
    /// API usage and credit information
    /// </summary>
    public class UsageInfo
    {
        /// <summary>
        /// Current billing period
        /// </summary>
        [JsonProperty("current_period")]
        public UsagePeriod CurrentPeriod { get; set; } = new UsagePeriod();

        /// <summary>
        /// Usage percentage
        /// </summary>
        [JsonProperty("usage_percentage")]
        public double UsagePercentage { get; set; }

        // Backward-compatible methods

        /// <summary>
        /// Get credits used (deprecated, use CurrentPeriod.CreditsUsed instead)
        /// </summary>
        [Obsolete("Use CurrentPeriod.CreditsUsed instead")]
        public int GetCreditsUsed() => CurrentPeriod.CreditsUsed;

        /// <summary>
        /// Get credits remaining (deprecated, use CurrentPeriod.CreditsRemaining instead)
        /// </summary>
        [Obsolete("Use CurrentPeriod.CreditsRemaining instead")]
        public int GetCreditsRemaining() => CurrentPeriod.CreditsRemaining;

        /// <summary>
        /// Get credits total (deprecated, use CurrentPeriod.CreditsLimit instead)
        /// </summary>
        [Obsolete("Use CurrentPeriod.CreditsLimit instead")]
        public int GetCreditsTotal() => CurrentPeriod.CreditsLimit;

        /// <summary>
        /// Get billing period start (deprecated, use CurrentPeriod.StartDate instead)
        /// </summary>
        [Obsolete("Use CurrentPeriod.StartDate instead")]
        public string GetBillingPeriodStart() => CurrentPeriod.StartDate;

        /// <summary>
        /// Get billing period end (deprecated, use CurrentPeriod.EndDate instead)
        /// </summary>
        [Obsolete("Use CurrentPeriod.EndDate instead")]
        public string GetBillingPeriodEnd() => CurrentPeriod.EndDate;
    }

    /// <summary>
    /// Scheduled product monitoring information
    /// </summary>
    public class ScheduledProduct
    {
        /// <summary>
        /// Product identifier
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Original identifier used for scheduling
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Monitoring frequency
        /// </summary>
        [JsonProperty("frequency")]
        public string Frequency { get; set; } = string.Empty;

        /// <summary>
        /// Optional retailer filter
        /// </summary>
        [JsonProperty("retailer")]
        public string? Retailer { get; set; }

        /// <summary>
        /// When monitoring was created
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Last refresh timestamp
        /// </summary>
        [JsonProperty("last_refreshed")]
        public string? LastRefreshed { get; set; }
    }

    /// <summary>
    /// Response from scheduling a product
    /// </summary>
    public class ScheduleResponse
    {
        /// <summary>
        /// Whether scheduling was successful
        /// </summary>
        [JsonProperty("scheduled")]
        public bool Scheduled { get; set; }

        /// <summary>
        /// Product identifier
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from batch scheduling
    /// </summary>
    public class ScheduleBatchResponse
    {
        /// <summary>
        /// Original identifier
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Whether scheduling was successful
        /// </summary>
        [JsonProperty("scheduled")]
        public bool Scheduled { get; set; }

        /// <summary>
        /// Product identifier
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from removing a product from schedule
    /// </summary>
    public class RemoveResponse
    {
        /// <summary>
        /// Whether removal was successful
        /// </summary>
        [JsonProperty("removed")]
        public bool Removed { get; set; }
    }

    /// <summary>
    /// Response from batch removal
    /// </summary>
    public class RemoveBatchResponse
    {
        /// <summary>
        /// Original identifier
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Whether removal was successful
        /// </summary>
        [JsonProperty("removed")]
        public bool Removed { get; set; }
    }
}
