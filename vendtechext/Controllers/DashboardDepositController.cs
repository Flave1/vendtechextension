using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("deposit/v1/")]
    [Authorize]
    public class DashboardDepositController : ControllerBase
    {
        private readonly IDepositService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor)
        {
            _service = depositService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepositRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value?? "");
            var result = await _service.CreateDeposit(request, integrator_id);
            return Ok(result);
        }
        [HttpPost("get")]
        public async Task<IActionResult> GetDeposits([FromBody] PaginatedSearchRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            var result = await _service.GetIntegratorDeposits(request);
            return Ok(result);
        }


        [HttpPost("export-report")]
        public async Task<IActionResult> ExportToExcel(PaginatedSearchRequest request)
        {
            if (User.IsInRole("Integrator"))
            {
                var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
                request.IntegratorId = integrator_id;
            }
            List<DepositExcelDto> transactions = await _service.GetDepositReportForExportAsync(request);

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

                // Return the Excel file
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Deposits.xlsx");
            }
        }
   
    }
}