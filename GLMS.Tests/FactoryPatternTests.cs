using GLMS.Web.Models;
using GLMS.Web.Patterns.Factory;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class FactoryPatternTests
    {
        private readonly IContractFactory _factory =
            new StandardContractFactory();

        [Fact]
        public void CreateContract_ReturnsContractObject()
        {
            var contract = _factory.CreateContract(
                1,
                DateTime.Today,
                DateTime.Today.AddYears(1),
                "Standard");

            Assert.NotNull(contract);
        }

        [Fact]
        public void CreateContract_SetsClientId_Correctly()
        {
            var contract = _factory.CreateContract(
                5,
                DateTime.Today,
                DateTime.Today.AddYears(1),
                "Premium");

            Assert.Equal(5, contract.ClientId);
        }

        [Fact]
        public void CreateContract_SetsServiceLevel_Correctly()
        {
            var contract = _factory.CreateContract(
                1,
                DateTime.Today,
                DateTime.Today.AddYears(1),
                "Enterprise");

            Assert.Equal("Enterprise", contract.ServiceLevel);
        }

        [Fact]
        public void CreateContract_DefaultStatus_IsDraft()
        {
            var contract = _factory.CreateContract(
                1,
                DateTime.Today,
                DateTime.Today.AddYears(1),
                "Standard");

            Assert.Equal(ContractStatus.Draft, contract.Status);
        }

        [Fact]
        public void CreateContract_SetsStartDate_Correctly()
        {
            var start = new DateTime(2025, 1, 1);
            var contract = _factory.CreateContract(
                1, start, start.AddYears(1), "Standard");

            Assert.Equal(start, contract.StartDate);
        }

        [Fact]
        public void CreateContract_SetsEndDate_Correctly()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2026, 1, 1);
            var contract = _factory.CreateContract(
                1, start, end, "Standard");

            Assert.Equal(end, contract.EndDate);
        }
    }
}