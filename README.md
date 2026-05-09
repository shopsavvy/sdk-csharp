# ShopSavvy Data API - C# SDK

[![NuGet Version](https://img.shields.io/nuget/v/ShopSavvy.Sdk.svg)](https://www.nuget.org/packages/ShopSavvy.Sdk/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0+-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Documentation](https://img.shields.io/badge/docs-shopsavvy.com-blue)](https://shopsavvy.com/data/documentation)

Official C# SDK for the [ShopSavvy Data API](https://shopsavvy.com/data). Access comprehensive product data, real-time pricing, and historical price trends across **thousands of retailers** and **millions of products**.

## ⚡ 30-Second Quick Start

```csharp
// Install
// dotnet add package ShopSavvy.Sdk

// Use
using ShopSavvy.DataApi;

var client = new ShopSavvyDataApiClient("ss_live_your_api_key_here");
var product = await client.GetProductDetailsAsync("012345678901");
var offers = await client.GetCurrentOffersAsync("012345678901");
var bestPrice = offers.Data.MinBy(o => o.Price);
Console.WriteLine($"{product.Data.Name} - Best price: ${bestPrice.Price:F2} at {bestPrice.Retailer}");
```

## 🚀 Installation & Setup

### NuGet Package Manager

```powershell
Install-Package ShopSavvy.Sdk
```

### .NET CLI

```bash
dotnet add package ShopSavvy.Sdk
```

### PackageReference

```xml
<PackageReference Include="ShopSavvy.Sdk" Version="1.0.0" />
```

### Get Your API Key

1. **Sign up**: Visit [shopsavvy.com/data](https://shopsavvy.com/data)
2. **Choose plan**: Select based on your usage needs  
3. **Get API key**: Copy from your dashboard
4. **Test**: Run the 30-second example above

## 📖 Complete API Reference

### Client Configuration

```csharp
// Basic configuration
using var client = new ShopSavvyDataApiClient("ss_live_your_api_key_here");

// Advanced configuration
var config = new ShopSavvyConfig
{
    ApiKey = "ss_live_your_api_key_here",
    BaseUrl = "https://api.shopsavvy.com/v1",  // Custom base URL
    Timeout = TimeSpan.FromSeconds(60),        // Request timeout
    RetryAttempts = 3,                         // Retry failed requests
    RetryDelay = TimeSpan.FromSeconds(1),      // Delay between retries
    UserAgent = "MyApp/1.0.0"                  // Custom user agent
};

using var client = new ShopSavvyDataApiClient(config);

// Environment variable configuration
var apiKey = Environment.GetEnvironmentVariable("SHOPSAVVY_API_KEY");
using var client = new ShopSavvyDataApiClient(apiKey);
```

### Product Lookup

#### Single Product
```csharp
// Look up by barcode, ASIN, URL, model number, or ShopSavvy ID
var product = await client.GetProductDetailsAsync("012345678901");
var amazonProduct = await client.GetProductDetailsAsync("B08N5WRWNW");
var urlProduct = await client.GetProductDetailsAsync("https://www.amazon.com/dp/B08N5WRWNW");
var modelProduct = await client.GetProductDetailsAsync("MQ023LL/A");  // iPhone model number

Console.WriteLine($"📦 Product: {product.Data.Name}");
Console.WriteLine($"🏷️ Brand: {product.Data.Brand}");
Console.WriteLine($"📂 Category: {product.Data.Category}");
Console.WriteLine($"🔢 Product ID: {product.Data.ProductId}");

if (!string.IsNullOrEmpty(product.Data.Asin))
    Console.WriteLine($"📦 ASIN: {product.Data.Asin}");

if (!string.IsNullOrEmpty(product.Data.Model))
    Console.WriteLine($"🔧 Model: {product.Data.Model}");
```

#### Bulk Product Lookup
```csharp
// Process up to 100 products at once (Pro plan)
var identifiers = new[]
{
    "012345678901", "B08N5WRWNW", "045496590048",
    "https://www.bestbuy.com/site/product/123456",
    "MQ023LL/A", "SM-S911U"  // iPhone and Samsung model numbers
};

var products = await client.GetProductDetailsBatchAsync(identifiers);

foreach (var product in products.Data)
{
    if (product != null)
    {
        Console.WriteLine($"✓ Found: {product.Name} by {product.Brand}");
        if (product.Identifiers?.Any() == true)
        {
            Console.WriteLine($"  Identifiers: {string.Join(", ", product.Identifiers.Select(i => $"{i.Key}:{i.Value}"))}");
        }
    }
}

// Handle potential errors in batch processing
for (int i = 0; i < products.Data.Length; i++)
{
    if (products.Data[i] == null)
    {
        Console.WriteLine($"❌ Failed to find product: {identifiers[i]}");
    }
}
```

### Real-Time Pricing

#### All Retailers Analysis
```csharp
var offers = await client.GetCurrentOffersAsync("012345678901");
Console.WriteLine($"Found {offers.Data.Length} offers across retailers");

// Advanced price analysis
var sortedOffers = offers.Data.OrderBy(o => o.Price).ToArray();
var cheapest = sortedOffers.First();
var mostExpensive = sortedOffers.Last();

Console.WriteLine($"💰 Best price: {cheapest.Retailer} - ${cheapest.Price:F2}");
Console.WriteLine($"💸 Highest price: {mostExpensive.Retailer} - ${mostExpensive.Price:F2}");
Console.WriteLine($"📊 Average price: ${offers.Data.Average(o => o.Price):F2}");
Console.WriteLine($"💡 Potential savings: ${mostExpensive.Price - cheapest.Price:F2}");

// Filter by availability and condition
var inStockOffers = offers.Data.Where(o => o.Availability == "in_stock").ToArray();
var newConditionOffers = offers.Data.Where(o => o.Condition == "new").ToArray();

Console.WriteLine($"✅ In-stock offers: {inStockOffers.Length}");
Console.WriteLine($"🆕 New condition: {newConditionOffers.Length}");
```

#### Retailer-Specific Queries
```csharp
// Major retailers
var retailers = new[] { "amazon", "walmart", "target", "bestbuy" };
var retailerPrices = new Dictionary<string, decimal>();

foreach (var retailer in retailers)
{
    var offers = await client.GetCurrentOffersAsync("012345678901", retailer);
    if (offers.Data.Any())
    {
        var bestOffer = offers.Data.MinBy(o => o.Price);
        retailerPrices[retailer] = bestOffer.Price;
    }
}

Console.WriteLine("Retailer price comparison:");
foreach (var (retailer, price) in retailerPrices.OrderBy(kvp => kvp.Value))
{
    Console.WriteLine($"  {char.ToUpper(retailer[0])}{retailer[1..]}: ${price:F2}");
}
```

#### Bulk Price Monitoring
```csharp
// Monitor multiple products simultaneously
var productList = new[]
{
    "012345678901", "B08N5WRWNW", "045496590048",
    "B07XJ8C8F5", "B09G9FPHY6"
};

var batchOffers = await client.GetCurrentOffersBatchAsync(productList);

foreach (var (identifier, offers) in batchOffers.Data)
{
    if (!offers.Any()) continue;
    
    var bestOffer = offers.MinBy(o => o.Price);
    Console.WriteLine($"{identifier}:");
    Console.WriteLine($"  Best price: {bestOffer.Retailer} - ${bestOffer.Price:F2}");
    Console.WriteLine($"  Total offers: {offers.Length}");
    Console.WriteLine($"  In stock: {offers.Count(o => o.Availability == "in_stock")}");
    Console.WriteLine();
}
```

### Historical Price Analysis

#### Comprehensive Price Trends
```csharp
// Get 90 days of price history for detailed analysis
var endDate = DateTime.Today;
var startDate = endDate.AddDays(-90);

var history = await client.GetPriceHistoryAsync(
    "012345678901",
    startDate.ToString("yyyy-MM-dd"),
    endDate.ToString("yyyy-MM-dd")
);

Console.WriteLine("📈 90-Day Price Analysis");
Console.WriteLine(new string('=', 50));

foreach (var offer in history.Data.Where(o => o.PriceHistory?.Any() == true))
{
    var prices = offer.PriceHistory.Select(ph => ph.Price).ToArray();
    var currentPrice = offer.Price;
    
    // Statistical analysis
    var avgPrice = prices.Average();
    var minPrice = prices.Min();
    var maxPrice = prices.Max();
    
    // Price trend calculation
    var recentPrices = prices.TakeLast(7).ToArray();  // Last week
    var olderPrices = prices.Take(Math.Max(prices.Length - 7, 1)).ToArray();
    
    var trend = "📊 Insufficient data";
    if (recentPrices.Any() && olderPrices.Any())
    {
        var recentAvg = recentPrices.Average();
        var olderAvg = olderPrices.Average();
        var changePct = Math.Round((recentAvg - olderAvg) / olderAvg * 100, 1);
        
        trend = changePct switch
        {
            > 5 => $"📈 Rising (+{changePct}%)",
            < -5 => $"📉 Falling ({changePct}%)",
            _ => $"📊 Stable ({changePct:+0.0;-0.0;0.0}%)"
        };
    }
    
    Console.WriteLine($"🏪 {offer.Retailer.ToUpper()}");
    Console.WriteLine($"  Current: ${currentPrice:F2}");
    Console.WriteLine($"  Average: ${avgPrice:F2}");
    Console.WriteLine($"  Range: ${minPrice:F2} - ${maxPrice:F2}");
    Console.WriteLine($"  Savings opportunity: ${currentPrice - minPrice:F2}");
    Console.WriteLine($"  Trend: {trend}");
    Console.WriteLine($"  Data points: {offer.PriceHistory.Length}");
    Console.WriteLine();
}
```

#### Retailer-Specific Historical Analysis
```csharp
// Compare price history across major retailers
var retailers = new[] { "amazon", "walmart", "target", "bestbuy" };
var historicalComparison = new Dictionary<string, dynamic>();

foreach (var retailer in retailers)
{
    var history = await client.GetPriceHistoryAsync(
        "012345678901",
        "2024-01-01",
        "2024-12-31",
        retailer
    );
    
    if (!history.Data.Any()) continue;
    
    var offer = history.Data.First();
    if (offer.PriceHistory?.Any() == true)
    {
        var prices = offer.PriceHistory.Select(ph => ph.Price).ToArray();
        historicalComparison[retailer] = new
        {
            Current = offer.Price,
            Average = prices.Average(),
            Lowest = prices.Min(),
            Highest = prices.Max(),
            Volatility = prices.Max() - prices.Min()
        };
    }
}

Console.WriteLine("Retailer Historical Comparison:");
foreach (var (retailer, data) in historicalComparison)
{
    Console.WriteLine($"{char.ToUpper(retailer[0])}{retailer[1..]}:");
    Console.WriteLine($"  Current: ${data.Current:F2}");
    Console.WriteLine($"  Average: ${data.Average:F2}");
    Console.WriteLine($"  Best ever: ${data.Lowest:F2}");
    Console.WriteLine($"  Worst: ${data.Highest:F2}");
    Console.WriteLine($"  Volatility: ${data.Volatility:F2}");
    Console.WriteLine();
}
```

## 🚀 Production Deployment

### ASP.NET Core Integration

```csharp
// Program.cs or Startup.cs
builder.Services.Configure<ShopSavvyConfig>(
    builder.Configuration.GetSection("ShopSavvy"));

builder.Services.AddScoped<ShopSavvyDataApiClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ShopSavvyConfig>>().Value;
    return new ShopSavvyDataApiClient(config);
});

// appsettings.json
{
  "ShopSavvy": {
    "ApiKey": "ss_live_your_api_key_here",
    "Timeout": "00:01:00",
    "RetryAttempts": 3
  }
}

// PriceController.cs
[ApiController]
[Route("api/[controller]")]
public class PriceController : ControllerBase
{
    private readonly ShopSavvyDataApiClient _client;
    
    public PriceController(ShopSavvyDataApiClient client)
    {
        _client = client;
    }
    
    [HttpGet("{identifier}/offers")]
    public async Task<IActionResult> GetOffers(string identifier)
    {
        try
        {
            var offers = await _client.GetCurrentOffersAsync(identifier);
            return Ok(new
            {
                success = true,
                productId = identifier,
                offers = offers.Data.Select(o => new
                {
                    retailer = o.Retailer,
                    price = o.Price,
                    availability = o.Availability,
                    condition = o.Condition,
                    url = o.Url
                }),
                bestPrice = offers.Data.MinBy(o => o.Price)?.Price,
                creditsRemaining = offers.CreditsRemaining
            });
        }
        catch (ShopSavvyApiException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}
```

### Background Services

```csharp
// PriceMonitoringService.cs
public class PriceMonitoringService : BackgroundService
{
    private readonly ShopSavvyDataApiClient _client;
    private readonly ILogger<PriceMonitoringService> _logger;
    
    public PriceMonitoringService(
        ShopSavvyDataApiClient client,
        ILogger<PriceMonitoringService> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorPrices();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in price monitoring service");
            }
        }
    }
    
    private async Task MonitorPrices()
    {
        var products = new[] { "012345678901", "B08N5WRWNW" };
        
        foreach (var productId in products)
        {
            try
            {
                var offers = await _client.GetCurrentOffersAsync(productId);
                var bestPrice = offers.Data.MinBy(o => o.Price)?.Price;
                
                // Store in database or send alerts
                _logger.LogInformation($"Product {productId}: Best price ${bestPrice:F2}");
            }
            catch (ShopSavvyRateLimitException)
            {
                // Implement exponential backoff
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
            catch (ShopSavvyApiException ex)
            {
                _logger.LogError(ex, $"API error for product {productId}");
            }
        }
    }
}
```

## 🛠️ Development & Testing

### Local Development Setup

```bash
# Clone the repository
git clone https://github.com/shopsavvy/sdk-csharp.git
cd sdk-csharp

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack
```

### Testing Your Integration

```csharp
using ShopSavvy.DataApi;

// Use test API key (starts with ss_test_)
using var client = new ShopSavvyDataApiClient("ss_test_your_test_key_here");

try
{
    // Test product lookup
    var product = await client.GetProductDetailsAsync("012345678901");
    Console.WriteLine($"✅ Product lookup: {product.Data.Name}");
    
    // Test current offers
    var offers = await client.GetCurrentOffersAsync("012345678901");
    Console.WriteLine($"✅ Current offers: {offers.Data.Length} found");
    
    // Test usage info
    var usage = await client.GetUsageAsync();
    Console.WriteLine($"✅ API usage: {usage.Data.CreditsRemaining} credits remaining");
    
    Console.WriteLine("\n🎉 All tests passed! SDK is working correctly.");
}
catch (ShopSavvyApiException ex)
{
    Console.WriteLine($"❌ Test failed: {ex.Message}");
}
```

## 📚 Additional Resources

- **[ShopSavvy Data API Documentation](https://shopsavvy.com/data/documentation)** - Complete API reference
- **[API Dashboard](https://shopsavvy.com/data/dashboard)** - Manage your API keys and usage
- **[GitHub Repository](https://github.com/shopsavvy/sdk-csharp)** - Source code and issues
- **[NuGet Package](https://www.nuget.org/packages/ShopSavvy.Sdk/)** - Package releases and stats
- **[Support](mailto:business@shopsavvy.com)** - Get help from our team

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details on:

- Reporting bugs
- Suggesting enhancements  
- Submitting pull requests
- Development workflow
- Code standards

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏢 About ShopSavvy

**ShopSavvy** is the world's first mobile shopping app, helping consumers find the best deals since 2008. With over **40 million downloads** and millions of active users, ShopSavvy has saved consumers billions of dollars.

### Our Data API Powers:
- 🛒 **E-commerce platforms** with competitive intelligence  
- 📊 **Market research** with real-time pricing data
- 🏪 **Retailers** with inventory and pricing optimization
- 📱 **Mobile apps** with product lookup and price comparison
- 🤖 **Business intelligence** with automated price monitoring

### Why Choose ShopSavvy Data API?
- ✅ **Trusted by millions** - Proven at scale since 2008
- ✅ **Comprehensive coverage** - 1000+ retailers, millions of products  
- ✅ **Real-time accuracy** - Fresh data updated continuously
- ✅ **Developer-friendly** - Easy integration, great documentation
- ✅ **Reliable infrastructure** - 99.9% uptime, enterprise-grade
- ✅ **Flexible pricing** - Plans for every use case and budget

---

**Ready to get started?** [Sign up for your API key](https://shopsavvy.com/data) • **Need help?** [Contact us](mailto:business@shopsavvy.com)