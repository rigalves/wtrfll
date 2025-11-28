using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Slices.Passages.Application;

namespace Wtrfll.Server.Tests.Common;

public sealed class PassagesApiFactory : WebApplicationFactory<Program>
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
            // Use in-memory SQLite for required DbContext.
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Replace passage provider with deterministic fake.
            services.RemoveAll(typeof(IPassageProvider));
            services.AddSingleton<IPassageProvider>(_ => new FakePassageProvider());

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
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
