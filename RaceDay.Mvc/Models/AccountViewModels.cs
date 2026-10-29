using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Add this line

namespace RaceDay.Mvc.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Participant";

        public string? Phone { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }

    
    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        [Required, StringLength(128)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(32)]
        public string? Phone { get; set; }

        [Display(Name = "Emergency Contact")]
        public string? EmergencyContact { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfileImage { get; set; }
    }
}
