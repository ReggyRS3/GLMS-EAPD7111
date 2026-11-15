using GLMS.Api.Models;

namespace GLMS.Api.Patterns.State
{
    public class ActiveState : IContractState
    {
        public bool CanRaiseServiceRequest() => true;

        public void Activate(Contract contract) =>
            throw new InvalidOperationException("This contract is already active.");

        public void PutOnHold(Contract contract) => contract.SetState(new OnHoldState());

        public void Expire(Contract contract) => contract.SetState(new ExpiredState());
    }
}