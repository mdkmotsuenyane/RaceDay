using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Models
{
    public class CreateEventViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public string? StartTime { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
        public string? StartPoint { get; set; }
        public string? FinishPoint { get; set; }

        [Display(Name = "Event Banner Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
