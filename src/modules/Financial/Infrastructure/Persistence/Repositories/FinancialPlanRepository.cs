using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

public class FinancialPlanRepository : IFinancialPlanRepository
{
    private readonly FinancialDbContext _context;

    public FinancialPlanRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.FinancialPlans.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<FinancialPlan>> GetByEnrollmentAsync(Guid enrollmentId, CancellationToken ct = default)
        => await _context.FinancialPlans
            .Where(x => x.EnrollmentId == enrollmentId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FinancialPlan>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.FinancialPlans.AsQueryable();

        if (activeOnly.HasValue)
            query = query.Where(x => x.IsActive == activeOnly.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FinancialPlan plan, CancellationToken ct = default)
    {
        await _context.FinancialPlans.AddAsync(plan, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(FinancialPlan plan, CancellationToken ct = default)
    {
        _context.FinancialPlans.Update(plan);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> HasPaidChargesAsync(Guid planId, CancellationToken ct = default)
    {
        // Will be implemented when MonthlyCharge entity exists
        return false;
    }
}
