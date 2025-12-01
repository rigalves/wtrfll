using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wtrfll.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LyricsStyleJson",
                table: "LyricsEntries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LyricsStyleJson",
                table: "LyricsEntries");
        }
    }
}
