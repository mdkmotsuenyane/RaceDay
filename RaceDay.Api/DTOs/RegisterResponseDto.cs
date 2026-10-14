namespace RaceDay.DTOs
{
    public class RegisterResponseDto
    {
        public int UserID { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}