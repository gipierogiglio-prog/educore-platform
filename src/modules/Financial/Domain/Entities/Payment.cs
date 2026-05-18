using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid MonthlyChargeId { get; private set; }
    public decimal Value { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Observation { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Audit
    public Guid CreatedByUserId { get; private set; }
    public string CreatedByUserName { get; private set; } = string.Empty;
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancelledByUserName { get; private set; }
    public string? CancelReason { get; private set; }

    // Navigation
    public MonthlyCharge MonthlyCharge { get; private set; } = null!;

    public bool IsCancelled => CancelledAt.HasValue;

    private Payment() { }

    public Payment(
        Guid monthlyChargeId,
        decimal value,
        DateTime paymentDate,
        PaymentMethod method,
        string? observation,
        Guid createdByUserId,
        string createdByUserName)
    {
        Id = Guid.NewGuid();
        MonthlyChargeId = monthlyChargeId;
        SetValue(value);
        SetPaymentDate(paymentDate);
        Method = method;
        SetObservation(observation);
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        CreatedByUserName = createdByUserName ?? "Sistema";
    }

    public void SetValue(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Payment value must be greater than zero", nameof(value));
        Value = value;
    }

    public void SetPaymentDate(DateTime paymentDate)
    {
        PaymentDate = paymentDate;
    }

    public void SetObservation(string? observation)
    {
        if (observation?.Length > 1000)
            throw new ArgumentException("Observation must be at most 1000 characters", nameof(observation));
        Observation = observation;
    }

    public void Cancel(Guid cancelledByUserId, string cancelledByUserName, string reason)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Payment is already cancelled.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancel reason is required.", nameof(reason));

        if (reason.Trim().Length < 10)
            throw new ArgumentException("Cancel reason must be at least 10 characters.", nameof(reason));

        if (reason.Trim().Length > 500)
            throw new ArgumentException("Cancel reason must be at most 500 characters.", nameof(reason));

        var daysSinceCreation = (DateTime.UtcNow - CreatedAt).TotalDays;
        if (daysSinceCreation > 90)
            throw new InvalidOperationException("Payments can only be cancelled within 90 days of registration.");

        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = cancelledByUserId;
        CancelledByUserName = cancelledByUserName;
        CancelReason = reason.Trim();
    }

    public static string? BuildObservation(decimal chargeValue, decimal paymentValue, string? userObservation)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(userObservation))
            parts.Add(userObservation.Trim());

        if (paymentValue > chargeValue)
        {
            var diff = paymentValue - chargeValue;
            parts.Add($"Diferença de R$ {diff:F2} registrada como crédito.");
        }
        else if (paymentValue < chargeValue)
        {
            var remaining = chargeValue - paymentValue;
            parts.Add($"Pagamento parcial: R$ {paymentValue:F2} de R$ {chargeValue:F2}. Restante: R$ {remaining:F2}");
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }
}
