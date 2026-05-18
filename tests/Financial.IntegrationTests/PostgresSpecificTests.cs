using Xunit;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Financial.IntegrationTests.Postgres;

/// <summary>
/// Critical tests that MUST run on PostgreSQL.
/// Tests scenarios where SQLite behavior differs from PostgreSQL.
/// </summary>
[Trait("Database", "PostgreSQL")]
public class PostgresSpecificTests : PostgresTestBase
{
    #region Decimal Precision

    [Fact]
    public async Task DecimalPrecision_ShouldMaintainScale()
    {
        // SQLite ignores HasPrecision(18,2) — PostgreSQL respects it
        var planRepo = new FinancialPlanRepository(DbContext);

        var plan = new FinancialPlan(Guid.NewGuid(), 1234.56m, 15, 1, 2026);
        await planRepo.AddAsync(plan);

        var saved = await planRepo.GetByIdAsync(plan.Id);
        Assert.Equal(1234.56m, saved!.BaseValue);
    }

    [Fact]
    public async Task DecimalRounding_ShouldBeExact()
    {
        var planRepo = new FinancialPlanRepository(DbContext);

        var plan = new FinancialPlan(Guid.NewGuid(), 10.00m, 15, 1, 2026, 33, DiscountType.Percentage);
        await planRepo.AddAsync(plan);

        // 10 - 33% = 6.70
        var monthlyValue = plan.CalculateMonthlyValue();
        Assert.Equal(6.70m, monthlyValue);
    }

    #endregion

    #region Unique Constraints (Case Sensitivity)

    [Fact]
    public async Task UniqueCategoryName_ShouldBeCaseSensitive()
    {
        // In PostgreSQL, "Água" and "água" are DIFFERENT
        // In SQLite, they would be the SAME
        var catRepo = new ExpenseCategoryRepository(DbContext);

        var cat1 = new ExpenseCategory("Água");
        await catRepo.AddAsync(cat1);

        var cat2 = new ExpenseCategory("água"); // Different case
        await catRepo.AddAsync(cat2); // Should NOT throw

        var all = await catRepo.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task UniqueCategoryName_ShouldRejectExactDuplicate()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);

        var cat1 = new ExpenseCategory("Internet");
        await catRepo.AddAsync(cat1);

        var cat2 = new ExpenseCategory("Internet"); // Exact same
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => catRepo.AddAsync(cat2));
        Assert.Contains("unique", ex.InnerException?.Message?.ToLowerInvariant() ?? "");
    }

    #endregion

    #region Concurrency (RowVersion Simulation)

    [Fact]
    public async Task ConcurrentPayment_ShouldBeConsistent()
    {
        // SQLite serializes writes - this test validates that
        // the repository pattern handles concurrent payments correctly
        var planRepo = new FinancialPlanRepository(DbContext);
        var plan = new FinancialPlan(Guid.NewGuid(), 500, 10, 1, 2026);
        await planRepo.AddAsync(plan);

        var chargeRepo = new MonthlyChargeRepository(DbContext);
        var charge = new MonthlyCharge(plan.Id, 5, 2026, 500, new DateTime(2026, 5, 10));
        chargeRepo.Add(charge);
        await DbContext.SaveChangesAsync();

        var paymentRepo = new PaymentRepository(DbContext);

        // Simulate two concurrent payments
        var task1 = Task.Run(async () =>
        {
            using var ctx2 = new FinancialDbContext(
                new DbContextOptionsBuilder<FinancialDbContext>()
                    .UseNpgsql(ConnectionString)
                    .Options);
            await ctx2.Database.EnsureCreatedAsync();
            var repo = new PaymentRepository(ctx2);
            repo.Add(new Payment(charge.Id, 250, DateTime.UtcNow, PaymentMethod.Pix, null,
                Guid.NewGuid(), "User1"));
            await ctx2.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            using var ctx3 = new FinancialDbContext(
                new DbContextOptionsBuilder<FinancialDbContext>()
                    .UseNpgsql(ConnectionString)
                    .Options);
            await ctx3.Database.EnsureCreatedAsync();
            var repo = new PaymentRepository(ctx3);
            repo.Add(new Payment(charge.Id, 250, DateTime.UtcNow, PaymentMethod.Pix, null,
                Guid.NewGuid(), "User2"));
            await ctx3.SaveChangesAsync();
        });

        await Task.WhenAll(task1, task2);

        var total = await paymentRepo.GetActiveTotalByChargeIdAsync(charge.Id);
        Assert.Equal(500.00m, total);
    }

    #endregion

    #region Date Handling

    [Fact]
    public async Task DateComparison_ShouldWorkCorrectly()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Teste");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Teste data", 100, new DateTime(2026, 5, 15));
        await expRepo.AddAsync(expense);

        // Query by date range - PostgreSQL respects date boundaries
        var found = await expRepo.GetAllAsync(
            startDate: new DateTime(2026, 5, 1),
            endDate: new DateTime(2026, 5, 31));

        Assert.Single(found);

        var notFound = await expRepo.GetAllAsync(
            startDate: new DateTime(2026, 6, 1),
            endDate: new DateTime(2026, 6, 30));

        Assert.Empty(notFound);
    }

    #endregion

    #region Large Dataset Performance

    [Fact]
    public async Task BatchInsert_ShouldRespectPrecision()
    {
        var planRepo = new FinancialPlanRepository(DbContext);

        for (int i = 0; i < 10; i++)
        {
            var plan = new FinancialPlan(Guid.NewGuid(),
                1000.99m + (i * 100.11m), 15, 1, 2026);
            await planRepo.AddAsync(plan);
        }

        var all = await planRepo.GetAllAsync();
        Assert.Equal(10, all.Count);

        // Verify precision preserved
        Assert.Equal(1000.99m, all[0].BaseValue);
    }

    #endregion
}
