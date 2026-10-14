using RaceDay.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }   // "M" | "F" | null for open

        [Column(TypeName = "decimal(6,2)")]
        public decimal? Distance { get; set; }

        // Navigation
        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }

        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}