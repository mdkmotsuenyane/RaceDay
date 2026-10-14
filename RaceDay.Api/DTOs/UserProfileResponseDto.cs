using System;

namespace RaceDay.DTOs
{
    public class UserProfileResponseDto
    {
        // Common to all users
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Participant-only fields
        public int? ParticipantID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }

        // Organiser-only fields
        public int? OrganiserID { get; set; }
        public string? OrganiserName { get; set; }
        public string? Address { get; set; }
    }
}