using Giglio.EduCore.Financial.Application.Commands.Plans;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/expense-categories")]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryRepository _repository;

    public ExpenseCategoriesController(IExpenseCategoryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var categories = await _repository.GetAllAsync(activeOnly, ct);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound(new { error = "Category not found" });
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        if (await _repository.ExistsByNameAsync(request.Name.Trim(), ct))
            return Conflict(new { error = "Category with this name already exists" });

        var category = new ExpenseCategory(request.Name, request.Description);
        await _repository.AddAsync(category, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound(new { error = "Category not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (await _repository.ExistsByNameAsync(request.Name.Trim(), ct))
                return Conflict(new { error = "Category with this name already exists" });
            category.SetName(request.Name);
        }

        if (request.Description != null)
            category.SetDescription(request.Description);

        await _repository.UpdateAsync(category, ct);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound(new { error = "Category not found" });

        if (await _repository.HasActiveExpensesAsync(id, ct))
            return Conflict(new { error = "Cannot delete category with active expenses" });

        category.Deactivate();
        await _repository.UpdateAsync(category, ct);
        return NoContent();
    }
}

public record CreateCategoryRequest(string Name, string? Description);
public record UpdateCategoryRequest(string? Name, string? Description);
