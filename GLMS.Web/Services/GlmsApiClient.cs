using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    // Thrown when the API refuses a request (the message comes from the API)
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }

    // Thrown when the API says the token is missing or expired
    public class ApiUnauthorizedException : Exception
    {
        public ApiUnauthorizedException() : base("Your session has expired. Please log in again.") { }
    }

    // The presentation layer's only way to get or save data: HttpClient calls to GLMS.Api
    public class GlmsApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _http;

        public GlmsApiClient(HttpClient http)
        {
            _http = http;
        }

        // ---------- Authentication ----------
        public async Task<LoginResult?> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password }, JsonOptions);
            if (response.StatusCode == HttpStatusCode.Unauthorized) return null;

            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<LoginResult>(JsonOptions);
        }

        // ---------- Clients ----------
        public async Task<List<Client>> GetClientsAsync() =>
            await GetAsync<List<Client>>("api/clients") ?? new();

        public Task<Client?> GetClientAsync(int id) =>
            GetAsync<Client>($"api/clients/{id}");

        public Task<Client> CreateClientAsync(Client client) =>
            SendAsync<Client>(HttpMethod.Post, "api/clients", client);

        public Task<Client> UpdateClientAsync(Client client) =>
            SendAsync<Client>(HttpMethod.Put, $"api/clients/{client.ClientId}", client);

        public Task DeleteClientAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/clients/{id}");

        // ---------- Contracts ----------
        public async Task<List<Contract>> GetContractsAsync(DateTime? fromDate, DateTime? toDate, ContractStatus? status)
        {
            var query = new List<string>();
            if (fromDate.HasValue) query.Add("fromDate=" + fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (toDate.HasValue) query.Add("toDate=" + toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (status.HasValue) query.Add("status=" + status.Value);

            var url = "api/contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            return await GetAsync<List<Contract>>(url) ?? new();
        }

        public Task<Contract?> GetContractAsync(int id) =>
            GetAsync<Contract>($"api/contracts/{id}");

        public async Task<List<ContractAuditLog>> GetContractHistoryAsync(int id) =>
            await GetAsync<List<ContractAuditLog>>($"api/contracts/{id}/history") ?? new();

        public Task<Contract> CreateContractAsync(Contract c) =>
            SendAsync<Contract>(HttpMethod.Post, "api/contracts",
                new { c.ClientId, c.StartDate, c.EndDate, c.ServiceLevel });

        public Task<Contract> UpdateContractAsync(Contract c) =>
            SendAsync<Contract>(HttpMethod.Put, $"api/contracts/{c.ContractId}",
                new { c.ClientId, c.StartDate, c.EndDate, c.ServiceLevel });

        // action = "Activate", "PutOnHold" or "Expire"
        public Task<Contract> ChangeContractStatusAsync(int id, string action) =>
            SendAsync<Contract>(HttpMethod.Patch, $"api/contracts/{id}/status", new { action });

        public async Task UploadAgreementAsync(int id, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.TryParse(file.ContentType, out var type)
                ? type
                : new MediaTypeHeaderValue("application/octet-stream");

            content.Add(fileContent, "file", file.FileName);

            var response = await _http.PostAsync($"api/contracts/{id}/agreement", content);
            await EnsureSuccessAsync(response);
        }

        public async Task<byte[]> DownloadAgreementAsync(int id)
        {
            var response = await _http.GetAsync($"api/contracts/{id}/agreement");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadAsByteArrayAsync();
        }

        public Task DeleteContractAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/contracts/{id}");

        // ---------- Service requests ----------
        public async Task<List<ServiceRequest>> GetServiceRequestsAsync() =>
            await GetAsync<List<ServiceRequest>>("api/servicerequests") ?? new();

        public Task<ServiceRequest?> GetServiceRequestAsync(int id) =>
            GetAsync<ServiceRequest>($"api/servicerequests/{id}");

        public Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest s) =>
            SendAsync<ServiceRequest>(HttpMethod.Post, "api/servicerequests",
                new { s.ContractId, s.Description, s.CostUsd });

        public Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest s) =>
            SendAsync<ServiceRequest>(HttpMethod.Put, $"api/servicerequests/{s.ServiceRequestId}",
                new { s.Description, s.Status });

        public Task DeleteServiceRequestAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/servicerequests/{id}");

        // ---------- Currency and dashboard ----------
        public async Task<decimal?> GetUsdToZarRateAsync()
        {
            var response = await _http.GetAsync("api/currency/usd-zar");
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
            if (!response.IsSuccessStatusCode) return null; // rate unavailable; the API still converts when saving

            var result = await response.Content.ReadFromJsonAsync<ExchangeRateResult>(JsonOptions);
            return result?.Rate;
        }

        public async Task<Dashboard> GetDashboardAsync() =>
            await GetAsync<Dashboard>("api/dashboard") ?? new();

        public Task DismissAlertAsync(int id) =>
            SendAsync(HttpMethod.Post, $"api/dashboard/alerts/{id}/dismiss");

        // ---------- Helpers ----------
        private async Task<T?> GetAsync<T>(string url)
        {
            var response = await _http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) return default;

            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }

        private async Task<T> SendAsync<T>(HttpMethod method, string url, object body)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };

            var response = await _http.SendAsync(request);
            await EnsureSuccessAsync(response);
            return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
        }

        private async Task SendAsync(HttpMethod method, string url)
        {
            var response = await _http.SendAsync(new HttpRequestMessage(method, url));
            await EnsureSuccessAsync(response);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();

            throw new ApiException(await ReadErrorMessageAsync(response));
        }

        // Reads { "message": "..." } or a validation error list from the API
        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var message) && message.GetString() is string text)
                    {
                        return text;
                    }

                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = errors.EnumerateObject()
                            .SelectMany(e => e.Value.EnumerateArray())
                            .Select(v => v.GetString());
                        return string.Join(" ", messages);
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON: fall through to the general message
            }

            return $"The GLMS API returned an error ({(int)response.StatusCode}).";
        }

        private class ExchangeRateResult
        {
            public decimal Rate { get; set; }
        }
    }
}