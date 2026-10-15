using RaceDay.DTOs;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EntriesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public EntriesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // ---------- helpers ----------

        private async Task<(HttpClient client, int userID)> RegisterUserAsync(
            string email, string role)
        {
            var client = _factory.CreateClient();

            var register = new RegisterRequestDto
            {
                Email = email,
                Password = "Test123!",
                FullName = "Test " + role,
                Role = role,
                Phone = "0821234567",
                DateOfBirth = role == "Participant" ? new DateTime(1995, 6, 15) : null
            };
            var reg = await client.PostAsJsonAsync("/api/v1/auth/register", register);
            var user = await reg.Content.ReadFromJsonAsync<RegisterResponseDto>();

            var login = new LoginRequestDto { Email = email, Password = "Test123!" };
            var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login", login);
            var loginBody = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.Token);

            return (client, user!.UserID);
        }

        private async Task<(HttpClient client, int participantID)> SetupParticipantAsync(string email)
        {
            var (client, userID) = await RegisterUserAsync(email, "Participant");

            var profile = new CreateParticipantRequestDto
            {
                UserID = userID,
                FirstName = "Test",
                LastName = "Runner",
                DateOfBirth = new DateTime(1995, 6, 15),
                Gender = "M",
                EmergencyContact = "0821111111"
            };
            var resp = await client.PostAsJsonAsync("/api/v1/participants", profile);
            var body = await resp.Content.ReadFromJsonAsync<ParticipantResponseDto>();

            return (client, body!.ParticipantID);
        }

        private async Task<(HttpClient client, int eventID)> SetupOrganiserWithEventAsync(string email)
        {
            var (client, userID) = await RegisterUserAsync(email, "Organiser");

            await client.PostAsJsonAsync("/api/v1/organisers", new CreateOrganiserRequestDto
            {
                UserID = userID,
                Name = "Test Org",
                Address = "Cape Town"
            });

            var eventResp = await client.PostAsJsonAsync("/api/v1/events", new CreateEventRequestDto
            {
                Name = "Test Race",
                EventDate = DateTime.Today.AddDays(30),
                Status = "Open",
                Location = "Cape Town"
            });
            var ev = await eventResp.Content.ReadFromJsonAsync<EventResponseDto>();

            return (client, ev!.EventID);
        }

        // ---------- GET /api/v1/participants/{id}/entries ----------

        [Fact]
        public async Task GetByParticipant_NonExistent_Returns404()
        {
            // Endpoint requires auth — authenticate first, then request a missing participant.
            var (client, _) = await SetupParticipantAsync("ent_getmiss@example.com");

            var response = await client.GetAsync("/api/v1/participants/999999/entries");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByParticipant_Self_ReturnsOk()
        {
            var (client, participantID) = await SetupParticipantAsync("ent_getself@example.com");

            var response = await client.GetAsync($"/api/v1/participants/{participantID}/entries");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByParticipant_OtherParticipant_Returns403()
        {
            var (_, otherParticipantID) = await SetupParticipantAsync("ent_other@example.com");
            var (client, _) = await SetupParticipantAsync("ent_attacker@example.com");

            var response = await client.GetAsync($"/api/v1/participants/{otherParticipantID}/entries");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- GET /api/v1/events/{id}/entries ----------

        [Fact]
        public async Task GetByEvent_NonExistent_Returns404()
        {
            // Endpoint requires Organiser role — authenticate first.
            var (client, _) = await SetupOrganiserWithEventAsync("ent_geteventmiss@example.com");

            var response = await client.GetAsync("/api/v1/events/999999/entries");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEvent_WithoutAuth_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/1/entries");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- POST /api/v1/entries ----------

        [Fact]
        public async Task Create_WithoutAuth_Returns401()
        {
            var (_, eventID) = await SetupOrganiserWithEventAsync("ent_noauth@example.com");
            var anon = _factory.CreateClient();

            var response = await anon.PostAsJsonAsync("/api/v1/entries", new CreateEntryRequestDto
            {
                ParticipantID = 1,
                EventID = eventID
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsParticipant_Returns201()
        {
            var (_, eventID) = await SetupOrganiserWithEventAsync("ent_org1@example.com");
            var (client, participantID) = await SetupParticipantAsync("ent_run1@example.com");

            var response = await client.PostAsJsonAsync("/api/v1/entries", new CreateEntryRequestDto
            {
                ParticipantID = participantID,
                EventID = eventID,
                Status = "Pending",
                Paid = 0
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<EntryResponseDto>();
            Assert.NotNull(body);
            Assert.Equal(participantID, body!.ParticipantID);
            Assert.Equal(eventID, body.EventID);
            Assert.True(body.EntryID > 0);
        }

        [Fact]
        public async Task Create_DuplicateEntry_Returns400()
        {
            var (_, eventID) = await SetupOrganiserWithEventAsync("ent_org2@example.com");
            var (client, participantID) = await SetupParticipantAsync("ent_dup@example.com");

            var dto = new CreateEntryRequestDto
            {
                ParticipantID = participantID,
                EventID = eventID,
                Status = "Pending"
            };

            var first = await client.PostAsJsonAsync("/api/v1/entries", dto);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var second = await client.PostAsJsonAsync("/api/v1/entries", dto);
            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task Create_OnClosedEvent_Returns400()
        {
            // Organiser creates a closed event
            var (orgClient, userID) = await RegisterUserAsync("ent_org3@example.com", "Organiser");
            await orgClient.PostAsJsonAsync("/api/v1/organisers", new CreateOrganiserRequestDto
            {
                UserID = userID,
                Name = "Org",
                Address = "CPT"
            });
            var eventResp = await orgClient.PostAsJsonAsync("/api/v1/events", new CreateEventRequestDto
            {
                Name = "Closed Race",
                EventDate = DateTime.Today.AddDays(30),
                Status = "Closed"
            });
            var ev = await eventResp.Content.ReadFromJsonAsync<EventResponseDto>();

            var (client, participantID) = await SetupParticipantAsync("ent_closed@example.com");

            var response = await client.PostAsJsonAsync("/api/v1/entries", new CreateEntryRequestDto
            {
                ParticipantID = participantID,
                EventID = ev!.EventID
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- DELETE /api/v1/entries/{id} ----------

        [Fact]
        public async Task Delete_OwnEntry_Returns204()
        {
            var (_, eventID) = await SetupOrganiserWithEventAsync("ent_org4@example.com");
            var (client, participantID) = await SetupParticipantAsync("ent_del@example.com");

            var createResp = await client.PostAsJsonAsync("/api/v1/entries", new CreateEntryRequestDto
            {
                ParticipantID = participantID,
                EventID = eventID
            });
            var entry = await createResp.Content.ReadFromJsonAsync<EntryResponseDto>();

            var response = await client.DeleteAsync($"/api/v1/entries/{entry!.EntryID}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistent_Returns404()
        {
            var (client, _) = await SetupParticipantAsync("ent_del404@example.com");

            var response = await client.DeleteAsync("/api/v1/entries/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}