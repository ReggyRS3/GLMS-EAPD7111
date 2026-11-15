using GLMS.Api.Models;

namespace GLMS.Api.Patterns.State
{
    public interface IContractState
    {
        bool CanRaiseServiceRequest();
        void Activate(Contract contract);
        void PutOnHold(Contract contract);
        void Expire(Contract contract);
    }
}