using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Observer
{
    // Observer Pattern: Notifies all registered observers when contract status changes
    public class ContractEventService
    {
        private readonly List<IContractObserver> _observers = new();

        public void Subscribe(IContractObserver observer)
        {
            _observers.Add(observer);
        }

        public async Task NotifyStatusChangedAsync(Contract contract, ContractStatus oldStatus, ContractStatus newStatus)
        {
            foreach (var observer in _observers)
            {
                await observer.OnContractStatusChangedAsync(contract, oldStatus, newStatus);
            }
        }
    }
}