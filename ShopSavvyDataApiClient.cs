using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShopSavvy.DataApi
{
    /// <summary>
    /// Official C# client for ShopSavvy Data API
    ///
    /// Provides access to product data, pricing information, and price history
    /// across thousands of retailers and millions of products.
    /// </summary>
    /// <example>
    /// <code>
    /// var client = new ShopSavvyDataApiClient("ss_live_your_api_key_here");
    /// var product = await client.GetProductsAsync("012345678901");
    /// Console.WriteLine($"Product: {product.Data[0].Title}");
    /// </code>
    /// </example>
    public class ShopSavvyDataApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ShopSavvyConfig _config;

        /// <summary>
        /// Absolute API root every request is built from (e.g. https://api.shopsavvy.com/v1),
        /// with no trailing slash.
        ///
        /// Requests are built as <c>_baseUrl + "/products/..."</c> rather than resolved against
        /// <see cref="HttpClient.BaseAddress"/>: resolving a root-relative path like
        /// <c>/products</c> against <c>https://api.shopsavvy.com/v1</c> REPLACES the path, so
        /// every call went to <c>https://api.shopsavvy.com/products</c> with the <c>/v1</c>
        /// silently dropped. The deals/batch/webhook/review methods also referenced this field
        /// before it was ever declared, so the package did not compile at all from 1.1.0 on.
        /// </summary>
        private readonly string _baseUrl;
        private bool _disposed = false;

        /// <summary>
        /// Initialize a new ShopSavvy Data API client
        /// </summary>
        /// <param name="apiKey">Your ShopSavvy API key</param>
        /// <param name="baseUrl">Base URL for API (default: https://api.shopsavvy.com/v1)</param>
        /// <param name="timeout">Request timeout in milliseconds (default: 30000)</param>
        public ShopSavvyDataApiClient(string apiKey, string? baseUrl = null, int timeout = 30000)
            : this(new ShopSavvyConfig
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl ?? "https://api.shopsavvy.com/v1",
                Timeout = TimeSpan.FromMilliseconds(timeout)
            })
        {
        }

        /// <summary>
        /// Initialize a new ShopSavvy Data API client with configuration
        /// </summary>
        /// <param name="config">Client configuration</param>
        public ShopSavvyDataApiClient(ShopSavvyConfig config)
            : this(config, null)
        {
        }

        /// <summary>
        /// Initialize a new ShopSavvy Data API client that sends its requests through a custom
        /// <see cref="HttpMessageHandler"/> — for proxies, logging/retry handlers, or tests.
        /// </summary>
        /// <param name="config">Client configuration</param>
        /// <param name="handler">Handler to send requests through; the default handler when null. Disposed with the client.</param>
        public ShopSavvyDataApiClient(ShopSavvyConfig config, HttpMessageHandler? handler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Validate API key
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new ArgumentException("API key is required. Get one at https://shopsavvy.com/data", nameof(config));
            }

            if (!Regex.IsMatch(_config.ApiKey, @"^ss_(live|test)_[a-zA-Z0-9]+$"))
            {
                throw new ArgumentException("Invalid API key format. API keys should start with ss_live_ or ss_test_", nameof(config));
            }

            _baseUrl = _config.BaseUrl.TrimEnd('/');

            // Create HTTP client
            _httpClient = handler == null ? new HttpClient() : new HttpClient(handler);
            _httpClient.Timeout = _config.Timeout;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"ShopSavvy-CSharp-SDK/{ShopSavvySdk.Version}");
        }

        /// <summary>
        /// Look up product details by identifier
        /// </summary>
        /// <param name="id">Product identifier (barcode, ASIN, URL, model number, or ShopSavvy product ID)</param>
        /// <returns>Product details array</returns>
        public async Task<ApiResponse<ProductDetails[]>> GetProductsAsync(string id)
        {
            var queryParams = new Dictionary<string, string> { { "ids", id } };
            return await MakeRequestAsync<ProductDetails[]>("GET", "/products", queryParams);
        }

        /// <summary>
        /// Look up details for multiple products
        /// </summary>
        /// <param name="ids">Array of product identifiers</param>
        /// <returns>Array of product details</returns>
        public async Task<ApiResponse<ProductDetails[]>> GetProductsBatchAsync(string[] ids)
        {
            var queryParams = new Dictionary<string, string> { { "ids", string.Join(",", ids) } };
            return await MakeRequestAsync<ProductDetails[]>("GET", "/products", queryParams);
        }

        /// <summary>
        /// Look up product details by identifier (deprecated, use GetProductsAsync)
        /// </summary>
        [Obsolete("Use GetProductsAsync instead")]
        public async Task<ApiResponse<ProductDetails>> GetProductDetailsAsync(string identifier, string? format = null)
        {
            var result = await GetProductsAsync(identifier);
            return new ApiResponse<ProductDetails>
            {
                Success = result.Success,
                Data = result.Data?.Length > 0 ? result.Data[0] : null!,
                Message = result.Message,
                Meta = result.Meta
            };
        }

        /// <summary>
        /// Look up details for multiple products (deprecated, use GetProductsBatchAsync)
        /// </summary>
        [Obsolete("Use GetProductsBatchAsync instead")]
        public async Task<ApiResponse<ProductDetails[]>> GetProductDetailsBatchAsync(string[] identifiers, string? format = null)
        {
            return await GetProductsBatchAsync(identifiers);
        }

        /// <summary>
        /// Search for products by query
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="limit">Maximum results (default 20, max 100)</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>Product search results with pagination</returns>
        public async Task<ProductSearchResult> SearchProductsAsync(string query, int limit = 20, int offset = 0)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "q", query },
                { "limit", limit.ToString() },
                { "offset", offset.ToString() }
            };

            return await MakeRequestDirectAsync<ProductSearchResult>("GET", "/products/search", queryParams);
        }

        /// <summary>
        /// Get current offers for a product
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <returns>Products with their current offers</returns>
        public async Task<ApiResponse<ProductWithOffers[]>> GetOffersAsync(string id, string? retailer = null)
        {
            var queryParams = new Dictionary<string, string> { { "ids", id } };
            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }

            return await MakeRequestAsync<ProductWithOffers[]>("GET", "/products/offers", queryParams);
        }

        /// <summary>
        /// Get current offers for multiple products
        /// </summary>
        /// <param name="ids">Array of product identifiers</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <returns>Products with their current offers</returns>
        public async Task<ApiResponse<ProductWithOffers[]>> GetOffersBatchAsync(string[] ids, string? retailer = null)
        {
            var queryParams = new Dictionary<string, string> { { "ids", string.Join(",", ids) } };
            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }

            return await MakeRequestAsync<ProductWithOffers[]>("GET", "/products/offers", queryParams);
        }

        /// <summary>
        /// Get current offers for a product (deprecated, use GetOffersAsync)
        /// </summary>
        [Obsolete("Use GetOffersAsync instead")]
        public async Task<ApiResponse<Offer[]>> GetCurrentOffersAsync(string identifier, string? retailer = null, string? format = null)
        {
            var result = await GetOffersAsync(identifier, retailer);
            var offers = result.Data?.Length > 0 ? result.Data[0].Offers : Array.Empty<Offer>();
            return new ApiResponse<Offer[]>
            {
                Success = result.Success,
                Data = offers,
                Message = result.Message,
                Meta = result.Meta
            };
        }

        /// <summary>
        /// Get current offers for multiple products (deprecated, use GetOffersBatchAsync)
        /// </summary>
        [Obsolete("Use GetOffersBatchAsync instead")]
        public async Task<ApiResponse<Dictionary<string, Offer[]>>> GetCurrentOffersBatchAsync(string[] identifiers, string? retailer = null, string? format = null)
        {
            var result = await GetOffersBatchAsync(identifiers, retailer);
            var dict = new Dictionary<string, Offer[]>();
            if (result.Data != null)
            {
                foreach (var product in result.Data)
                {
                    dict[product.Shopsavvy] = product.Offers;
                }
            }
            return new ApiResponse<Dictionary<string, Offer[]>>
            {
                Success = result.Success,
                Data = dict,
                Message = result.Message,
                Meta = result.Meta
            };
        }

        /// <summary>
        /// Get price history for a product
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <param name="startDate">Start date (YYYY-MM-DD format)</param>
        /// <param name="endDate">End date (YYYY-MM-DD format)</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <returns>
        /// One entry per matched product (product fields plus <c>Offers</c>), where each offer
        /// carries its own <c>History</c> of price points, newest first
        /// </returns>
        public async Task<ApiResponse<ProductWithPriceHistory[]>> GetPriceHistoryAsync(string id, string startDate, string endDate, string? retailer = null)
        {
            var queryParams = new Dictionary<string, string>
            {
                // Wire params are "start"/"end" — what GET /products/offers/history
                // reads, and what the OpenAPI spec and public docs document. The old
                // "start_date"/"end_date" names came from the MCP tool's argument
                // convention (a different interface entirely) and 400'd every call.
                { "ids", id },
                { "start", startDate },
                { "end", endDate }
            };

            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }

            return await MakeRequestAsync<ProductWithPriceHistory[]>("GET", "/products/offers/history", queryParams);
        }

        /// <summary>
        /// Get price history for multiple products
        /// </summary>
        /// <param name="ids">Array of product identifiers</param>
        /// <param name="startDate">Start date (YYYY-MM-DD format)</param>
        /// <param name="endDate">End date (YYYY-MM-DD format)</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <returns>One entry per matched product, each with its offers and their price history</returns>
        public async Task<ApiResponse<ProductWithPriceHistory[]>> GetPriceHistoryBatchAsync(string[] ids, string startDate, string endDate, string? retailer = null)
        {
            var queryParams = new Dictionary<string, string>
            {
                // Wire params are "start"/"end" — see the single-id variant above.
                { "ids", string.Join(",", ids) },
                { "start", startDate },
                { "end", endDate }
            };

            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }

            return await MakeRequestAsync<ProductWithPriceHistory[]>("GET", "/products/offers/history", queryParams);
        }

        /// <summary>
        /// Schedule product monitoring
        /// </summary>
        /// <param name="identifier">Product identifier</param>
        /// <param name="frequency">How often to refresh ('hourly', 'daily', 'weekly')</param>
        /// <param name="retailer">Optional retailer domain to monitor (e.g. "amazon.com")</param>
        /// <returns>The scheduled product (an array: the API always returns <c>data</c> as a list), with its schedule and retailer</returns>
        public async Task<ApiResponse<ScheduledProduct[]>> ScheduleProductMonitoringAsync(string identifier, string frequency, string? retailer = null)
        {
            return await MakeRequestAsync<ScheduledProduct[]>("PUT", "/products/scheduled", ScheduleQueryParams(new[] { identifier }, frequency, retailer));
        }

        /// <summary>
        /// Schedule monitoring for multiple products
        /// </summary>
        /// <param name="identifiers">Array of product identifiers</param>
        /// <param name="frequency">How often to refresh</param>
        /// <param name="retailer">Optional retailer domain to monitor (e.g. "amazon.com")</param>
        /// <returns>One entry per identifier that resolved to a product (unknown identifiers are omitted, and not charged)</returns>
        public async Task<ApiResponse<ScheduledProduct[]>> ScheduleProductMonitoringBatchAsync(string[] identifiers, string frequency, string? retailer = null)
        {
            return await MakeRequestAsync<ScheduledProduct[]>("PUT", "/products/scheduled", ScheduleQueryParams(identifiers, frequency, retailer));
        }

        /// <summary>
        /// Get all scheduled products
        /// </summary>
        /// <returns>List of scheduled products, each with its schedule and retailer</returns>
        public async Task<ApiResponse<ScheduledProduct[]>> GetScheduledProductsAsync()
        {
            return await MakeRequestAsync<ScheduledProduct[]>("GET", "/products/scheduled");
        }

        /// <summary>
        /// Remove product from monitoring schedule
        /// </summary>
        /// <param name="identifier">Product identifier to remove</param>
        /// <returns>Confirmation (<c>success</c>, <c>message</c>, <c>meta</c>; the API returns no <c>data</c>)</returns>
        public async Task<UnscheduleResponse> RemoveProductFromScheduleAsync(string identifier)
        {
            var queryParams = new Dictionary<string, string> { { "ids", identifier } };
            return await MakeRequestDirectAsync<UnscheduleResponse>("DELETE", "/products/scheduled", queryParams);
        }

        /// <summary>
        /// Remove multiple products from monitoring schedule
        /// </summary>
        /// <param name="identifiers">Array of product identifiers to remove</param>
        /// <returns>Confirmation (<c>success</c>, <c>message</c>, <c>meta</c>; the API returns no per-product results)</returns>
        public async Task<UnscheduleResponse> RemoveProductsFromScheduleAsync(string[] identifiers)
        {
            var queryParams = new Dictionary<string, string> { { "ids", string.Join(",", identifiers) } };
            return await MakeRequestDirectAsync<UnscheduleResponse>("DELETE", "/products/scheduled", queryParams);
        }

        /// <summary>
        /// Query string for PUT /products/scheduled.
        ///
        /// The schedule/unschedule handlers read ONLY query parameters — <c>ids</c>
        /// (comma-separated), <c>schedule</c> and optional <c>retailer</c>. These methods used
        /// to POST/DELETE a JSON body (<c>identifier</c>/<c>frequency</c>) to /products/schedule,
        /// which the server ignores entirely, so every call failed for want of <c>ids</c>.
        /// </summary>
        private static Dictionary<string, string> ScheduleQueryParams(string[] identifiers, string frequency, string? retailer)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "ids", string.Join(",", identifiers) },
                { "schedule", frequency }
            };
            if (retailer != null && retailer.Length > 0)
            {
                queryParams["retailer"] = retailer;
            }
            return queryParams;
        }

        /// <summary>
        /// Get API usage information
        /// </summary>
        /// <returns>Current usage and credit information</returns>
        public async Task<ApiResponse<UsageInfo>> GetUsageAsync()
        {
            return await MakeRequestAsync<UsageInfo>("GET", "/usage");
        }

        /// <summary>Browse current shopping deals</summary>
        public async Task<string> GetDealsAsync(Dictionary<string, string>? parameters = null)
        {
            var url = "/deals";
            if (parameters != null && parameters.Count > 0)
            {
                var query = string.Join("&", parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                url += "?" + query;
            }
            var response = await _httpClient.GetAsync(_baseUrl + url);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>Look up multiple products at once (sync for <=20, async for >20)</summary>
        public async Task<string> BatchLookupAsync(string[] identifiers, string[]? include = null)
        {
            var body = new Dictionary<string, object> { { "identifiers", identifiers } };
            if (include != null) body["include"] = include;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + "/products/batch", content);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>Poll for async batch job results</summary>
        public async Task<string> GetBatchStatusAsync(string batchId)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"/batch/{Uri.EscapeDataString(batchId)}");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateWebhookAsync(string url, string[] events)
        {
            var body = new { url, events };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + "/webhooks", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> ListWebhooksAsync()
        {
            var response = await _httpClient.GetAsync(_baseUrl + "/webhooks");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> TestWebhookAsync(string webhookId)
        {
            var response = await _httpClient.PostAsync(_baseUrl + $"/webhooks/{Uri.EscapeDataString(webhookId)}/test", null);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Update a webhook. All parameters are optional, but at least one of
        /// <paramref name="url"/>, <paramref name="events"/>, or
        /// <paramref name="isActive"/> must be non-null.
        /// </summary>
        public async Task<string> UpdateWebhookAsync(string webhookId, string? url = null, string[]? events = null, bool? isActive = null)
        {
            if (url == null && events == null && !isActive.HasValue)
            {
                throw new ArgumentException("UpdateWebhookAsync requires at least one of url, events, or isActive");
            }
            var body = new System.Collections.Generic.Dictionary<string, object>();
            if (url != null) body["url"] = url;
            if (events != null) body["events"] = events;
            if (isActive.HasValue) body["is_active"] = isActive.Value;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, _baseUrl + $"/webhooks/{Uri.EscapeDataString(webhookId)}") { Content = content };
            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteWebhookAsync(string webhookId)
        {
            var response = await _httpClient.DeleteAsync(_baseUrl + $"/webhooks/{Uri.EscapeDataString(webhookId)}");
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>Get TLDR review for a product</summary>
        public async Task<string> GetProductReviewAsync(string identifier)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"/products/reviews?id={Uri.EscapeDataString(identifier)}");
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<ApiResponse<T>> MakeRequestAsync<T>(string method, string endpoint, Dictionary<string, string>? queryParams = null, object? body = null)
        {
            var url = _baseUrl + endpoint;
            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{query}";
            }

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw CreateExceptionFromResponse(response.StatusCode, responseContent);
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
                return apiResponse ?? throw new ShopSavvyApiException("Failed to deserialize response");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ShopSavvyTimeoutException($"Request timeout after {_config.Timeout.TotalSeconds} seconds");
            }
            catch (HttpRequestException ex)
            {
                throw new ShopSavvyNetworkException($"Network error: {ex.Message}", ex);
            }
        }

        private async Task<T> MakeRequestDirectAsync<T>(string method, string endpoint, Dictionary<string, string>? queryParams = null, object? body = null)
        {
            var url = _baseUrl + endpoint;
            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{query}";
            }

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw CreateExceptionFromResponse(response.StatusCode, responseContent);
                }

                var result = JsonConvert.DeserializeObject<T>(responseContent);
                return result ?? throw new ShopSavvyApiException("Failed to deserialize response");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ShopSavvyTimeoutException($"Request timeout after {_config.Timeout.TotalSeconds} seconds");
            }
            catch (HttpRequestException ex)
            {
                throw new ShopSavvyNetworkException($"Network error: {ex.Message}", ex);
            }
        }

        private static Exception CreateExceptionFromResponse(System.Net.HttpStatusCode statusCode, string responseContent)
        {
            var errorMessage = "Unknown error";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.error?.ToString() ?? errorMessage;
            }
            catch
            {
                errorMessage = responseContent;
            }

            return (int)statusCode switch
            {
                401 => new ShopSavvyAuthenticationException("Authentication failed. Check your API key."),
                404 => new ShopSavvyNotFoundException("Resource not found"),
                422 => new ShopSavvyValidationException("Request validation failed. Check your parameters."),
                429 => new ShopSavvyRateLimitException("Rate limit exceeded. Please slow down your requests."),
                _ => new ShopSavvyApiException($"HTTP {(int)statusCode}: {errorMessage}")
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
