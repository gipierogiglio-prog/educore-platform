using System.Text;
using Educore.Identity.Api.Data;
using Educore.Identity.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Educore-SuperSecret-Key-2024!@#$%";

// Database
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connStr))
    builder.Services.AddDbContext<IdentityDbContext>(opt => opt.UseNpgsql(connStr));
else
    builder.Services.AddDbContext<IdentityDbContext>(opt => opt.UseSqlite("Data Source=identity.db"));

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Educore",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Educore-App",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-create DB + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var org = new Educore.Core.Entities.Organization
        {
            Name = "Escola Demo", Slug = "escola-demo", Status = "active"
        };
        db.Organizations.Add(org);
        db.Users.Add(new Educore.Core.Entities.User
        {
            Name = "Administrador", Email = "admin@escola.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "org_admin", OrganizationId = org.Id
        });
        db.SaveChanges();
    }

    if (!db.Permissions.Any())
    {
        var perms = new List<Educore.Core.Entities.Permission>
        {
            new() { Resource = "students", Action = "view", Name = "Visualizar Alunos" },
            new() { Resource = "students", Action = "create", Name = "Cadastrar Alunos" },
            new() { Resource = "students", Action = "edit", Name = "Editar Alunos" },
            new() { Resource = "students", Action = "delete", Name = "Excluir Alunos" },
            new() { Resource = "teachers", Action = "view", Name = "Visualizar Professores" },
            new() { Resource = "teachers", Action = "create", Name = "Cadastrar Professores" },
            new() { Resource = "teachers", Action = "edit", Name = "Editar Professores" },
            new() { Resource = "classes", Action = "view", Name = "Visualizar Turmas" },
            new() { Resource = "classes", Action = "create", Name = "Cadastrar Turmas" },
            new() { Resource = "classes", Action = "edit", Name = "Editar Turmas" },
            new() { Resource = "subjects", Action = "view", Name = "Visualizar Disciplinas" },
            new() { Resource = "subjects", Action = "create", Name = "Cadastrar Disciplinas" },
            new() { Resource = "subjects", Action = "edit", Name = "Editar Disciplinas" },
            new() { Resource = "grades", Action = "view", Name = "Visualizar Notas" },
            new() { Resource = "grades", Action = "create", Name = "Lançar Notas" },
            new() { Resource = "grades", Action = "edit", Name = "Editar Notas" },
            new() { Resource = "attendance", Action = "view", Name = "Visualizar Frequência" },
            new() { Resource = "attendance", Action = "create", Name = "Registrar Frequência" },
            new() { Resource = "reports", Action = "view", Name = "Visualizar Relatórios" },
            new() { Resource = "users", Action = "view", Name = "Visualizar Usuários" },
            new() { Resource = "users", Action = "create", Name = "Cadastrar Usuários" },
            new() { Resource = "permissions", Action = "manage", Name = "Gerenciar Permissões" },
            new() { Resource = "grading", Action = "manage", Name = "Gerenciar Regras de Notas" },
        };
        db.Permissions.AddRange(perms);
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
