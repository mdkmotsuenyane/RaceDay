using RaceDay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Entry
    {
        [Key]
        public int EntryID { get; set; }

        public int? ParticipantID { get; set; }

        public int? EventId { get; set; }

        public int? CategoryID { get; set; }

        public DateTime? RegistrationDate { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string? Status { get; set; } = "Pending";

        public int? StartNumber { get; set; }

        public int? Paid { get; set; } = 0;

        // Navigation
        [ForeignKey(nameof(ParticipantID))]
        public Participant? Participant { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}