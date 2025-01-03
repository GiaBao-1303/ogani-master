namespace ogani_master.Payments.dto
{
    public class MomoPaymentNotifyDto
    {
        public string partnerCode { get; set; }  
        public string orderId { get; set; } 
        public string requestId { get; set; }
        public string amount { get; set; } 
        public string transId { get; set; }
        public string message { get; set; }
        public string resultCode { get; set; }
        public string signature { get; set; }
    }
}
