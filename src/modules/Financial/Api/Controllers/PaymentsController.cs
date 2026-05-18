using Giglio.EduCore.Financial.Application.Commands.Payments;
using Giglio.EduCore.Financial.Application.DTOs;
using Giglio.EduCore.Financial.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Financial.Api.Controllers;

[ApiController]
[Route("api/financial/payments")]
public class PaymentsController : ControllerBase
{
    private readonly RegisterPaymentCommandHandler _registerHandler;
    private readonly CancelPaymentCommandHandler _cancelHandler;
    private readonly GetPaymentsByChargeQueryHandler _listQuery;
    private readonly GetPaymentDetailQueryHandler _detailQuery;

    public PaymentsController(
        RegisterPaymentCommandHandler registerHandler,
        CancelPaymentCommandHandler cancelHandler,
        GetPaymentsByChargeQueryHandler listQuery,
        GetPaymentDetailQueryHandler detailQuery)
    {
        _registerHandler = registerHandler;
        _cancelHandler = cancelHandler;
        _listQuery = listQuery;
        _detailQuery = detailQuery;
    }

    /// <summary>
    /// POST /api/financial/payments — Registrar pagamento
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPaymentCommand command,
        CancellationToken ct)
    {
        var validator = new RegisterPaymentCommandValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Validation Error", status = 400, errors });
        }

        var currentUserId = GetCurrentUserId();
        var currentUserName = GetCurrentUserName();

        var result = await _registerHandler.Handle(command, currentUserId, currentUserName, ct);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new
            {
                type = result.StatusCode == 409
                    ? "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                    : "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = result.Error,
                status = result.StatusCode,
                detail = result.Error
            });

        var response = (CreatePaymentResponse)result.Data!;
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// GET /api/financial/payments?monthlyChargeId={id} — Listar pagamentos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByCharge(
        [FromQuery] Guid monthlyChargeId,
        [FromQuery] bool includeCancelled = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (monthlyChargeId == Guid.Empty)
            return BadRequest(new { error = "monthlyChargeId is required" });

        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var result = await _listQuery.Handle(monthlyChargeId, includeCancelled, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/financial/payments/{id} — Detalhe do pagamento
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _detailQuery.Handle(id, ct);
        if (result == null)
            return NotFound(new { error = "Payment not found" });
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/financial/payments/{id}?reason={reason} — Estornar pagamento
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromQuery] string reason,
        CancellationToken ct)
    {
        var command = new CancelPaymentCommand(id, reason);
        var validator = new CancelPaymentCommandValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Validation Error", status = 400, errors });
        }

        var currentUserId = GetCurrentUserId();
        var currentUserName = GetCurrentUserName();

        var result = await _cancelHandler.Handle(command, currentUserId, currentUserName, ct);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new
            {
                title = result.StatusCode switch
                {
                    422 => "Cancellation period expired",
                    400 => "Cancellation Error",
                    _ => "Error"
                },
                status = result.StatusCode,
                detail = result.Error
            });

        return Ok(result.Data);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Sistema";
    }
}
