using System;
using System.Collections.Generic;

namespace RaceDay.DTOs
{
    public class EventDetailsDto
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string? OrganiserName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string? StartTime { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? StartPoint { get; set; }
        public string? FinishPoint { get; set; }
        public string? MapUrl { get; set; }
        public string? ElevationUrl { get; set; }
        public string? WeatherForecast { get; set; }
        public decimal? Temperature { get; set; }
        public int? WindSpeed { get; set; }
        public int? Precipitation { get; set; }

        // ADDED THIS LINE:
        public string? ImageUrl { get; set; }

        public List<CategoryResponseDto> Categories { get; set; } = new();
    }
}
