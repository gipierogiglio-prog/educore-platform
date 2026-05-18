using Giglio.EduCore.Financial.Application.Commands.Plans;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/plans")]
public class FinancialPlansController : ControllerBase
{
    private readonly IFinancialPlanRepository _repository;

    public FinancialPlansController(IFinancialPlanRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var plans = await _repository.GetAllAsync(activeOnly, ct);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var plan = await _repository.GetByIdAsync(id, ct);
        if (plan is null)
            return NotFound(new { error = "Financial plan not found" });
        return Ok(plan);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFinancialPlanCommand command, CancellationToken ct)
    {
        var validator = new CreateFinancialPlanValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var plan = new FinancialPlan(
            command.EnrollmentId,
            command.BaseValue,
            command.DueDay,
            command.StartMonth,
            command.StartYear,
            command.DiscountPercent,
            command.ResolvedDiscountType,
            command.EndMonth,
            command.EndYear);

        await _repository.AddAsync(plan, ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFinancialPlanCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var plan = await _repository.GetByIdAsync(id, ct);
        if (plan is null)
            return NotFound(new { error = "Financial plan not found" });

        if (!plan.IsActive)
            return Conflict(new { error = "Cannot update an inactive plan" });

        if (command.BaseValue.HasValue)
            plan.SetBaseValue(command.BaseValue.Value);

        if (command.DueDay.HasValue)
            plan.SetDueDay(command.DueDay.Value);

        if (command.DiscountPercent.HasValue)
            plan.SetDiscount(command.DiscountPercent, command.ResolvedDiscountType);

        await _repository.UpdateAsync(plan, ct);
        return Ok(plan);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var plan = await _repository.GetByIdAsync(id, ct);
        if (plan is null)
            return NotFound(new { error = "Financial plan not found" });

        bool hasPaid = await _repository.HasPaidChargesAsync(id, ct);
        if (hasPaid)
            return Conflict(new { error = "Cannot deactivate plan with paid charges" });

        plan.Deactivate();
        await _repository.UpdateAsync(plan, ct);
        return NoContent();
    }
}
