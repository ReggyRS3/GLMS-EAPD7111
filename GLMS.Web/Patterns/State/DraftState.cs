using GLMS.Web.Models;

namespace GLMS.Web.Patterns.State
{
    public class DraftState : IContractState
    {
        public bool CanRaiseServiceRequest() => false;

        public void Activate(Contract contract) => contract.SetState(new ActiveState());

        public void PutOnHold(Contract contract) => contract.SetState(new OnHoldState());

        public void Expire(Contract contract) =>
            throw new InvalidOperationException("A draft contract cannot expire. Activate it first.");
    }
}