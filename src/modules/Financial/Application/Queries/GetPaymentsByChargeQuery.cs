using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Domain.Interfaces;

namespace Giglio.EduCore.Financial.Application.Queries;

public class GetPaymentsByChargeQueryHandler
{
    private readonly IPaymentRepository _paymentRepo;

    public GetPaymentsByChargeQueryHandler(IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public async Task<PaymentListResponse> Handle(
        Guid monthlyChargeId,
        bool includeCancelled = false,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var items = await _paymentRepo.GetByChargeIdAsync(
            monthlyChargeId, includeCancelled, page, pageSize, ct);

        var totalItems = await _paymentRepo.GetCountByChargeIdAsync(monthlyChargeId, ct);

        return new PaymentListResponse(
            items.Select(PaymentDto.FromEntity).ToList(),
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize),
            page,
            pageSize);
    }
}
