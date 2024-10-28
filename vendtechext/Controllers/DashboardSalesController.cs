using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("sales/v1/")]
    [Authorize]
    public class DashboardSalesController : BaseController
    {
        private readonly ISalesService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public DashboardSalesController(ILogger<APISalesController> logger, ISalesService salesService, IHttpContextAccessor contextAccessor) : base(logger)
        {
            _service = salesService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("get-report")]
        public async Task<IActionResult> GetSalesReport([FromBody] PaginatedSearchRequest request)
        {

            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            APIResponse reponse = await _service.GetSalesReportAsync(request);
            return Ok(reponse);
        }

        [HttpPost("export-report")]
        public async Task<IActionResult> ExportToExcel(PaginatedSearchRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            List<TransactionExportDto> transactions  = await _service.GetSalesReportForExportAsync(request);
            
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

                // Return the Excel file
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Transactions.xlsx");
            }
        }

    }
}
