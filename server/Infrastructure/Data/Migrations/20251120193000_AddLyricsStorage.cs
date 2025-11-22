using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wtrfll.Server.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20251120193000_AddLyricsStorage")]
public partial class AddLyricsStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LyricsEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Author = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                LyricsChordPro = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LyricsEntries", x => x.Id);
            });

        migrationBuilder.Sql("""
INSERT INTO "LyricsEntries" ("Id","Title","Author","LyricsChordPro","CreatedAt","UpdatedAt") VALUES
('11111111-2222-4333-8444-555555555555','Digno y Santo','Tradicional','{title: Digno y Santo}
{comment: Verso}
Digno y santo
Cordero de Dios
Hijo del Padre

{comment: Coro}
{start_of_lyrics}
[Santo] santo santo
Dios todopoderoso
Quien fue y quien es y quien vendrá
{end_of_lyrics}','2025-11-20T00:00:00Z',NULL),
('22222222-3333-4444-8555-666666666666','Mi Roca','Living Streams','{title: Mi Roca}
{comment: Verso}
Cuando la noche cae
Y siento temor
Tú eres mi paz

{comment: Puente}
Tú eres fiel
Nunca fallarás
{comment: Coro}
Mi roca firme en la tempestad','2025-11-20T00:00:00Z',NULL);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LyricsEntries");
    }
}
