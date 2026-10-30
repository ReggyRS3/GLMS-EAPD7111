using GLMS.Web.Models;
using GLMS.Web.Services;
using Xunit;

namespace GLMS.Test
{
    public class ContractFilterTests
    {
        private static IQueryable<Contract> SampleContracts() => new List<Contract>
        {
            new() { ContractId = 1, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), Status = ContractStatus.Expired, ServiceLevel = "Standard" },
            new() { ContractId = 2, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = ContractStatus.Active,  ServiceLevel = "Premium" },
            new() { ContractId = 3, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2027, 2, 28),  Status = ContractStatus.OnHold,  ServiceLevel = "Express" },
            new() { ContractId = 4, StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31), Status = ContractStatus.Draft,   ServiceLevel = "Standard" }
        }.AsQueryable();

        private static List<int> Ids(IQueryable<Contract> query) =>
            query.Select(c => c.ContractId).OrderBy(id => id).ToList();

        [Fact]
        public void Apply_NoFilters_ReturnsAllContracts()
        {
            var result = ContractFilter.Apply(SampleContracts(), null, null, null);

            Assert.Equal(new List<int> { 1, 2, 3, 4 }, Ids(result));
        }

        [Fact]
        public void Apply_StatusOnly_ReturnsMatchingStatus()
        {
            var result = ContractFilter.Apply(SampleContracts(), null, null, ContractStatus.Active);

            Assert.Equal(new List<int> { 2 }, Ids(result));
        }

        [Fact]
        public void Apply_FromDateOnly_ReturnsContractsStillRunning()
        {
            var result = ContractFilter.Apply(SampleContracts(), new DateTime(2027, 1, 1), null, null);

            Assert.Equal(new List<int> { 3, 4 }, Ids(result));
        }

        [Fact]
        public void Apply_DateRange_ReturnsContractsRunningInThatPeriod()
        {
            var result = ContractFilter.Apply(SampleContracts(), new DateTime(2025, 1, 1), new DateTime(2025, 6, 30), null);

            Assert.Equal(new List<int> { 1 }, Ids(result));
        }

        [Fact]
        public void Apply_DateRangeAndStatus_AppliesBothFilters()
        {
            var result = ContractFilter.Apply(SampleContracts(), new DateTime(2027, 1, 1), new DateTime(2027, 12, 31), ContractStatus.OnHold);

            Assert.Equal(new List<int> { 3 }, Ids(result));
        }

        [Fact]
        public void Apply_NoMatches_ReturnsEmpty()
        {
            var result = ContractFilter.Apply(SampleContracts(), new DateTime(2027, 1, 1), null, ContractStatus.Expired);

            Assert.Empty(result);
        }
    }
}