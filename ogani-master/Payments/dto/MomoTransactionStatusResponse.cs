namespace ogani_master.Payments.dto
{
    public class MomoTransactionStatusResponse
    {
        public string partnerCode { get; set; }
        public string requestId { get; set; }
        public string orderId { get; set; }
        public long amount { get; set; }
        public long transId { get; set; }
        public string payType { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public long responseTime { get; set; }
    }
}
