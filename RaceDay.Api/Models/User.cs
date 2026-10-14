using RaceDay.Models;
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}