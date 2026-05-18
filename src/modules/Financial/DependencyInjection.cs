using Giglio.EduCore.Financial.Application.Services;
using Giglio.EduCore.Financial.Domain.Interfaces;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Giglio.EduCore.Financial;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddDbContext<FinancialDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IFinancialPlanRepository, FinancialPlanRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IMonthlyChargeRepository, MonthlyChargeRepository>();

        // Domain Services
        services.AddScoped<IChargeGenerationService, ChargeGenerationService>();

        // Background Jobs
        services.AddHostedService<MonthlyChargeGenerationJob>();
        services.AddHostedService<OverdueMarkingJob>();

        return services;
    }
}
