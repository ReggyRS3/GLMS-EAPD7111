using GLMS.Test.Fakes;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Xunit;

namespace GLMS.Test
{
    public class ServiceRequestServiceTests
    {
        private const decimal FixedRate = 18.50m;

        private static async Task<(ServiceRequestService Service, Web.Data.GlmsDbContext Context)> CreateServiceWithContract(ContractStatus status)
        {
            var context = TestDbContextFactory.Create();
            context.Contracts.Add(new Contract
            {
                ContractId = 1,
                ClientId = 1,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                ServiceLevel = "Premium",
                Status = status
            });
            await context.SaveChangesAsync();

            var service = new ServiceRequestService(context, new FakeCurrencyConverter(FixedRate));
            return (service, context);
        }

        [Fact]
        public async Task CreateRequestAsync_ActiveContract_SavesRequestWithZarCost()
        {
            var (service, context) = await CreateServiceWithContract(ContractStatus.Active);

            var request = await service.CreateRequestAsync(1, "Container delivery to Durban", 200m);

            Assert.Equal(3700.00m, request.CostZar);          // 200 x 18.50
            Assert.Equal(FixedRate, request.ExchangeRate);
            Assert.Equal(ServiceRequestStatus.Pending, request.Status);
            Assert.Single(context.ServiceRequests);
        }

        [Theory]
        [InlineData(ContractStatus.Expired)]
        [InlineData(ContractStatus.OnHold)]
        [InlineData(ContractStatus.Draft)]
        public async Task CreateRequestAsync_InactiveContract_IsRefusedAndNothingSaved(ContractStatus status)
        {
            var (service, context) = await CreateServiceWithContract(status);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateRequestAsync(1, "Urgent freight", 100m));

            Assert.Contains(status.ToString(), exception.Message);
            Assert.Empty(context.ServiceRequests);
        }

        [Fact]
        public async Task CreateRequestAsync_ContractDoesNotExist_IsRefused()
        {
            var (service, context) = await CreateServiceWithContract(ContractStatus.Active);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateRequestAsync(999, "Unknown contract", 50m));

            Assert.Empty(context.ServiceRequests);
        }
    }
}