using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Repository
{
    public class TransactionRepository
    {
        private readonly DataContext _context;

        public TransactionRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateTransactionLog(ElectricitySaleRequest request, string integratorId)
        {
            var trans = new TransactionsBuilder()
                .WithTransactionId(UniqueIDGenerator.NewTransactionId())
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTransactionUniqueId(request.TransactionId)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithCreatedAt(DateTime.Now)
                .WithAmount(request.Amount)
                .Build();

            _context.Transactions.Add(trans);
            await _context.SaveChangesAsync();
            return trans;
        }

        public async Task UpdateSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.SuccessResponse.Voucher.VendStatusDescription)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(executionResult.ReceivedFrom)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.FailedResponse.ErrorDetail)
                .WithTransactionStatus(TransactionStatus.Failed)
                .WithReceivedFrom(executionResult.ReceivedFrom)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task<Transaction> GetTransaction(string transactionId, string integratorid)
        {
            var trans = await _context.Transactions.FirstOrDefaultAsync(d => d.TransactionUniqueId == transactionId && d.IntegratorId == integratorid) ?? null;
            return trans;
        }

        public async Task InternalValidation(ElectricitySaleRequest request, string integratorid)
        {
            if (await _context.Transactions.AnyAsync(d => d.IntegratorId == integratorid && d.TransactionUniqueId == request.TransactionId))
                throw new BadRequestException("Transaction ID already exist for this terminal.");
        }
    }
}
