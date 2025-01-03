namespace ogani_master.Payments.dto
{
    public class MomoConfirmDto
    {
        public string partnerCode { get; set; }
        public string requestId { get; set; }
        public string orderId { get; set; }
        public string lang {  get; set; }

        public string? signature { get; set; }

    }
}
