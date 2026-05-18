using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Application.Commands.Payments;

public class RegisterPaymentCommandHandler
{
    private readonly FinancialDbContext _dbContext;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IMonthlyChargeRepository _chargeRepo;

    public RegisterPaymentCommandHandler(
        FinancialDbContext dbContext,
        IPaymentRepository paymentRepo,
        IMonthlyChargeRepository chargeRepo)
    {
        _dbContext = dbContext;
        _paymentRepo = paymentRepo;
        _chargeRepo = chargeRepo;
    }

    public async Task<IResult> Handle(
        RegisterPaymentCommand command,
        Guid currentUserId,
        string currentUserName,
        CancellationToken ct)
    {
        // Validate method
        var method = command.ResolvedMethod;
        if (method == null)
            return Result.Fail("Invalid payment method.", 400);

        // Load charge with row version for concurrency
        var charge = await _chargeRepo.GetByIdAsync(command.MonthlyChargeId, ct);
        if (charge == null)
            return Result.Fail("Monthly charge not found.", 404);

        // Validate charge status
        if (charge.Status == ChargeStatus.Paid)
            return Result.Fail(
                $"Monthly charge {command.MonthlyChargeId} is already paid.",
                409);
        if (charge.Status == ChargeStatus.Cancelled)
            return Result.Fail(
                "Monthly charge is cancelled.",
                409);

        // Validate value vs charge value
        if (command.Value > charge.Value * 2)
            return Result.Fail(
                "Payment value cannot exceed charge value by more than 2x.",
                400);

        // Validate payment date
        if (command.PaymentDate > DateTime.UtcNow.AddDays(1))
            return Result.Fail("Payment date cannot be in the future.", 400);

        if (command.PaymentDate < charge.CreatedAt.Date)
            return Result.Fail("Payment date cannot be before charge creation.", 400);

        // Build observation
        var observation = Payment.BuildObservation(charge.Value, command.Value, command.Observation);

        // Retry pattern for concurrency
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await ExecuteTransaction(
                    charge, command, method.Value, observation,
                    currentUserId, currentUserName, ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
            {
                // Reload charge and retry
                await _dbContext.Entry(charge).ReloadAsync(ct);
                if (charge.Status == ChargeStatus.Paid)
                    return Result.Fail(
                        $"Monthly charge {command.MonthlyChargeId} is already paid.",
                        409);
                if (charge.Status == ChargeStatus.Cancelled)
                    return Result.Fail(
                        "Monthly charge is cancelled.",
                        409);
                await Task.Delay(100 * attempt, ct);
            }
        }

        return Result.Fail("Concurrency conflict. Please try again.", 409);
    }

    private async Task<IResult> ExecuteTransaction(
        MonthlyCharge charge,
        RegisterPaymentCommand command,
        PaymentMethod method,
        string? observation,
        Guid userId,
        string userName,
        CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var payment = new Payment(
                command.MonthlyChargeId,
                command.Value,
                command.PaymentDate,
                method,
                observation,
                userId,
                userName);

            _paymentRepo.Add(payment);

            // Calculate total paid (existing active payments + this one)
            var totalPaid = await _paymentRepo.GetActiveTotalByChargeIdAsync(
                                command.MonthlyChargeId, ct)
                            + command.Value;

            if (totalPaid >= charge.Value)
            {
                charge.MarkAsPaid(command.PaymentDate);
                _chargeRepo.Update(charge);
            }

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success(new CreatePaymentResponse(
                payment.Id,
                payment.MonthlyChargeId,
                payment.Value,
                payment.PaymentDate,
                payment.Method.ToString(),
                payment.Observation,
                charge.Status.ToString()));
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
