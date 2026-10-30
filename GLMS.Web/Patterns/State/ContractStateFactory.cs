using GLMS.Web.Models;

namespace GLMS.Web.Patterns.State
{
    public static class ContractStateFactory
    {
        public static IContractState FromStatus(ContractStatus status) => status switch
        {
            ContractStatus.Active => new ActiveState(),
            ContractStatus.OnHold => new OnHoldState(),
            ContractStatus.Expired => new ExpiredState(),
            _ => new DraftState()
        };

        public static ContractStatus ToStatus(IContractState state) => state switch
        {
            ActiveState => ContractStatus.Active,
            OnHoldState => ContractStatus.OnHold,
            ExpiredState => ContractStatus.Expired,
            _ => ContractStatus.Draft
        };
    }
}