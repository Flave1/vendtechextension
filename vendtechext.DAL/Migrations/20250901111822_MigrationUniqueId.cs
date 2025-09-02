using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MigrationUniqueId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MigrationUniqueId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigrationUniqueId",
                table: "Users");
        }
    }
}
