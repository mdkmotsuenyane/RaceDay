namespace RaceDay.DTOs
{
    public class ResultResponseDto
    {
        public int ResultID { get; set; }
        public int? EntryID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? Status { get; set; }
        public string? ChipTime { get; set; }
        public string? GunTime { get; set; }

        // NEW FIELDS FOR RUBRIC
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string? CategoryName { get; set; }
        public int TotalFinishers { get; set; }
    }
}
