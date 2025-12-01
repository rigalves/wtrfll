using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Slices.Lyrics.Application;
using Wtrfll.Server.Slices.Lyrics.Application.Models;

// Assume we are running from tools/ChordImport/bin/...; navigate to the repo root.
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var serverConfigPath = Path.Combine(solutionRoot, "server");

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.SetBasePath(serverConfigPath);
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((ctx, services) =>
    {
        var rawConnectionString = ctx.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var csb = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(rawConnectionString);
        if (!Path.IsPathRooted(csb.DataSource))
        {
            csb.DataSource = Path.GetFullPath(Path.Combine(serverConfigPath, csb.DataSource));
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(csb.ToString()));
        services.AddScoped<LyricsReadService>();
    });

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.EnsureCreatedAsync();
var lyricsService = scope.ServiceProvider.GetRequiredService<LyricsReadService>();

// Reuse solutionRoot for default import folder to avoid double "tools" nesting.
var defaultFolder = Path.Combine(solutionRoot, "tools", "ChordImport", "imports");
Directory.CreateDirectory(defaultFolder);

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : defaultFolder;
Console.WriteLine(args.Length > 0
    ? $"Importing chord files from: {root}"
    : $"No folder argument supplied. Importing chord files from default: {root}");

if (!Directory.Exists(root))
{
    Console.WriteLine($"Folder not found: {root}");
    return;
}

var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".chord", ".cho", ".pro", ".chopro", ".chordpro"
};

var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
    .Where(f => allowedExtensions.Contains(Path.GetExtension(f)))
    .ToList();

if (files.Count == 0)
{
    Console.WriteLine("No chord files found.");
    return;
}

var imported = 0;
foreach (var file in files)
{
    try
    {
        var title = Path.GetFileNameWithoutExtension(file);
        var content = await File.ReadAllTextAsync(file);
        var request = new UpsertLyricsEntryRequest(title, null, content);
        await lyricsService.SaveLyricsAsync(null, request, CancellationToken.None);
        imported++;
        Console.WriteLine($"Imported: {title}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to import {file}: {ex.Message}");
    }
}

Console.WriteLine($"Done. Imported {imported} file(s) from {root}.");

// Flush WAL so tools/viewers that ignore WAL still see the imported rows.
await db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");
