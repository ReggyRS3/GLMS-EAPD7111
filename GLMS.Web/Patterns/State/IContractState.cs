using GLMS.Web.Models;

namespace GLMS.Web.Patterns.State
{
    public interface IContractState
    {
        bool CanRaiseServiceRequest();
        void Activate(Contract contract);
        void PutOnHold(Contract contract);
        void Expire(Contract contract);
    }
}