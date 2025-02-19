using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class payment_method_onDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Deposits_PaymentTypeId",
                table: "Deposits",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_PaymentMethod_PaymentTypeId",
                table: "Deposits",
                column: "PaymentTypeId",
                principalTable: "PaymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_PaymentMethod_PaymentTypeId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_PaymentTypeId",
                table: "Deposits");
        }
    }
}
