using Giglio.EduCore.DailyRoutines.Application.Queries;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Giglio.EduCore.DailyRoutines.Infrastructure.Persistence;
using Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Giglio.EduCore.DailyRoutines;

public static class DependencyInjection
{
    public static IServiceCollection AddDailyRoutinesModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddDbContext<DailyRoutinesDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IRoutineRepository, RoutineRepository>();
        services.AddScoped<IClassRoutineRepository, ClassRoutineRepository>();
        services.AddScoped<IDailyRoutineRecordRepository, DailyRoutineRecordRepository>();

        // Queries
        services.AddScoped<GetRoutineStatusQuery>();
        services.AddScoped<GetClassRoutinesForDateQuery>();

        return services;
    }
}
