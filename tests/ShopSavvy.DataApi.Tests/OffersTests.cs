using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ShopSavvy.DataApi;
using Xunit;

namespace ShopSavvy.DataApi.Tests
{
    /// <summary>
    /// GET /products/offers returns data as a list of products (every product field) each with
    /// an <c>offers</c> array. These feed a real-shape response through the client's parsing.
    /// </summary>
    public class OffersTests
    {
        private static string Fixture() =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test-fixture-offers-response.json"));

        private static (ShopSavvyDataApiClient client, RecordingHandler handler) ClientWithFixture()
        {
            var handler = new RecordingHandler(Fixture());
            return (new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler), handler);
        }

        [Fact]
        public async Task GetOffersParsesProductsWithFullFieldsAndOffers()
        {
            var (client, handler) = ClientWithFixture();

            var response = await client.GetOffersAsync("611247373064");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://api.shopsavvy.com/v1/products/offers?ids=611247373064", request.RequestUri!.ToString());

            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Length);

            var kMini = response.Data[0];
            Assert.Equal("Keurig K-Mini Single Serve Coffee Maker, Black", kMini.Title);
            Assert.Equal("products/3ONn300xybP3y66ibqc1", kMini.Shopsavvy);
            Assert.Equal("611247373064", kMini.Barcode);
            Assert.Equal("B07G14HTBZ", kMini.Amazon);
            Assert.Equal("5000200237", kMini.Mpn);
            Assert.Equal(2, kMini.Images!.Length);
            // Expanded product fields the offers endpoint also sends
            Assert.Equal("Keurig K-Mini", kMini.TitleShort);
            Assert.Equal("keurig-k-mini-single-serve-coffee-maker-black", kMini.Slug);
            Assert.Equal("Brews any cup size between 6-12oz.", kMini.Description);
            Assert.Equal(new[] { "Home & Kitchen", "Coffee Makers" }, kMini.Categories);
            Assert.Equal("12 oz", kMini.Attributes!["Capacity"]);
            Assert.Equal(4.6, Convert.ToDouble(kMini.Rating!["value"]));
            Assert.Equal(new[] { "single serve", "coffee maker" }, kMini.Keywords);
            Assert.Equal("611247373064", kMini.Identifiers!["upc"].ToString());

            Assert.Equal(3, kMini.Offers.Length);
            var amazon = kMini.Offers[0];
            Assert.Equal("o_amazon_b07g14htbz", amazon.Id);
            Assert.Equal("in", amazon.Availability);
            Assert.Equal("new", amazon.Condition);
            Assert.Equal("Amazon", amazon.Retailer);
            Assert.Equal("USD", amazon.Currency);
            Assert.Equal(59.99m, amazon.Price);
            Assert.Equal("Amazon.com", amazon.Seller);
            Assert.Equal("https://www.amazon.com/dp/B07G14HTBZ", amazon.Url);
            Assert.Equal("2026-09-26T18:04:11.000Z", amazon.Timestamp);
            Assert.NotNull(amazon.History);
            Assert.Empty(amazon.History!);

            // Unknown availability and no marketplace seller: the API omits both keys
            var walmart = kMini.Offers[1];
            Assert.Null(walmart.Availability);
            Assert.Null(walmart.Seller);
            Assert.Equal(64.5m, walmart.Price);

            Assert.Equal("out", kMini.Offers[2].Availability);
            Assert.Equal(1, kMini.Offers.Count(o => o.Availability == "in"));

            var kElite = response.Data[1];
            Assert.Null(kElite.Category);
            Assert.Null(kElite.Amazon);
            Assert.Empty(kElite.Offers);

            Assert.Equal("req_offers_01", response.Meta!.RequestId);
            Assert.Equal(3, response.CreditsUsed());
            Assert.Equal(997, response.CreditsRemaining());
            Assert.Equal(58, response.Meta.RateLimitRemaining);
        }

        [Fact]
        public async Task GetOffersBatchJoinsIdsAndSendsRetailer()
        {
            var (client, handler) = ClientWithFixture();

            var response = await client.GetOffersBatchAsync(new[] { "611247373064", "611247369449" }, "amazon.com");

            var request = Assert.Single(handler.Requests);
            Assert.Equal("https://api.shopsavvy.com/v1/products/offers", request.RequestUri!.GetLeftPart(UriPartial.Path));
            Assert.Equal("?ids=611247373064%2C611247369449&retailer=amazon.com", request.RequestUri.Query);
            Assert.Equal(new[] { "611247373064", "611247369449" }, response.Data.Select(p => p.Barcode));
        }

#pragma warning disable CS0618 // exercising the deprecated compatibility wrappers on purpose
        [Fact]
        public async Task DeprecatedGetCurrentOffersFlattensFirstProductsOffers()
        {
            var (client, _) = ClientWithFixture();

            var response = await client.GetCurrentOffersAsync("611247373064");

            Assert.True(response.Success);
            Assert.Equal(new[] { "Amazon", "Walmart", "Target" }, response.Data.Select(o => o.Retailer));
            Assert.Equal(59.99m, response.Data.Min(o => o.Price));
            Assert.Equal(3, response.CreditsUsed());
        }

        [Fact]
        public async Task DeprecatedGetCurrentOffersBatchKeysOffersByShopsavvyId()
        {
            var (client, _) = ClientWithFixture();

            var response = await client.GetCurrentOffersBatchAsync(new[] { "611247373064", "611247369449" });

            Assert.Equal(new[] { "products/3ONn300xybP3y66ibqc1", "products/DrKWneG0MpFlZpwZXNYa" }, response.Data.Keys.ToArray());
            Assert.Equal(3, response.Data["products/3ONn300xybP3y66ibqc1"].Length);
            Assert.Empty(response.Data["products/DrKWneG0MpFlZpwZXNYa"]);
        }
#pragma warning restore CS0618
    }
}
