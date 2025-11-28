using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Wtrfll.Server.Infrastructure.Data;

namespace Wtrfll.Server.Tests.Common;

public sealed class SqliteTestDb : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext DbContext { get; }

    private SqliteTestDb(SqliteConnection connection, AppDbContext dbContext)
    {
        _connection = connection;
        DbContext = dbContext;
    }

    public static async Task<SqliteTestDb> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new SqliteTestDb(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
