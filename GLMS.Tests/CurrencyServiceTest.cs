using GLMS.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class CurrencyServiceTests
    {
        private CurrencyService CreateService(
            HttpClient? client = null)
        {
            var logger = new Mock<ILogger<CurrencyService>>().Object;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ExchangeRateApi:ApiKey"] = "test_key",
                    ["ExchangeRateApi:BaseUrl"] =
                        "https://v6.exchangerate-api.com/v6/"
                })
                .Build();

            return new CurrencyService(
                client ?? new HttpClient(), logger, config);
        }

        // ── ConvertUsdToZar ───────────────────────────────────

        [Fact]
        public void ConvertUsdToZar_CorrectAmount_ReturnsExpected()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(100m, 18.50m);
            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(0m, 18.50m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ConvertUsdToZar_NegativeAmount_ReturnsZero()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(-50m, 18.50m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ConvertUsdToZar_ZeroRate_ReturnsZero()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(100m, 0m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ConvertUsdToZar_RoundsToTwoDecimalPlaces()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(1m, 18.333333m);
            Assert.Equal(18.33m, result);
        }

        [Fact]
        public void ConvertUsdToZar_LargeAmount_ReturnsCorrectValue()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(10000m, 18.50m);
            Assert.Equal(185000.00m, result);
        }

        [Fact]
        public void ConvertUsdToZar_DecimalAmount_ReturnsCorrectValue()
        {
            var service = CreateService();
            var result = service.ConvertUsdToZar(99.99m, 18.50m);
            Assert.Equal(1849.82m, result);
        }
    }
}