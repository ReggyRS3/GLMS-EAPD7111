using GLMS.Test.Fakes;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Observer;
using Xunit;

namespace GLMS.Test
{
    public class ContractObserverTests
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

        [Fact]
        public void StatusChange_NotifiesEveryAttachedObserver()
        {
            var contract = NewContract(ContractStatus.Draft);
            var first = new RecordingObserver();
            var second = new RecordingObserver();
            contract.Attach(first);
            contract.Attach(second);

            contract.Activate();

            Assert.Single(first.Notifications);
            Assert.Single(second.Notifications);
            Assert.Equal(("Draft", ContractStatus.Active), first.Notifications[0]);
        }

        [Fact]
        public void DetachedObserver_IsNotNotified()
        {
            var contract = NewContract(ContractStatus.Active);
            var observer = new RecordingObserver();
            contract.Attach(observer);
            contract.Detach(observer);

            contract.PutOnHold();

            Assert.Empty(observer.Notifications);
        }

        [Fact]
        public void RefusedStatusChange_DoesNotNotifyObservers()
        {
            var contract = NewContract(ContractStatus.Expired);
            var observer = new RecordingObserver();
            contract.Attach(observer);

            Assert.Throws<InvalidOperationException>(() => contract.Activate());

            Assert.Empty(observer.Notifications);
        }

        [Fact]
        public async Task AuditLogObserver_SavesStatusChangeToDatabase()
        {
            using var context = TestDbContextFactory.Create();
            var contract = NewContract(ContractStatus.Active);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            contract.Attach(new AuditLogObserver(context));
            contract.PutOnHold();
            await context.SaveChangesAsync();

            var log = Assert.Single(context.ContractAuditLogs);
            Assert.Equal("Active", log.OldStatus);
            Assert.Equal("OnHold", log.NewStatus);
        }

        [Fact]
        public async Task ManagerAlertObserver_ContractPutOnHold_CreatesAlert()
        {
            using var context = TestDbContextFactory.Create();
            var contract = NewContract(ContractStatus.Active);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            contract.Attach(new ManagerAlertObserver(context));
            contract.PutOnHold();
            await context.SaveChangesAsync();

            var alert = Assert.Single(context.ManagerAlerts);
            Assert.Contains("OnHold", alert.Message);
            Assert.False(alert.IsDismissed);
        }

        [Fact]
        public async Task ManagerAlertObserver_ContractActivated_CreatesNoAlert()
        {
            using var context = TestDbContextFactory.Create();
            var contract = NewContract(ContractStatus.OnHold);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            contract.Attach(new ManagerAlertObserver(context));
            contract.Activate();
            await context.SaveChangesAsync();

            Assert.Empty(context.ManagerAlerts);
        }
    }
}