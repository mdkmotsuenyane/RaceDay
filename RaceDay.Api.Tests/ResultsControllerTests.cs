using RaceDay.DTOs;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class ResultsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public ResultsControllerTests(TestWebApplicationFactory factory)
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

        /// <summary>
        /// Full setup: organiser with event, participant with entry.
        /// Returns an organiser-authenticated client and the entry ID.
        /// </summary>
        private async Task<(HttpClient orgClient, int entryID, int eventID)> SetupEntryAsync(
            string orgEmail, string participantEmail)
        {
            var (orgClient, eventID) = await SetupOrganiserWithEventAsync(orgEmail);
            var (participantClient, participantID) = await SetupParticipantAsync(participantEmail);

            var entryResp = await participantClient.PostAsJsonAsync("/api/v1/entries",
                new CreateEntryRequestDto
                {
                    ParticipantID = participantID,
                    EventID = eventID,
                    Status = "Confirmed"
                });
            var entry = await entryResp.Content.ReadFromJsonAsync<EntryResponseDto>();

            return (orgClient, entry!.EntryID, eventID);
        }

        private static CreateResultRequestDto SampleResult() => new()
        {
            FinishTime = "01:23:45",
            Position = 1,
            Status = "Finished",
            ChipTime = "01:23:30",
            GunTime = "01:23:45"
        };

        // ---------- GET /api/v1/events/{eventId}/results (public leaderboard) ----------

        [Fact]
        public async Task GetByEvent_NonExistent_Returns404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/events/999999/results");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEvent_ExistingEvent_ReturnsOk()
        {
            var (_, eventID) = await SetupOrganiserWithEventAsync("res_list@example.com");
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/v1/events/{eventID}/results");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- POST /api/v1/entries/{entryId}/result ----------

        [Fact]
        public async Task Create_WithoutAuth_Returns401()
        {
            var (_, entryID, _) = await SetupEntryAsync("res_noauth@example.com", "res_p_noauth@example.com");
            var anon = _factory.CreateClient();

            var response = await anon.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result",
                SampleResult());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsParticipant_Returns403()
        {
            var (_, entryID, _) = await SetupEntryAsync("res_orgp@example.com", "res_p_403@example.com");
            var (participantClient, _) = await SetupParticipantAsync("res_otherp@example.com");

            var response = await participantClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result",
                SampleResult());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsOwnerOrganiser_Returns201()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org1@example.com", "res_p1@example.com");

            var response = await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result",
                SampleResult());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<ResultResponseDto>();
            Assert.NotNull(body);
            Assert.Equal(entryID, body!.EntryID);
            Assert.Equal(1, body.Position);
            Assert.Equal("01:23:45", body.FinishTime);
            Assert.Equal("Finished", body.Status);
            Assert.True(body.ResultID > 0);
        }

        [Fact]
        public async Task Create_OnNonExistentEntry_Returns404()
        {
            var (orgClient, _) = await SetupOrganiserWithEventAsync("res_org404@example.com");

            var response = await orgClient.PostAsJsonAsync(
                "/api/v1/entries/999999/result",
                SampleResult());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_DuplicateResult_Returns409()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org2@example.com", "res_p2@example.com");

            var first = await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result", SampleResult());
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var second = await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result", SampleResult());
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        // ---------- GET /api/v1/entries/{entryId}/result ----------

        [Fact]
        public async Task GetByEntry_WhenNoResult_Returns404()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org3@example.com", "res_p3@example.com");

            var response = await orgClient.GetAsync($"/api/v1/entries/{entryID}/result");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEntry_AfterPosting_ReturnsOk()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org4@example.com", "res_p4@example.com");

            await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result", SampleResult());

            var response = await orgClient.GetAsync($"/api/v1/entries/{entryID}/result");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<ResultResponseDto>();
            Assert.NotNull(body);
            Assert.Equal(1, body!.Position);
        }

        // ---------- PUT /api/v1/results/{id} ----------

        [Fact]
        public async Task Update_AsOwner_ReturnsOk()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org5@example.com", "res_p5@example.com");

            var createResp = await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result", SampleResult());
            var created = await createResp.Content.ReadFromJsonAsync<ResultResponseDto>();

            var update = new UpdateResultRequestDto
            {
                Position = 5,
                FinishTime = "01:30:00"
            };
            var response = await orgClient.PutAsJsonAsync(
                $"/api/v1/results/{created!.ResultID}", update);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<ResultResponseDto>();
            Assert.NotNull(body);
            Assert.Equal(5, body!.Position);
            Assert.Equal("01:30:00", body.FinishTime);
        }

        // ---------- DELETE /api/v1/results/{id} ----------

        [Fact]
        public async Task Delete_AsOwner_Returns204()
        {
            var (orgClient, entryID, _) = await SetupEntryAsync(
                "res_org6@example.com", "res_p6@example.com");

            var createResp = await orgClient.PostAsJsonAsync(
                $"/api/v1/entries/{entryID}/result", SampleResult());
            var created = await createResp.Content.ReadFromJsonAsync<ResultResponseDto>();

            var response = await orgClient.DeleteAsync($"/api/v1/results/{created!.ResultID}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistent_Returns404()
        {
            var (orgClient, _) = await SetupOrganiserWithEventAsync("res_org404del@example.com");

            var response = await orgClient.DeleteAsync("/api/v1/results/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}