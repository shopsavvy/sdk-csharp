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
    /// var product = await client.GetProductDetailsAsync("012345678901");
    /// Console.WriteLine($"Product: {product.Data.Name}");
    /// </code>
    /// </example>
    public class ShopSavvyDataApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ShopSavvyConfig _config;
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

            // Create HTTP client
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.BaseUrl),
                Timeout = _config.Timeout
            };

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ShopSavvy-CSharp-SDK/1.0.0");
        }

        /// <summary>
        /// Look up product details by identifier
        /// </summary>
        /// <param name="identifier">Product identifier (barcode, ASIN, URL, model number, or ShopSavvy product ID)</param>
        /// <param name="format">Response format ('json' or 'csv')</param>
        /// <returns>Product details</returns>
        public async Task<ApiResponse<ProductDetails>> GetProductDetailsAsync(string identifier, string? format = null)
        {
            var queryParams = new Dictionary<string, string> { { "identifier", identifier } };
            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
            }

            return await MakeRequestAsync<ProductDetails>("GET", "/products/details", queryParams);
        }

        /// <summary>
        /// Look up details for multiple products
        /// </summary>
        /// <param name="identifiers">Array of product identifiers</param>
        /// <param name="format">Response format ('json' or 'csv')</param>
        /// <returns>Array of product details</returns>
        public async Task<ApiResponse<ProductDetails[]>> GetProductDetailsBatchAsync(string[] identifiers, string? format = null)
        {
            var queryParams = new Dictionary<string, string> { { "identifiers", string.Join(",", identifiers) } };
            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
            }

            return await MakeRequestAsync<ProductDetails[]>("GET", "/products/details", queryParams);
        }

        /// <summary>
        /// Get current offers for a product
        /// </summary>
        /// <param name="identifier">Product identifier</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <param name="format">Response format ('json' or 'csv')</param>
        /// <returns>Current offers</returns>
        public async Task<ApiResponse<Offer[]>> GetCurrentOffersAsync(string identifier, string? retailer = null, string? format = null)
        {
            var queryParams = new Dictionary<string, string> { { "identifier", identifier } };
            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }
            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
            }

            return await MakeRequestAsync<Offer[]>("GET", "/products/offers", queryParams);
        }

        /// <summary>
        /// Get current offers for multiple products
        /// </summary>
        /// <param name="identifiers">Array of product identifiers</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <param name="format">Response format ('json' or 'csv')</param>
        /// <returns>Dictionary mapping identifiers to their offers</returns>
        public async Task<ApiResponse<Dictionary<string, Offer[]>>> GetCurrentOffersBatchAsync(string[] identifiers, string? retailer = null, string? format = null)
        {
            var queryParams = new Dictionary<string, string> { { "identifiers", string.Join(",", identifiers) } };
            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }
            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
            }

            return await MakeRequestAsync<Dictionary<string, Offer[]>>("GET", "/products/offers", queryParams);
        }

        /// <summary>
        /// Get price history for a product
        /// </summary>
        /// <param name="identifier">Product identifier</param>
        /// <param name="startDate">Start date (YYYY-MM-DD format)</param>
        /// <param name="endDate">End date (YYYY-MM-DD format)</param>
        /// <param name="retailer">Optional retailer to filter by</param>
        /// <param name="format">Response format ('json' or 'csv')</param>
        /// <returns>Offers with price history</returns>
        public async Task<ApiResponse<OfferWithHistory[]>> GetPriceHistoryAsync(string identifier, string startDate, string endDate, string? retailer = null, string? format = null)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "identifier", identifier },
                { "start_date", startDate },
                { "end_date", endDate }
            };

            if (!string.IsNullOrEmpty(retailer))
            {
                queryParams["retailer"] = retailer;
            }
            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
            }

            return await MakeRequestAsync<OfferWithHistory[]>("GET", "/products/history", queryParams);
        }

        /// <summary>
        /// Schedule product monitoring
        /// </summary>
        /// <param name="identifier">Product identifier</param>
        /// <param name="frequency">How often to refresh ('hourly', 'daily', 'weekly')</param>
        /// <param name="retailer">Optional retailer to monitor</param>
        /// <returns>Scheduling confirmation</returns>
        public async Task<ApiResponse<ScheduleResponse>> ScheduleProductMonitoringAsync(string identifier, string frequency, string? retailer = null)
        {
            var body = new { identifier, frequency, retailer };
            return await MakeRequestAsync<ScheduleResponse>("POST", "/products/schedule", body: body);
        }

        /// <summary>
        /// Schedule monitoring for multiple products
        /// </summary>
        /// <param name="identifiers">Array of product identifiers</param>
        /// <param name="frequency">How often to refresh</param>
        /// <param name="retailer">Optional retailer to monitor</param>
        /// <returns>Scheduling confirmation for all products</returns>
        public async Task<ApiResponse<ScheduleBatchResponse[]>> ScheduleProductMonitoringBatchAsync(string[] identifiers, string frequency, string? retailer = null)
        {
            var body = new { identifiers = string.Join(",", identifiers), frequency, retailer };
            return await MakeRequestAsync<ScheduleBatchResponse[]>("POST", "/products/schedule", body: body);
        }

        /// <summary>
        /// Get all scheduled products
        /// </summary>
        /// <returns>List of scheduled products</returns>
        public async Task<ApiResponse<ScheduledProduct[]>> GetScheduledProductsAsync()
        {
            return await MakeRequestAsync<ScheduledProduct[]>("GET", "/products/scheduled");
        }

        /// <summary>
        /// Remove product from monitoring schedule
        /// </summary>
        /// <param name="identifier">Product identifier to remove</param>
        /// <returns>Removal confirmation</returns>
        public async Task<ApiResponse<RemoveResponse>> RemoveProductFromScheduleAsync(string identifier)
        {
            var body = new { identifier };
            return await MakeRequestAsync<RemoveResponse>("DELETE", "/products/schedule", body: body);
        }

        /// <summary>
        /// Remove multiple products from monitoring schedule
        /// </summary>
        /// <param name="identifiers">Array of product identifiers to remove</param>
        /// <returns>Removal confirmation for all products</returns>
        public async Task<ApiResponse<RemoveBatchResponse[]>> RemoveProductsFromScheduleAsync(string[] identifiers)
        {
            var body = new { identifiers = string.Join(",", identifiers) };
            return await MakeRequestAsync<RemoveBatchResponse[]>("DELETE", "/products/schedule", body: body);
        }

        /// <summary>
        /// Get API usage information
        /// </summary>
        /// <returns>Current usage and credit information</returns>
        public async Task<ApiResponse<UsageInfo>> GetUsageAsync()
        {
            return await MakeRequestAsync<UsageInfo>("GET", "/usage");
        }

        private async Task<ApiResponse<T>> MakeRequestAsync<T>(string method, string endpoint, Dictionary<string, string>? queryParams = null, object? body = null)
        {
            var url = endpoint;
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