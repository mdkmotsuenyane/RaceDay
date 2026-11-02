using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Models
{
    public class CreateEventViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        [StringLength(10)]
        public string? StartTime { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        public string? StartPoint { get; set; }
        public string? FinishPoint { get; set; }

        // --- ADDED MAP AND ELEVATION URLS ---
        [Url(ErrorMessage = "Must be a valid URL (e.g., https://...)")]
        [Display(Name = "Google Maps URL")]
        public string? MapUrl { get; set; }

        [Url(ErrorMessage = "Must be a valid URL (e.g., https://...)")]
        [Display(Name = "Elevation Profile URL")]
        public string? ElevationUrl { get; set; }
        // ------------------------------------

        [StringLength(50)]
        [Display(Name = "Weather Forecast")]
        public string? WeatherForecast { get; set; }

        [Range(-50, 60, ErrorMessage = "Temperature must be realistic")]
        [Display(Name = "Temperature (°C)")]
        public decimal? Temperature { get; set; }

        [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200")]
        [Display(Name = "Wind Speed (km/h)")]
        public int? WindSpeed { get; set; }

        [Range(0, 500, ErrorMessage = "Precipitation must be between 0 and 500")]
        [Display(Name = "Precipitation (mm)")]
        public int? Precipitation { get; set; }

        [Display(Name = "Event Banner Image")]
        public IFormFile? ImageFile { get; set; }

        // --- DATE VALIDATION (Blocks past dates) ---
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventDate.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Event date cannot be in the past.",
                    new[] { nameof(EventDate) });
            }
        }
    }
}
