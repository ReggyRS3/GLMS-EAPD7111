using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using Xunit;

namespace GLMS.Test.Integration
{
    public class ApiIntegrationTests : IClassFixture<GlmsApiFactory>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly GlmsApiFactory _factory;

        public ApiIntegrationTests(GlmsApiFactory factory)
        {
            _factory = factory;
        }

        // ---------- Helpers ----------
        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { username = "admin", password = "Admin@123" });
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
            return client;
        }

        private static async Task<ContractDto> CreateActiveContractAsync(HttpClient client)
        {
            var create = await client.PostAsJsonAsync("/api/contracts", new
            {
                clientId = 1,
                startDate = DateTime.Today,
                endDate = DateTime.Today.AddYears(1),
                serviceLevel = "Premium"
            });
            create.EnsureSuccessStatusCode();
            var contract = await create.Content.ReadFromJsonAsync<ContractDto>(JsonOptions);

            var activate = await client.PatchAsJsonAsync($"/api/contracts/{contract!.ContractId}/status",
                new { action = "Activate" });
            activate.EnsureSuccessStatusCode();

            return (await activate.Content.ReadFromJsonAsync<ContractDto>(JsonOptions))!;
        }

        // ---------- Authentication ----------
        [Fact]
        public async Task GetContracts_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/contracts");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { username = "admin", password = "Admin@123" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
            Assert.False(string.IsNullOrWhiteSpace(login?.Token));
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { username = "admin", password = "wrong-password" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- Contracts ----------
        [Fact]
        public async Task GetContracts_WithToken_Returns200AndJsonIsNotNull()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/contracts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var contracts = await response.Content.ReadFromJsonAsync<List<ContractDto>>(JsonOptions);
            Assert.NotNull(contracts);
            Assert.NotEmpty(contracts);
        }

        [Fact]
        public async Task GetContracts_FilterByStatus_ReturnsOnlyThatStatus()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/contracts?status=Expired");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var contracts = await response.Content.ReadFromJsonAsync<List<ContractDto>>(JsonOptions);
            Assert.NotEmpty(contracts!);
            Assert.All(contracts!, c => Assert.Equal(ContractStatus.Expired, c.Status));
        }

        [Fact]
        public async Task PostContract_CreatesContractAsDraft()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/contracts", new
            {
                clientId = 2,
                startDate = DateTime.Today,
                endDate = DateTime.Today.AddMonths(6),
                serviceLevel = "Express"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var contract = await response.Content.ReadFromJsonAsync<ContractDto>(JsonOptions);
            Assert.Equal(ContractStatus.Draft, contract!.Status);
            Assert.False(contract.CanRaiseServiceRequest);
        }

        [Fact]
        public async Task PatchStatus_ActivateExpiredContract_Returns409()
        {
            var client = await CreateAuthenticatedClientAsync();

            // Seeded contract 2 is Expired
            var response = await client.PatchAsJsonAsync("/api/contracts/2/status", new { action = "Activate" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UploadAgreement_ExeFile_Returns400()
        {
            var client = await CreateAuthenticatedClientAsync();
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(Encoding.ASCII.GetBytes("MZ fake program")), "file", "malware.exe");

            var response = await client.PostAsync("/api/contracts/1/agreement", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- Service requests ----------
        [Fact]
        public async Task PostServiceRequest_ActiveContract_Returns201WithZarCost()
        {
            var client = await CreateAuthenticatedClientAsync();
            var contract = await CreateActiveContractAsync(client);

            var response = await client.PostAsJsonAsync("/api/servicerequests", new
            {
                contractId = contract.ContractId,
                description = "Integration test delivery",
                costUsd = 100m
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var request = await response.Content.ReadFromJsonAsync<ServiceRequestDto>(JsonOptions);
            Assert.Equal(1850.00m, request!.CostZar); // 100 x 18.50
            Assert.Equal(GlmsApiFactory.FixedRate, request.ExchangeRate);
        }

        [Fact]
        public async Task PostServiceRequest_ExpiredContract_Returns400()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/servicerequests", new
            {
                contractId = 2,
                description = "Should be refused",
                costUsd = 50m
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}