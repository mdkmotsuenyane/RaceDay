using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateEventRequestDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
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
    }
}