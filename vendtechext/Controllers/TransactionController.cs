using Microsoft.AspNetCore.Mvc;
using signalrserver.Models.DTO;
using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly DataContext ctx;

        public TransactionController(ILogger<TransactionController> logger, DataContext dataContext)
        {
            _logger = logger;
            ctx = dataContext;
        }


        [HttpPost("check-tranx-by-token", Name = "check-tranx-by-token")]
        public IActionResult check([FromBody] checkTrxDTO request)
        {
            var prevtrx = ctx.TransactionDetails.FirstOrDefault(d => d.MeterToken1 == request.token);
            if (prevtrx == null)
            {
                return NotFound(new { results = "TRANSACTION NOT FOUND" });
            }

            return Ok(new { result = prevtrx });
        }

        [HttpPost("check-tranx-by-transactionId", Name = "check-tranx-by-transactionId")]
        public IActionResult check1([FromBody] checkTrxDTO1 request)
        {
            var prevtrx = ctx.TransactionDetails.FirstOrDefault(d => d.TransactionId == request.transactionId);
            if (prevtrx == null)
            {
                return NotFound(new { result = "TRANSACTION NOT FOUND" });
            }

            return Ok(new { result = prevtrx });
        }

        [HttpPost("insert-tranx", Name = "insert-tranx")]
        public async Task<IActionResult> Insertrx([FromBody] TransactionDTO request)
        {
            try
            {
                var prevtrx = ctx.TransactionDetails.FirstOrDefault(d => d.Posid == request.POSId);
                if (prevtrx == null)
                {
                    return BadRequest(new { result = "unable to insert transaction".ToUpper() });
                }
                var tranxExist = ctx.TransactionDetails.Any(d => d.TransactionId == request.TransactionId);
                if (tranxExist)
                {
                    return BadRequest(new { result = "transaction with similar ID already exist".ToUpper() });
                }

                var tranxBytokenExist = ctx.TransactionDetails.Any(d => d.MeterToken1 == request.MeterToken1);
                if (tranxBytokenExist)
                {
                    return BadRequest(new { result = "transaction with similar voucher number already exist".ToUpper() });
                }

                var pos = ctx.Pos.FirstOrDefault(d => d.Posid == request.POSId);
                if (pos == null)
                {
                    return BadRequest(new { result = "POS with this ID does not exist".ToUpper() });
                }

                if (request.Status == 2)
                {
                    await Insert_transaction_without_deducting_vendor_balance(request, pos);
                }
                else if (request.Status == 1)
                {
                    await Insert_transaction_and_deduct_vendor_balance(request, pos);
                }

                return Ok(new { result = "Successfull inserted transaction".ToUpper() });
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex.Message);
                return StatusCode(500, new { result = "An internal server error occurred" });
            }
        }

        private async Task Insert_transaction_without_deducting_vendor_balance(TransactionDTO tx, Po pos)
        {

            var obj = StackTx(tx, pos);
            ctx.TransactionDetails.Add(obj);
            await ctx.SaveChangesAsync();
        }

        private async Task Insert_transaction_and_deduct_vendor_balance(TransactionDTO tx, Po pos)
        {
            var obj = StackTx(tx, pos);
            ctx.TransactionDetails.Add(obj);
            await ctx.SaveChangesAsync();
            await Deductbalace(obj, pos);
        }

        private TransactionDetail StackTx(TransactionDTO tx, Po pos)
        {
            return new TransactionDetail
            {
                Status = tx.Status,
                AccountNumber = tx.AccountNumber,
                Amount = tx.Amount,
                BalanceBefore = tx.BalanceBefore,
                CostOfUnits = tx.CostOfUnits,
                CreatedAt = Convert.ToDateTime(tx.CreatedAt),
                CurrentDealerBalance = tx.CurrentDealerBalance,
                CurrentVendorBalance = tx.CurrentVendorBalance,
                Customer = tx.Customer,
                CustomerAddress = tx.CustomerAddress,
                DateAndTimeFinalised = tx.CreatedAt,
                DateAndTimeLinked = tx.CreatedAt,
                DateAndTimeSold = tx.CreatedAt,
                DebitRecovery = tx.DebitRecovery,
                Finalised = true,
                IsDeleted = false,
                MeterId = tx.MeterId != null && tx.MeterId.Value > 0 ? tx.MeterId.Value : null,
                MeterNumber1 = tx.MeterNumber1,
                MeterToken1 = tx.MeterToken1,
                PlatFormId = 1,
                Posid = tx.POSId,
                QueryStatusCount = 0,
                ReceiptNumber = tx.ReceiptNumber,
                Request = "",
                RequestDate = Convert.ToDateTime(tx.CreatedAt),
                Response = "",
                //RTSUniqueID = tx.RTSUniqueID,
                SerialNumber = tx.SerialNumber,
                ServiceCharge = tx.ServiceCharge,
                Sold = true,
                StatusRequestCount = 1,
                StatusResponse = "",
                Tariff = tx.Tariff,
                TaxCharge = tx.TaxCharge,
                TenderedAmount = tx.Amount,
                TransactionAmount = tx.Amount,
                TransactionId = tx.TransactionId,
                Units = tx.Units,
                UserId = (long)pos.VendorId,
                VendStatus = "",
                VendStatusDescription = "",
                VoucherSerialNumber = tx.VoucherSerialNumber,
                //VProvider = tx.VProvider,

            };
        }
        async Task Deductbalace(TransactionDetail trans, Po pos)
        {
            pos.Balance = pos.Balance - trans.Amount;
            ctx.Pos.Update(pos);
            await ctx.SaveChangesAsync();
        }

    }
}