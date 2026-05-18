using Xunit;
using FluentAssertions;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;
using System;
using System.Threading.Tasks;

namespace EduCore.Financial.IntegrationTests;

public class MonthlyChargeTests : TestBase
{
    private async Task<FinancialPlan> CreatePlan(decimal value = 500)
    {
        var plan = new FinancialPlan(Guid.NewGuid(), value, 10, 1, 2026);
        await new FinancialPlanRepository(DbContext).AddAsync(plan);
        return plan;
    }

    private MonthlyChargeRepository Repo => new(DbContext);

    [Fact]
    public async Task CreateCharge_ShouldPersist()
    {
        var plan = await CreatePlan(890);
        var charge = new MonthlyCharge(plan.Id, 5, 2026, 890.00m, new DateTime(2026, 5, 15));
        Repo.Add(charge);
        await DbContext.SaveChangesAsync();

        var saved = await Repo.GetByIdAsync(charge.Id);
        Assert.NotNull(saved);
        Assert.Equal(890.00m, saved.Value);
        Assert.Equal(ChargeStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task MarkAsPaid_ShouldUpdateStatus()
    {
        var plan = await CreatePlan();
        var charge = new MonthlyCharge(plan.Id, 5, 2026, 500, new DateTime(2026, 5, 10));
        Repo.Add(charge);
        await DbContext.SaveChangesAsync();

        charge.MarkAsPaid(new DateTime(2026, 5, 5));
        await DbContext.SaveChangesAsync();

        var saved = await Repo.GetByIdAsync(charge.Id);
        Assert.Equal(ChargeStatus.Paid, saved!.Status);
    }

    [Fact]
    public async Task MarkAsOverdue_ShouldUpdateStatus()
    {
        var plan = await CreatePlan();
        var charge = new MonthlyCharge(plan.Id, 3, 2026, 500, new DateTime(2026, 3, 10));
        Repo.Add(charge);
        await DbContext.SaveChangesAsync();

        charge.MarkAsOverdue();
        await DbContext.SaveChangesAsync();

        var saved = await Repo.GetByIdAsync(charge.Id);
        Assert.Equal(ChargeStatus.Overdue, saved!.Status);
    }

    [Fact]
    public async Task CancelCharge_ShouldUpdateStatus()
    {
        var plan = await CreatePlan();
        var charge = new MonthlyCharge(plan.Id, 4, 2026, 500, new DateTime(2026, 4, 10));
        Repo.Add(charge);
        await DbContext.SaveChangesAsync();

        charge.Cancel();
        await DbContext.SaveChangesAsync();

        var saved = await Repo.GetByIdAsync(charge.Id);
        Assert.Equal(ChargeStatus.Cancelled, saved!.Status);
    }

    [Fact]
    public async Task GetByPlan_ShouldReturnAll()
    {
        var plan = await CreatePlan();
        Repo.Add(new MonthlyCharge(plan.Id, 1, 2026, 500, new DateTime(2026, 1, 10)));
        Repo.Add(new MonthlyCharge(plan.Id, 2, 2026, 500, new DateTime(2026, 2, 10)));
        Repo.Add(new MonthlyCharge(plan.Id, 3, 2026, 500, new DateTime(2026, 3, 10)));
        await DbContext.SaveChangesAsync();

        var charges = await Repo.GetByPlanIdAsync(plan.Id);
        Assert.Equal(3, charges.Count);
    }

    [Fact]
    public async Task Exists_ShouldDetectDuplicate()
    {
        var plan = await CreatePlan();
        Repo.Add(new MonthlyCharge(plan.Id, 5, 2026, 500, new DateTime(2026, 5, 10)));
        await DbContext.SaveChangesAsync();

        var exists = await Repo.ExistsAsync(plan.Id, 5, 2026);
        Assert.True(exists);

        var notExists = await Repo.ExistsAsync(plan.Id, 6, 2026);
        Assert.False(notExists);
    }

    [Fact]
    public async Task OverdueCandidates_ShouldDetect()
    {
        var plan = await CreatePlan();
        Repo.Add(new MonthlyCharge(plan.Id, 1, 2025, 500, new DateTime(2025, 1, 10)));
        Repo.Add(new MonthlyCharge(plan.Id, 5, 2026, 500, new DateTime(2026, 5, 10)));
        await DbContext.SaveChangesAsync();

        // First charge is overdue, second is not
        var candidates = await Repo.GetOverdueCandidatesAsync();
        Assert.Contains(candidates, c => c.Id != Guid.Empty);
    }

    [Fact]
    public async Task AddRange_ShouldCreateMultiple()
    {
        var plan = await CreatePlan();
        var charges = new List<MonthlyCharge>
        {
            new(plan.Id, 1, 2026, 500, new DateTime(2026, 1, 10)),
            new(plan.Id, 2, 2026, 500, new DateTime(2026, 2, 10)),
            new(plan.Id, 3, 2026, 500, new DateTime(2026, 3, 10))
        };
        Repo.AddRange(charges);
        await DbContext.SaveChangesAsync();

        var saved = await Repo.GetByPlanIdAsync(plan.Id);
        Assert.Equal(3, saved.Count);
    }
}
