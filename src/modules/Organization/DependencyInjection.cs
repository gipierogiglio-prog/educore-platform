using Giglio.EduCore.Organization.Domain.Interfaces;
using Giglio.EduCore.Organization.Infrastructure.Persistence;
using Giglio.EduCore.Organization.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Giglio.EduCore.Organization;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ISchoolUnitRepository, SchoolUnitRepository>();

        return services;
    }
}