using GLMS.Web.Models;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class ClientModelTests
    {
        [Fact]
        public void NewClient_Contracts_IsEmptyCollection()
        {
            var client = new Client();
            Assert.NotNull(client.Contracts);
            Assert.Empty(client.Contracts);
        }

        [Fact]
        public void Client_Name_CanBeSetAndRetrieved()
        {
            var client = new Client { Name = "Acme Freight Ltd" };
            Assert.Equal("Acme Freight Ltd", client.Name);
        }

        [Fact]
        public void Client_Region_CanBeSetAndRetrieved()
        {
            var client = new Client { Region = "Africa" };
            Assert.Equal("Africa", client.Region);
        }

        [Theory]
        [InlineData("Africa")]
        [InlineData("Europe")]
        [InlineData("North America")]
        [InlineData("Asia Pacific")]
        [InlineData("Middle East")]
        public void Client_ValidRegions_AreAccepted(string region)
        {
            var client = new Client { Region = region };
            Assert.False(string.IsNullOrWhiteSpace(client.Region));
        }

        [Fact]
        public void Client_WithContracts_ReturnsCorrectCount()
        {
            var client = new Client
            {
                Name = "Test Client",
                Contracts = new List<Contract>
                {
                    new Contract { ServiceLevel = "Standard" },
                    new Contract { ServiceLevel = "Premium"  },
                }
            };
            Assert.Equal(2, client.Contracts.Count);
        }
    }
}