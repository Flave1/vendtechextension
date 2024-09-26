namespace vendtechext.BLL.DTO
{
    public class VendorInformation
    {
        public long VendorId { get; set; }
        public long POSId { get; set; }
        public string MeterNumber { get; set; }
        public decimal Amount { get; set; }

        public VendorInformation Update(string MeterNumber, decimal Amount)
        {
            this.MeterNumber = MeterNumber;
            this.Amount = Amount;
            return this;
        }
    }
}
