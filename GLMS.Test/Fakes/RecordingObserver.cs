using GLMS.Api.Models;
using GLMS.Api.Patterns.Observer;

namespace GLMS.Test.Fakes
{
    public class RecordingObserver : IContractObserver
    {
        public List<(string OldStatus, ContractStatus NewStatus)> Notifications { get; } = new();

        public void Update(Contract contract, string oldStatus)
        {
            Notifications.Add((oldStatus, contract.Status));
        }
    }
}