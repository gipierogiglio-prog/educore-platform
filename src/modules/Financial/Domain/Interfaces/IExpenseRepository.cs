using Giglio.EduCore.Financial.Domain.Entities;

namespace Giglio.EduCore.Financial.Domain.Interfaces;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task AddAsync(ExpenseCategory category, CancellationToken ct = default);
    Task UpdateAsync(ExpenseCategory category, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<bool> HasActiveExpensesAsync(Guid categoryId, CancellationToken ct = default);
}

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetAllAsync(Guid? categoryId = null, string? status = null,
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    Task AddAsync(Expense expense, CancellationToken ct = default);
    Task UpdateAsync(Expense expense, CancellationToken ct = default);
}
