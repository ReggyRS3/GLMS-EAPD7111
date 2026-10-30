using GLMS.Web.Services;

namespace GLMS.Test.Fakes
{
    // Fixed rate, so tests never depend on the internet or today's exchange rate
    public class FakeCurrencyConverter : ICurrencyConverter
    {
        private readonly decimal _fixedRate;

        public FakeCurrencyConverter(decimal fixedRate)
        {
            _fixedRate = fixedRate;
        }

        public Task<decimal> GetUsdToZarRateAsync() => Task.FromResult(_fixedRate);

        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate) =>
            Math.Round(usdAmount * rate, 2, MidpointRounding.AwayFromZero);
    }
}