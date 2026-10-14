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
        // Register a new user (Participant or Organiser). Public.
        // 201 Created | 400 Bad Request (duplicate email / validation)
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Role must be one of the two allowed values
            if (dto.Role != "Participant" && dto.Role != "Organiser")
                return BadRequest(new { message = "Role must be 'Participant' or 'Organiser'." });

            // Participant requires dateOfBirth (per API plan)
            if (dto.Role == "Participant" && dto.DateOfBirth == null)
                return BadRequest(new { message = "dateOfBirth is required for Participants." });

            // Duplicate email check (DbSet is named "User")
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

            // NOTE: dateOfBirth is not stored on User (per your model).
            // It belongs on the Participant profile, created via
            // POST /api/v1/participants with { userID, ..., dateOfBirth }.

            var response = new RegisterResponseDto
            {
                UserID = user.UserId,   // <-- note: UserId, not UserID
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // ============================================================
        // POST /api/v1/auth/login
        // Authenticate and receive JWT token. Public.
        // 200 OK | 401 Unauthorized
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user);

            return Ok(new LoginResponseDto
            {
                Token = accessToken,
                ExpiresIn = 86400,           // 24 hours, as per API plan
                RefreshToken = refreshToken
            });
        }

        // ============================================================
        // POST /api/v1/auth/refresh
        // Refresh an expired JWT. Public.
        // 200 OK | 401 Unauthorized (invalid refresh token)
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

            // Must be a refresh-type token
            var tokenType = principal.FindFirst("tokenType")?.Value;
            if (tokenType != "refresh")
                return Unauthorized(new { message = "Invalid refresh token." });

            // Rebuild the User from claims
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

            // Refresh tokens live longer (e.g. 7 days)
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