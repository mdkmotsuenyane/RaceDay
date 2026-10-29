using RaceDay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        [StringLength(10)]
        public string? StartTime { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Open";

        [StringLength(255)]
        public string? StartPoint { get; set; }

        [StringLength(255)]
        public string? FinishPoint { get; set; }

        [StringLength(500)]
        public string? MapUrl { get; set; }

        [StringLength(500)]
        public string? ElevationUrl { get; set; }

        [StringLength(50)]
        public string? WeatherForecast { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? Temperature { get; set; }

        public int? WindSpeed { get; set; }

        public int? Precipitation { get; set; }

        // ADDED THIS PROPERTY
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // Navigation
        [ForeignKey(nameof(OrganiserID))]
        public Organiser? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
