using RaceDay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantID { get; set; }

        [Required]
        public int UserID { get; set; }

        [StringLength(64)]
        public string? FirstName { get; set; }

        [StringLength(64)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }   // "M" | "F"

        [StringLength(64)]
        public string? EmergencyContact { get; set; }

        // Navigation
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}