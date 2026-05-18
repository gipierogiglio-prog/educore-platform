namespace Giglio.EduCore.Financial.Application.DTOs;

public record ExpenseDetailDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ProviderName { get; init; }
    public decimal Value { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? PaymentDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsOverdue { get; init; }
    public int DaysOverdue { get; init; }
    public DateTime CreatedAt { get; init; }
}
