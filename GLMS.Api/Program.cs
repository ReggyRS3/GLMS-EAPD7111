using System.Text.Json.Serialization;
using GLMS.Api.Data;
using GLMS.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers return JSON; enums are sent as text ("Active") instead of numbers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger: self-documenting, testable API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// The API is the only part of the system that talks to the database
builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

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

// Create or update the database on start-up (needed for Docker, where the database starts empty)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GlmsDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the integration tests start the API in memory
public partial class Program { }