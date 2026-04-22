using GLMS.Web.Models;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class ServiceRequestTests
    {
        // ── Model defaults ────────────────────────────────────

        [Fact]
        public void NewServiceRequest_DefaultStatus_IsPending()
        {
            var sr = new ServiceRequest();
            Assert.Equal(RequestStatus.Pending, sr.Status);
        }

        [Fact]
        public void NewServiceRequest_CreatedAt_IsSetToNow()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var sr = new ServiceRequest();
            var after = DateTime.UtcNow.AddSeconds(1);
            Assert.InRange(sr.CreatedAt, before, after);
        }

        // ── Cost validation ───────────────────────────────────

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(0.01, true)]
        [InlineData(100, true)]
        [InlineData(9999, true)]
        public void ServiceRequest_CostUSD_MustBePositive(
            decimal cost, bool isValid)
        {
            Assert.Equal(isValid, cost > 0);
        }

        // ── Description validation ────────────────────────────

        [Theory]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("Delivery", true)]
        [InlineData("Pick up freight from Cape Town port", true)]
        public void ServiceRequest_Description_MustNotBeEmpty(
            string description, bool isValid)
        {
            Assert.Equal(isValid, !string.IsNullOrWhiteSpace(description));
        }
    }
}