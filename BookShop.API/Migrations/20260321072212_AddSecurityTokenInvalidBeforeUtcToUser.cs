using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityTokenInvalidBeforeUtcToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SecurityTokenInvlaidBeforeUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "SecurityTokenInvlaidBeforeUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityTokenInvlaidBeforeUtc",
                table: "Users");
        }
    }
}
