using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class SalesService: BaseService, ISalesService
    {
        private readonly TransactionRepository _repository;
        public SalesService(
            TransactionRepository transactionRepository)
        {
            _repository = transactionRepository;
        }

        public async Task<APIResponse> GetSalesReportAsync(PaginatedSearchRequest request)
        {
            var transationsQuery = _repository.GetSalesTransactionQuery(request.Status, request.ClaimedStatus, request.IntegratorId);

            var totalRecords = await transationsQuery.CountAsync();

            if (!string.IsNullOrEmpty(request.TransactionId))
            {
                transationsQuery = transationsQuery.Where(d => d.TransactionUniqueId == request.TransactionId);
            }

            var transactions = await transationsQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new TransactionDto(d))
                .ToListAsync();

            var result = new PagedResponse<TransactionDto>(transactions, totalRecords, request.PageNumber, request.PageSize);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }
    }
}
