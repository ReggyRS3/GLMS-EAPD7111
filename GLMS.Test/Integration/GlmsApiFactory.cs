using GLMS.Api.Data;
using GLMS.Api.Services;
using GLMS.Test.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GLMS.Test.Integration
{
    // Starts the real GLMS.Api in memory, with a test database and a fixed exchange rate
    public class GlmsApiFactory : WebApplicationFactory<Program>
    {
        public const decimal FixedRate = 18.50m;

        private readonly string _databaseName = "GlmsIntegrationTests-" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Swap SQL Server for an in-memory database
                services.RemoveAll<DbContextOptions<GlmsDbContext>>();
                services.AddDbContext<GlmsDbContext>(options => options.UseInMemoryDatabase(_databaseName));

                // Swap the real currency API for a fixed rate, so tests never need the internet
                services.RemoveAll<ICurrencyConverter>();
                services.AddSingleton<ICurrencyConverter>(new FakeCurrencyConverter(FixedRate));
            });
        }
    }
}