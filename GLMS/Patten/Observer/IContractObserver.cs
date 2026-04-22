using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Observer
{
    public interface IContractObserver
    {
        Task OnContractStatusChangedAsync(Contract contract, ContractStatus oldStatus, ContractStatus newStatus);
    }
}