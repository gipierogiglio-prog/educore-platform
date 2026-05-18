using Giglio.EduCore.Financial.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Giglio.EduCore.Financial.Application.Services;

public class MonthlyChargeGenerationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyChargeGenerationJob> _logger;

    public MonthlyChargeGenerationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<MonthlyChargeGenerationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonthlyChargeGenerationJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Rodar no dia 1 de cada mês às 02:00 UTC
            var nextRun = new DateTime(now.Year, now.Month, 1, 2, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var delay = nextRun - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            await GenerateCharges(stoppingToken);
        }
    }

    private async Task GenerateCharges(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var planRepo = scope.ServiceProvider.GetRequiredService<IFinancialPlanRepository>();
            var chargeGen = scope.ServiceProvider.GetRequiredService<IChargeGenerationService>();
            var chargeRepo = scope.ServiceProvider.GetRequiredService<IMonthlyChargeRepository>();

            var activePlans = await planRepo.GetAllAsync(activeOnly: true, ct);
            int totalGenerated = 0;

            foreach (var plan in activePlans)
            {
                try
                {
                    var newCharges = await chargeGen.GenerateChargesAsync(plan, ct);
                    if (newCharges.Count > 0)
                    {
                        chargeRepo.AddRange(newCharges);
                        await chargeRepo.SaveChangesAsync(ct);
                        _logger.LogInformation(
                            "Generated {Count} charges for plan {PlanId}",
                            newCharges.Count, plan.Id);
                        totalGenerated += newCharges.Count;
                    }
                    else
                    {
                        _logger.LogDebug("No new charges for plan {PlanId}", plan.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to generate charges for plan {PlanId}", plan.Id);
                }
            }

            _logger.LogInformation(
                "Charge generation job completed. Generated {Total} charges across {PlanCount} active plans",
                totalGenerated, activePlans.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Charge generation job failed");
        }
    }
}
