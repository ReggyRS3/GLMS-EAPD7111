using GLMS.Web.Models;

namespace GLMS.Web.Patterns.State
{
    public class ExpiredState : IContractState
    {
        public bool CanRaiseServiceRequest() => false;

        public void Activate(Contract contract) =>
            throw new InvalidOperationException("An expired contract cannot be reactivated. Create a new contract.");

        public void PutOnHold(Contract contract) =>
            throw new InvalidOperationException("An expired contract cannot be put on hold.");

        public void Expire(Contract contract) =>
            throw new InvalidOperationException("This contract has already expired.");
    }
}