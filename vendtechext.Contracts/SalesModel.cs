namespace vendtechext.Contracts
{
    public class ElectricitySaleRequest
    {
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
    }

    public class SaleStatusRequest
    {
        public string TransactionId { get; set; }
    }
}
