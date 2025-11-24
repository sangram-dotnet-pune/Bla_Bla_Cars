using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Dto;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppUserDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppUserDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto req)
        {
            if (await _context.AppUsers.AnyAsync(x => x.Email == req.Email))
                return BadRequest("Email already exists");

            string hashed = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var user = new AppUser
            {
                FullName = req.FullName,
                PhoneNumber=req.PhoneNumber,
                Email = req.Email,
                Password = hashed
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registered successfully");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto req)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Email == req.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);

            return Ok(new { token, user.UserId, user.FullName });
        }

        private string GenerateJwtToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var claims = new[]
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("email", user.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public class UpdateProfileDto
        {
            public Guid UserId { get; set; }
            public string? FullName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto req)
        {
            
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.UserId == req.UserId);

            if (user == null)
                return NotFound("User not found");

            // Update fields only if provided
            if (!string.IsNullOrWhiteSpace(req.FullName))
                user.FullName = req.FullName;

            if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
                user.PhoneNumber = req.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(req.Email))
                user.Email = req.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully",
                user = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber
                }
            });
        }


        public class ChangePasswordDto
        {
            
            public Guid UserId { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto req)
        {
          

            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.UserId == req.UserId);

            if (user == null)
                return NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(req.OldPassword, user.Password))
                return BadRequest("Old password is incorrect");

            user.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

            await _context.SaveChangesAsync();

            return Ok("Password changed successfully");
        }



    }
}
