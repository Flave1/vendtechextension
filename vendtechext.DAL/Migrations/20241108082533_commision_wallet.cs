using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class commision_wallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionId",
                table: "Deposits");

            migrationBuilder.AddColumn<int>(
                name: "CommissionId",
                table: "Wallets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionId",
                table: "Wallets");

            migrationBuilder.AddColumn<int>(
                name: "CommissionId",
                table: "Deposits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
