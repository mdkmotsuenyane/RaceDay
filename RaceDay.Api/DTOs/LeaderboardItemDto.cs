namespace RaceDay.DTOs
{
    public class LeaderboardItemDto
    {
        public int ResultID { get; set; }
        public int? EntryID { get; set; }
        public int? ParticipantID { get; set; }
        public string? ParticipantName { get; set; }
        public string? CategoryName { get; set; }
        public int? Position { get; set; }
        public string? FinishTime { get; set; }
        public string? ChipTime { get; set; }
        public string? GunTime { get; set; }
        public string? Status { get; set; }
    }
}