using RaceDay.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [StringLength(128)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        // Navigation
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}