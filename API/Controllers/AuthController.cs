using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(ApplicationDbContext context, TokenService tokenService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly TokenService _tokenService = tokenService;

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true, 
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest(new { Message = "O e-mail informado já está em uso." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = model.Role,
                AreaInteresse = model.AreaInteresse,
                InepCode = model.InepCode
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            SetTokenCookie(token);

            return StatusCode(201, new
            {
                Message = "Usuário registrado com sucesso.",
                UserId = user.Id,
                Role = user.Role.ToString()
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "Usuário ou senha inválidos." });
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Usuário ou senha inválidos." });
            }

            var token = _tokenService.GenerateToken(user);
            SetTokenCookie(token);

            return Ok(new
            {
                Message = "Login realizado com sucesso.",
                UserId = user.Id,
                Role = user.Role.ToString()
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok(new { Message = "Logout realizado com sucesso." });
        }
    }
}