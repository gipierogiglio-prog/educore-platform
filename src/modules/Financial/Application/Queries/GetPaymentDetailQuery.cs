using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Interfaces;

namespace Giglio.EduCore.Financial.Application.Queries;

public class GetPaymentDetailQueryHandler
{
    private readonly IPaymentRepository _paymentRepo;

    public GetPaymentDetailQueryHandler(IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public async Task<PaymentDetailDto?> Handle(Guid id, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(id, ct);
        return payment == null ? null : PaymentDetailDto.FromEntity(payment);
    }
}
