using Giglio.EduCore.DailyRoutines.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.DailyRoutines.Api.Controllers;

/// <summary>
/// Dashboard de status de rotinas (Task #202 e #203).
/// </summary>
[ApiController]
[Route("api/daily-routines/status")]
public class RoutineStatusController : ControllerBase
{
    private readonly GetRoutineStatusQuery _query;

    public RoutineStatusController(GetRoutineStatusQuery query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(
        [FromQuery] Guid classId,
        [FromQuery] DateTime? date,
        CancellationToken ct)
    {
        var queryDate = date?.Date ?? DateTime.UtcNow.Date;

        var result = await _query.ExecuteAsync(classId, queryDate, ct);
        if (result is null)
            return NotFound(new { error = "Class not found" });

        return Ok(result);
    }
}
