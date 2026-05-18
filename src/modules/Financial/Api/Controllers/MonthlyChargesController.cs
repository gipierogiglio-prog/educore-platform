using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/charges")]
public class MonthlyChargesController : ControllerBase
{
    private readonly IMonthlyChargeRepository _chargeRepo;
    private readonly IFinancialPlanRepository _planRepo;

    public MonthlyChargesController(
        IMonthlyChargeRepository chargeRepo,
        IFinancialPlanRepository planRepo)
    {
        _chargeRepo = chargeRepo;
        _planRepo = planRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? planId,
        [FromQuery] int? startMonth,
        [FromQuery] int? startYear,
        [FromQuery] int? endMonth,
        [FromQuery] int? endYear,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 12;
        if (pageSize > 100) pageSize = 100;

        var statusFilter = !string.IsNullOrWhiteSpace(status)
            ? status.Split(',').Select(s => s.Trim()).ToList()
            : null;

        var charges = await _chargeRepo.GetFilteredAsync(
            studentId, planId,
            startMonth, startYear,
            endMonth, endYear,
            statusFilter, page, pageSize, ct);

        var total = await _chargeRepo.GetFilteredCountAsync(
            studentId, planId,
            startMonth, startYear,
            endMonth, endYear,
            statusFilter, ct);

        return Ok(new
        {
            data = charges.Select(MapCharge),
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    [HttpGet("by-student/{studentId:guid}")]
    public async Task<IActionResult> GetByStudent(Guid studentId, CancellationToken ct)
    {
        var charges = await _chargeRepo.GetByStudentIdAsync(studentId, ct);
        return Ok(charges.Select(MapCharge));
    }

    [HttpGet("by-plan/{planId:guid}")]
    public async Task<IActionResult> GetByPlan(Guid planId, CancellationToken ct)
    {
        var charges = await _chargeRepo.GetByPlanIdAsync(planId, ct);
        return Ok(charges.Select(MapCharge));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var charge = await _chargeRepo.GetByIdAsync(id, ct);
        if (charge is null)
            return NotFound(new { error = "Monthly charge not found" });
        return Ok(MapCharge(charge));
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayChargeRequest request, CancellationToken ct)
    {
        var charge = await _chargeRepo.GetByIdAsync(id, ct);
        if (charge is null)
            return NotFound(new { error = "Monthly charge not found" });

        try
        {
            charge.MarkAsPaid(request.PaidAt);
            _chargeRepo.Update(charge);
            await _chargeRepo.SaveChangesAsync(ct);
            return Ok(MapCharge(charge));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var charge = await _chargeRepo.GetByIdAsync(id, ct);
        if (charge is null)
            return NotFound(new { error = "Monthly charge not found" });

        try
        {
            charge.Cancel();
            _chargeRepo.Update(charge);
            await _chargeRepo.SaveChangesAsync(ct);
            return Ok(MapCharge(charge));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/overdue")]
    public async Task<IActionResult> MarkOverdue(Guid id, CancellationToken ct)
    {
        var charge = await _chargeRepo.GetByIdAsync(id, ct);
        if (charge is null)
            return NotFound(new { error = "Monthly charge not found" });

        charge.MarkAsOverdue();
        _chargeRepo.Update(charge);
        await _chargeRepo.SaveChangesAsync(ct);
        return Ok(MapCharge(charge));
    }

    private static object MapCharge(MonthlyCharge charge)
    {
        return new
        {
            charge.Id,
            charge.FinancialPlanId,
            enrollmentId = charge.FinancialPlan?.EnrollmentId,
            referenceLabel = $"{charge.ReferenceMonth:D2}/{charge.ReferenceYear}",
            charge.ReferenceMonth,
            charge.ReferenceYear,
            charge.Value,
            charge.DueDate,
            charge.Status,
            charge.PaidAt,
            charge.CreatedAt,
            charge.UpdatedAt
        };
    }
}

public record PayChargeRequest(DateTime PaidAt);
