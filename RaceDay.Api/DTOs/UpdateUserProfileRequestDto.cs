using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class UpdateUserProfileRequestDto
    {
        [StringLength(128)]
        public string? FullName { get; set; }

        [StringLength(32)]
        public string? Phone { get; set; }

        [StringLength(64)]
        public string? EmergencyContact { get; set; }
    }
}