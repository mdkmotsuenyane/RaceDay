using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class UpdateEntryRequestDto
    {
        [StringLength(20)]
        public string? Status { get; set; }

        public int? StartNumber { get; set; }

        public int? Paid { get; set; }
    }
}