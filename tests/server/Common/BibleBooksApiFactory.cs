using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wtrfll.Server.Slices.BibleBooks.Application;
using Wtrfll.Server.Slices.BibleBooks.Infrastructure;
using Wtrfll.Server.Tests.Common;

namespace Wtrfll.Server.Tests.Common;

public sealed class BibleBooksApiFactory : WebApplicationFactory<Program>
{
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
            // Replace filesystem store with a deterministic fake.
            services.RemoveAll(typeof(IBibleBookMetadataStore));
            services.AddSingleton<IBibleBookMetadataStore, FakeBibleBookMetadataStore>();
            // Remove options dependency since fake does not need files.
            services.RemoveAll(typeof(IConfigureOptions<BibleBookMetadataOptions>));
            services.RemoveAll(typeof(BibleBookMetadataOptions));
        });
    }
}
