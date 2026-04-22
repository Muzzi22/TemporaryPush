using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Observer
{
    // Concrete Observer: Logs when a contract expires
    public class ContractExpiryObserver : IContractObserver
    {
        private readonly ILogger<ContractExpiryObserver> _logger;

        public ContractExpiryObserver(ILogger<ContractExpiryObserver> logger)
        {
            _logger = logger;
        }

        public Task OnContractStatusChangedAsync(Contract contract, ContractStatus oldStatus, ContractStatus newStatus)
        {
            if (newStatus == ContractStatus.Expired)
            {
                _logger.LogWarning("CONTRACT EXPIRED: Contract #{Id} for Client #{ClientId} changed from {Old} to {New}",
                    contract.Id, contract.ClientId, oldStatus, newStatus);
            }
            return Task.CompletedTask;
        }
    }
}