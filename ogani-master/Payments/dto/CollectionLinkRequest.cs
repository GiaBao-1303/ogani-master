namespace ogani_master.Payments.dto
{
    public class CollectionLinkRequest
    {
        public string orderInfo { get; set; }
        public string partnerCode { get; set; }
        public string redirectUrl { get; set; }
        public string ipnUrl { get; set; }
        public decimal amount { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public string requestType { get; set; }
        public string extraData { get; set; }
        public string partnerName { get; set; }
        public string storeId { get; set; }
        public string orderGroupId { get; set; }
        public bool autoCapture { get; set; }
        public string lang { get; set; }
        public string signature { get; set; }

    }

}
