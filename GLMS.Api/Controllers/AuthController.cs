using GLMS.Api.Data;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // the only endpoint you can call without a token
    public class AuthController : ControllerBase
    {
        private readonly GlmsDbContext _context;
        private readonly TokenService _tokens;

        public AuthController(GlmsDbContext context, TokenService tokens)
        {
            _context = context;
            _tokens = tokens;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null ||
                new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, dto.Password)
                    == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var (token, expiresAt) = _tokens.CreateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                ExpiresAt = expiresAt
            });
        }
    }
}