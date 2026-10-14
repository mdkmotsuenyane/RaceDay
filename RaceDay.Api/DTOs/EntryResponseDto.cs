using System;

namespace RaceDay.DTOs
{
    public class EntryResponseDto
    {
        public int EntryID { get; set; }
        public int? ParticipantID { get; set; }
        public int? EventID { get; set; }
        public int? CategoryID { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? Status { get; set; }
        public int? StartNumber { get; set; }
        public int? Paid { get; set; }
    }
}