using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateParticipantRequestDto
    {
        [Required]
        public int UserID { get; set; }

        [StringLength(64)]
        public string? FirstName { get; set; }

        [StringLength(64)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }   // "M" | "F"

        [StringLength(64)]
        public string? EmergencyContact { get; set; }
    }
}