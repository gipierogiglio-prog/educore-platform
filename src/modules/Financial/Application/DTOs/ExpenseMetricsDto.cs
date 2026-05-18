namespace Giglio.EduCore.Financial.Application.DTOs;

public record ExpenseMetricsDto
{
    public decimal TotalPrevisto { get; init; }
    public decimal TotalRealizado { get; init; }
    public decimal TotalPendente { get; init; }
    public decimal TotalPago { get; init; }
    public decimal TotalAtrasado { get; init; }
    public decimal TotalCancelado { get; init; }
    public decimal PercentualRealizado { get; init; }
    public decimal PercentualPendente { get; init; }
    public decimal PercentualAtrasado { get; init; }
    public int TotalDespesas { get; init; }
    public int TotalPagas { get; init; }
    public int TotalAtrasadas { get; init; }
}
