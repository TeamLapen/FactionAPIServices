using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactionAPI.Services.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Supporters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Supporters",
                type: "integer",
                nullable: true);
        }
    }
}
