using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateEntryRequestDto
    {
        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int EventID { get; set; }

        public int? CategoryID { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }   // defaults to "Pending"

        public int? Paid { get; set; }        // defaults to 0
    }
}