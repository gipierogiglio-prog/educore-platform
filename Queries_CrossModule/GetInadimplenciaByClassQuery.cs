using Educore.Core.Entities;
using Educore.Database;
using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Giglio.EduCore.Financial.Application.Queries;

/// <summary>Query handler para relatório de inadimplência por turma.</summary>
public sealed class GetInadimplenciaByClassQueryHandler
{
    private readonly AppDbContext _appDb;
    private readonly FinancialDbContext _financialDb;
    private readonly ILogger<GetInadimplenciaByClassQueryHandler> _logger;

    public GetInadimplenciaByClassQueryHandler(
        AppDbContext appDb,
        FinancialDbContext financialDb,
        ILogger<GetInadimplenciaByClassQueryHandler> logger)
    {
        _appDb = appDb;
        _financialDb = financialDb;
        _logger = logger;
    }

    /// <summary>
    /// Executa a query de inadimplência.
    /// </summary>
    /// <param name="classId">Id da turma (obrigatório).</param>
    /// <param name="month">Mês opcional (1-12).</param>
    /// <param name="year">Ano opcional.</param>
    /// <param name="page">Página (default 1).</param>
    /// <param name="pageSize">Itens por página (default 20, max 100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Relatório ou null se turma não encontrada.</returns>
    public async Task<InadimplenciaReportDto?> Handle(
        Guid classId,
        int? month = null,
        int? year = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // 1. Buscar turma
        var turma = await _appDb.Classes
            .Where(c => c.Id == classId)
            .Select(c => new TurmaInadimplenciaDto(c.Id, c.Name, c.Shift, c.Year))
            .FirstOrDefaultAsync(ct);

        if (turma is null) return null;

        // 2. Buscar matrículas ativas da turma + dados dos alunos
        var enrollments = await _appDb.Enrollments
            .Where(e => e.ClassId == classId && e.Status == "active")
            .Join(
                _appDb.Students,
                e => e.StudentId,
                s => s.Id,
                (e, s) => new { e.Id, StudentId = s.Id, s.Enrollment, s.UserId })
            .Join(
                _appDb.Users,
                x => x.UserId,
                u => u.Id,
                (x, u) => new { x.Id, x.StudentId, x.Enrollment, StudentName = u.Name })
            .ToListAsync(ct);

        if (enrollments.Count == 0)
        {
            return new InadimplenciaReportDto(
                turma, 0, 0, 0m, 0m, 0m,
                Array.Empty<AlunoInadimplenciaDto>(), 0, 0);
        }

        var enrollmentIds = enrollments.Select(e => e.Id).ToList();

        // 3. Buscar planos financeiros vinculados às matrículas
        var plans = await _financialDb.FinancialPlans
            .Where(fp => enrollmentIds.Contains(fp.EnrollmentId) && fp.IsActive)
            .Select(fp => new
            {
                fp.Id,
                fp.EnrollmentId,
                fp.BaseValue,
                fp.StartMonth,
                fp.StartYear,
                fp.EndMonth,
                fp.EndYear,
                MonthlyValue = fp.BaseValue -
                    (fp.DiscountType == Giglio.EduCore.Financial.Domain.Enums.DiscountType.Percentage
                        ? fp.BaseValue * (fp.DiscountPercent ?? 0) / 100m
                    : fp.DiscountType == Giglio.EduCore.Financial.Domain.Enums.DiscountType.Fixed
                        ? (fp.DiscountPercent ?? 0)
                    : 0m)
            })
            .ToListAsync(ct);

        var planIds = plans.Select(p => p.Id).ToList();
        var planByEnrollment = plans.ToDictionary(p => p.EnrollmentId);

        // 4. Buscar charges dos planos
        var chargesQuery = _financialDb.MonthlyCharges
            .Where(mc => planIds.Contains(mc.FinancialPlanId));

        if (month.HasValue)
            chargesQuery = chargesQuery.Where(mc => mc.ReferenceMonth == month.Value);
        if (year.HasValue)
            chargesQuery = chargesQuery.Where(mc => mc.ReferenceYear == year.Value);

        var charges = await chargesQuery
            .Select(mc => new
            {
                mc.Id,
                mc.FinancialPlanId,
                mc.ReferenceMonth,
                mc.ReferenceYear,
                mc.Value,
                mc.DueDate,
                mc.Status,
                mc.PaidAt,
                TotalPaid = mc.Payments
                    .Where(p => !p.IsCancelled)
                    .Sum(p => (decimal?)p.Value) ?? 0m
            })
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .ToListAsync(ct);

        var chargesByPlan = charges
            .GroupBy(c => c.FinancialPlanId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 5. Construir DTOs dos alunos
        var alunos = new List<AlunoInadimplenciaDto>();
        int totalInadimplentes = 0;
        decimal totalDevido = 0;
        decimal totalRecebido = 0;

        foreach (var enrollment in enrollments)
        {
            var planExists = planByEnrollment.TryGetValue(enrollment.Id, out var plan);
            string planoNome;
            decimal valorMensalidade;
            List<ChargeDetailDto> alunoCharges;

            if (!planExists || plan is null)
            {
                _logger.LogWarning("Matrícula {EnrollmentId} sem plano financeiro ativo.", enrollment.Id);
                planoNome = "Sem plano";
                valorMensalidade = 0;
                alunoCharges = new List<ChargeDetailDto>();
            }
            else
            {
                planoNome = $"Plano R$ {plan.MonthlyValue:F2}";
                valorMensalidade = plan.MonthlyValue;

                var planCharges = chargesByPlan.TryGetValue(plan.Id, out var pc) ? pc : new();
                alunoCharges = planCharges.Select(c => new ChargeDetailDto(
                    c.Id, c.ReferenceMonth, c.ReferenceYear, c.Value,
                    c.DueDate, c.Status.ToString(), c.PaidAt, c.TotalPaid > 0 ? c.TotalPaid : null
                )).ToList();
            }

            // Calcular métricas
            var chargesNaoPagas = alunoCharges
                .Where(c => c.Status is "Pending" or "Overdue")
                .ToList();

            var mesesEmAtraso = alunoCharges
                .Count(c => c.Status is "Pending" or "Overdue");

            var valorDevido = chargesNaoPagas.Sum(c => c.Valor);
            var valorPago = alunoCharges
                .Where(c => c.Status == "Paid")
                .Sum(c => c.ValorPago ?? c.Valor);

            // Saldo total (considerando também charge paga)
            totalDevido += valorDevido;
            totalRecebido += valorPago;

            string statusGeral = mesesEmAtraso > 0 ? "Inadimplente" : "Regular";
            if (!planExists || plan is null)
                statusGeral = "Sem plano financeiro";

            if (mesesEmAtraso > 0) totalInadimplentes++;

            alunos.Add(new AlunoInadimplenciaDto(
                enrollment.StudentId,
                enrollment.StudentName,
                enrollment.Enrollment,
                planoNome,
                valorMensalidade,
                statusGeral,
                mesesEmAtraso,
                valorDevido,
                valorPago,
                alunoCharges));
        }

        // 6. Paginação
        int totalAlunos = alunos.Count;
        int totalPages = (int)Math.Ceiling(totalAlunos / (double)pageSize);
        var alunosPaginados = alunos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        decimal percentual = totalAlunos > 0
            ? Math.Round((decimal)totalInadimplentes / totalAlunos * 100, 1)
            : 0;

        _logger.LogInformation(
            "Relatório inadimplência turma {ClassId}: {Total} alunos, {Inadimplentes} inadimplentes ({Percentual}%)",
            classId, totalAlunos, totalInadimplentes, percentual);

        return new InadimplenciaReportDto(
            turma,
            totalAlunos,
            totalInadimplentes,
            percentual,
            totalDevido,
            totalRecebido,
            alunosPaginados,
            totalPages,
            page);
    }
}
