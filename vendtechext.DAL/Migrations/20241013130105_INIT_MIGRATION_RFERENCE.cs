using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class INIT_MIGRATION_RFERENCE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "IntegratorId",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegratorId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "Integrators",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IntegratorId",
                table: "Integrators",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegratorId",
                table: "Deposits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets",
                column: "IntegratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IntegratorId",
                table: "Transactions",
                column: "IntegratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrators_AppUserId",
                table: "Integrators",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_IntegratorId",
                table: "Deposits",
                column: "IntegratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Integrators_IntegratorId",
                table: "Deposits",
                column: "IntegratorId",
                principalTable: "Integrators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Integrators_Users_AppUserId",
                table: "Integrators",
                column: "AppUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Integrators_IntegratorId",
                table: "Transactions",
                column: "IntegratorId",
                principalTable: "Integrators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Integrators_IntegratorId",
                table: "Wallets",
                column: "IntegratorId",
                principalTable: "Integrators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Integrators_IntegratorId",
                table: "Deposits");

            migrationBuilder.DropForeignKey(
                name: "FK_Integrators_Users_AppUserId",
                table: "Integrators");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Integrators_IntegratorId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Integrators_IntegratorId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_IntegratorId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_IntegratorId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Integrators_AppUserId",
                table: "Integrators");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_IntegratorId",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "IntegratorId",
                table: "Integrators");

            migrationBuilder.AlterColumn<string>(
                name: "IntegratorId",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "IntegratorId",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "Integrators",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntegratorId",
                table: "Deposits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
