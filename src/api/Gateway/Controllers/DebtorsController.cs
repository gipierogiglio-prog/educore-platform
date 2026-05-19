using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/financial/debtors")]
[Authorize(Roles = "org_admin,financial")]
public class DebtorsController : ControllerBase
{
    private readonly FinancialDbContext _db;
    public DebtorsController(FinancialDbContext db) => _db = db;

    /// <summary>
    /// List all debtors (enrollments with overdue monthly charges)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDebtors([FromQuery] int? year, [FromQuery] int? month)
    {
        var queryYear = year ?? DateTime.UtcNow.Year;
        var queryMonth = month ?? DateTime.UtcNow.Month;

        // Get overdue charges grouped by financial plan (which links to enrollment)
        var overdueData = await _db.MonthlyCharges
            .Where(mc => mc.ReferenceYear == queryYear && mc.ReferenceMonth == queryMonth && mc.Status == ChargeStatus.Overdue)
            .Include(mc => mc.Payments)
            .Join(_db.FinancialPlans, mc => mc.FinancialPlanId, fp => fp.Id, (mc, fp) => new { mc, fp })
            .GroupBy(x => x.fp.EnrollmentId)
            .Select(g => new
            {
                enrollmentId = g.Key,
                totalDue = g.Sum(x => x.mc.Value),
                totalPaid = g.Sum(x => x.mc.Payments
                    .Where(p => p.CancelledAt == null)
                    .Sum(p => p.Value)),
                chargesCount = g.Count()
            })
            .OrderByDescending(d => d.totalDue)
            .ToListAsync();

        var result = overdueData.Select(d => new
        {
            d.enrollmentId,
            d.totalDue,
            d.totalPaid,
            balance = d.totalDue - d.totalPaid,
            d.chargesCount
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get detailed debt info for a specific enrollment
    /// </summary>
    [HttpGet("{enrollmentId:guid}")]
    public async Task<IActionResult> GetEnrollmentDebt(Guid enrollmentId, [FromQuery] int? year)
    {
        var queryYear = year ?? DateTime.UtcNow.Year;

        var charges = await _db.MonthlyCharges
            .Where(mc => mc.ReferenceYear == queryYear && mc.Status == ChargeStatus.Overdue)
            .Include(mc => mc.Payments)
            .Join(_db.FinancialPlans.Where(fp => fp.EnrollmentId == enrollmentId),
                  mc => mc.FinancialPlanId, fp => fp.Id, (mc, fp) => mc)
            .OrderBy(mc => mc.ReferenceYear).ThenBy(mc => mc.ReferenceMonth)
            .ToListAsync();

        var result = charges.Select(mc =>
        {
            var paidAmount = mc.Payments
                .Where(p => p.CancelledAt == null)
                .Sum(p => p.Value);
            return new
            {
                mc.Id,
                month = mc.ReferenceMonth,
                year = mc.ReferenceYear,
                amount = mc.Value,
                paidAmount,
                balance = mc.Value - paidAmount,
                mc.DueDate,
                status = mc.Status.ToString()
            };
        }).ToList();

        var totalDue = result.Sum(c => c.amount);
        var totalPaid = result.Sum(c => c.paidAmount);

        return Ok(new
        {
            enrollmentId,
            year = queryYear,
            totalDue,
            totalPaid,
            totalBalance = totalDue - totalPaid,
            overdueCharges = result
        });
    }
}
