using Microsoft.EntityFrameworkCore;
using signalrserver.Models.DTO;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class ElectricitySalesService: IElectricitySalesService
    {
        private readonly DataContext _dataContext;
        private readonly RequestExecutionContext executionContext;
        public ElectricitySalesService(DataContext dtcxt, RequestExecutionContext executionContext)
        {
            _dataContext = dtcxt;
            this.executionContext = executionContext;
        }

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid)
        {
            //await InternalValidation(request, integratorid);

            var vtechSalesService = new VendtechTransactionsService();
            var tranx = await vtechSalesService.CreateRecordBeforeVend(request.MeterNumber, request.Amount);

            request.TransactionId = tranx.TransactionId;
            Transaction transactionLog = await CreateTransactionLog(request, integratorid);

            var executionResult = await ExecuteTransaction(request);
            if (executionResult.Status == "success")
            {
                executionResult.SuccessResponse.UpdateResponse(transactionLog);
                await UpdateSuccessTransactionLog(executionResult, transactionLog);
                return Response.Instance.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.SuccessResponse.VendStatus).WithType(executionResult).GenerateResponse();
            }
            else
            {
                await UpdateFailedTransactionLog(executionResult, transactionLog);
                return Response.Instance.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
            }
        }

        private async Task<Transaction> CreateTransactionLog(ElectricitySaleRequest request, string integratorId)
        {
            var trans = new TransactionsBuilder()
                .WithTransactionId(UniqueIDGenerator.NewTransactionId())
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTerminalId(request.TransactionId)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithCreatedAt(DateTime.Now)
                .WithAmount(request.Amount)
                .Build();

            _dataContext.Transactions.Add(trans);
            await _dataContext.SaveChangesAsync();
            return trans;
        }

        private async Task UpdateSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithCustomerAddress(executionResult.SuccessResponse.CustomerAddress)
                .WithAccountNumber(executionResult.SuccessResponse.AccountNumber)
                .WithReceiptNumber(executionResult.SuccessResponse.ReceiptNumber)
                .WithServiceCharge(executionResult.SuccessResponse.ServiceCharge)
                .WithSerialNumber(executionResult.SuccessResponse.SerialNumber)
                .WithMeterToken1(executionResult.SuccessResponse.MeterToken1)
                .WithMeterToken2(executionResult.SuccessResponse.MeterToken1)
                .WithMeterToken3(executionResult.SuccessResponse.MeterToken1)
                .WithCostOfUnits(executionResult.SuccessResponse.CostOfUnits)
                .WithRTSUniqueID(executionResult.SuccessResponse.RTSUniqueID)
                .WithTaxCharge(executionResult.SuccessResponse.TaxCharge)
                .WithVProvider(executionResult.SuccessResponse.VProvider)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithTariff(executionResult.SuccessResponse.Tariff)
                .WithUnits(executionResult.SuccessResponse.Units)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request) 
                .WithFinalised(true)
                .WithSold(true)
                .Build();

            await _dataContext.SaveChangesAsync();
        }

        private async Task UpdateFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.FailedResponse.ErrorDetail)
                .WithStatusResponse(executionResult.FailedResponse.ErrorMessage)
                .WithTransactionStatus(TransactionStatus.Failed)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request)
                .Build();

            await _dataContext.SaveChangesAsync();
        }

        private static RTSResponse GenerateMockSuccessResponse(ElectricitySaleRequest model)
        {
            // Here you would generate your mock data
            RTSResponse response = new RTSResponse
            {
                Status = "Success",
                ErrorLog = new string[] { "No errors" },
                Content = new Content
                {
                    Data = new DataResponse
                    {
                        Data = new Datum[]
                        {
                            new Datum
                            {
                                Barcode = 1234567890,
                                DateAndTime = DateTime.Now.ToString("o"),
                                DealerBalance = 5000,
                                Denomination = 100,
                                Id = 1,
                                Instructions = "Sample instructions",
                                PinNumber = "17222353360943207043",
                                PinNumber2 = "17222353360943207043",
                                PinNumber3 = "17222353360943207043",
                                Provider = "Sample Provider",
                                SerialNumber = "SN123456",
                                VoucherProfit = 50,
                                XmlResponse = "<xml>executionResult</xml>",
                                PowerHubVoucher = new PowerHubVoucher
                                {
                                    AccountCredit = 1000,
                                    AccountNumber = "123456789",
                                    CostOfUnits = "50.00",
                                    CustAccNo = "CUST1234",
                                    CustAddress = "123 Sample Street",
                                    CustCanVend = "Yes",
                                    CustContactNo = "555-1234",
                                    CustDaysLastPurchase = "30",
                                    CustLocalRef = "Local123",
                                    CustMsno = "MS1234",
                                    CustMinVendAmt = "10",
                                    CustName = "John Doe",
                                    Customer = "CustomerX",
                                    DebtRecoveryAmt = 200,
                                    DebtRecoveryBalance = 100,
                                    MeterNumber = "MTR1234",
                                    PayAccDesc = "Payment Account Description",
                                    PayAccNo = "PAY1234",
                                    PayAmount = "500.00",
                                    PayBalance = "0.00",
                                    PayReceiptNo = "REC1234",
                                    Pin1 = "17222353360943207043",
                                    Pin2 = "17222353360943207043",
                                    Pin3 = "17222353360943207043",
                                    RtsUniqueId = "RTS1234",
                                    ReceiptNumber = "RECEIPT1234",
                                    Sgc = "SGC1234",
                                    ServiceCharge = "10.00",
                                    Tariff = "12.0",
                                    TaxCharge = "5.00",
                                    TenderedAmount = "505.00",
                                    TransactionAmount = "500.00",
                                    Units = "100",
                                    VatNumber = 123456789
                                },
                                Tym2SellVoucher = new Tym2SellVoucher
                                {
                                    Account = "Account123",
                                    ClientId = "Client123",
                                    CostOfUnits = "50.00",
                                    Customer = "CustomerY",
                                    GovermentLevy = 5,
                                    KeyChangeDetected = false,
                                    KeyChangeToken1 = "Key1",
                                    KeyChangeToken2 = "Key2",
                                    ReceiptNumber = "Receipt123",
                                    StandingCharge = 2,
                                    StsMeter = "STS123",
                                    TenderedAmount = "55.00",
                                    Units = "10",
                                    Vat = "5.00",
                                    VatNo = "VAT123",
                                    VoucherTextDecodeFailed = false
                                }
                            }
                        },
                        DataName = "SampleData",
                        Error = "None",
                        ErrorCode = 0
                    },
                    ProcessOption = "Option1"
                },
                RequestModel = model
                // Populate other properties as needed
            };

            return response;
        }

        private async Task<ExecutionResult> ExecuteTransaction(ElectricitySaleRequest request)
        {
            executionContext.BuildRequest(request.Amount, request.MeterNumber, request.TransactionId);
            await executionContext.ExecuteRequest();
            await executionContext.ProcessResponse();

            ExecutionResult executionResult = executionContext.salesResponse;
            executionResult.InitializeRequestAndResponse(executionContext);

            return executionResult;
        }

        private async Task InternalValidation(ElectricitySaleRequest request, string integrator)
        {
            //Check for balance

            //check if transactionid exist for this terminal
            if (await _dataContext.Transactions.AnyAsync(d => d.IntegratorId == integrator && d.TransactionId == request.TransactionId))
                throw new BadRequestException("Transaction ID already exist for this terminal.");
        }
    }
}
