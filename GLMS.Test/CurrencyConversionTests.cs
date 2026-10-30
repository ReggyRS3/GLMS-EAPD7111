using System.Globalization;
using System.Net;
using GLMS.Test.Fakes;
using GLMS.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GLMS.Test
{
    public class CurrencyConversionTests
    {
        private const string SuccessJson = """{ "result": "success", "base_code": "USD", "rates": { "USD": 1, "ZAR": 18.25, "EUR": 0.92 } }""";

        private static ExchangeRateApiAdapter CreateAdapter(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://open.er-api.com/v6/") };
            return new ExchangeRateApiAdapter(
                httpClient,
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<ExchangeRateApiAdapter>.Instance);
        }

        private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        [Theory]
        [InlineData("100", "18.50", "1850.00")]
        [InlineData("12.50", "18.2345", "227.93")]
        [InlineData("1", "18.25", "18.25")]
        [InlineData("0", "18.25", "0")]
        public void ConvertUsdToZar_GivenRate_ReturnsCorrectZarAmount(string usd, string rate, string expected)
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson));

            var result = adapter.ConvertUsdToZar(D(usd), D(rate));

            Assert.Equal(D(expected), result);
        }

        [Fact]
        public void ConvertUsdToZar_HalfCent_RoundsAwayFromZero()
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson));

            // 0.50 x 18.25 = 9.125, which must round up to 9.13
            var result = adapter.ConvertUsdToZar(0.50m, 18.25m);

            Assert.Equal(9.13m, result);
        }

        [Fact]
        public void ConvertUsdToZar_NegativeAmount_Throws()
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson));

            Assert.Throws<ArgumentOutOfRangeException>(() => adapter.ConvertUsdToZar(-10m, 18.25m));
        }

        [Fact]
        public void ConvertUsdToZar_ZeroRate_Throws()
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson));

            Assert.Throws<ArgumentOutOfRangeException>(() => adapter.ConvertUsdToZar(100m, 0m));
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_ReadsZarRateFromApiResponse()
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson));

            var rate = await adapter.GetUsdToZarRateAsync();

            Assert.Equal(18.25m, rate);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_SecondCall_UsesCacheInsteadOfApi()
        {
            var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SuccessJson);
            var adapter = CreateAdapter(handler);

            await adapter.GetUsdToZarRateAsync();
            await adapter.GetUsdToZarRateAsync();

            Assert.Equal(1, handler.CallCount);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_ApiDownAndNoCachedRate_Throws()
        {
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.InternalServerError, "{}"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.GetUsdToZarRateAsync());
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_ZarMissingFromResponse_Throws()
        {
            var json = """{ "result": "success", "rates": { "USD": 1 } }""";
            var adapter = CreateAdapter(new StubHttpMessageHandler(HttpStatusCode.OK, json));

            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.GetUsdToZarRateAsync());
        }
    }
}