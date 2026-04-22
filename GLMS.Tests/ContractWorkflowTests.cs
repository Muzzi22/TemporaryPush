using GLMS.Web.Models;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class ContractWorkflowTests
    {
        // ── CanCreateServiceRequest ───────────────────────────

        private bool CanCreateServiceRequest(ContractStatus status)
        {
            return status != ContractStatus.Expired &&
                   status != ContractStatus.OnHold;
        }

        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, true)]
        [InlineData(ContractStatus.Expired, false)]
        [InlineData(ContractStatus.OnHold, false)]
        public void ServiceRequest_CanBeCreated_BasedOnContractStatus(
            ContractStatus status, bool expected)
        {
            Assert.Equal(expected, CanCreateServiceRequest(status));
        }

        // ── Contract date validation ──────────────────────────

        [Fact]
        public void Contract_EndDateAfterStartDate_IsValid()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2026, 1, 1);
            Assert.True(end > start);
        }

        [Fact]
        public void Contract_EndDateBeforeStartDate_IsInvalid()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2025, 1, 1);
            Assert.False(end > start);
        }

        [Fact]
        public void Contract_EndDateEqualToStartDate_IsInvalid()
        {
            var date = new DateTime(2025, 6, 1);
            Assert.False(date > date);
        }

        // ── Contract model defaults ───────────────────────────

        [Fact]
        public void NewContract_DefaultStatus_IsDraft()
        {
            var contract = new Contract();
            Assert.Equal(ContractStatus.Draft, contract.Status);
        }

        [Fact]
        public void NewContract_ServiceRequests_IsEmptyCollection()
        {
            var contract = new Contract();
            Assert.NotNull(contract.ServiceRequests);
            Assert.Empty(contract.ServiceRequests);
        }

        // ── Status transition logic ───────────────────────────

        [Fact]
        public void Contract_StatusCanBeChangedToActive()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Draft
            };
            contract.Status = ContractStatus.Active;
            Assert.Equal(ContractStatus.Active, contract.Status);
        }

        [Fact]
        public void Contract_StatusCanBeChangedToExpired()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active
            };
            contract.Status = ContractStatus.Expired;
            Assert.Equal(ContractStatus.Expired, contract.Status);
        }

        [Fact]
        public void Contract_ActiveToOnHold_BlocksServiceRequests()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Active
            };
            contract.Status = ContractStatus.OnHold;
            Assert.False(CanCreateServiceRequest(contract.Status));
        }
    }
}