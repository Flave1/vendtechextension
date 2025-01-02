using iTextSharp.text.pdf;
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
                    worksheet.Cells[1, 1].Value = "DATE";
                    //worksheet.Cells[1, 2].Value = "INTEGRATOR";
                    //worksheet.Cells[1, 3].Value = "WALLETE ID";
                    worksheet.Cells[1, 2].Value = "TRANX ID";
                    worksheet.Cells[1, 3].Value = "VTECH ID";
                    worksheet.Cells[1, 4].Value = "STATUS";
                    worksheet.Cells[1, 5].Value = "METER";
                    worksheet.Cells[1, 6].Value = "BALANCE BEFORE";
                    worksheet.Cells[1, 7].Value = "AMOUNT";
                    worksheet.Cells[1, 8].Value = "BAL AFTER";

                    // Add data
                    for (int i = 0; i < transactions.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = transactions[i].Date;
                        //worksheet.Cells[i + 2, 2].Value = transactions[i].Integrator;
                        //worksheet.Cells[i + 2, 3].Value = transactions[i].WalletId;
                        worksheet.Cells[i + 2, 2].Value = transactions[i].TransactionId;
                        worksheet.Cells[i + 2, 3].Value = transactions[i].VendtechTransactionID;
                        worksheet.Cells[i + 2, 4].Value = transactions[i].Status;
                        worksheet.Cells[i + 2, 5].Value = transactions[i].MeterNumber;
                        worksheet.Cells[i + 2, 6].Value = transactions[i].BalanceBefore;
                        worksheet.Cells[i + 2, 7].Value = transactions[i].Amount;
                        worksheet.Cells[i + 2, 8].Value = transactions[i].BalanceAfter;
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
                var stream = new MemoryStream();
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add headers
                var table = new iTextSharp.text.pdf.PdfPTable(12);

                table.AddCell("DATE");
                //table.AddCell("INTEGRATOR");
                //table.AddCell("WALLETE ID");
                table.AddCell("TRANX ID");
                table.AddCell("VTECH ID");
                table.AddCell("STATUS");
                table.AddCell("METER");
                table.AddCell("BALANCE BEFORE");
                table.AddCell("AMOUNT");
                table.AddCell("BAL AFTER");

                // Add data rows
                foreach (var transaction in transactions)
                {
                    table.AddCell(transaction.Date);
                    //table.AddCell(transaction.Integrator);
                    //table.AddCell(transaction.WalletId);
                    table.AddCell(transaction.TransactionId);
                    table.AddCell(transaction.VendtechTransactionID);
                    table.AddCell(transaction.Status);
                    table.AddCell(transaction.MeterNumber);
                    table.AddCell(transaction.BalanceBefore.ToString());
                    table.AddCell(transaction.Amount.ToString());
                    table.AddCell(transaction.BalanceAfter.ToString());
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
