using System.Security.Claims;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(ApplicationDbContext context, TokenService tokenService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly TokenService _tokenService = tokenService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.Email == model.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    return Unauthorized(new { Message = "Usuário ou senha inválidos." });
                }

                var token = _tokenService.GenerateToken(user);

                return Ok(new
                {
                    Message = "Login realizado com sucesso.",
                    UserId = user.Id,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Falha interna.", Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
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
                    City = model.City,
                    State = model.State
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateToken(user);

                return StatusCode(201, new
                {
                    Message = "Usuário registrado com sucesso.",
                    UserId = user.Id,
                    Role = user.Role.ToString(),
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Falha interna ao registrar usuário.", Error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
                var userId = int.Parse(userIdClaim?.Value ?? "0");

                var user = await _context.Users.FindAsync(userId);

                if (user == null) return Unauthorized(new { Message = "Usuário não encontrado." });

                return Ok(new 
                { 
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    AreaInteresse = user.AreaInteresse,
                    Bio = user.Bio,
                    IsAuthenticated = true
                });
            }
            catch (Exception)
            {
                return Unauthorized(new { Message = "Token inválido." });
            }
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            user.Name = model.Name;
            user.AreaInteresse = model.AreaInteresse;
            user.Bio = model.Bio;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Perfil atualizado com sucesso." });
        }
    }
}