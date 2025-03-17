using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCommisionDepsitAsReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommissionDepositId",
                table: "Deposits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_CommissionDepositId",
                table: "Deposits",
                column: "CommissionDepositId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Deposits_CommissionDepositId",
                table: "Deposits",
                column: "CommissionDepositId",
                principalTable: "Deposits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Deposits_CommissionDepositId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_CommissionDepositId",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "CommissionDepositId",
                table: "Deposits");
        }
    }
}
