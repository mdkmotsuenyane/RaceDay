using RaceDay.Models; // You can remove this redundant using if you want
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(32)]
        public string Role { get; set; } = "Participant";

        [StringLength(128)]
        public string? FullName { get; set; }

        [StringLength(32)]
        public string? Phone { get; set; }

        // THIS IS THE NEW PROPERTY FOR THE AVATAR
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}
