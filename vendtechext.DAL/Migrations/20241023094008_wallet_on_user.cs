using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class wallet_on_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets",
                column: "IntegratorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets",
                column: "IntegratorId");
        }
    }
}
