using Giglio.EduCore.Financial.Application.Queries.ExpenseReports;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/reports")]
public class ReportsController : ControllerBase
{
    private readonly GetExpenseReportHandler _reportHandler;
    private readonly GetExpenseReportQueryValidator _validator;
    private readonly IExpenseCategoryRepository _categoryRepo;

    public ReportsController(
        GetExpenseReportHandler reportHandler,
        GetExpenseReportQueryValidator validator,
        IExpenseCategoryRepository categoryRepo)
    {
        _reportHandler = reportHandler;
        _validator = validator;
        _categoryRepo = categoryRepo;
    }

    /// <summary>
    /// Relatório de contas a pagar com métricas, agrupamento por categoria e detalhamento paginado.
    /// </summary>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(Application.DTOs.ExpenseReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpenseReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetExpenseReportQuery
        {
            StartDate = startDate ?? DateTime.MinValue,
            EndDate = endDate ?? DateTime.MinValue,
            CategoryId = categoryId,
            Status = status,
            Page = page,
            PageSize = pageSize
        };

        var validationResult = _validator.Validate(query);
        if (!validationResult.IsValid)
            return BadRequest(new
            {
                type = "https://httpstatuses.com/400",
                title = "Validation Error",
                status = 400,
                errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });

        // Validar se a categoria existe (se fornecida)
        if (categoryId.HasValue)
        {
            var categoryExists = await _categoryRepo.GetByIdAsync(categoryId.Value, ct);
            if (categoryExists is null)
                return BadRequest(new
                {
                    type = "https://httpstatuses.com/400",
                    title = "Validation Error",
                    status = 400,
                    errors = new Dictionary<string, string[]>
                    {
                        ["categoryId"] = new[] { "Categoria não encontrada." }
                    }
                });
        }

        var report = await _reportHandler.Handle(query, ct);
        return Ok(report);
    }
}
