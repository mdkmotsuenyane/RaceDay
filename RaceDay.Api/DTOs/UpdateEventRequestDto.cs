using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class UpdateEventRequestDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? EventDate { get; set; }

        [StringLength(10)]
        public string? StartTime { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

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

        public decimal? Temperature { get; set; }

        public int? WindSpeed { get; set; }

        public int? Precipitation { get; set; }
    }
}