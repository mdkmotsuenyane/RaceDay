using System;

namespace RaceDay.DTOs
{
    public class ParticipantResponseDto
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
    }
}