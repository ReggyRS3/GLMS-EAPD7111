namespace GLMS.Web.Services
{
    public interface ICurrencyConverter
    {
        Task<decimal> GetUsdToZarRateAsync();
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
    }
}