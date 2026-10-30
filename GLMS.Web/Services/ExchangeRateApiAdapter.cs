using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace GLMS.Web.Services
{
    public class ExchangeRateApiAdapter : ICurrencyConverter
    {
        private const string RateCacheKey = "UsdToZarRate";
        private const string LastKnownRateKey = "UsdToZarRate_LastKnown";

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateApiAdapter> _logger;

        public ExchangeRateApiAdapter(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateApiAdapter> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // The provider updates once a day, so a cached rate avoids unnecessary API calls
            if (_cache.TryGetValue(RateCacheKey, out decimal cachedRate))
            {
                return cachedRate;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>("latest/USD");

                if (response?.Result != "success" || response.Rates == null ||
                    !response.Rates.TryGetValue("ZAR", out var rate) || rate <= 0)
                {
                    throw new InvalidOperationException("The exchange rate service returned an unexpected response.");
                }

                _cache.Set(RateCacheKey, rate, TimeSpan.FromHours(1));
                _cache.Set(LastKnownRateKey, rate);

                return rate;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                          or InvalidOperationException or JsonException)
            {
                // Fallback: keep working with the last rate we successfully received
                if (_cache.TryGetValue(LastKnownRateKey, out decimal lastKnownRate))
                {
                    _logger.LogWarning(ex, "Exchange rate API unavailable. Using last known rate {Rate}.", lastKnownRate);
                    return lastKnownRate;
                }

                _logger.LogError(ex, "Exchange rate API unavailable and no previous rate is cached.");
                throw new InvalidOperationException(
                    "The live exchange rate is currently unavailable. Please try again in a few minutes.", ex);
            }
        }

        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            if (usdAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usdAmount), "The USD amount cannot be negative.");
            }

            if (rate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rate), "The exchange rate must be greater than zero.");
            }

            return Math.Round(usdAmount * rate, 2, MidpointRounding.AwayFromZero);
        }

        // Only the parts of the API's JSON that GLMS needs
        private class ExchangeRateApiResponse
        {
            [JsonPropertyName("result")]
            public string? Result { get; set; }

            [JsonPropertyName("rates")]
            public Dictionary<string, decimal>? Rates { get; set; }
        }
    }
}