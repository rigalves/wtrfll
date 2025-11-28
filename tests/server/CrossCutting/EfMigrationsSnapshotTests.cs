using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.CrossCutting;

public sealed class EfMigrationsSnapshotTests
{
    [Fact]
    public async Task Model_matches_snapshot()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var model = db.DbContext.Model;
        var differences = model.GetEntityTypes().SelectMany(et => et.GetProperties()).Where(p => p.IsShadowProperty()).ToList();
        differences.Should().BeEmpty();
    }
}
