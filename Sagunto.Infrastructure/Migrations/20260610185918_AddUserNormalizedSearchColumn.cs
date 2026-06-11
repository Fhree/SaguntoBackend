using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sagunto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNormalizedSearchColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "normalized_search",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "normalized_search",
                table: "users");
        }
    }
}
