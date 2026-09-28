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
        public const string Version = "1.4.0";
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
        /// Server-side request identifier — quote it when contacting support
        /// </summary>
        [JsonProperty("request_id")]
        public string? RequestId { get; set; }

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
    /// One product in a <c>GET /products/offers</c> response, as returned by
    /// <c>GetOffersAsync</c> / <c>GetOffersBatchAsync</c>: every product field the products
    /// endpoint returns (title, identifiers, images, description, rating, score, …) plus the
    /// product's current <see cref="Offers"/>.
    ///
    /// Until 1.4.0 this class re-declared only the ten core product fields, so the expanded
    /// fields the API sends on this endpoint (<c>title_short</c>, <c>slug</c>,
    /// <c>description</c>, <c>categories</c>, <c>attributes</c>, <c>rating</c>, <c>score</c>,
    /// <c>keywords</c>, <c>identifiers</c>) were silently dropped.
    /// </summary>
    public class ProductWithOffers : ProductDetails
    {
        /// <summary>
        /// Current offers for this product (filtered to one retailer when a retailer was
        /// requested). Each offer's <see cref="Offer.History"/> is empty on this endpoint; use
        /// <c>GetPriceHistoryAsync</c> for history.
        /// </summary>
        [JsonProperty("offers")]
        public Offer[] Offers { get; set; } = Array.Empty<Offer>();
    }

    /// <summary>
    /// Single price point in history.
    ///
    /// The timestamp property binds to <c>timestamp</c>, matching the parent Offer's own
    /// <c>timestamp</c> and the real wire shape (<c>{availability, price, timestamp}</c>).
    /// Every SDK in the fleet bound it to a <c>date</c> key — one the API has never sent —
    /// until 2026-08-10, and Json.NET leaves an unmatched property at its default, so this
    /// value read as <c>string.Empty</c> for every consumer
    /// (ShopSavvy prospector-audit s28-t2-2).
    /// </summary>
    public class PriceHistoryEntry
    {
        /// <summary>
        /// ISO-8601 timestamp of the observation
        /// </summary>
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Price at this date
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// ISO 4217 code the Price is denominated in. Null on an archived point with no recorded
        /// currency - never assume a missing value means USD (ShopSavvy prospector-audit d5-t3-1).
        /// </summary>
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        /// <summary>
        /// Availability at this observation (<c>"in"</c> or <c>"out"</c>). Null when the
        /// availability at that point was not confirmed — the API omits the key rather than
        /// sending a guess.
        /// </summary>
        [JsonProperty("availability")]
        public string? Availability { get; set; }
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
        /// Availability: <c>"in"</c>, <c>"out"</c>, <c>"limited"</c>, <c>"pre-order"</c>,
        /// <c>"coming-soon"</c> or <c>"discontinued"</c>. Null when unknown — the API omits the key.
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
    /// Offer with historical price data: every field of <see cref="Offer"/> (the offer's
    /// CURRENT state at that retailer) plus its <see cref="History"/> of price points.
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
        /// Historical price data.
        ///
        /// Binds to <c>history</c>. It used to bind to <c>price_history</c>, a key the API
        /// has never sent, so this array silently stayed at its empty default on every
        /// successful response (ShopSavvy prospector-audit s28-t2-2).
        /// </summary>
        [JsonProperty("history")]
        public PriceHistoryEntry[] History { get; set; } = Array.Empty<PriceHistoryEntry>();
    }

    /// <summary>
    /// One product in a price-history response, as returned by
    /// <c>GetPriceHistoryAsync</c> / <c>GetPriceHistoryBatchAsync</c>.
    ///
    /// <c>GET /products/offers/history</c> returns one entry PER PRODUCT — the same product
    /// fields as the products endpoint — with an <c>offers</c> array, and each offer carries
    /// its own <c>history</c>. Until 1.4.0 the SDK typed <c>data</c> as a flat array of
    /// <see cref="OfferWithHistory"/>, so Json.NET bound product objects onto offer fields:
    /// every <c>Id</c>, <c>Price</c> and <c>History</c> read empty on a successful response.
    /// </summary>
    public class ProductWithPriceHistory : ProductDetails
    {
        /// <summary>
        /// Offers for this product at each retailer, each with its price history
        /// </summary>
        [JsonProperty("offers")]
        public OfferWithHistory[] Offers { get; set; } = Array.Empty<OfferWithHistory>();
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
    /// One product on the refresh schedule: every product field the products endpoint returns,
    /// plus the refresh <see cref="Schedule"/> and the optional <see cref="Retailer"/> filter.
    ///
    /// This is the element type of <c>data</c> for BOTH <c>PUT /products/scheduled</c>
    /// (<see cref="ShopSavvyDataApiClient.ScheduleProductMonitoringAsync"/>, which echoes back the
    /// products it scheduled) and <c>GET /products/scheduled</c>
    /// (<see cref="ShopSavvyDataApiClient.GetScheduledProductsAsync"/>). Until 1.4.0 the SDK
    /// modelled these as <c>{scheduled, product_id}</c> and <c>{product_id, identifier,
    /// frequency, created_at, last_refreshed}</c> — keys the API has never sent — so every
    /// field read at its default on a successful response.
    /// </summary>
    public class ScheduledProduct : ProductDetails
    {
        /// <summary>
        /// Refresh schedule: <c>"hourly"</c>, <c>"daily"</c> or <c>"weekly"</c>. Always present on
        /// a schedule response. On the list it is null when the product's refresh interval has no
        /// Data API label (e.g. a 4h/12h interval set from ShopSavvy Business) — the API omits it.
        /// </summary>
        [JsonProperty("schedule")]
        public string? Schedule { get; set; }

        /// <summary>
        /// Retailer domain the schedule is restricted to (e.g. <c>"amazon.com"</c>). Null when the
        /// product is scheduled across all retailers — the API omits the key.
        /// </summary>
        [JsonProperty("retailer")]
        public string? Retailer { get; set; }
    }

    /// <summary>
    /// Response from <c>DELETE /products/scheduled</c>.
    ///
    /// The API returns <c>{success, message, meta}</c> with NO <c>data</c> key; it does not report
    /// per-product results. Until 1.4.0 the SDK typed this as <c>data: {removed}</c> /
    /// <c>data: [{identifier, removed}]</c>, which always read <c>false</c>/empty.
    /// </summary>
    public class UnscheduleResponse
    {
        /// <summary>
        /// Whether the request succeeded
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Confirmation message, e.g. "Products successfully removed from schedule"
        /// </summary>
        [JsonProperty("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Response metadata (unscheduling is free: credits_used is 0)
        /// </summary>
        [JsonProperty("meta")]
        public ApiMeta? Meta { get; set; }
    }
}
