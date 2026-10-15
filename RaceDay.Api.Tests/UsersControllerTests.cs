using RaceDay.DTOs;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class UsersControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public UsersControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // ---------- helpers ----------

        /// <summary>
        /// Registers a user, logs them in, returns an authenticated client.
        /// </summary>
        private async Task<HttpClient> CreateAuthenticatedClientAsync(
            string email, string role = "Participant")
        {
            var client = _factory.CreateClient();

            var register = new RegisterRequestDto
            {
                Email = email,
                Password = "Test123!",
                FullName = "Original Name",
                Role = role,
                Phone = "0821234567",
                DateOfBirth = role == "Participant" ? new DateTime(1995, 6, 15) : null
            };
            await client.PostAsJsonAsync("/api/v1/auth/register", register);

            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            return client;
        }

        /// <summary>
        /// Registers a Participant and creates their Participant profile.
        /// Needed to test the emergencyContact field update on PUT /users/me.
        /// </summary>
        private async Task<HttpClient> CreateParticipantWithProfileAsync(string email)
        {
            var client = _factory.CreateClient();

            var register = new RegisterRequestDto
            {
                Email = email,
                Password = "Test123!",
                FullName = "Original Name",
                Role = "Participant",
                Phone = "0821234567",
                DateOfBirth = new DateTime(1995, 6, 15)
            };
            var reg = await client.PostAsJsonAsync("/api/v1/auth/register", register);
            var user = await reg.Content.ReadFromJsonAsync<RegisterResponseDto>();

            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            // Create the Participant profile
            await client.PostAsJsonAsync("/api/v1/participants", new CreateParticipantRequestDto
            {
                UserID = user!.UserID,
                FirstName = "Test",
                LastName = "Runner",
                DateOfBirth = new DateTime(1995, 6, 15),
                Gender = "M",
                EmergencyContact = "0820000000"
            });

            return client;
        }

        // ---------- GET /api/v1/users/me ----------

        [Fact]
        public async Task GetMe_WithoutAuth_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/users/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMe_AsParticipant_ReturnsOkWithProfile()
        {
            var client = await CreateAuthenticatedClientAsync(
                "users_get_me@example.com", "Participant");

            var response = await client.GetAsync("/api/v1/users/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("users_get_me@example.com", body!.Email);
            Assert.Equal("Participant", body.Role);
            Assert.Equal("Original Name", body.FullName);
            Assert.Equal("0821234567", body.Phone);
            Assert.True(body.UserId > 0);
        }

        [Fact]
        public async Task GetMe_AsOrganiser_ReturnsOkWithProfile()
        {
            var client = await CreateAuthenticatedClientAsync(
                "users_get_org@example.com", "Organiser");

            var response = await client.GetAsync("/api/v1/users/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("Organiser", body!.Role);
        }

        // ---------- PUT /api/v1/users/me ----------

        [Fact]
        public async Task UpdateMe_WithoutAuth_Returns401()
        {
            var client = _factory.CreateClient();

            var update = new UpdateUserProfileRequestDto
            {
                FullName = "New Name"
            };
            var response = await client.PutAsJsonAsync("/api/v1/users/me", update);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMe_ChangesFullNameAndPhone_Returns200()
        {
            var client = await CreateAuthenticatedClientAsync(
                "users_update@example.com", "Participant");

            var update = new UpdateUserProfileRequestDto
            {
                FullName = "Updated Name",
                Phone = "0829999999"
            };
            var response = await client.PutAsJsonAsync("/api/v1/users/me", update);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("Updated Name", body!.FullName);
            Assert.Equal("0829999999", body.Phone);
        }

        [Fact]
        public async Task UpdateMe_PersistsChanges_ReturnsUpdatedOnNextGet()
        {
            var client = await CreateAuthenticatedClientAsync(
                "users_persist@example.com", "Participant");

            // Update
            await client.PutAsJsonAsync("/api/v1/users/me", new UpdateUserProfileRequestDto
            {
                FullName = "Persisted Name"
            });

            // Get again to verify persistence
            var get = await client.GetAsync("/api/v1/users/me");
            var body = await get.Content.ReadFromJsonAsync<UserProfileResponseDto>();

            Assert.NotNull(body);
            Assert.Equal("Persisted Name", body!.FullName);
        }

        [Fact]
        public async Task UpdateMe_WithEmergencyContact_UpdatesParticipantProfile()
        {
            var client = await CreateParticipantWithProfileAsync("users_emergency@example.com");

            var update = new UpdateUserProfileRequestDto
            {
                EmergencyContact = "0821234000"
            };
            var response = await client.PutAsJsonAsync("/api/v1/users/me", update);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
            Assert.NotNull(body);
            Assert.Equal("0821234000", body!.EmergencyContact);
        }
    }
}