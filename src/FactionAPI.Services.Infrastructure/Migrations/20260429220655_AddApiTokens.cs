using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactionAPI.Services.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiTokens : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTokens");
        }
    }
}
