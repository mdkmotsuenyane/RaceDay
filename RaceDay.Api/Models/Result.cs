using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int? EntryID { get; set; }

        [StringLength(20)]
        public string? FinishTime { get; set; }

        public int? Position { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }   // "Finished" | "DNF" | "DNS" | "DSQ"

        [StringLength(20)]
        public string? ChipTime { get; set; }

        [StringLength(20)]
        public string? GunTime { get; set; }

        // Navigation
        [ForeignKey(nameof(EntryID))]
        public Entry? Entry { get; set; }
    }
}