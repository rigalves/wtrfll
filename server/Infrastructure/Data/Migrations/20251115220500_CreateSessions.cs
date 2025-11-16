using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wtrfll.Server.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20251115220500_CreateSessions")]

public partial class CreateSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Sessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ShortCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                ControllerJoinCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DisplayJoinCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SessionParticipants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                Role = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SessionParticipants", x => x.Id);
                table.ForeignKey(
                    name: "FK_SessionParticipants_Sessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "Sessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SessionParticipants_SessionId",
            table: "SessionParticipants",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_SessionParticipants_SessionId_Role",
            table: "SessionParticipants",
            columns: new[] { "SessionId", "Role" });

        migrationBuilder.CreateIndex(
            name: "IX_Sessions_ShortCode",
            table: "Sessions",
            column: "ShortCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SessionParticipants");

        migrationBuilder.DropTable(
            name: "Sessions");
    }
}
