using Xunit;
using FluentAssertions;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

namespace EduCore.Financial.IntegrationTests;

public class PaymentTests : TestBase
{
    private readonly Guid _userId = Guid.NewGuid();
    private const string _userName = "Test User";

    private async Task<(FinancialPlan plan, MonthlyCharge charge)> CreatePlanAndCharge(decimal value = 500)
    {
        var planRepo = new FinancialPlanRepository(DbContext);
        var plan = new FinancialPlan(Guid.NewGuid(), value, 10, 1, 2026);
        await planRepo.AddAsync(plan);

        var chargeRepo = new MonthlyChargeRepository(DbContext);
        var charge = new MonthlyCharge(plan.Id, 5, 2026, value, new DateTime(2026, 5, 10));
        chargeRepo.Add(charge);
        await DbContext.SaveChangesAsync();

        return (plan, charge);
    }

    [Fact]
    public async Task RegisterPayment_ShouldPersist()
    {
        var (_, charge) = await CreatePlanAndCharge(890);
        var paymentRepo = new PaymentRepository(DbContext);

        var payment = new Payment(charge.Id, 890.00m, new DateTime(2026, 5, 10), PaymentMethod.Pix, null, _userId, _userName);
        paymentRepo.Add(payment);
        await DbContext.SaveChangesAsync();

        var saved = await paymentRepo.GetByIdAsync(payment.Id);
        Assert.NotNull(saved);
        Assert.Equal(890.00m, saved.Value);
        Assert.Equal(PaymentMethod.Pix, saved.Method);
    }

    [Fact]
    public async Task RegisterPayment_WithObservation()
    {
        var (_, charge) = await CreatePlanAndCharge();
        var paymentRepo = new PaymentRepository(DbContext);

        var payment = new Payment(charge.Id, 450.00m, new DateTime(2026, 5, 8), PaymentMethod.Cash,
            "Pagamento parcial", _userId, _userName);
        paymentRepo.Add(payment);
        await DbContext.SaveChangesAsync();

        var saved = await paymentRepo.GetByIdAsync(payment.Id);
        Assert.Equal("Pagamento parcial", saved!.Observation);
    }

    [Fact]
    public async Task GetPaymentsByCharge_ShouldReturnAll()
    {
        var (_, charge) = await CreatePlanAndCharge();
        var paymentRepo = new PaymentRepository(DbContext);

        paymentRepo.Add(new Payment(charge.Id, 250, new DateTime(2026, 5, 5), PaymentMethod.Pix, null, _userId, _userName));
        paymentRepo.Add(new Payment(charge.Id, 250, new DateTime(2026, 5, 8), PaymentMethod.Pix, null, _userId, _userName));
        await DbContext.SaveChangesAsync();

        var payments = await paymentRepo.GetByChargeIdAsync(charge.Id);
        Assert.Equal(2, payments.Count);
    }

    [Fact]
    public async Task MarkChargeAsPaid_ShouldUpdateStatus()
    {
        var (_, charge) = await CreatePlanAndCharge();
        var chargeRepo = new MonthlyChargeRepository(DbContext);

        charge.MarkAsPaid(new DateTime(2026, 5, 5));
        await DbContext.SaveChangesAsync();

        var saved = await chargeRepo.GetByIdAsync(charge.Id);
        Assert.Equal(ChargeStatus.Paid, saved!.Status);
    }

    [Fact]
    public async Task FullPaymentFlow_ShouldComplete()
    {
        var (_, charge) = await CreatePlanAndCharge(890);
        var paymentRepo = new PaymentRepository(DbContext);

        var payment = new Payment(charge.Id, 890.00m, new DateTime(2026, 6, 10), PaymentMethod.Pix, null, _userId, _userName);
        paymentRepo.Add(payment);
        await DbContext.SaveChangesAsync();

        charge.MarkAsPaid(new DateTime(2026, 6, 10));
        await DbContext.SaveChangesAsync();

        var savedCharge = await new MonthlyChargeRepository(DbContext).GetByIdAsync(charge.Id);
        Assert.Equal(ChargeStatus.Paid, savedCharge!.Status);

        var total = await paymentRepo.GetActiveTotalByChargeIdAsync(charge.Id);
        Assert.Equal(890.00m, total);
    }

    [Fact]
    public async Task PartialPayments_ShouldAccumulate()
    {
        var (_, charge) = await CreatePlanAndCharge(1000);
        var paymentRepo = new PaymentRepository(DbContext);

        paymentRepo.Add(new Payment(charge.Id, 300, new DateTime(2026, 6, 5), PaymentMethod.Pix, null, _userId, _userName));
        paymentRepo.Add(new Payment(charge.Id, 300, new DateTime(2026, 6, 8), PaymentMethod.Pix, null, _userId, _userName));
        paymentRepo.Add(new Payment(charge.Id, 400, new DateTime(2026, 6, 10), PaymentMethod.Pix, null, _userId, _userName));
        await DbContext.SaveChangesAsync();

        var total = await paymentRepo.GetActiveTotalByChargeIdAsync(charge.Id);
        Assert.Equal(1000.00m, total);
    }

    [Fact]
    public async Task GetByChargeId_ShouldIncludeCancelled()
    {
        var (_, charge) = await CreatePlanAndCharge(500);
        var paymentRepo = new PaymentRepository(DbContext);

        paymentRepo.Add(new Payment(charge.Id, 200, DateTime.UtcNow, PaymentMethod.Cash, null, _userId, _userName));
        paymentRepo.Add(new Payment(charge.Id, 300, DateTime.UtcNow, PaymentMethod.Pix, null, _userId, _userName));
        await DbContext.SaveChangesAsync();

        var payments = await paymentRepo.GetByChargeIdAsync(charge.Id, includeCancelled: true);
        Assert.Equal(2, payments.Count);

        var activeTotal = await paymentRepo.GetActiveTotalByChargeIdAsync(charge.Id);
        Assert.Equal(500.00m, activeTotal);
    }
}
