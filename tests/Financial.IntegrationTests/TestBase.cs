using Xunit;
using Giglio.EduCore.Financial.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Financial.IntegrationTests;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly FinancialDbContext DbContext;

    protected TestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new FinancialDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Close();
    }
}
