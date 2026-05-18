using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly FinancialDbContext _context;

    public PaymentRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Payments
            .Include(x => x.MonthlyCharge)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Payment>> GetByChargeIdAsync(Guid monthlyChargeId, bool includeCancelled = false,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.Payments
            .Include(x => x.MonthlyCharge)
            .Where(x => x.MonthlyChargeId == monthlyChargeId)
            .AsQueryable();

        if (!includeCancelled)
            query = query.Where(x => !x.IsCancelled);

        return await query
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default)
        => await _context.Payments
            .CountAsync(x => x.MonthlyChargeId == monthlyChargeId, ct);

    public async Task<decimal> GetActiveTotalByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default)
        => await _context.Payments
            .Where(x => x.MonthlyChargeId == monthlyChargeId && !x.IsCancelled)
            .SumAsync(x => (decimal?)x.Value) ?? 0m;

    public async Task<bool> HasActivePaymentsByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default)
        => await _context.Payments
            .AnyAsync(x => x.MonthlyChargeId == monthlyChargeId && !x.IsCancelled, ct);

    public void Add(Payment payment)
    {
        _context.Payments.Add(payment);
    }
}
