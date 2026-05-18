using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

public class MonthlyChargeRepository : IMonthlyChargeRepository
{
    private readonly FinancialDbContext _context;

    public MonthlyChargeRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyCharge?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.MonthlyCharges
            .Include(x => x.FinancialPlan)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid financialPlanId, int month, int year, CancellationToken ct = default)
        => await _context.MonthlyCharges.AnyAsync(
            x => x.FinancialPlanId == financialPlanId
              && x.ReferenceMonth == month
              && x.ReferenceYear == year, ct);

    public async Task<List<MonthlyCharge>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
        => await _context.MonthlyCharges
            .Where(x => x.FinancialPlanId == planId)
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .ToListAsync(ct);

    public async Task<List<MonthlyCharge>> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken ct = default)
        => await _context.MonthlyCharges
            .Include(x => x.FinancialPlan)
            .Where(x => x.FinancialPlan.EnrollmentId == enrollmentId)
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .ToListAsync(ct);

    public async Task<List<MonthlyCharge>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        => await _context.MonthlyCharges
            .Include(x => x.FinancialPlan)
            .Where(x => x.FinancialPlan.EnrollmentId == studentId) // EnrollmentId = StudentId
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .ToListAsync(ct);

    public async Task<List<MonthlyCharge>> GetOverdueCandidatesAsync(CancellationToken ct = default)
        => await _context.MonthlyCharges
            .Where(x => x.Status == ChargeStatus.Pending && x.DueDate < DateTime.UtcNow)
            .OrderBy(x => x.DueDate)
            .ToListAsync(ct);

    public async Task<List<MonthlyCharge>> GetFilteredAsync(
        Guid? studentId = null,
        Guid? planId = null,
        int? startMonth = null,
        int? startYear = null,
        int? endMonth = null,
        int? endYear = null,
        List<string>? statusFilter = null,
        int page = 1,
        int pageSize = 12,
        CancellationToken ct = default)
    {
        var query = BuildFilterQuery(studentId, planId, startMonth, startYear, endMonth, endYear, statusFilter);
        return await query
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetFilteredCountAsync(
        Guid? studentId = null,
        Guid? planId = null,
        int? startMonth = null,
        int? startYear = null,
        int? endMonth = null,
        int? endYear = null,
        List<string>? statusFilter = null,
        CancellationToken ct = default)
    {
        var query = BuildFilterQuery(studentId, planId, startMonth, startYear, endMonth, endYear, statusFilter);
        return await query.CountAsync(ct);
    }

    private IQueryable<MonthlyCharge> BuildFilterQuery(
        Guid? studentId,
        Guid? planId,
        int? startMonth,
        int? startYear,
        int? endMonth,
        int? endYear,
        List<string>? statusFilter)
    {
        var query = _context.MonthlyCharges
            .Include(x => x.FinancialPlan)
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(x => x.FinancialPlan.EnrollmentId == studentId.Value);

        if (planId.HasValue)
            query = query.Where(x => x.FinancialPlanId == planId.Value);

        if (startMonth.HasValue && startYear.HasValue)
        {
            var startDate = new DateTime(startYear.Value, startMonth.Value, 1);
            query = query.Where(x =>
                x.ReferenceYear > startYear.Value ||
                (x.ReferenceYear == startYear.Value && x.ReferenceMonth >= startMonth.Value));
        }

        if (endMonth.HasValue && endYear.HasValue)
        {
            query = query.Where(x =>
                x.ReferenceYear < endYear.Value ||
                (x.ReferenceYear == endYear.Value && x.ReferenceMonth <= endMonth.Value));
        }

        if (statusFilter != null && statusFilter.Count > 0)
        {
            var statusValues = statusFilter
                .Select(s => Enum.TryParse<ChargeStatus>(s, true, out var st) ? st : (ChargeStatus?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();

            if (statusValues.Count > 0)
                query = query.Where(x => statusValues.Contains(x.Status));
        }

        return query;
    }

    public void Add(MonthlyCharge charge)
    {
        _context.MonthlyCharges.Add(charge);
    }

    public void AddRange(IEnumerable<MonthlyCharge> charges)
    {
        _context.MonthlyCharges.AddRange(charges);
    }

    public void Update(MonthlyCharge charge)
    {
        _context.MonthlyCharges.Update(charge);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
