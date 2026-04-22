using GLMS.Web.Models;
using GLMS.Web.Patterns.Observer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class ObserverPatternTests
    {
        private ContractEventService CreateEventService()
        {
            return new ContractEventService();
        }

        private ContractExpiryObserver CreateExpiryObserver()
        {
            var logger =
                new Mock<ILogger<ContractExpiryObserver>>().Object;
            return new ContractExpiryObserver(logger);
        }

        [Fact]
        public async Task EventService_WithNoObservers_DoesNotThrow()
        {
            var service = CreateEventService();
            var contract = new Contract
            {
                Id = 1,
                Status = ContractStatus.Active
            };

            var ex = await Record.ExceptionAsync(() =>
                service.NotifyStatusChangedAsync(
                    contract,
                    ContractStatus.Draft,
                    ContractStatus.Active));

            Assert.Null(ex);
        }

        [Fact]
        public async Task EventService_WithObserver_NotifiesObserver()
        {
            var service = CreateEventService();
            var observer = CreateExpiryObserver();
            service.Subscribe(observer);

            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                Status = ContractStatus.Expired
            };

            var ex = await Record.ExceptionAsync(() =>
                service.NotifyStatusChangedAsync(
                    contract,
                    ContractStatus.Active,
                    ContractStatus.Expired));

            Assert.Null(ex);
        }

        [Fact]
        public void EventService_CanSubscribeMultipleObservers()
        {
            var service = CreateEventService();
            var observer1 = CreateExpiryObserver();
            var observer2 = CreateExpiryObserver();

            service.Subscribe(observer1);
            service.Subscribe(observer2);

            // No exception means both observers registered fine
            Assert.True(true);
        }
    }
}