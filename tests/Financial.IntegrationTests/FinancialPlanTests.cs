using Xunit;
using FluentAssertions;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

namespace EduCore.Financial.IntegrationTests;

public class FinancialPlanTests : TestBase
{
    [Fact]
    public async Task CreateFinancialPlan_ShouldPersist()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var plan = new FinancialPlan(enrollmentId, 890.00m, 15, 1, 2026);
        await repo.AddAsync(plan);

        var saved = await repo.GetByIdAsync(plan.Id);
        saved.Should().NotBeNull();
        saved!.BaseValue.Should().Be(890.00m);
        saved.DueDay.Should().Be(15);
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateFinancialPlan_WithPercentageDiscount_ShouldCalculateCorrectly()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var plan = new FinancialPlan(enrollmentId, 1000.00m, 10, 1, 2026, 10, DiscountType.Percentage);
        await repo.AddAsync(plan);

        plan.CalculateMonthlyValue().Should().Be(900.00m);
    }

    [Fact]
    public async Task CreateFinancialPlan_WithFixedDiscount_ShouldCalculateCorrectly()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var plan = new FinancialPlan(enrollmentId, 1000.00m, 10, 1, 2026, 200, DiscountType.Fixed);
        await repo.AddAsync(plan);

        plan.CalculateMonthlyValue().Should().Be(800.00m);
    }

    [Fact]
    public async Task DeactivatePlan_ShouldSetInactive()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var plan = new FinancialPlan(enrollmentId, 500.00m, 5, 1, 2026);
        await repo.AddAsync(plan);

        plan.Deactivate();
        await repo.UpdateAsync(plan);

        var saved = await repo.GetByIdAsync(plan.Id);
        saved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPlans_FilterActive_ShouldReturnOnlyActive()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var active = new FinancialPlan(enrollmentId, 500.00m, 5, 1, 2026);
        var inactive = new FinancialPlan(enrollmentId, 600.00m, 10, 1, 2026);
        inactive.Deactivate();

        await repo.AddAsync(active);
        await repo.AddAsync(inactive);

        var all = await repo.GetAllAsync(activeOnly: true);
        all.Should().HaveCount(1);
        all[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreatePlan_WithNegativeBaseValue_ShouldThrow()
    {
        var enrollmentId = Guid.NewGuid();
        var act = () => new FinancialPlan(enrollmentId, -100, 15, 1, 2026);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreatePlan_WithInvalidDueDay_ShouldThrow()
    {
        var enrollmentId = Guid.NewGuid();
        var act = () => new FinancialPlan(enrollmentId, 500, 32, 1, 2026);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreatePlan_WithFixedDiscountExceedingBase_ShouldThrow()
    {
        var enrollmentId = Guid.NewGuid();
        var act = () => new FinancialPlan(enrollmentId, 500, 15, 1, 2026, 600, DiscountType.Fixed);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task GetDueDateForMonth_ShouldUseLastDay_WhenDueDayExceeds()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var plan = new FinancialPlan(Guid.NewGuid(), 500, 31, 1, 2026);
        await repo.AddAsync(plan);

        var dueDate = plan.GetDueDateForMonth(2, 2026); // February
        dueDate.Day.Should().Be(28); // 2026 is not a leap year
    }

    [Fact]
    public async Task GetPlansByEnrollment_ShouldReturnOrdered()
    {
        var repo = new FinancialPlanRepository(DbContext);
        var enrollmentId = Guid.NewGuid();

        var p1 = new FinancialPlan(enrollmentId, 500, 5, 1, 2026);
        var p2 = new FinancialPlan(enrollmentId, 600, 10, 1, 2026);

        await repo.AddAsync(p1);
        await repo.AddAsync(p2);

        var plans = await repo.GetByEnrollmentAsync(enrollmentId);
        plans.Should().HaveCount(2);
        plans[0].CreatedAt.Should().BeOnOrAfter(plans[1].CreatedAt);
    }
}
