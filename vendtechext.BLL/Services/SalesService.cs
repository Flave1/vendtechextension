using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.Helper;
using vendtechext.BLL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class SalesService: BaseService, ISalesService
    {
        private readonly TransactionRepository _repository;
        public SalesService(TransactionRepository transactionRepository)
        {
            _repository = transactionRepository;
        }

        public async Task<APIResponse> GetSalesReportAsync(PaginatedSearchRequest req)
        {
            var query = _repository.GetSalesTransactionQuery(req.Status, req.IsClaimedStatus);

            query = FilterQuery(req, query);

            int totalRecords = await query.CountAsync();

            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);

            List<TransactionDto> transactions = await query.Select(d => new TransactionDto(d)).ToListAsync();

            PagedResponse<TransactionDto> result = new PagedResponse<TransactionDto>(transactions, totalRecords, req.PageNumber, req.PageSize);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }
        public async Task<List<TransactionExportDto>> GetSalesReportForExportAsync(PaginatedSearchRequest req)
        {
            IQueryable<Transaction> query = _repository.GetSalesTransactionQuery(req.Status, req.IsClaimedStatus);

            query = FilterQuery(req, query);

            return await query.Select(d => new TransactionExportDto(d)).ToListAsync();
        }

        private IQueryable<Transaction> FilterQuery(PaginatedSearchRequest req, IQueryable<Transaction> query)
        {

            if (req.IntegratorId != null)
                query = query.Where(d => d.IntegratorId == req.IntegratorId);

            if (!string.IsNullOrEmpty(req.From))
            {
                var fromDate = DateTime.Parse(req.From).Date;
                query = query.Where(p => p.CreatedAt.Date >= fromDate);
            }

            if (!string.IsNullOrEmpty(req.To))
            {
                var toDate = DateTime.Parse(req.To).Date;
                query = query.Where(p => p.CreatedAt.Date <= toDate);
            }

            if (Utils.IsAscending(req.SortOrder))
                query = query.OrderBy(d => d.CreatedAt);
            else
                query = query.OrderByDescending(d => d.CreatedAt);

            if (!string.IsNullOrEmpty(req.SortValue))
            {
                if (req.SortBy == "TRANSACTION_ID")
                    query = query.Where(d => d.TransactionUniqueId.Contains(req.SortValue));
                else if (req.SortBy == "AMOUNT")
                    query = query.Where(d => d.Amount.ToString().Contains(req.SortValue));
                else if (req.SortBy == "METER_NUMBER")
                    query = query.Where(d => d.MeterNumber.Contains(req.SortValue));
                else if (req.SortBy == "WALLET_ID")
                    query = query.Where(d => d.Integrator.Wallet.WALLET_ID.Contains(req.SortValue));
                else if (req.SortBy == "INTEGRATOR")
                    query = query.Where(d => d.Integrator.BusinessName.Contains(req.SortValue));
            }
            return query;
        }
    }
}
