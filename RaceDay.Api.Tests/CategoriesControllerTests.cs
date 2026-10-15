using RaceDay.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class CategoriesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public CategoriesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // ---------- helpers ----------

        /// <summary>
        /// Registers an Organiser, creates their profile, creates an event,
        /// returns an authenticated client + the new event's ID.
        /// </summary>
        private async Task<(HttpClient client, int eventID)> SetupOrganiserWithEvent(string email)
        {
            var client = _factory.CreateClient();

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

            var organiserDto = new CreateOrganiserRequestDto
            {
                UserID = user!.UserID,
                Name = "Test Org Inc",
                Address = "Cape Town"
            };
            await client.PostAsJsonAsync("/api/v1/organisers", organiserDto);

            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            var eventDto = new CreateEventRequestDto
            {
                Name = "Test Event For Categories",
                EventDate = System.DateTime.Today.AddDays(30),
                Status = "Open",
                Location = "Cape Town"
            };
            var eventResponse = await client.PostAsJsonAsync("/api/v1/events", eventDto);
            var ev = await eventResponse.Content.ReadFromJsonAsync<EventResponseDto>();

            return (client, ev!.EventID);
        }

        private static CreateCategoryRequestDto SampleCategory(string name = "Open Men") => new()
        {
            Name = name,
            Description = "Open category",
            MinAge = 18,
            MaxAge = 99,
            Gender = "M",
            Distance = 42.2m
        };

        // ---------- GET /api/v1/events/{eventId}/categories ----------

        [Fact]
        public async Task GetByEvent_NonExistentEvent_Returns404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/999999/categories");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEvent_ExistingEvent_ReturnsOk()
        {
            var (client, eventID) = await SetupOrganiserWithEvent("cat_list@example.com");

            var response = await client.GetAsync($"/api/v1/events/{eventID}/categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- POST /api/v1/events/{eventId}/categories ----------

        [Fact]
        public async Task Create_WithoutAuth_Returns401()
        {
            var (_, eventID) = await SetupOrganiserWithEvent("cat_noauth@example.com");
            var anonClient = _factory.CreateClient();

            var response = await anonClient.PostAsJsonAsync(
                $"/api/v1/events/{eventID}/categories",
                SampleCategory());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsOwnerOrganiser_Returns201()
        {
            var (client, eventID) = await SetupOrganiserWithEvent("cat_create@example.com");

            var response = await client.PostAsJsonAsync(
                $"/api/v1/events/{eventID}/categories",
                SampleCategory("Elite Men"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("Elite Men", body!.Name);
            Assert.Equal(eventID, body.EventID);
            Assert.Equal(42.2m, body.Distance);
        }

        [Fact]
        public async Task Create_OnNonExistentEvent_Returns404()
        {
            var (client, _) = await SetupOrganiserWithEvent("cat_badevent@example.com");

            var response = await client.PostAsJsonAsync(
                "/api/v1/events/999999/categories",
                SampleCategory());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- GET /api/v1/categories/{id} ----------

        [Fact]
        public async Task GetById_NonExistent_Returns404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/categories/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ExistingCategory_ReturnsOk()
        {
            var (client, eventID) = await SetupOrganiserWithEvent("cat_byid@example.com");

            var createResp = await client.PostAsJsonAsync(
                $"/api/v1/events/{eventID}/categories",
                SampleCategory("Women Open"));
            var created = await createResp.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var response = await client.GetAsync($"/api/v1/categories/{created!.CategoryID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("Women Open", body!.Name);
        }

        // ---------- PUT /api/v1/categories/{id} ----------

        [Fact]
        public async Task Update_AsOwner_ReturnsOk()
        {
            var (client, eventID) = await SetupOrganiserWithEvent("cat_update@example.com");

            var createResp = await client.PostAsJsonAsync(
                $"/api/v1/events/{eventID}/categories",
                SampleCategory("Old Name"));
            var created = await createResp.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var update = new UpdateCategoryRequestDto
            {
                Name = "New Name",
                Distance = 21.1m
            };
            var response = await client.PutAsJsonAsync(
                $"/api/v1/categories/{created!.CategoryID}",
                update);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("New Name", body!.Name);
            Assert.Equal(21.1m, body.Distance);
        }

        // ---------- DELETE /api/v1/categories/{id} ----------

        [Fact]
        public async Task Delete_AsOwner_Returns204()
        {
            var (client, eventID) = await SetupOrganiserWithEvent("cat_delete@example.com");

            var createResp = await client.PostAsJsonAsync(
                $"/api/v1/events/{eventID}/categories",
                SampleCategory("To Delete"));
            var created = await createResp.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var response = await client.DeleteAsync($"/api/v1/categories/{created!.CategoryID}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistent_Returns404()
        {
            var (client, _) = await SetupOrganiserWithEvent("cat_del404@example.com");

            var response = await client.DeleteAsync("/api/v1/categories/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}