using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShopSavvy.DataApi;
using Xunit;

namespace ShopSavvy.DataApi.Tests
{
    /// <summary>
    /// Records every request and answers with a canned response body. The only stubbed piece:
    /// URL building, headers, status handling and JSON parsing all run through the real client.
    /// </summary>
    internal sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        public RecordingHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            _body = body;
            _status = status;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }

    public class PriceHistoryTests
    {
        private static string Fixture() =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test-fixture-price-history-response.json"));

        private static (ShopSavvyDataApiClient client, RecordingHandler handler) ClientWith(string body)
        {
            var handler = new RecordingHandler(body);
            var client = new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler);
            return (client, handler);
        }

        private static Dictionary<string, string> Query(Uri uri) =>
            uri.Query.TrimStart('?')
                .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split(new[] { '=' }, 2))
                .ToDictionary(kv => Uri.UnescapeDataString(kv[0]), kv => Uri.UnescapeDataString(kv[1]));

        [Fact]
        public async Task SendsStartAndEndToTheVersionedHistoryEndpoint()
        {
            var (client, handler) = ClientWith(Fixture());

            await client.GetPriceHistoryAsync("611247373064", "2022-11-20", "2022-11-27", "amazon.com");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://api.shopsavvy.com/v1/products/offers/history", request.RequestUri!.GetLeftPart(UriPartial.Path));
            var query = Query(request.RequestUri);
            Assert.Equal("611247373064", query["ids"]);
            Assert.Equal("2022-11-20", query["start"]);
            Assert.Equal("2022-11-27", query["end"]);
            Assert.Equal("amazon.com", query["retailer"]);
            Assert.False(query.ContainsKey("start_date"));
            Assert.False(query.ContainsKey("end_date"));
            Assert.Equal("Bearer ss_test_abc123", request.Headers.Authorization!.ToString());
            Assert.Contains($"ShopSavvy-CSharp-SDK/{ShopSavvySdk.Version}", request.Headers.UserAgent.ToString());
        }

        [Fact]
        public async Task BatchJoinsIdsAndOmitsRetailerWhenNotGiven()
        {
            var (client, handler) = ClientWith(Fixture());

            await client.GetPriceHistoryBatchAsync(new[] { "611247373064", "611247369449" }, "2022-11-20", "2022-11-27");

            var query = Query(Assert.Single(handler.Requests).RequestUri!);
            Assert.Equal("611247373064,611247369449", query["ids"]);
            Assert.Equal("2022-11-20", query["start"]);
            Assert.Equal("2022-11-27", query["end"]);
            Assert.False(query.ContainsKey("retailer"));
        }

        [Fact]
        public async Task ParsesProductsOffersAndHistoryPoints()
        {
            var (client, _) = ClientWith(Fixture());

            var response = await client.GetPriceHistoryBatchAsync(new[] { "611247373064", "611247369449" }, "2022-11-20", "2022-11-27");

            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Length);

            // Envelope meta
            Assert.NotNull(response.Meta);
            Assert.Equal("req-7f3c9a", response.Meta!.RequestId);
            Assert.Equal(14, response.CreditsUsed());
            Assert.Equal(986, response.CreditsRemaining());
            Assert.Equal(999, response.Meta.RateLimitRemaining);

            // Product 1: full product fields inherited from ProductDetails
            var mini = response.Data[0];
            Assert.Equal("Keurig K-Mini Single Serve Coffee Maker, Black", mini.Title);
            Assert.Equal("3ONn300xybP3y66ibqc1", mini.Shopsavvy);
            Assert.Equal("611247373064", mini.Barcode);
            Assert.Equal("B07G14HTBZ", mini.Amazon);
            Assert.Equal("Keurig", mini.Brand);
            Assert.Equal("K-MINI", mini.Model);
            Assert.Equal("5000200237", mini.Mpn);
            Assert.Equal("Keurig K-Mini", mini.TitleShort);
            Assert.Equal("keurig-k-mini-single-serve-coffee-maker-black", mini.Slug);
            Assert.Single(mini.Images!);
            Assert.Equal("611247373064", mini.Identifiers!["upc"]);
            Assert.Equal(2, mini.Offers.Length);

            // Offer 1: all offer fields plus a three-point history, newest first
            var amazon = mini.Offers[0];
            Assert.Equal("0IUouCFtZEhxeOablTPl", amazon.Id);
            Assert.Equal("Amazon", amazon.Retailer);
            Assert.Equal(74.96m, amazon.Price);
            Assert.Equal("USD", amazon.Currency);
            Assert.Equal("in", amazon.Availability);
            Assert.Equal("new", amazon.Condition);
            Assert.Equal("ACME Deals", amazon.Seller);
            Assert.Equal("https://www.amazon.com/dp/B07G14HTBZ?m=A1GKQADQC2VI6E", amazon.Url);
            Assert.Equal("2022-11-27T22:36:33.236Z", amazon.Timestamp);
            Assert.Equal(3, amazon.History.Length);

            Assert.Equal("2022-11-27T22:36:33.236Z", amazon.History[0].Timestamp);
            Assert.Equal(74.96m, amazon.History[0].Price);
            Assert.Equal("USD", amazon.History[0].Currency);
            Assert.Equal("in", amazon.History[0].Availability);

            Assert.Equal("2022-11-24T10:00:00.000Z", amazon.History[1].Timestamp);
            Assert.Equal(70.99m, amazon.History[1].Price);
            Assert.Equal("out", amazon.History[1].Availability);

            // Point with unknown availability (key omitted) and a null currency
            Assert.Equal("2022-11-21T08:15:00.000Z", amazon.History[2].Timestamp);
            Assert.Equal(79.99m, amazon.History[2].Price);
            Assert.Null(amazon.History[2].Currency);
            Assert.Null(amazon.History[2].Availability);

            // Offer 2: availability omitted, seller null
            var bestBuy = mini.Offers[1];
            Assert.Equal("Z9kQ2mBestBuyOffer01", bestBuy.Id);
            Assert.Equal("Best Buy", bestBuy.Retailer);
            Assert.Equal(59.99m, bestBuy.Price);
            Assert.Null(bestBuy.Availability);
            Assert.Null(bestBuy.Seller);
            Assert.Equal(new[] { 59.99m, 64.99m }, bestBuy.History.Select(h => h.Price).ToArray());

            // Product 2: null optional product fields, empty images, offer with empty history
            var elite = response.Data[1];
            Assert.Equal("DrKWneG0MpFlZpwZXNYa", elite.Shopsavvy);
            Assert.Equal("611247369449", elite.Barcode);
            Assert.Null(elite.Amazon);
            Assert.Null(elite.Category);
            Assert.Null(elite.Color);
            Assert.Null(elite.Mpn);
            Assert.Empty(elite.Images!);
            var ebay = Assert.Single(elite.Offers);
            Assert.Equal("eBayListing000000001", ebay.Id);
            Assert.Equal("used", ebay.Condition);
            Assert.Equal(89.5m, ebay.Price);
            Assert.Empty(ebay.History);
        }

        [Fact]
        public async Task NonSuccessStatusRaisesTypedError()
        {
            var handler = new RecordingHandler("{\"success\":false,\"error\":\"nope\"}", HttpStatusCode.NotFound);
            var client = new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler);

            await Assert.ThrowsAsync<ShopSavvyNotFoundException>(() =>
                client.GetPriceHistoryAsync("000000000000", "2022-11-20", "2022-11-27"));
        }

        [Fact]
        public async Task CustomBaseUrlKeepsItsPathPrefix()
        {
            var handler = new RecordingHandler(Fixture());
            var client = new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123", BaseUrl = "https://proxy.example.com/shopsavvy/v1/" }, handler);

            await client.GetPriceHistoryAsync("611247373064", "2022-11-20", "2022-11-27");

            Assert.Equal("https://proxy.example.com/shopsavvy/v1/products/offers/history",
                Assert.Single(handler.Requests).RequestUri!.GetLeftPart(UriPartial.Path));
        }
    }
}
