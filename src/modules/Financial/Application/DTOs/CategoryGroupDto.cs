namespace Giglio.EduCore.Financial.Application.DTOs;

public record CategoryGroupDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public decimal ValorPago { get; init; }
    public decimal ValorPendente { get; init; }
    public decimal ValorAtrasado { get; init; }
    public int Quantidade { get; init; }
    public int QuantidadePaga { get; init; }
    public int QuantidadePendente { get; init; }
    public int QuantidadeAtrasada { get; init; }
    public decimal PercentualDoTotal { get; set; }
}
