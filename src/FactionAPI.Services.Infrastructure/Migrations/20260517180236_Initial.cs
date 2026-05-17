using System;
using System.Collections.Generic;
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
                name: "ApiTokens",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<byte[]>(type: "bytea", nullable: false),
                    ModIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    LegacyAll = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTokens", x => x.Name);
                });

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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FactionId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TextureName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supporters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryEntries",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Side = table.Column<string>(type: "text", nullable: true),
                    MinecraftVersion = table.Column<string>(type: "text", nullable: true),
                    ModVersion = table.Column<string>(type: "text", nullable: true),
                    ModCount = table.Column<int>(type: "integer", nullable: true),
                    ModId = table.Column<string>(type: "text", nullable: true)
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
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupporterAppearance", x => new { x.PlayerId, x.Key });
                    table.ForeignKey(
                        name: "FK_SupporterAppearance_Supporters_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Supporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryDependingMods",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DependingModId = table.Column<string>(type: "text", nullable: false),
                    ModId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryDependingMods", x => new { x.Timestamp, x.DependingModId });
                    table.ForeignKey(
                        name: "FK_TelemetryDependingMods_TelemetryEntries_Timestamp",
                        column: x => x.Timestamp,
                        principalTable: "TelemetryEntries",
                        principalColumn: "Timestamp",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryDependingMods_ModId_DependingModId",
                table: "TelemetryDependingMods",
                columns: new[] { "ModId", "DependingModId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTokens");

            migrationBuilder.DropTable(
                name: "ConfigValues");

            migrationBuilder.DropTable(
                name: "SupporterAppearance");

            migrationBuilder.DropTable(
                name: "TelemetryDependingMods");

            migrationBuilder.DropTable(
                name: "Supporters");

            migrationBuilder.DropTable(
                name: "TelemetryEntries");
        }
    }
}
