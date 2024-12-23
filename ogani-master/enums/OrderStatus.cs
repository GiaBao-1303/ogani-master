using System.ComponentModel.DataAnnotations;

namespace ogani_master.enums
{
    public enum OrderStatus
    {
        [Display(Name = "Pending Approval")]
        Pending = 1,

        [Display(Name = "Order Confirmed")]
        Confirmed = 2,

        [Display(Name = "Preparing for Shipment")]
        Preparing = 3,

        [Display(Name = "Out for Delivery")]
        Shipping = 4,

        [Display(Name = "Delivered Successfully")]
        Delivered = 5,

        [Display(Name = "Order Canceled")]
        Canceled = 6,

        [Display(Name = "Returned by Customer")]
        Returned = 7
    }
}
