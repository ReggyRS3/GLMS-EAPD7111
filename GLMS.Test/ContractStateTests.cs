using GLMS.Web.Models;
using Xunit;

namespace GLMS.Test
{
    public class ContractStateTests
    {
        private static Contract NewContract(ContractStatus status) => new()
        {
            ContractId = 1,
            ClientId = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            ServiceLevel = "Premium",
            Status = status
        };

        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, false)]
        [InlineData(ContractStatus.OnHold, false)]
        [InlineData(ContractStatus.Expired, false)]
        public void CanRaiseServiceRequest_OnlyActiveContractsAllowRequests(ContractStatus status, bool expected)
        {
            var contract = NewContract(status);

            Assert.Equal(expected, contract.CanRaiseServiceRequest());
        }

        [Fact]
        public void Activate_DraftContract_BecomesActive()
        {
            var contract = NewContract(ContractStatus.Draft);

            contract.Activate();

            Assert.Equal(ContractStatus.Active, contract.Status);
        }

        [Fact]
        public void PutOnHold_ActiveContract_BecomesOnHold()
        {
            var contract = NewContract(ContractStatus.Active);

            contract.PutOnHold();

            Assert.Equal(ContractStatus.OnHold, contract.Status);
        }

        [Fact]
        public void Activate_OnHoldContract_BecomesActiveAgain()
        {
            var contract = NewContract(ContractStatus.OnHold);

            contract.Activate();

            Assert.Equal(ContractStatus.Active, contract.Status);
        }

        [Fact]
        public void Expire_ActiveContract_BecomesExpired()
        {
            var contract = NewContract(ContractStatus.Active);

            contract.Expire();

            Assert.Equal(ContractStatus.Expired, contract.Status);
        }

        [Fact]
        public void Activate_ExpiredContract_IsRefusedAndStatusUnchanged()
        {
            var contract = NewContract(ContractStatus.Expired);

            Assert.Throws<InvalidOperationException>(() => contract.Activate());
            Assert.Equal(ContractStatus.Expired, contract.Status);
        }

        [Fact]
        public void Expire_DraftContract_IsRefused()
        {
            var contract = NewContract(ContractStatus.Draft);

            Assert.Throws<InvalidOperationException>(() => contract.Expire());
            Assert.Equal(ContractStatus.Draft, contract.Status);
        }
    }
}