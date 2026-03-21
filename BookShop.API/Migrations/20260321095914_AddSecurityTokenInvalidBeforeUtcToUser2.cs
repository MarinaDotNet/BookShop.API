using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityTokenInvalidBeforeUtcToUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecurityTokenInvlaidBeforeUtc",
                table: "Users",
                newName: "SecurityTokenInvalidBeforeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecurityTokenInvalidBeforeUtc",
                table: "Users",
                newName: "SecurityTokenInvlaidBeforeUtc");
        }
    }
}
