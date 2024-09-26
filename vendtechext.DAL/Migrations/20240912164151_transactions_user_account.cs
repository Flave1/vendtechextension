using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vendtechext.DAL.Migrations
{
    /// <inheritdoc />
    public partial class transactions_user_account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Clientkey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntegratorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentDealerBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenderedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MeterNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatus = table.Column<int>(type: "int", nullable: false),
                    ClaimedStatus = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PlatFormId = table.Column<int>(type: "int", nullable: false),
                    MeterToken1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeterToken2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeterToken3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RTSUniqueID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceCharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tariff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostOfUnits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Units = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DebitRecovery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Finalised = table.Column<bool>(type: "bit", nullable: false),
                    StatusRequestCount = table.Column<int>(type: "int", nullable: false),
                    Sold = table.Column<bool>(type: "bit", nullable: false),
                    VoucherSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendStatusDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAndTimeSold = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAndTimeFinalised = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAndTimeLinked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryStatusCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessUsers");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
