using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateResultRequestDto
    {
        [StringLength(20)]
        public string? FinishTime { get; set; }

        public int? Position { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }   // "Finished" | "DNF" | "DNS" | "DSQ"

        [StringLength(20)]
        public string? ChipTime { get; set; }

        [StringLength(20)]
        public string? GunTime { get; set; }
    }
}