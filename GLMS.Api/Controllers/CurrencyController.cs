using GLMS.Api.Dtos;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConverter _converter;

        public CurrencyController(ICurrencyConverter converter)
        {
            _converter = converter;
        }

        // GET: api/currency/usd-zar
        [HttpGet("usd-zar")]
        public async Task<ActionResult<ExchangeRateDto>> GetUsdToZar()
        {
            try
            {
                var rate = await _converter.GetUsdToZarRateAsync();
                return Ok(new ExchangeRateDto { Rate = rate });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
        }
    }
}