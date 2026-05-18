using Xunit;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EduCore.Financial.IntegrationTests.Postgres;

/// <summary>
/// Base class for PostgreSQL integration tests using Testcontainers.
/// Tests are skipped if Docker is not available.
/// </summary>
public abstract class PostgresTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("educore_test")
        .WithUsername("test")
        .WithPassword("test123")
        .Build();

    protected FinancialDbContext DbContext { get; private set; } = null!;
    protected string ConnectionString => _container.GetConnectionString();

    public virtual async Task InitializeAsync()
    {
        if (!CheckDockerAvailable())
        {
            throw new SkipException("Docker is not available. Skipping PostgreSQL integration tests.");
        }

        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        DbContext = new FinancialDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (DbContext is not null)
            await DbContext.DisposeAsync();

        if (_container is not null)
            await _container.DisposeAsync();
    }

    private static bool CheckDockerAvailable()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

public class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
}
