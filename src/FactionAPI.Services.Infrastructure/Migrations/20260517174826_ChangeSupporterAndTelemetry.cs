using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactionAPI.Services.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSupporterAndTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupporterAppearance_Supporters_PlayerId",
                table: "SupporterAppearance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supporters",
                table: "Supporters");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "Supporters",
                newName: "TextureName");

            migrationBuilder.AddColumn<List<string>>(
                name: "DependingMods",
                table: "TelemetryEntries",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ModId",
                table: "TelemetryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Supporters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerId",
                table: "SupporterAppearance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supporters",
                table: "Supporters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupporterAppearance_Supporters_PlayerId",
                table: "SupporterAppearance",
                column: "PlayerId",
                principalTable: "Supporters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupporterAppearance_Supporters_PlayerId",
                table: "SupporterAppearance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supporters",
                table: "Supporters");

            migrationBuilder.DropColumn(
                name: "DependingMods",
                table: "TelemetryEntries");

            migrationBuilder.DropColumn(
                name: "ModId",
                table: "TelemetryEntries");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Supporters");

            migrationBuilder.RenameColumn(
                name: "TextureName",
                table: "Supporters",
                newName: "PlayerId");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerId",
                table: "SupporterAppearance",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supporters",
                table: "Supporters",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupporterAppearance_Supporters_PlayerId",
                table: "SupporterAppearance",
                column: "PlayerId",
                principalTable: "Supporters",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
