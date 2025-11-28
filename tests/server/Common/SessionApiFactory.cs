using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Slices.Passages.Application;

namespace Wtrfll.Server.Tests.Common;

public sealed class SessionApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.None);
        });
        builder.ConfigureServices(services =>
        {
            // Replace the real database with an in-memory SQLite instance.
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Replace passage providers with a deterministic fake to keep realtime tests simple.
            services.RemoveAll(typeof(IPassageProvider));
            services.AddSingleton<IPassageProvider>(_ => new FakePassageProvider());

            // Build and initialize the database schema for the test server.
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
