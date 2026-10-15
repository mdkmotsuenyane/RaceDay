using RaceDay.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public EventsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // ---------- helpers ----------

        private async Task<HttpClient> CreateAuthenticatedOrganiserClientAsync(string email)
        {
            var client = _factory.CreateClient();

            // Register as Organiser
            var register = new RegisterRequestDto
            {
                Email = email,
                Password = "Test123!",
                FullName = "Test Organiser",
                Role = "Organiser",
                Phone = "0821234567"
            };
            var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", register);
            var user = await regResponse.Content.ReadFromJsonAsync<RegisterResponseDto>();

            // Create the Organiser profile (required before creating events)
            var organiserDto = new CreateOrganiserRequestDto
            {
                UserID = user!.UserID,
                Name = "Test Organisers Inc",
                Address = "Cape Town"
            };
            await client.PostAsJsonAsync("/api/v1/organisers", organiserDto);

            // Login to get the JWT
            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            return client;
        }

        private async Task<HttpClient> CreateAuthenticatedParticipantClientAsync(string email)
        {
            var client = _factory.CreateClient();

            var register = new RegisterRequestDto
            {
                Email = email,
                Password = "Test123!",
                FullName = "Test Participant",
                Role = "Participant",
                DateOfBirth = new DateTime(1995, 6, 15)
            };
            await client.PostAsJsonAsync("/api/v1/auth/register", register);

            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            return client;
        }

        // ---------- GET /api/v1/events ----------

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- GET /api/v1/events/upcoming ----------

        [Fact]
        public async Task GetUpcoming_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/upcoming");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- GET /api/v1/events/{id} ----------

        [Fact]
        public async Task GetById_NonExistent_Returns404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- POST /api/v1/events ----------

        [Fact]
        public async Task Create_WithoutAuth_Returns401()
        {
            var client = _factory.CreateClient();

            var dto = new CreateEventRequestDto
            {
                Name = "Unauthorized Race",
                EventDate = DateTime.Today.AddDays(30),
                Status = "Open"
            };

            var response = await client.PostAsJsonAsync("/api/v1/events", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsParticipant_Returns403()
        {
            var client = await CreateAuthenticatedParticipantClientAsync("part_events@example.com");

            var dto = new CreateEventRequestDto
            {
                Name = "Should Fail",
                EventDate = DateTime.Today.AddDays(30),
                Status = "Open"
            };

            var response = await client.PostAsJsonAsync("/api/v1/events", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsOrganiser_Returns201()
        {
            var client = await CreateAuthenticatedOrganiserClientAsync("org_events@example.com");

            var dto = new CreateEventRequestDto
            {
                Name = "Cape Town Marathon",
                Description = "42km road race",
                EventDate = DateTime.Today.AddDays(30),
                StartTime = "06:00",
                Location = "Cape Town",
                Status = "Open",
                StartPoint = "City Hall",
                FinishPoint = "Stadium"
            };

            var response = await client.PostAsJsonAsync("/api/v1/events", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<EventResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("Cape Town Marathon", body!.Name);
            Assert.Equal("Cape Town", body.Location);
            Assert.True(body.EventID > 0);
        }

        // ---------- GET /api/v1/events/{id}/weather ----------

        [Fact]
        public async Task GetWeather_NonExistent_Returns404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/999999/weather");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}