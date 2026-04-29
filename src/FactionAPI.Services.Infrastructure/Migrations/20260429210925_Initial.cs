using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactionAPI.Services.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigValues",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigValues", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Supporters",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    FactionId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supporters", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryEntries",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Side = table.Column<string>(type: "text", nullable: true),
                    MinecraftVersion = table.Column<string>(type: "text", nullable: true),
                    ModVersion = table.Column<string>(type: "text", nullable: true),
                    ModCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryEntries", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "SupporterAppearance",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupporterAppearance", x => new { x.PlayerId, x.Key });
                    table.ForeignKey(
                        name: "FK_SupporterAppearance_Supporters_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Supporters",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigValues");

            migrationBuilder.DropTable(
                name: "SupporterAppearance");

            migrationBuilder.DropTable(
                name: "TelemetryEntries");

            migrationBuilder.DropTable(
                name: "Supporters");
        }
    }
}
