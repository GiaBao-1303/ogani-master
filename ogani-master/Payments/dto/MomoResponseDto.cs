namespace ogani_master.Payments.dto
{
    public class MomoResponseDto
    {
        public string PartnerCode { get; set; }
        public string RequestId { get; set; }
        public long Amount { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public long TransId { get; set; }
        public string Message { get; set; }
        public string LocalMessage { get; set; }
        public long ResponseTime { get; set; }
        public string ErrorCode { get; set; }
        public string PayType { get; set; }
        public string ExtraData { get; set; }
        public int resultCode { get; set; }
    }

}
