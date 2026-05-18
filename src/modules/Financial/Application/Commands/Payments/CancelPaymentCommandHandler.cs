using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Application.Commands.Payments;

public class CancelPaymentCommandHandler
{
    private readonly FinancialDbContext _dbContext;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IMonthlyChargeRepository _chargeRepo;

    public CancelPaymentCommandHandler(
        FinancialDbContext dbContext,
        IPaymentRepository paymentRepo,
        IMonthlyChargeRepository chargeRepo)
    {
        _dbContext = dbContext;
        _paymentRepo = paymentRepo;
        _chargeRepo = chargeRepo;
    }

    public async Task<IResult> Handle(
        CancelPaymentCommand command,
        Guid currentUserId,
        string currentUserName,
        CancellationToken ct)
    {
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await ExecuteTransaction(command, currentUserId, currentUserName, ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
            {
                await Task.Delay(100 * attempt, ct);
            }
        }

        return Result.Fail("Concurrency conflict. Please try again.", 409);
    }

    private async Task<IResult> ExecuteTransaction(
        CancelPaymentCommand command,
        Guid currentUserId,
        string currentUserName,
        CancellationToken ct)
    {
        var payment = await _paymentRepo.GetByIdAsync(command.PaymentId, ct);
        if (payment == null)
            return Result.Fail("Payment not found.", 404);

        try
        {
            payment.Cancel(currentUserId, currentUserName, command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message switch
            {
                var m when m.Contains("already cancelled") => Result.Fail(m, 400),
                var m when m.Contains("within 90 days") => Result.Fail(m, 422),
                _ => Result.Fail(ex.Message, 400)
            };
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var charge = payment.MonthlyCharge;

            // Calculate total of OTHER active payments (excluding this one since it's now cancelled)
            var totalOtherActive = await _paymentRepo.GetActiveTotalByChargeIdAsync(
                payment.MonthlyChargeId, ct);

            // totalOtherActive includes this payment because it's not saved yet
            // We need to subtract this payment's value
            var otherPaymentsTotal = totalOtherActive - payment.Value;

            bool isLastActivePayment = otherPaymentsTotal <= 0.01m; // tolerance for rounding

            if (isLastActivePayment)
            {
                // This is the only/relevant active payment being cancelled
                // Revert charge status based on due date
                charge.MarkAsUnpaid();
            }

            _chargeRepo.Update(charge);
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success(new CancelPaymentResponse(
                payment.Id,
                payment.IsCancelled,
                payment.CancelledAt,
                payment.CancelReason,
                charge.Status.ToString()));
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
