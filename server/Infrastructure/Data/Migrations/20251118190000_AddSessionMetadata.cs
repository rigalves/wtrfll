using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wtrfll.Server.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20251118190000_AddSessionMetadata")]
public partial class AddSessionMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "Sessions",
            type: "TEXT",
            maxLength: 160,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "ScheduledAt",
            table: "Sessions",
            type: "TEXT",
            nullable: true);

        migrationBuilder.Sql(
            "UPDATE \"Sessions\" SET \"Name\" = CASE WHEN ifnull(trim(\"Name\"), '') = '' THEN 'Session ' || \"ShortCode\" ELSE \"Name\" END;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Name",
            table: "Sessions");

        migrationBuilder.DropColumn(
            name: "ScheduledAt",
            table: "Sessions");
    }
}
