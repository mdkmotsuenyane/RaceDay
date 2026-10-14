using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CreateCategoryRequestDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        public decimal? Distance { get; set; }
    }
}