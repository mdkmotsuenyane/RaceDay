namespace RaceDay.DTOs
{
    public class EventResponseDto
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public System.DateTime EventDate { get; set; }
        public string? StartTime { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
    }
}
