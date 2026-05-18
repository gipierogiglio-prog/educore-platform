using Giglio.EduCore.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence;

public class FinancialDbContext : DbContext
{
    public FinancialDbContext(DbContextOptions<FinancialDbContext> options) : base(options) { }

    public DbSet<FinancialPlan> FinancialPlans => Set<FinancialPlan>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<MonthlyCharge> MonthlyCharges => Set<MonthlyCharge>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
