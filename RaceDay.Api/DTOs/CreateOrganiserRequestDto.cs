using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateOrganiserRequestDto
    {
        [Required]
        public int UserID { get; set; }

        [StringLength(128)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
    }
}