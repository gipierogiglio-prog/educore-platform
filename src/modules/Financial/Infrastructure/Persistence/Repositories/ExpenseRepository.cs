using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly FinancialDbContext _context;

    public ExpenseCategoryRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.ExpenseCategories.AsQueryable();
        if (activeOnly.HasValue)
            query = query.Where(x => x.IsActive == activeOnly.Value);
        return await query.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public async Task AddAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        await _context.ExpenseCategories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _context.ExpenseCategories.Update(category);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _context.ExpenseCategories.AnyAsync(x => x.Name == name, ct);

    public async Task<bool> HasActiveExpensesAsync(Guid categoryId, CancellationToken ct = default)
        => await _context.Expenses.AnyAsync(x => x.CategoryId == categoryId
            && (x.Status == Domain.Enums.ExpenseStatus.Pending
                || x.Status == Domain.Enums.ExpenseStatus.Overdue), ct);
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly FinancialDbContext _context;

    public ExpenseRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Expenses
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Expense>> GetAllAsync(Guid? categoryId = null, string? status = null,
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
    {
        var query = _context.Expenses
            .Include(x => x.Category)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.ExpenseStatus>(status, true, out var statusEnum))
            query = query.Where(x => x.Status == statusEnum);

        if (startDate.HasValue)
            query = query.Where(x => x.DueDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.DueDate <= endDate.Value);

        return await query.OrderByDescending(x => x.DueDate).ToListAsync(ct);
    }

    public async Task AddAsync(Expense expense, CancellationToken ct = default)
    {
        await _context.Expenses.AddAsync(expense, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Expense expense, CancellationToken ct = default)
    {
        _context.Expenses.Update(expense);
        await _context.SaveChangesAsync(ct);
    }
}
