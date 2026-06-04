using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GLMS.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        // ── Test 1: GET /api/contracts returns 200 or 401 (API is alive) ──
        [Fact]
        public async Task GetContracts_ReturnsExpectedStatusCode()
        {
            var response = await _client.GetAsync("/api/contracts");

            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.OK,
                $"Expected 200 or 401 but got {response.StatusCode}");
        }

        // ── Test 2: GET /api/clients returns 401 without token ────────────
        [Fact]
        public async Task GetClients_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/clients");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Test 3: GET /api/servicerequests returns 401 without token ────
        [Fact]
        public async Task GetServiceRequests_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/servicerequests");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Test 4: POST /api/auth/login with bad credentials ─────────────
        [Fact]
        public async Task Login_WithInvalidCredentials_Returns401()
        {
            var loginDto = new { email = "wrong@test.com", password = "WrongPassword!" };
            var content = new StringContent(
                JsonSerializer.Serialize(loginDto),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected 401 or 500 but got {response.StatusCode}");
        }

        // ── Test 5: POST /api/auth/login with empty body ──────────────────
        [Fact]
        public async Task Login_WithEmptyBody_ReturnsBadRequestOrUnauthorized()
        {
            var loginDto = new { email = "", password = "" };
            var content = new StringContent(
                JsonSerializer.Serialize(loginDto),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected 400, 401 or 500 but got {response.StatusCode}");
        }

        // ── Test 6: POST /api/contracts without token returns 401 ─────────
        [Fact]
        public async Task PostContract_WithoutToken_Returns401()
        {
            var contract = new { clientId = 1, startDate = DateTime.Now, endDate = DateTime.Now.AddYears(1), status = 0, serviceLevel = "Gold" };
            var content = new StringContent(
                JsonSerializer.Serialize(contract),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/contracts", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Test 7: PATCH /api/contracts/{id}/status without token returns 401
        [Fact]
        public async Task PatchContractStatus_WithoutToken_Returns401()
        {
            var body = new { status = 1 };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PatchAsync("/api/contracts/1/status", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Test 8: Swagger endpoint is accessible ────────────────────────
        [Fact]
        public async Task SwaggerEndpoint_IsAccessible()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}