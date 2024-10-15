using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class add_logo_integrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Integrators",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Integrators");
        }
    }
}
