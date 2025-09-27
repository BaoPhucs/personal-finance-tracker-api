using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTrackerAPI.Interfaces;
using PersonalFinanceTrackerAPI.Models;
using PersonalFinanceTrackerAPI.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Text;

namespace PersonalFinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _configuration;

        public AuthController(IUserRepository userRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingByUsername = await _userRepo.GetByUsernameAsync(dto.Username);
            if (existingByUsername != null) return BadRequest("Username already exists");

            var existingByEmail = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingByEmail != null) return BadRequest("Email already exists");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(dto.Passwordhash)
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            var userDto = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, CreatedAt = user.Createdat.Value };
            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            User? user = null;
            // allow username or email
            user = await _userRepo.GetByUsernameAsync(dto.UsernameOrEmail) ?? await _userRepo.GetByEmailAsync(dto.UsernameOrEmail);

            if (user == null) return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Passwordhash, user.Passwordhash))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, CreatedAt = user.Createdat.Value } });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "PFT";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "PFT_Audience";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
