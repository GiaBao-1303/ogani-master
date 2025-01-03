using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Orders")]
    public class Order : BaseModel
    {
        [Key]
        public int ORD_ID { get; set; }

        [Required]
        [Display(Name = "Member ID")]
        public int MEM_ID { get; set; }

        [Display(Name = "Quantity Ordered")]
        public int Quantity { get; set; }

        [Display(Name = "Product ID")]
        public int PROD_ID { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }

        [Display(Name = "Shipping Address")]
        public string? Address { get; set; }

        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Discount (%)")]
        public decimal? Discount { get; set; }

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Is Paid?")]
        public bool IsPaid { get; set; }

        [Display(Name = "Order Notes")]
        public string? Note { get; set; }

        [Display(Name = "Order Status")]
        public int Status { get; set; }

        [ForeignKey("PROD_ID")]
        [Display(Name = "Product Details")]
        public virtual Product Product { get; set; }

        [ForeignKey("MEM_ID")]
        [Display(Name = "User Details")]
        public virtual User? User { get; set; }
    }
}
