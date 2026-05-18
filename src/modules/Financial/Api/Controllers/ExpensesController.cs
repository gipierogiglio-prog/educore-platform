using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly IExpenseCategoryRepository _categoryRepo;

    public ExpensesController(IExpenseRepository expenseRepo, IExpenseCategoryRepository categoryRepo)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
    {
        var expenses = await _expenseRepo.GetAllAsync(categoryId, status, startDate, endDate, ct);
        return Ok(expenses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var expense = await _expenseRepo.GetByIdAsync(id, ct);
        if (expense is null)
            return NotFound(new { error = "Expense not found" });
        return Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return BadRequest(new { error = "Category not found" });

        if (!category.IsActive)
            return BadRequest(new { error = "Category is inactive" });

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 3)
            return BadRequest(new { error = "Description must be at least 3 characters" });

        if (request.Value <= 0)
            return BadRequest(new { error = "Value must be greater than zero" });

        var expense = new Expense(
            request.CategoryId,
            request.Description,
            request.Value,
            request.DueDate,
            request.ProviderName);

        await _expenseRepo.AddAsync(expense, ct);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken ct)
    {
        var expense = await _expenseRepo.GetByIdAsync(id, ct);
        if (expense is null)
            return NotFound(new { error = "Expense not found" });

        if (expense.Status == ExpenseStatus.Paid || expense.Status == ExpenseStatus.Cancelled)
            return Conflict(new { error = "Cannot modify a paid or cancelled expense" });

        if (!string.IsNullOrWhiteSpace(request.Description))
            expense.SetDescription(request.Description);

        if (request.Value.HasValue)
            expense.SetValue(request.Value.Value);

        if (request.DueDate.HasValue)
            expense.SetDueDate(request.DueDate.Value);

        if (request.ProviderName != null)
        {
            var field = typeof(Expense).GetField("ProviderName",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(expense, request.ProviderName?.Trim());
        }

        await _expenseRepo.UpdateAsync(expense, ct);
        return Ok(expense);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayExpenseRequest request, CancellationToken ct)
    {
        var expense = await _expenseRepo.GetByIdAsync(id, ct);
        if (expense is null)
            return NotFound(new { error = "Expense not found" });

        try
        {
            expense.Pay(request.PaymentDate);
            await _expenseRepo.UpdateAsync(expense, ct);
            return Ok(expense);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var expense = await _expenseRepo.GetByIdAsync(id, ct);
        if (expense is null)
            return NotFound(new { error = "Expense not found" });

        try
        {
            expense.Cancel();
            await _expenseRepo.UpdateAsync(expense, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}

public record CreateExpenseRequest(Guid CategoryId, string Description, decimal Value, DateTime DueDate, string? ProviderName);
public record UpdateExpenseRequest(string? Description, decimal? Value, DateTime? DueDate, string? ProviderName);
public record PayExpenseRequest(DateTime PaymentDate);
