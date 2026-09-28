using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ShopSavvy.DataApi;
using Xunit;

namespace ShopSavvy.DataApi.Tests
{
    /// <summary>
    /// The schedule/unschedule handlers read only query parameters; any body is ignored.
    /// These drive the real client (stubbed HttpMessageHandler only) and assert method,
    /// path, query string and the absence of a body.
    /// </summary>
    public class ScheduleTests
    {
        private const string OkBody = "{\"success\":true,\"data\":[],\"meta\":{\"credits_used\":1,\"credits_remaining\":999}}";
        private const string OkUnscheduleBody = "{\"success\":true,\"message\":\"Products successfully removed from schedule\",\"meta\":{\"credits_used\":0,\"credits_remaining\":0,\"rate_limit_remaining\":0}}";

        private static (ShopSavvyDataApiClient client, RecordingHandler handler) ClientWith(string body)
        {
            var handler = new RecordingHandler(body);
            return (new ShopSavvyDataApiClient(new ShopSavvyConfig { ApiKey = "ss_test_abc123" }, handler), handler);
        }

        private static Dictionary<string, string> Query(Uri uri) =>
            uri.Query.TrimStart('?')
                .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split(new[] { '=' }, 2))
                .ToDictionary(kv => Uri.UnescapeDataString(kv[0]), kv => Uri.UnescapeDataString(kv[1]));

        private static void AssertScheduledRequest(HttpRequestMessage request, HttpMethod method, Dictionary<string, string> expectedQuery)
        {
            Assert.Equal(method, request.Method);
            Assert.Equal("https://api.shopsavvy.com/v1/products/scheduled", request.RequestUri!.GetLeftPart(UriPartial.Path));
            Assert.Equal(expectedQuery, Query(request.RequestUri));
            Assert.Null(request.Content);
        }

