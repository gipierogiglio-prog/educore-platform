namespace Giglio.EduCore.Financial.Application.DTOs;

public record ExpenseReportResponse
{
    public PeriodDto Periodo { get; init; } = null!;
    public ExpenseMetricsDto Metricas { get; init; } = null!;
    public List<CategoryGroupDto> AgrupamentoCategorias { get; init; } = [];
    public PagedResult<ExpenseDetailDto> Despesas { get; init; } = null!;
}
