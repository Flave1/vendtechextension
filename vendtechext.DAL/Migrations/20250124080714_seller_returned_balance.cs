using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class seller_returned_balance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SellerReturnedBalance",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerReturnedBalance",
                table: "Transactions");
        }
    }
}
