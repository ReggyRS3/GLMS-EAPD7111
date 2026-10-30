using GLMS.Web.Models;

namespace GLMS.Web.Patterns.State
{
    public class OnHoldState : IContractState
    {
        public bool CanRaiseServiceRequest() => false;

        public void Activate(Contract contract) => contract.SetState(new ActiveState());

        public void PutOnHold(Contract contract) =>
            throw new InvalidOperationException("This contract is already on hold.");

        public void Expire(Contract contract) => contract.SetState(new ExpiredState());
    }
}