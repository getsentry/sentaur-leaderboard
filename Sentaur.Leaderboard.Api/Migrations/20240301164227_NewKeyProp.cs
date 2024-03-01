using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentaur.Leaderboard.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewKeyProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Key",
                table: "ScoreEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScoreEntries",
                table: "ScoreEntries",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ScoreEntries",
                table: "ScoreEntries");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ScoreEntries");
        }
    }
}
