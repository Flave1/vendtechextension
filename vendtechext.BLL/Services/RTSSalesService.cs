using Azure.Core;
using Microsoft.Extensions.Options;
using vendtechext.BLL.Common;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class RTSSalesService: IRTSSalesService
    {
        private readonly DataContext dtcxt;
        private readonly RTSInformation rts;
        public RTSSalesService(DataContext dtcxt, IOptions<RTSInformation> rts)
        {
            this.rts = rts.Value;
            this.dtcxt = dtcxt;
        }

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid)
        {
            //Create Transaction Log
            Transaction transactionLog = await CreateTransactionLog(request, integratorid);

            //Send to RTS
            var result = await ProcessTransaction(request, transactionLog);

            var rtsResponse = result.Item1;
            string rtsReponseAsJson = result.Item2;
            string requestModelAsStrings = request.ToString();

            //Check for Finalized
            //here
            //Check for Finalized

            //Update Transaction Log
            await UpdateTransactionLog(rtsResponse.Content.Data.Data.FirstOrDefault(), transactionLog);


            return Response.Instance.WithStatus("success").WithStatusCode(200).WithMessage("Meter Purchase Successful").WithType(transactionLog).GenerateResponse();
        }

        private async Task<Transaction> CreateTransactionLog(ElectricitySaleRequest request, string integratorId)
        {
            var trans = new TransactionsBuilder()
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTransactionId(request.TransactionId)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithCreatedAt(DateTime.Now)
                .WithAmount(request.Amount)
                .Build();

            dtcxt.Transactions.Add(trans);
            await dtcxt.SaveChangesAsync();
            return trans;
        }

        private async Task UpdateTransactionLog(Datum response_data, Transaction trans)
        {
            var response = response_data?.PowerHubVoucher;
            new TransactionsBuilder(trans)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithSerialNumber(response_data.SerialNumber)
                .WithAccountNumber(response.AccountNumber)
                .WithReceiptNumber(response.ReceiptNumber)
                .WithServiceCharge(response.ServiceCharge)
                .WithCustomerAddress(response.CustAddress)
                .WithCostOfUnits(response.CostOfUnits)
                .WithRTSUniqueID(response.RtsUniqueId)
                .WithVProvider(response_data.Provider)
                .WithTaxCharge(response.TaxCharge)
                .WithMeterToken1(response.Pin1)
                .WithMeterToken2(response.Pin2)
                .WithMeterToken3(response.Pin3)
                .WithTariff(response.Tariff)
                .WithUnits(response.Units)
                .WithFinalised(true)
                .WithSold(true)
                .Build();

            await dtcxt.SaveChangesAsync();
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
                                XmlResponse = "<xml>response</xml>",
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

        private async Task<(RTSResponse, string)> ProcessTransaction(ElectricitySaleRequest request, Transaction log)
        {
            RTSResponse response = GenerateMockSuccessResponse(request);
            string rtsReponseAsJson = await Task.Run(() => Utils.WriteResponseToFile(response, log.TransactionId));
            return (response, rtsReponseAsJson);
        }

    }
}
