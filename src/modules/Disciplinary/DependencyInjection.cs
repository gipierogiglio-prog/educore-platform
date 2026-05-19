using Giglio.EduCore.Disciplinary.Domain.Interfaces;
using Giglio.EduCore.Disciplinary.Infrastructure.Persistence;
using Giglio.EduCore.Disciplinary.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Giglio.EduCore.Disciplinary;

public static class DependencyInjection
{
    public static IServiceCollection AddDisciplinaryModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddDbContext<DisciplinaryDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IDisciplinaryIncidentRepository, DisciplinaryIncidentRepository>();

        return services;
    }
}
