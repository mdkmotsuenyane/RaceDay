namespace RaceDay.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public int ExpiresIn { get; set; } = 86400;
        public string RefreshToken { get; set; } = null!;
    }
}