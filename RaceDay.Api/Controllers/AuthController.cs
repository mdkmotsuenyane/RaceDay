using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ============================================================
        // POST /api/v1/auth/register
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Role != "Participant" && dto.Role != "Organiser")
                return BadRequest(new { message = "Role must be 'Participant' or 'Organiser'." });

            if (dto.Role == "Participant" && dto.DateOfBirth == null)
                return BadRequest(new { message = "dateOfBirth is required for Participants." });

            bool emailTaken = await _context.User.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
                return BadRequest(new { message = "Duplicate email." });

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = dto.Role,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var response = new RegisterResponseDto
            {
                UserID = user.UserId,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // ============================================================
        // POST /api/v1/auth/login
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // If the hash in the DB is invalid (e.g., seeded placeholder), treat it as a bad login
                return Unauthorized(new { message = "Invalid email or password. (Database hash is corrupted)" });
            }

            if (!isPasswordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user);

            return Ok(new LoginResponseDto
            {
                Token = accessToken,
                ExpiresIn = 86400,
                RefreshToken = refreshToken
            });
        }

        // ============================================================
        // POST /api/v1/auth/refresh
        // ============================================================
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return Unauthorized(new { message = "Invalid refresh token." });

            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromToken(dto.RefreshToken, validateLifetime: true);
            }
            catch
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            var tokenType = principal.FindFirst("tokenType")?.Value;
            if (tokenType != "refresh")
                return Unauthorized(new { message = "Invalid refresh token." });

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid refresh token." });

            var user = _context.User.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return Unauthorized(new { message = "Invalid refresh token." });

            var newAccess = GenerateAccessToken(user);
            var newRefresh = GenerateRefreshToken(user);

            return Ok(new LoginResponseDto
            {
                Token = newAccess,
                ExpiresIn = 86400,
                RefreshToken = newRefresh
            });
        }

        // ================= Helpers =================

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("fullName", user.FullName ?? string.Empty),
                new Claim("tokenType", "access")
            };

            return BuildToken(claims, DateTime.UtcNow.AddSeconds(86400));
        }

        private string GenerateRefreshToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim("tokenType", "refresh")
            };

            return BuildToken(claims, DateTime.UtcNow.AddDays(7));
        }

        private string BuildToken(IEnumerable<Claim> claims, DateTime expires)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, parameters, out _);
        }
    }
}
