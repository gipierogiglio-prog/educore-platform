using Giglio.EduCore.Academic.Domain.Interfaces;
using Giglio.EduCore.Academic.Infrastructure.Persistence;
using Giglio.EduCore.Academic.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Giglio.EduCore.Academic;

public static class DependencyInjection
{
    public static IServiceCollection AddAcademicModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddDbContext<AcademicDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ISeriesRepository, SeriesRepository>();
        services.AddScoped<ICurriculumMatrixRepository, CurriculumMatrixRepository>();

        return services;
    }
}