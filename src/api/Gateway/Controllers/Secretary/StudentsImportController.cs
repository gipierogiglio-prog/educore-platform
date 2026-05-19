using System.Globalization;
using System.Security.Claims;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Educore.Api.Controllers.Secretary;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsImportController : ControllerBase
{
    private readonly AppDbContext _db;
    public StudentsImportController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    /// <summary>
    /// Preview CSV/Excel before importing — returns parsed rows for client review
    /// </summary>
    [HttpPost("import/preview")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> Preview(IFormFile file)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Arquivo não enviado" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        try
        {
            var rows = extension switch
            {
                ".csv" => await ParseCsv(file.OpenReadStream()),
                ".xlsx" => await ParseExcel(file.OpenReadStream()),
                _ => throw new InvalidOperationException("Formato não suportado. Use .csv ou .xlsx")
            };

            // Check for existing emails
            var emails = rows.Where(r => !string.IsNullOrWhiteSpace(r.Email)).Select(r => r.Email).ToList();
            var existingEmails = await _db.Users
                .Where(u => emails.Contains(u.Email))
                .Select(u => u.Email)
                .ToListAsync();

            return Ok(new
            {
                totalRows = rows.Count,
                rows,
                existingEmails
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao processar arquivo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Confirm and execute the import
    /// </summary>
    [HttpPost("import/execute")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Execute(IFormFile file)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Arquivo não enviado" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        try
        {
            var rows = extension switch
            {
                ".csv" => await ParseCsv(file.OpenReadStream()),
                ".xlsx" => await ParseExcel(file.OpenReadStream()),
                _ => throw new InvalidOperationException("Formato não suportado. Use .csv ou .xlsx")
            };

            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Email))
                {
                    skipped++;
                    errors.Add($"Linha {imported + skipped}: Nome ou email vazio");
                    continue;
                }

                if (await _db.Users.AnyAsync(u => u.Email == row.Email))
                {
                    skipped++;
                    continue;
                }

                var password = row.Password ?? "123456";
                var user = new User
                {
                    Name = row.Name,
                    Email = row.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "student",
                    OrganizationId = oid,
                    Phone = row.Phone
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                var enrollment = $"STU{DateTime.UtcNow:yyyy}{await _db.Students.CountAsync(s => s.OrganizationId == oid) + 1:D4}";

                _db.Students.Add(new Student
                {
                    UserId = user.Id,
                    Enrollment = enrollment,
                    OrganizationId = oid.Value
                });
                imported++;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                imported,
                skipped,
                errors = errors.Take(20).ToList(),
                totalInFile = rows.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao importar: {ex.Message}" });
        }
    }

    private static async Task<List<StudentImportRow>> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        });

        csv.Context.RegisterClassMap<StudentImportRowMap>();

        var records = new List<StudentImportRow>();
        await foreach (var record in csv.GetRecordsAsync<StudentImportRow>())
        {
            records.Add(record);
        }

        return records;
    }

    private static async Task<List<StudentImportRow>> ParseExcel(Stream stream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension?.Rows ?? 0;

        var records = new List<StudentImportRow>();

        // Find column headers (row 1)
        var headers = new Dictionary<string, int>();
        for (int col = 1; col <= (worksheet.Dimension?.Columns ?? 0); col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(header))
                headers[header] = col;
        }

        for (int row = 2; row <= rowCount; row++)
        {
            var record = new StudentImportRow
            {
                Name = GetExcelValue(worksheet, row, headers, "nome"),
                Email = GetExcelValue(worksheet, row, headers, "email"),
                Password = GetExcelValue(worksheet, row, headers, "senha") ?? GetExcelValue(worksheet, row, headers, "password"),
                Phone = GetExcelValue(worksheet, row, headers, "telefone") ?? GetExcelValue(worksheet, row, headers, "phone")
            };

            if (!string.IsNullOrWhiteSpace(record.Name) || !string.IsNullOrWhiteSpace(record.Email))
                records.Add(record);
        }

        return await Task.FromResult(records);
    }

    private static string? GetExcelValue(OfficeOpenXml.ExcelWorksheet ws, int row, Dictionary<string, int> headers, string key)
    {
        if (headers.TryGetValue(key, out var col))
            return ws.Cells[row, col].Text?.Trim();
        return null;
    }
}

public class StudentImportRow
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Password { get; set; }
    public string? Phone { get; set; }
}

public sealed class StudentImportRowMap : ClassMap<StudentImportRow>
{
    public StudentImportRowMap()
    {
        Map(m => m.Name).Name("nome", "Nome", "name", "Name", "aluno", "Aluno", "student", "Student");
        Map(m => m.Email).Name("email", "Email", "e-mail", "E-mail", "E-Mail");
        Map(m => m.Password).Name("senha", "Senha", "password", "Password");
        Map(m => m.Phone).Name("telefone", "Telefone", "phone", "Phone", "celular", "Celular");
    }
}
