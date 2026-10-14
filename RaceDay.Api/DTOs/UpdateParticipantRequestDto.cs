using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class UpdateParticipantRequestDto
    {
        [StringLength(64)]
        public string? FirstName { get; set; }

        [StringLength(64)]
        public string? LastName { get; set; }

        [StringLength(64)]
        public string? EmergencyContact { get; set; }
    }
}