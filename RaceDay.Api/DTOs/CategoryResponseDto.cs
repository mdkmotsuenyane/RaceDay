namespace RaceDay.DTOs
{
    public class CategoryResponseDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Gender { get; set; }
        public decimal? Distance { get; set; }
    }
}