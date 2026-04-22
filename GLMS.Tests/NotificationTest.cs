using GLMS.Web.Models;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class NotificationTests
    {
        [Fact]
        public void NewNotification_IsRead_DefaultsFalse()
        {
            var n = new ContractNotification();
            Assert.False(n.IsRead);
        }

        [Fact]
        public void NewNotification_CreatedAt_IsSetToNow()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var n = new ContractNotification();
            var after = DateTime.UtcNow.AddSeconds(1);
            Assert.InRange(n.CreatedAt, before, after);
        }

        [Fact]
        public void Notification_CanBeMarkedAsRead()
        {
            var n = new ContractNotification { IsRead = false };
            n.IsRead = true;
            Assert.True(n.IsRead);
        }

        [Fact]
        public void Notification_Message_CanBeSetAndRetrieved()
        {
            var msg = "Your contract is now Active.";
            var n = new ContractNotification { Message = msg };
            Assert.Equal(msg, n.Message);
        }

        [Fact]
        public void Notification_ContractId_CanBeSet()
        {
            var n = new ContractNotification { ContractId = 42 };
            Assert.Equal(42, n.ContractId);
        }

        [Fact]
        public void Notification_UserId_CanBeSet()
        {
            var n = new ContractNotification
            {
                UserId = "user-abc-123"
            };
            Assert.Equal("user-abc-123", n.UserId);
        }
    }
}