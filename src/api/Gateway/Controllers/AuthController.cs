using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email ou senha inválidos" });
        if (!user.Active) return Unauthorized(new { message = "Usuário inativo" });
        return Ok(GenerateToken(user));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email já cadastrado" });
        var user = new User
        {
            Name = req.Name, Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role.ToLower(), OrganizationId = req.OrganizationId,
        };
        _db.Users.Add(user); await _db.SaveChangesAsync();
        return Ok(GenerateToken(user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return Unauthorized();
        var user = await _db.Users.FindAsync(Guid.Parse(uid));
        if (user == null) return NotFound();
        return Ok(new { user.Id, user.Name, user.Email, user.Role, user.OrganizationId, user.Active });
    }

    private object GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? "Educore-SuperSecret-Key-2024!@#$%"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(7);
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "Educore",
            audience: _config["Jwt:Audience"] ?? "Educore-App",
            claims: new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("organizationId", user.OrganizationId?.ToString() ?? ""),
            },
            expires: expires, signingCredentials: creds
        ));
        return new { token, user.Name, user.Email, user.Role, user.OrganizationId, expires };
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Name, string Email, string Password, string Role, Guid? OrganizationId);
