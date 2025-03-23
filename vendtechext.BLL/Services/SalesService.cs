using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.Helper;
using vendtechext.DAL.Models;
using vendtechext.BLL.Exceptions;
using Newtonsoft.Json;
using vendtechext.DAL.Common;

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

            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GetSingleTransactionAsync(SingleTransation req)
        {
            ExecutionResult executionResult = null;
            List<RequestResponse> requestResponses = new List<RequestResponse>();
            var transaction = await _repository.GetSaleTransaction(req.TransactionId, req.Integratorid);
            if (transaction == null)
                throw new BadRequestException("Transaction not found found");


            var initial_request = new ElectricitySaleRequest
            {
                Amount = transaction.Amount,
                MeterNumber = transaction.MeterNumber,
                TransactionId = transaction.TransactionUniqueId
            };
            executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
            executionResult.successResponse.UpdateResponseForStatusQuery(transaction);
            var requestResponse1 = new RequestResponse(initial_request, executionResult);
            requestResponses.Add(requestResponse1);

            if (req.IsAdmin)
            {
                if (!string.IsNullOrEmpty(transaction.Request) && !string.IsNullOrEmpty(transaction.Response))
                {
                    if(transaction.TransactionStatus == (int)TransactionStatus.Success && transaction.ReceivedFrom == "rts_init")
                    {
                        RTSRequestmodel rTSRequest = JsonConvert.DeserializeObject<RTSRequestmodel>(transaction.Request);
                        RTSResponse rTSResponse = JsonConvert.DeserializeObject<RTSResponse>(transaction.Response);
                        var requestResponse3 = new RequestResponse(rTSRequest, rTSResponse);
                        requestResponses.Add(requestResponse3);
                    }else if(transaction.TransactionStatus == (int)TransactionStatus.Failed &&  transaction.ReceivedFrom == "rts_init")
                    {
                        RTSRequestmodel rTSRequest = JsonConvert.DeserializeObject<RTSRequestmodel>(transaction.Request);
                        RTSErorResponse rTSResponse = JsonConvert.DeserializeObject<RTSErorResponse>(transaction.Response);
                        var requestResponse3 = new RequestResponse(rTSRequest, rTSResponse);
                        requestResponses.Add(requestResponse3);
                    }
                       
                }
                
            }

            switch (transaction.TransactionStatus)
            {
                case 1:
                    executionResult.status = "success";
                    executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                    break;
                case 2:
                    executionResult.status = "pending";
                    executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                    break;
                case 3:
                    executionResult.status = "failed";
                    executionResult.code = new RTSProperties().ReadErrorAndReturnStatusCode(transaction.VendStatusDescription);
                    break;
                default:
                    break;
            }
            return Response.WithType(requestResponses).GenerateResponse();
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
                if (req.SortBy == "VTECHEXT_ID")
                    query = query.Where(d => d.VendtechTransactionID.Contains(req.SortValue));
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


        public async Task<List<TransactionDto>> GetTransactionReportAsync(PaginatedSearchRequest req)
        {
            var query = _repository.GetSalesTransactionQuery(req.Status, req.IsClaimedStatus);
            query = FilterQuery(req, query);
            int totalRecords = await query.CountAsync();

            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);

            List<TransactionDto> transactions = await query.Select(d => new TransactionDto(d)).ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Date = t.Date, // Ensure you have a Date property in your Transaction model
                Amount = t.Amount,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter
            }).ToList();
        }
    }
}
