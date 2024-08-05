using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using vendtechext.BLL.Common;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL;
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

        public async Task<RTSResponse> PurchaseElectricity(RTSRequestmodel request)
        {

            //Validate Client and Extract Vendor Information
            VendorInformation vendor = GetVendor(request);

            //Create Transaction Log
            ElectricityTrxLog transactionLog = await CreateTransactionLog(vendor, request);

            //Send to RTS
            var result = await ProcessTransaction(request, transactionLog);
           
            var rtsResponse = result.Item1;
            string rtsReponseAsJson = result.Item2;
            string requestModelAsStrings = request.ToString();

            //Check for Finalized 
            //here
            //Check for Finalized 

            //Update Transaction Log
            UpdateTransactionLog(rtsResponse.Content.Data.Data.FirstOrDefault(), transactionLog, rtsReponseAsJson);


            //Create new Sale
            await CreateNewSaleTransanction(rtsResponse, rtsReponseAsJson, vendor);

            return rtsResponse;
        }

        private async Task<ElectricityTrxLog> CreateTransactionLog(VendorInformation client, RTSRequestmodel request)
        {
            var trans = new ElectricityTrxLog();
            trans.PlatFormId = (int)PlatformTypeEnum.ELECTRICITY;
            trans.UserId = client.VendorId;
            trans.MeterNumber = request.Parameters[6].ToString();
            trans.MeterToken1 = "";
            trans.IsDeleted = false;
            trans.Status = (int)TransactionStatus.Pending;
            trans.CreatedAt = DateTime.UtcNow;
            trans.AccountNumber = "";
            trans.Customer = "";
            trans.ReceiptNumber = "";
            trans.RequestDate = DateTime.UtcNow;
            trans.SerialNumber = "";
            trans.ServiceCharge = "";
            trans.Tariff = "";
            trans.TaxCharge = "";
            trans.TenderedAmount = Convert.ToDecimal(request.Parameters[5].ToString());
            trans.TransactionAmount = Convert.ToDecimal(request.Parameters[5].ToString());
            trans.Units = "";
            trans.Vprovider = "";
            trans.Finalised = false;
            trans.StatusRequestCount = 0;
            trans.Sold = false;
            trans.DateAndTimeSold = "";
            trans.DateAndTimeFinalised = "";
            trans.DateAndTimeLinked = "";
            trans.VoucherSerialNumber = "";
            trans.VendStatus = "";
            trans.VendStatusDescription = "";
            trans.StatusResponse = "";
            trans.DebitRecovery = "0";
            trans.CostOfUnits = "0";
            
            trans.TransactionId = Utils.GetElectricityLastTrxId();
            dtcxt.ElectricityTrxLogs.Add(trans);
            await dtcxt.SaveChangesAsync();

            return trans;
        }

        private void UpdateTransactionLog(Datum response_data, ElectricityTrxLog trans, string rtsReponseAsJson)
        {
            trans.CostOfUnits = response_data.PowerHubVoucher.CostOfUnits;
            trans.MeterToken1 = response_data?.PowerHubVoucher.Pin1?.ToString() ?? string.Empty;
            trans.MeterToken2 = response_data?.PowerHubVoucher?.Pin2?.ToString() ?? string.Empty;
            trans.MeterToken3 = response_data?.PowerHubVoucher?.Pin3?.ToString() ?? string.Empty;
            trans.Status = (int)TransactionStatus.Success;
            trans.AccountNumber = response_data.PowerHubVoucher?.AccountNumber ?? string.Empty;
            trans.Customer = response_data.PowerHubVoucher?.Customer ?? string.Empty;
            trans.ReceiptNumber = response_data?.PowerHubVoucher.ReceiptNumber ?? string.Empty;
            trans.SerialNumber = response_data?.SerialNumber ?? string.Empty;
            trans.RtsuniqueId = response_data.PowerHubVoucher.RtsUniqueId;
            trans.ServiceCharge = response_data?.PowerHubVoucher?.ServiceCharge;
            trans.Tariff = response_data.PowerHubVoucher?.Tariff;
            trans.TaxCharge = response_data?.PowerHubVoucher?.TaxCharge;
            trans.Units = response_data?.PowerHubVoucher?.Units;
            trans.CustomerAddress = response_data?.PowerHubVoucher?.CustAddress;
            trans.Finalised = true;
            trans.Vprovider = response_data.Provider;
            trans.StatusRequestCount = 0;
            trans.Sold = true;
            trans.VoucherSerialNumber = response_data?.SerialNumber;
            trans.VendStatus = "";
            trans.StatusResponse = rtsReponseAsJson;
        }
        private async Task CreateNewSaleTransanction(RTSResponse response, string rtsReponseAsJson, VendorInformation vendor)
        {
            var platform = dtcxt.Platforms.FirstOrDefault(d => d.PlatformType == (int)PlatformTypeEnum.ELECTRICITY);
            var rtsResponse = response.Content.Data.Data.FirstOrDefault();
            var trans = new TransactionDetail();
            trans.PlatFormId = (int)platform.PlatformId;
            trans.UserId = vendor.VendorId;
            trans.MeterId = null;
            trans.Posid = vendor.POSId;
            trans.MeterNumber1 = vendor.MeterNumber;
            trans.MeterToken1 = rtsResponse.PinNumber;
            trans.MeterToken1 = rtsResponse.PinNumber2;
            trans.MeterToken1 = rtsResponse.PinNumber3;
            trans.Amount = vendor.Amount;
            trans.IsDeleted = false;
            trans.Status = (int)TransactionStatus.Success;
            trans.CreatedAt = DateTime.UtcNow;
            trans.AccountNumber = rtsResponse.PowerHubVoucher?.AccountNumber ?? string.Empty; ;
            trans.CurrentDealerBalance = rtsResponse.DealerBalance;
            trans.Customer = rtsResponse.PowerHubVoucher?.Customer ?? string.Empty; ;
            trans.ReceiptNumber = rtsResponse.PowerHubVoucher.ReceiptNumber ?? string.Empty;
            trans.RequestDate = DateTime.UtcNow;
            trans.RtsuniqueId = rtsResponse.PowerHubVoucher.RtsUniqueId;
            trans.SerialNumber = rtsResponse?.SerialNumber ?? string.Empty; ;
            trans.ServiceCharge = rtsResponse?.PowerHubVoucher?.ServiceCharge; ;
            trans.Tariff = rtsResponse.PowerHubVoucher?.Tariff;
            trans.TaxCharge = rtsResponse?.PowerHubVoucher?.TaxCharge;
            trans.TenderedAmount = vendor.Amount;
            trans.TransactionAmount = vendor.Amount;
            trans.Units = rtsResponse?.PowerHubVoucher?.Units;
            trans.Vprovider = "";
            trans.CustomerAddress = rtsResponse?.PowerHubVoucher?.CustAddress;
            trans.Finalised = true;
            trans.StatusRequestCount = 0;
            trans.Sold = true;
            trans.DateAndTimeSold = "";
            trans.DateAndTimeFinalised = "";
            trans.DateAndTimeLinked = "";
            trans.VoucherSerialNumber = "";
            trans.VendStatus = "";
            trans.VendStatusDescription = "";
            trans.StatusResponse = "";
            trans.DebitRecovery = "0";
            trans.CostOfUnits = "0";
            trans.TransactionId = Utils.NewTransactionId();
            trans.Request = rtsReponseAsJson;
            trans.Response = response.ToString();
            dtcxt.TransactionDetails.Add(trans);
            await dtcxt.SaveChangesAsync();

            //return trans;
        }

        private static RTSResponse GenerateMockSuccessResponse(RTSRequestmodel model)
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

        private static string WriteResponseToFile(RTSResponse response, string transactionId)
        {
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText($"{transactionId}.json", json);
            Console.WriteLine($"Response written to file {transactionId}.json");
            return json;
        }

        private async Task<(RTSResponse, string)> ProcessTransaction(RTSRequestmodel request, ElectricityTrxLog log)
        {
            RTSResponse response = GenerateMockSuccessResponse(request);
            string rtsReponseAsJson = await Task.Run(() => WriteResponseToFile(response, log.TransactionId));
            return (response, rtsReponseAsJson);
        }

        private VendorInformation GetVendor(RTSRequestmodel request)
        {
            var clientInfor = dtcxt.B2bUserAccesses
                .Where(d => request.Auth.UserName.Equals(d.Clientkey) && request.Auth.Password.Equals(d.Apikey))
                .Include(d => d.User).FirstOrDefault();
            
            if (clientInfor == null)
                throw new Exception("Unable to validate information");

            //Get Vendtech btb vendor


            return new VendorInformation
            {
                POSId = 20034,
                VendorId = clientInfor.UserId,
                MeterNumber = request.Parameters[6].ToString(),
                Amount = Convert.ToDecimal(request.Parameters[5].ToString())
            };
        }
    }
}
