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
        private const string OkObjectBody = "{\"success\":true,\"data\":{},\"meta\":{\"credits_used\":1,\"credits_remaining\":999}}";

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
            var (client, handler) = ClientWith(OkObjectBody);

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
            var (client, handler) = ClientWith(OkObjectBody);

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
            var (client, handler) = ClientWith(OkObjectBody);

            await client.RemoveProductFromScheduleAsync("611247373064");

            AssertScheduledRequest(Assert.Single(handler.Requests), HttpMethod.Delete, new Dictionary<string, string>
            {
                { "ids", "611247373064" }
            });
        }

        [Fact]
        public async Task UnscheduleBatchSendsDeleteWithJoinedIds()
        {
            var (client, handler) = ClientWith(OkBody);

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
    }
}
