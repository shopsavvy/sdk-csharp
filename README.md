# ShopSavvy C# SDK

Official C# SDK for the ShopSavvy Data API - Access product data, pricing information, and price history across thousands of retailers and millions of products.

## Installation

### NuGet Package Manager

```
Install-Package ShopSavvy.DataApi
```

### .NET CLI

```
dotnet add package ShopSavvy.DataApi
```

### PackageReference

```xml
<PackageReference Include="ShopSavvy.DataApi" Version="1.0.0" />
```

## Quick Start

```csharp
using System;
using System.Threading.Tasks;
using ShopSavvy.DataApi;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a new client
        using var client = new ShopSavvyDataApiClient("ss_live_your_api_key_here");
        
        // Look up a product by barcode
        var product = await client.GetProductDetailsAsync("012345678901");
        Console.WriteLine($"Product: {product.Data.Name}");
        
        if (!string.IsNullOrEmpty(product.Data.Brand))
        {
            Console.WriteLine($"Brand: {product.Data.Brand}");
        }
        
        // Get current offers
        var offers = await client.GetCurrentOffersAsync("012345678901");
        Console.WriteLine($"Found {offers.Data.Length} offers:");
        
        foreach (var offer in offers.Data)
        {
            Console.WriteLine($"  {offer.Retailer}: ${offer.Price:F2} ({offer.Availability})");
        }
    }
}
```

## Configuration

You can customize the client behavior:

```csharp
var config = new ShopSavvyConfig
{
    ApiKey = "ss_live_your_api_key_here",
    BaseUrl = "https://api.shopsavvy.com/v1",
    Timeout = TimeSpan.FromSeconds(60)
};

using var client = new ShopSavvyDataApiClient(config);
```

## API Methods

### Product Details

```csharp
// Single product
var product = await client.GetProductDetailsAsync("012345678901");

// Multiple products
var products = await client.GetProductDetailsBatchAsync(new[] { "012345678901", "B08N5WRWNW" });

// CSV format
var productCsv = await client.GetProductDetailsAsync("012345678901", "csv");
```

### Current Offers

```csharp
// All retailers
var offers = await client.GetCurrentOffersAsync("012345678901");

// Specific retailer
var amazonOffers = await client.GetCurrentOffersAsync("012345678901", "amazon");

// Multiple products
var offersBatch = await client.GetCurrentOffersBatchAsync(new[] { "012345678901", "B08N5WRWNW" });
```

### Price History

```csharp
var history = await client.GetPriceHistoryAsync("012345678901", "2024-01-01", "2024-01-31");

// With specific retailer
var amazonHistory = await client.GetPriceHistoryAsync("012345678901", "2024-01-01", "2024-01-31", "amazon");
```

### Product Monitoring

```csharp
// Schedule monitoring
var result = await client.ScheduleProductMonitoringAsync("012345678901", "daily");

// Schedule multiple products
var results = await client.ScheduleProductMonitoringBatchAsync(
    new[] { "012345678901", "B08N5WRWNW" }, 
    "daily"
);

// Get scheduled products
var scheduled = await client.GetScheduledProductsAsync();

// Remove from schedule
var removed = await client.RemoveProductFromScheduleAsync("012345678901");

// Remove multiple from schedule
var removedBatch = await client.RemoveProductsFromScheduleAsync(new[] { "012345678901", "B08N5WRWNW" });
```

### Usage Information

```csharp
var usage = await client.GetUsageAsync();
Console.WriteLine($"Credits remaining: {usage.Data.CreditsRemaining}");
```

## Error Handling

The SDK provides specific exception types for different scenarios:

```csharp
try
{
    var product = await client.GetProductDetailsAsync("invalid-id");
}
catch (ShopSavvyAuthenticationException ex)
{
    Console.WriteLine("Invalid API key");
}
catch (ShopSavvyNotFoundException ex)
{
    Console.WriteLine("Product not found");
}
catch (ShopSavvyValidationException ex)
{
    Console.WriteLine("Invalid request parameters");
}
catch (ShopSavvyRateLimitException ex)
{
    Console.WriteLine("Rate limit exceeded");
}
catch (ShopSavvyNetworkException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (ShopSavvyTimeoutException ex)
{
    Console.WriteLine($"Request timeout: {ex.Message}");
}
catch (ShopSavvyApiException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Response Structure

All API responses follow the same structure:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string? Message { get; set; }
    public int? CreditsUsed { get; set; }
    public int? CreditsRemaining { get; set; }
}
```

## Async/Await Support

All methods are fully async and support cancellation tokens:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var product = await client.GetProductDetailsAsync("012345678901");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request was cancelled");
}
```

## Dependency Injection

The client can be easily used with dependency injection:

```csharp
services.AddSingleton(new ShopSavvyConfig
{
    ApiKey = configuration["ShopSavvy:ApiKey"],
    Timeout = TimeSpan.FromSeconds(60)
});

services.AddScoped<ShopSavvyDataApiClient>();
```

## Requirements

- .NET Standard 2.0 or higher
- Valid ShopSavvy API key (get one at [shopsavvy.com/data](https://shopsavvy.com/data))

## Supported Platforms

- .NET Core 2.0+
- .NET Framework 4.6.1+
- .NET 5.0+
- Xamarin.iOS
- Xamarin.Android
- Unity 2018.1+

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Links

- [ShopSavvy Data API](https://shopsavvy.com/data)
- [API Documentation](https://shopsavvy.com/data/documentation)
- [Get API Key](https://shopsavvy.com/data)
- [Report Issues](https://github.com/shopsavvy/csharp-sdk/issues)