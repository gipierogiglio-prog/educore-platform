namespace Giglio.EduCore.Financial.Application.DTOs;

public record PeriodDto
{
    public DateTime Inicio { get; init; }
    public DateTime Fim { get; init; }
    public int DiasNoPeriodo { get; init; }
}
