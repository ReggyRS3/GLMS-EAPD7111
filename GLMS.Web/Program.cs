using System.Globalization;
using GLMS.Web.Data;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Adapter pattern: HttpClient is injected into the adapter that calls the currency API
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ICurrencyConverter, ExchangeRateApiAdapter>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExchangeRateApi:BaseUrl"] ?? "https://open.er-api.com/v6/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<ServiceRequestService>();

var app = builder.Build();

// South African formats, but with "." as the decimal separator so amounts like 12.50 are accepted
var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-ZA").Clone();
culture.NumberFormat.NumberDecimalSeparator = ".";
culture.NumberFormat.CurrencyDecimalSeparator = ".";
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();