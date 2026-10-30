using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Observer
{
    public interface IContractObserver
    {
        void Update(Contract contract, string oldStatus);
    }
}