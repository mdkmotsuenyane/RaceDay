namespace RaceDay.DTOs
{
    public class EntryWithParticipantDto
    {
        public int EntryID { get; set; }
        public int? ParticipantID { get; set; }
        public int? EventID { get; set; }
        public int? CategoryID { get; set; }
        public System.DateTime? RegistrationDate { get; set; }
        public string? Status { get; set; }
        public int? StartNumber { get; set; }
        public int? Paid { get; set; }

        // Flattened participant fields
        public string? ParticipantFirstName { get; set; }
        public string? ParticipantLastName { get; set; }
        public string? ParticipantEmail { get; set; }
        public string? ParticipantProfileImageUrl { get; set; }
        public string? CategoryName { get; set; }
    }
}
