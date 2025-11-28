using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Slices.BibleBooks.Endpoints;
using Wtrfll.Server.Slices.Bibles.Endpoints;
using Wtrfll.Server.Modules.Health;
using Wtrfll.Server.Slices.Passages.Endpoints;
using Wtrfll.Server.Slices.Sessions.Endpoints;
using Wtrfll.Server.Slices.Lyrics.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.AddSessionsModule();
builder.Services.AddBiblesModule(builder.Configuration);
builder.Services.AddPassagesModule(builder.Configuration);
builder.Services.AddBibleBooksModule(builder.Configuration);
builder.Services.AddLyricsModule();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
#if DEBUG
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine);
#endif
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthEndpoints();
app.MapBiblesEndpoints();
app.MapBibleBooksEndpoints();
app.MapPassageEndpoints();
app.MapSessionsEndpoints();
app.MapSessionsRealtimeEndpoints();
app.MapLyricsEndpoints();

app.Run();

public partial class Program { }

