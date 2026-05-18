using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Giglio.EduCore.Financial.Application.Services;

public class OverdueMarkingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueMarkingJob> _logger;

    public OverdueMarkingJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueMarkingJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueMarkingJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Rodar todo dia às 03:00 UTC
            var nextRun = now.Date.AddHours(3);
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            await MarkOverdueCharges(stoppingToken);
        }
    }

    private async Task MarkOverdueCharges(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var chargeRepo = scope.ServiceProvider.GetRequiredService<IMonthlyChargeRepository>();

            var overdueCandidates = await chargeRepo.GetOverdueCandidatesAsync(ct);
            int marked = 0;

            foreach (var charge in overdueCandidates)
            {
                charge.MarkAsOverdue();
                marked++;
            }

            if (marked > 0)
            {
                await chargeRepo.SaveChangesAsync(ct);
                _logger.LogInformation("Marked {Count} charges as Overdue", marked);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Overdue marking job failed");
        }
    }
}
