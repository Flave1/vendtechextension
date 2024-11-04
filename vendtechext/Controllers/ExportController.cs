using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("export/v1/")]
    [Authorize]
    public class ExportController : BaseController
    {
        private readonly ISalesService _salesService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDepositService _depositService;

        public ExportController(ILogger<APISalesController> logger, ISalesService salesService, IHttpContextAccessor contextAccessor, IDepositService depositService) : base(logger)
        {
            _salesService = salesService;
            _contextAccessor = contextAccessor;
            _depositService = depositService;
        }


        [HttpPost("export-sales-report/{format}")]
        public async Task<IActionResult> ExportSalesReport([FromBody] PaginatedSearchRequest request, string format)
        {
            if (User.IsInRole("Integrator"))
            {
                var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
                request.IntegratorId = integrator_id;
            }

            List<TransactionExportDto> transactions = await _salesService.GetSalesReportForExportAsync(request);

            if (format == "excel")
            {
                // Generate Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Transactions");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Transaction Unique ID";
                    worksheet.Cells[1, 2].Value = "Vendtech Transaction ID";
                    worksheet.Cells[1, 3].Value = "Balance Before";
                    worksheet.Cells[1, 4].Value = "Amount";
                    worksheet.Cells[1, 5].Value = "Balance After";
                    worksheet.Cells[1, 6].Value = "Meter Number";
                    worksheet.Cells[1, 7].Value = "Transaction Status";
                    worksheet.Cells[1, 8].Value = "Vend Status Description";
                    worksheet.Cells[1, 9].Value = "Request";
                    worksheet.Cells[1, 10].Value = "Response";
                    worksheet.Cells[1, 11].Value = "Date";
                    worksheet.Cells[1, 12].Value = "Is Claimed";

                    // Add data
                    for (int i = 0; i < transactions.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = transactions[i].TransactionUniqueId;
                        worksheet.Cells[i + 2, 2].Value = transactions[i].VendtechTransactionID;
                        worksheet.Cells[i + 2, 3].Value = transactions[i].BalanceBefore;
                        worksheet.Cells[i + 2, 4].Value = transactions[i].Amount;
                        worksheet.Cells[i + 2, 5].Value = transactions[i].BalanceAfter;
                        worksheet.Cells[i + 2, 6].Value = transactions[i].MeterNumber;
                        worksheet.Cells[i + 2, 7].Value = transactions[i].TransactionStatus;
                        worksheet.Cells[i + 2, 8].Value = transactions[i].VendStatusDescription;
                        worksheet.Cells[i + 2, 9].Value = transactions[i].Request;
                        worksheet.Cells[i + 2, 10].Value = transactions[i].Response;
                        worksheet.Cells[i + 2, 11].Value = transactions[i].Date;
                        worksheet.Cells[i + 2, 12].Value = transactions[i].IsClaimed;
                    }

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Transactions.xlsx");
                }
            }
            else if (format == "pdf")
            {
                // Generate PDF
                var stream = new MemoryStream();  // Do not use "using" here
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add headers
                var table = new iTextSharp.text.pdf.PdfPTable(12);
                table.AddCell("Transaction Unique ID");
                table.AddCell("Vendtech Transaction ID");
                table.AddCell("Balance Before");
                table.AddCell("Amount");
                table.AddCell("Balance After");
                table.AddCell("Meter Number");
                table.AddCell("Transaction Status");
                table.AddCell("Vend Status Description");
                table.AddCell("Request");
                table.AddCell("Response");
                table.AddCell("Date");
                table.AddCell("Is Claimed");

                // Add data rows
                foreach (var transaction in transactions)
                {
                    table.AddCell(transaction.TransactionUniqueId);
                    table.AddCell(transaction.VendtechTransactionID);
                    table.AddCell(transaction.BalanceBefore.ToString());
                    table.AddCell(transaction.Amount.ToString());
                    table.AddCell(transaction.BalanceAfter.ToString());
                    table.AddCell(transaction.MeterNumber);
                    table.AddCell(transaction.TransactionStatus.ToString());
                    table.AddCell(transaction.VendStatusDescription);
                    table.AddCell(transaction.Request);
                    table.AddCell(transaction.Response);
                    table.AddCell(transaction.Date.ToString());
                    table.AddCell(transaction.IsClaimed.ToString());
                }

                document.Add(table);
                document.Close(); // Only close the document

                stream.Position = 0;
                return File(stream, "application/pdf", "Transactions.pdf");
            }


            return BadRequest("Invalid format specified.");
        }


        [HttpPost("export-deposits-report/{format}")]
        public async Task<IActionResult> ExportDepositToExcel([FromBody] PaginatedSearchRequest request, string format)
        {
            if (User.IsInRole("Integrator"))
            {
                var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
                request.IntegratorId = integrator_id;
            }
            List<DepositExcelDto> transactions = await _depositService.GetDepositReportForExportAsync(request);

            if (format == "excel")
            {
                // Generate Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Transactions");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Date";
                    worksheet.Cells[1, 2].Value = "Wallet ID";
                    worksheet.Cells[1, 3].Value = "Reference";
                    worksheet.Cells[1, 4].Value = "Payment Type";
                    worksheet.Cells[1, 5].Value = "Transaction ID";
                    worksheet.Cells[1, 6].Value = "Balance Before";
                    worksheet.Cells[1, 7].Value = "Amount";
                    worksheet.Cells[1, 8].Value = "Balance After";

                    // Add data
                    for (int i = 0; i < transactions.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = transactions[i].Date;
                        worksheet.Cells[i + 2, 2].Value = transactions[i].WalletId;
                        worksheet.Cells[i + 2, 3].Value = transactions[i].Reference;
                        worksheet.Cells[i + 2, 4].Value = transactions[i].PaymentTypeName;
                        worksheet.Cells[i + 2, 5].Value = transactions[i].TransactionId;
                        worksheet.Cells[i + 2, 6].Value = transactions[i].BalanceBefore;
                        worksheet.Cells[i + 2, 7].Value = transactions[i].Amount;
                        worksheet.Cells[i + 2, 8].Value = transactions[i].BalanceAfter;
                    }

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Deposits.xlsx");
                }
            }
            else if (format == "pdf")
            {
                // Generate PDF
                var stream = new MemoryStream();  // Do not use "using" here
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add headers
                var table = new iTextSharp.text.pdf.PdfPTable(8);
                table.AddCell("Date");
                table.AddCell("Wallet ID");
                table.AddCell("Reference");
                table.AddCell("Payment Type");
                table.AddCell("Transaction ID");
                table.AddCell("Balance Before");
                table.AddCell("Amount");
                table.AddCell("Balance After");

                // Add data rows
                foreach (var transaction in transactions)
                {
                    table.AddCell(transaction.Date.ToString());
                    table.AddCell(transaction.WalletId);
                    table.AddCell(transaction.Reference);
                    table.AddCell(transaction.PaymentTypeName);
                    table.AddCell(transaction.TransactionId);
                    table.AddCell(transaction.BalanceBefore.ToString());
                    table.AddCell(transaction.Amount.ToString());
                    table.AddCell(transaction.BalanceAfter.ToString());
                }

                document.Add(table);
                document.Close(); // Close document only

                stream.Position = 0;
                return File(stream, "application/pdf", "Deposits.pdf");
            }

            return BadRequest("Invalid format specified.");
        }

    }

}
