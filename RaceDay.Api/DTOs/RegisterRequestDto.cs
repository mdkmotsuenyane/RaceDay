namespace RaceDay.DTOs
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!; 
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!; 
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; } 
    }
}