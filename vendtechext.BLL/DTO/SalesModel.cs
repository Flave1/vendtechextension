namespace vendtechext.BLL.DTO
{
    public class ElectricitySaleRequest
    {
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
    }
}
