using System.Text;
using Educore.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Educore-SuperSecret-Key-2024!@#$%";

// Database unificado
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connStr))
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));
else
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=educore.db"));

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Educore",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Educore-App",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed inicial
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Organizations.Add(new Educore.Core.Entities.Organization
        {
            Name = "Escola Demo", Slug = "escola-demo", Status = "active"
        });
        db.SaveChanges();
        var org = db.Organizations.First();
        db.Users.Add(new Educore.Core.Entities.User
        {
            Name = "Administrador", Email = "admin@escola.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "org_admin", OrganizationId = org.Id
        });
        db.SaveChanges();
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
