using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShopSavvy.DataApi
{
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
        /// Credits used for this request
        /// </summary>
        [JsonProperty("credits_used")]
        public int? CreditsUsed { get; set; }

        /// <summary>
        /// Remaining credits
        /// </summary>
        [JsonProperty("credits_remaining")]
        public int? CreditsRemaining { get; set; }
    }

    /// <summary>
    /// Product details information
    /// </summary>
    public class ProductDetails
    {
        /// <summary>
        /// Unique product identifier
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

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
        /// Product image URL
        /// </summary>
        [JsonProperty("image_url")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Product barcode
        /// </summary>
        [JsonProperty("barcode")]
        public string? Barcode { get; set; }

        /// <summary>
        /// Amazon ASIN
        /// </summary>
        [JsonProperty("asin")]
        public string? Asin { get; set; }

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
        /// Product description
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Additional product identifiers
        /// </summary>
        [JsonProperty("identifiers")]
        public Dictionary<string, string>? Identifiers { get; set; }
    }

    /// <summary>
    /// Product offer from a retailer
    /// </summary>
    public class Offer
    {
        /// <summary>
        /// Unique offer identifier
        /// </summary>
        [JsonProperty("offer_id")]
        public string OfferId { get; set; } = string.Empty;

        /// <summary>
        /// Retailer name
        /// </summary>
        [JsonProperty("retailer")]
        public string Retailer { get; set; } = string.Empty;

        /// <summary>
        /// Product price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Availability status
        /// </summary>
        [JsonProperty("availability")]
        public string Availability { get; set; } = string.Empty;

        /// <summary>
        /// Product condition
        /// </summary>
        [JsonProperty("condition")]
        public string Condition { get; set; } = string.Empty;

        /// <summary>
        /// Product URL at retailer
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Shipping cost
        /// </summary>
        [JsonProperty("shipping")]
        public decimal? Shipping { get; set; }

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; } = string.Empty;
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
    /// Offer with historical price data
    /// </summary>
    public class OfferWithHistory : Offer
    {
        /// <summary>
        /// Historical price data
        /// </summary>
        [JsonProperty("price_history")]
        public PriceHistoryEntry[] PriceHistory { get; set; } = Array.Empty<PriceHistoryEntry>();
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
    /// API usage and credit information
    /// </summary>
    public class UsageInfo
    {
        /// <summary>
        /// Credits used this billing period
        /// </summary>
        [JsonProperty("credits_used")]
        public int CreditsUsed { get; set; }

        /// <summary>
        /// Credits remaining this billing period
        /// </summary>
        [JsonProperty("credits_remaining")]
        public int CreditsRemaining { get; set; }

        /// <summary>
        /// Total credits for this billing period
        /// </summary>
        [JsonProperty("credits_total")]
        public int CreditsTotal { get; set; }

        /// <summary>
        /// Current billing period start date
        /// </summary>
        [JsonProperty("billing_period_start")]
        public string BillingPeriodStart { get; set; } = string.Empty;

        /// <summary>
        /// Current billing period end date
        /// </summary>
        [JsonProperty("billing_period_end")]
        public string BillingPeriodEnd { get; set; } = string.Empty;

        /// <summary>
        /// Plan name
        /// </summary>
        [JsonProperty("plan_name")]
        public string PlanName { get; set; } = string.Empty;
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