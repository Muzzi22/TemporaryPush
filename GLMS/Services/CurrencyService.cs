using System.Text.Json;

namespace GLMS.Web.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _config;

        public CurrencyService(
            HttpClient httpClient,
            ILogger<CurrencyService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                string apiKey = _config["ExchangeRateApi:ApiKey"] ?? "";
                string baseUrl = _config["ExchangeRateApi:BaseUrl"]
                                 ?? "https://v6.exchangerate-api.com/v6/";

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("ExchangeRate API key is missing from appsettings.json");
                    return GetFallbackRate();
                }

                string url = $"{baseUrl}{apiKey}/latest/USD";
                _logger.LogInformation("Calling exchange rate API: {Url}", url);

                // Set a timeout so the app does not hang
                _httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API returned status code: {Code}",
                        response.StatusCode);
                    return GetFallbackRate();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response received: {Json}",
                    json.Substring(0, Math.Min(json.Length, 200)));

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Check the result field
                if (root.TryGetProperty("result", out var resultProp))
                {
                    string resultVal = resultProp.GetString() ?? "";
                    if (resultVal != "success")
                    {
                        string errorType = root.TryGetProperty("error-type", out var errEl)
                            ? errEl.GetString() ?? "unknown"
                            : "unknown";
                        _logger.LogError("API error: {Error}", errorType);
                        return GetFallbackRate();
                    }
                }

                // Pull ZAR rate out of conversion_rates
                if (!root.TryGetProperty("conversion_rates", out var rates))
                {
                    _logger.LogError("conversion_rates not found in API response");
                    return GetFallbackRate();
                }

                if (!rates.TryGetProperty("ZAR", out var zarProp))
                {
                    _logger.LogError("ZAR not found in conversion_rates");
                    return GetFallbackRate();
                }

                decimal zarRate = zarProp.GetDecimal();
                _logger.LogInformation("Live USD to ZAR rate: {Rate}", zarRate);

                return zarRate;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("API call timed out after 10 seconds. Using fallback.");
                return GetFallbackRate();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling exchange rate API. Using fallback.");
                return GetFallbackRate();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse API response JSON. Using fallback.");
                return GetFallbackRate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error. Using fallback rate.");
                return GetFallbackRate();
            }
        }

        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            if (usdAmount <= 0 || rate <= 0) return 0m;
            return Math.Round(usdAmount * rate, 2);
        }

        private decimal GetFallbackRate()
        {
            _logger.LogWarning("Using fallback ZAR rate of 16.40");
            return 16.40m; // Updated to current approximate rate
        }
    }
}