        [Fact]
        public async Task ScheduleSingleSendsPutWithQueryParams()
        {
            var (client, handler) = ClientWith(OkBody);

            await client.ScheduleProductMonitoringAsync("611247373064", "daily");

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Put, new Dictionary<string, string>
            {
                { "ids", "611247373064" },
                { "schedule", "daily" }
            });
        }

        [Fact]
        public async Task ScheduleSingleWithRetailerSendsRetailer()
        {
            var (client, handler) = ClientWith(OkBody);

            await client.ScheduleProductMonitoringAsync("611247373064", "hourly", "bestbuy.com");

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Put, new Dictionary<string, string>
            {
                { "ids", "611247373064" },
                { "schedule", "hourly" },
                { "retailer", "bestbuy.com" }
            });
        }

        [Fact]
        public async Task ScheduleBatchJoinsIdsWithCommas()
        {
            var (client, handler) = ClientWith(OkBody);

            await client.ScheduleProductMonitoringBatchAsync(new[] { "611247373064", "B07G14HTBZ" }, "weekly");

            var request = Assert.Single(handler.Requests);
            AssertScheduledRequest(request, HttpMethod.Put, new Dictionary<string, string>
            {
                { "ids", "611247373064,B07G14HTBZ" },
                { "schedule", "weekly" }
            });
            // Commas are URL-encoded on the wire
            Assert.Contains("ids=611247373064%2CB07G14HTBZ", request.RequestUri!.Query);
        }

        [Fact]
        public async Task ScheduleBatchWithRetailerSendsRetailer()
        {
            var (client, handler) = ClientWith(OkBody);

            await client.ScheduleProductMonitoringBatchAsync(new[] { "611247373064", "611247369449" }, "daily", "amazon.com");

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Put, new Dictionary<string, string>
            {
                { "ids", "611247373064,611247369449" },
                { "schedule", "daily" },
                { "retailer", "amazon.com" }
            });
        }

        [Fact]
        public async Task UnscheduleSingleSendsDeleteWithIds()
        {
            var (client, handler) = ClientWith(OkUnscheduleBody);

            await client.RemoveProductFromScheduleAsync("611247373064");

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Delete, new Dictionary<string, string>
            {
                { "ids", "611247373064" }
            });
        }

        [Fact]
        public async Task UnscheduleBatchSendsDeleteWithJoinedIds()
        {
            var (client, handler) = ClientWith(OkUnscheduleBody);

            await client.RemoveProductsFromScheduleAsync(new[] { "611247373064", "611247369449" });

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Delete, new Dictionary<string, string>
            {
                { "ids", "611247373064,611247369449" }
            });
        }

        [Fact]
        public async Task ListScheduledSendsGet()
        {
            var (client, handler) = ClientWith(OkBody);

            await client.GetScheduledProductsAsync();

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Get, new Dictionary<string, string>());
        }

        private static string Fixture(string name) =>
            System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, name));

        [Fact]
        public async Task ScheduleResponseParsesProductsWithScheduleAndRetailer()
        {
            var (client, _) = ClientWith(Fixture("test-fixture-schedule-response.json"));

            var response = await client.ScheduleProductMonitoringBatchAsync(new[] { "045496590444", "194253397168" }, "daily", "bestbuy.com");

            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Length);

            var controller = response.Data[0];
            Assert.Equal("Nintendo Switch Pro Controller", controller.Title);
            Assert.Equal("products/1bdc8c1ce5d3b56abb4b2cb0adf1b0b1", controller.Shopsavvy);
            Assert.Equal("045496590444", controller.Barcode);
            Assert.Equal("B01NAWKYZ0", controller.Amazon);
            Assert.Equal("HACAFSSKA", controller.Model);
            Assert.Null(controller.Mpn);
            Assert.Equal("Switch Pro Controller", controller.TitleShort);
            Assert.Equal(new[] { "https://images.shopsavvy.com/pro-controller.jpg" }, controller.Images);
            Assert.Equal("daily", controller.Schedule);
            Assert.Equal("bestbuy.com", controller.Retailer);

            var airpods = response.Data[1];
            Assert.Equal("194253397168", airpods.Barcode);
            Assert.Null(airpods.Amazon);
            Assert.Empty(airpods.Images!);
            Assert.Equal("daily", airpods.Schedule);

            Assert.Equal("req_sched_01", response.Meta!.RequestId);
            Assert.Equal(2, response.CreditsUsed());
            Assert.Equal(998, response.CreditsRemaining());
            Assert.Equal(59, response.Meta.RateLimitRemaining);
        }

        [Fact]
        public async Task ScheduleSingleResponseIsAnArrayOfOneProduct()
        {
            var body = "{\"success\":true,\"data\":[{\"title\":\"Nintendo Switch Pro Controller\",\"shopsavvy\":\"products/1bdc8c1ce5d3b56abb4b2cb0adf1b0b1\",\"barcode\":\"045496590444\",\"images\":[],\"schedule\":\"weekly\"}],\"meta\":{\"request_id\":\"r\",\"credits_used\":1,\"credits_remaining\":41,\"rate_limit_remaining\":10}}";
            var (client, _) = ClientWith(body);

            var response = await client.ScheduleProductMonitoringAsync("045496590444", "weekly");

            var product = Assert.Single(response.Data);
            Assert.Equal("products/1bdc8c1ce5d3b56abb4b2cb0adf1b0b1", product.Shopsavvy);
            Assert.Equal("weekly", product.Schedule);
            // No retailer param -> the API omits the key
            Assert.Null(product.Retailer);
            Assert.Equal(1, response.CreditsUsed());
        }

        [Fact]
        public async Task UnscheduleResponseParsesSuccessMessageAndMeta()
        {
            var (client, _) = ClientWith(Fixture("test-fixture-unschedule-response.json"));

            var response = await client.RemoveProductsFromScheduleAsync(new[] { "045496590444", "194253397168" });

            Assert.True(response.Success);
            Assert.Equal("Products successfully removed from schedule", response.Message);
            Assert.Equal("req_unsched_01", response.Meta!.RequestId);
            Assert.Equal(0, response.Meta.CreditsUsed);
            Assert.Equal(0, response.Meta.CreditsRemaining);
            Assert.Equal(0, response.Meta.RateLimitRemaining);
        }

        [Fact]
        public async Task UnscheduleSingleResponseParses()
        {
            var (client, _) = ClientWith(Fixture("test-fixture-unschedule-response.json"));

            var response = await client.RemoveProductFromScheduleAsync("045496590444");

            Assert.True(response.Success);
            Assert.Equal("Products successfully removed from schedule", response.Message);
        }

        [Fact]
        public async Task ScheduledListParsesProductsWithOptionalScheduleAndRetailer()
        {
            var (client, _) = ClientWith(Fixture("test-fixture-scheduled-list-response.json"));

            var response = await client.GetScheduledProductsAsync();

            Assert.True(response.Success);
            Assert.Equal(3, response.Data.Length);

            Assert.Equal("Nintendo Switch Pro Controller", response.Data[0].Title);
            Assert.Equal("045496590444", response.Data[0].Barcode);
            Assert.Equal("hourly", response.Data[0].Schedule);
            Assert.Equal("amazon.com", response.Data[0].Retailer);

            Assert.Equal("products/9a1f2c3d4e5f60718293a4b5c6d7e8f9", response.Data[1].Shopsavvy);
            Assert.Equal("weekly", response.Data[1].Schedule);
            Assert.Null(response.Data[1].Retailer);

            // A refresh interval with no Data API label (e.g. 4h from ShopSavvy Business): no schedule key
            Assert.Equal("673419319126", response.Data[2].Barcode);
            Assert.Null(response.Data[2].Schedule);
            Assert.Null(response.Data[2].Retailer);

            Assert.Equal("req_list_01", response.Meta!.RequestId);
            Assert.Equal(0, response.CreditsUsed());
        }
    }
}
