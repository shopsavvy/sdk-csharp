using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ShopSavvy.DataApi;
using Xunit;

namespace ShopSavvy.DataApi.Tests
{
    /// <summary>
    /// GET /products/offers carries, per product, an <c>in_store_prices</c> array: shelf prices
    /// read from shoppers' scanned receipts, labeled source "user-collected".
    /// </summary>
    public class InStorePricesTests
    {
        private static string Fixture() =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test-fixture-offers-in-store-response.json"));

        [Fact]
        public async Task GetOffersParsesUserCollectedInStorePrices()
        {
            var handler = new RecordingHandler(Fixture());
            var client = new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler);

            var response = await client.GetOffersAsync("011110838360");

            var product = response.Data[0];
            var inStore = Assert.Single(product.InStorePrices);
            Assert.Equal("user-collected", inStore.Source);
            Assert.Equal("Kroger", inStore.Retailer);
            Assert.Equal("1014 Vine St, Cincinnati, OH 45202, USA", inStore.Store.Address);
            Assert.Equal(39.1086, inStore.Store.Latitude);
            Assert.Equal(3.49, inStore.Price);
            Assert.Equal("2026-11-07", inStore.ObservedOn);
            Assert.Equal("Walmart", product.Offers[0].Retailer);
        }

        [Fact]
        public async Task InStorePricesAreEmptyWhenTheServerSendsNone()
        {
            var body = JObject.Parse(Fixture());
            ((JObject)body["data"]![0]!).Remove("in_store_prices");
            var handler = new RecordingHandler(body.ToString());
            var client = new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler);

            var response = await client.GetOffersAsync("011110838360");

            Assert.Empty(response.Data[0].InStorePrices);
        }
    }
}
