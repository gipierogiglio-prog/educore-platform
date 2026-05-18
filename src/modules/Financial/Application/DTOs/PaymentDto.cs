using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid MonthlyChargeId,
    decimal Value,
    DateTime PaymentDate,
    string Method,
    string? Observation,
    bool IsCancelled,
    DateTime? CancelledAt,
    string? CancelledByUserName,
    string? CancelReason,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    string CreatedByUserName)
{
    public static PaymentDto FromEntity(Payment payment) => new(
        payment.Id,
        payment.MonthlyChargeId,
        payment.Value,
        payment.PaymentDate,
        payment.Method.ToString(),
        payment.Observation,
        payment.IsCancelled,
        payment.CancelledAt,
        payment.CancelledByUserName,
        payment.CancelReason,
        payment.CreatedAt,
        payment.CreatedByUserId,
        payment.CreatedByUserName);
}

public record PaymentDetailDto(
    Guid Id,
    Guid MonthlyChargeId,
    decimal Value,
    DateTime PaymentDate,
    string Method,
    string? Observation,
    bool IsCancelled,
    DateTime? CancelledAt,
    string? CancelledByUserName,
    string? CancelReason,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    string CreatedByUserName,
    MonthlyChargeSummaryDto MonthlyCharge)
{
    public static PaymentDetailDto FromEntity(Payment payment) => new(
        payment.Id,
        payment.MonthlyChargeId,
        payment.Value,
        payment.PaymentDate,
        payment.Method.ToString(),
        payment.Observation,
        payment.IsCancelled,
        payment.CancelledAt,
        payment.CancelledByUserName,
        payment.CancelReason,
        payment.CreatedAt,
        payment.CreatedByUserId,
        payment.CreatedByUserName,
        new MonthlyChargeSummaryDto(
            payment.MonthlyCharge.Id,
            payment.MonthlyCharge.ReferenceMonth,
            payment.MonthlyCharge.ReferenceYear,
            payment.MonthlyCharge.Value,
            payment.MonthlyCharge.DueDate,
            payment.MonthlyCharge.Status.ToString()));
}

public record MonthlyChargeSummaryDto(
    Guid Id,
    int ReferenceMonth,
    int ReferenceYear,
    decimal Value,
    DateTime DueDate,
    string Status);

public record CreatePaymentRequest(
    Guid MonthlyChargeId,
    decimal Value,
    DateTime PaymentDate,
    string Method,
    string? Observation);

public record CreatePaymentResponse(
    Guid Id,
    Guid MonthlyChargeId,
    decimal Value,
    DateTime PaymentDate,
    string Method,
    string? Observation,
    string ChargeStatus);

public record CancelPaymentResponse(
    Guid Id,
    bool IsCancelled,
    DateTime? CancelledAt,
    string? CancelReason,
    string ChargeStatus);

public record PaymentListResponse(
    List<PaymentDto> Items,
    int TotalItems,
    int TotalPages,
    int Page,
    int PageSize);
