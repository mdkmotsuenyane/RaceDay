using RaceDay.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public AuthControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static RegisterRequestDto ValidRegister(
            string email = "test@example.com",
            string role = "Participant") => new()
            {
                Email = email,
                Password = "Test123!",
                FullName = "Test User",
                Role = role,
                Phone = "0821234567",
                DateOfBirth = new DateTime(1995, 6, 15)
            };

        [Fact]
        public async Task Register_WithValidData_Returns201()
        {
            var client = _factory.CreateClient();
            var dto = ValidRegister("register_ok@example.com");

            var response = await client.PostAsJsonAsync("/api/v1/auth/register", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("register_ok@example.com", body!.Email);
            Assert.Equal("Participant", body.Role);
            Assert.True(body.UserID > 0);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns400()
        {
            var client = _factory.CreateClient();
            var dto = ValidRegister("dup@example.com");

            var first = await client.PostAsJsonAsync("/api/v1/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var second = await client.PostAsJsonAsync("/api/v1/auth/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidRole_Returns400()
        {
            var client = _factory.CreateClient();
            var dto = ValidRegister("badrole@example.com", role: "Admin");

            var response = await client.PostAsJsonAsync("/api/v1/auth/register", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_Returns200AndToken()
        {
            var client = _factory.CreateClient();

            var register = ValidRegister("login_ok@example.com");
            var reg = await client.PostAsJsonAsync("/api/v1/auth/register", register);
            Assert.Equal(HttpStatusCode.Created, reg.StatusCode);

            var login = new LoginRequestDto
            {
                Email = "login_ok@example.com",
                Password = "Test123!"
            };
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.Token));
            Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
            Assert.Equal(86400, body.ExpiresIn);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            var client = _factory.CreateClient();

            var register = ValidRegister("wrongpw@example.com");
            await client.PostAsJsonAsync("/api/v1/auth/register", register);

            var login = new LoginRequestDto
            {
                Email = "wrongpw@example.com",
                Password = "NotTheRightPassword!"
            };
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401()
        {
            var client = _factory.CreateClient();

            var login = new LoginRequestDto
            {
                Email = "nobody@example.com",
                Password = "Whatever123!"
            };

            var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}