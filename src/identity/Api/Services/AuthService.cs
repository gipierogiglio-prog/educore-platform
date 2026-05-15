using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Educore.Core.Entities;
using Educore.Identity.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Educore.Identity.Api.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string name, string email, string password, string role, Guid? organizationId);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> DeactivateOrganizationAsync(Guid orgId);
    Task<bool> ReactivateOrganizationAsync(Guid orgId);
}

public record AuthResult(string Token, string Name, string Email, string Role, Guid? OrganizationId, DateTime ExpiresAt);

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(IdentityDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        if (!user.Active)
            throw new UnauthorizedAccessException("Usuário inativo");

        return GenerateToken(user);
    }

    public async Task<AuthResult> RegisterAsync(string name, string email, string password, string role, Guid? organizationId)
    {
        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Email já cadastrado");

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role.ToLower(),
            OrganizationId = organizationId,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return GenerateToken(user);
    }

    public async Task<User?> GetByIdAsync(Guid id) => await _db.Users.FindAsync(id);

    public async Task<bool> DeactivateOrganizationAsync(Guid orgId)
    {
        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) return false;

        org.Status = "inactive";
        var users = await _db.Users.Where(u => u.OrganizationId == orgId && u.Active).ToListAsync();
        foreach (var u in users)
        {
            u.Active = false;
            u.AutoDeactivated = true;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateOrganizationAsync(Guid orgId)
    {
        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) return false;

        org.Status = "active";
        var users = await _db.Users.Where(u => u.OrganizationId == orgId && u.AutoDeactivated).ToListAsync();
        foreach (var u in users)
        {
            u.Active = true;
            u.AutoDeactivated = false;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    private AuthResult GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "Educore-SuperSecret-Key-2024!@#$%";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("organizationId", user.OrganizationId?.ToString() ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "Educore",
            audience: _config["Jwt:Audience"] ?? "Educore-App",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Name, user.Email, user.Role,
            user.OrganizationId, expires
        );
    }
}
