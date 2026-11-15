using GLMS.Api.Models;

namespace GLMS.Api.Patterns.Observer
{
    public interface IContractObserver
    {
        void Update(Contract contract, string oldStatus);
    }
}