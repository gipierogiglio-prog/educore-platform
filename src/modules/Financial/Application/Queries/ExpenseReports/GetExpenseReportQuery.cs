using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Application.Queries.ExpenseReports;

public record GetExpenseReportQuery
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetExpenseReportHandler
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<GetExpenseReportHandler> _logger;

    public GetExpenseReportHandler(FinancialDbContext context, ILogger<GetExpenseReportHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExpenseReportResponse> Handle(
        GetExpenseReportQuery query, CancellationToken ct)
    {
        _logger.LogInformation(
            "Gerando relatório de despesas: período {Start:yyyy-MM-dd} a {End:yyyy-MM-dd}, " +
            "categoria {CategoryId}, status {Status}, página {Page}/{PageSize}",
            query.StartDate, query.EndDate,
            query.CategoryId?.ToString() ?? "todas",
            query.Status ?? "todos",
            query.Page, query.PageSize);

        // Execução paralela das 3 queries
        var metricsTask = GetMetricsAsync(query, ct);
        var categoryTask = GetCategoryGroupingAsync(query, ct);
        var detailTask = GetExpensesPagedAsync(query, ct);

        await Task.WhenAll(metricsTask, categoryTask, detailTask);

        return new ExpenseReportResponse
        {
            Periodo = new PeriodDto
            {
                Inicio = query.StartDate,
                Fim = query.EndDate,
                DiasNoPeriodo = (query.EndDate - query.StartDate).Days + 1
            },
            Metricas = metricsTask.Result,
            AgrupamentoCategorias = categoryTask.Result,
            Despesas = detailTask.Result
        };
    }

    private IQueryable<Domain.Entities.Expense> BuildBaseQuery(
        GetExpenseReportQuery query)
    {
        var q = _context.Set<Domain.Entities.Expense>()
            .AsNoTracking()
            .Where(e => e.DueDate >= query.StartDate)
            .Where(e => e.DueDate <= query.EndDate);

        if (query.CategoryId.HasValue)
            q = q.Where(e => e.CategoryId == query.CategoryId.Value);

        return q;
    }

    private async Task<ExpenseMetricsDto> GetMetricsAsync(
        GetExpenseReportQuery query, CancellationToken ct)
    {
        var baseQuery = BuildBaseQuery(query);

        var metrics = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new ExpenseMetricsDto
            {
                TotalPrevisto = g.Where(e => e.Status != ExpenseStatus.Cancelled).Sum(e => e.Value),
                TotalRealizado = g.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Value),
                TotalPendente = g.Where(e => e.Status == ExpenseStatus.Pending).Sum(e => e.Value),
                TotalPago = g.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Value),
                TotalAtrasado = g.Where(e => e.Status == ExpenseStatus.Overdue).Sum(e => e.Value),
                TotalCancelado = g.Where(e => e.Status == ExpenseStatus.Cancelled).Sum(e => e.Value),
                TotalDespesas = g.Count(),
                TotalPagas = g.Count(e => e.Status == ExpenseStatus.Paid),
                TotalAtrasadas = g.Count(e => e.Status == ExpenseStatus.Overdue)
            })
            .FirstOrDefaultAsync(ct) ?? new ExpenseMetricsDto();

        if (metrics.TotalPrevisto > 0)
        {
            metrics = metrics with
            {
                PercentualRealizado = Math.Round(metrics.TotalRealizado / metrics.TotalPrevisto * 100, 2),
                PercentualPendente = Math.Round(metrics.TotalPendente / metrics.TotalPrevisto * 100, 2),
                PercentualAtrasado = Math.Round(metrics.TotalAtrasado / metrics.TotalPrevisto * 100, 2)
            };
        }

        return metrics;
    }

    private async Task<List<CategoryGroupDto>> GetCategoryGroupingAsync(
        GetExpenseReportQuery query, CancellationToken ct)
    {
        var baseQuery = BuildBaseQuery(query);

        var totalPrevisto = await baseQuery
            .Where(e => e.Status != ExpenseStatus.Cancelled)
            .SumAsync(e => (decimal?)e.Value, ct) ?? 0;

        var groups = await baseQuery
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new CategoryGroupDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                ValorTotal = g.Sum(e => e.Value),
                ValorPago = g.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Value),
                ValorPendente = g.Where(e => e.Status == ExpenseStatus.Pending).Sum(e => e.Value),
                ValorAtrasado = g.Where(e => e.Status == ExpenseStatus.Overdue).Sum(e => e.Value),
                Quantidade = g.Count(),
                QuantidadePaga = g.Count(e => e.Status == ExpenseStatus.Paid),
                QuantidadePendente = g.Count(e => e.Status == ExpenseStatus.Pending),
                QuantidadeAtrasada = g.Count(e => e.Status == ExpenseStatus.Overdue),
                PercentualDoTotal = 0
            })
            .OrderByDescending(g => g.ValorTotal)
            .ToListAsync(ct);

        foreach (var group in groups)
        {
            group.PercentualDoTotal = totalPrevisto > 0
                ? Math.Round(group.ValorTotal / totalPrevisto * 100, 2)
                : 0;
        }

        return groups;
    }

    private async Task<PagedResult<ExpenseDetailDto>> GetExpensesPagedAsync(
        GetExpenseReportQuery query, CancellationToken ct)
    {
        var baseQuery = BuildBaseQuery(query)
            .Include(e => e.Category);

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<ExpenseStatus>(query.Status, ignoreCase: true, out var statusEnum))
        {
            baseQuery = baseQuery.Where(e => e.Status == statusEnum);
        }

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(e => e.DueDate)
            .ThenBy(e => e.Category.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new ExpenseDetailDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                Description = e.Description,
                ProviderName = e.ProviderName,
                Value = e.Value,
                DueDate = e.DueDate,
                PaymentDate = e.PaymentDate,
                Status = e.Status.ToString(),
                IsOverdue = e.Status == ExpenseStatus.Pending && e.DueDate < DateTime.UtcNow,
                DaysOverdue = e.Status == ExpenseStatus.Pending && e.DueDate < DateTime.UtcNow
                    ? (int)(DateTime.UtcNow.Date - e.DueDate.Date).TotalDays
                    : 0,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<ExpenseDetailDto>
        {
            Data = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }
}
