using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ogani_master.Models
{
    [Table("Products")]

    public class Product:BaseModel
    {
        [Key]
        public int PRO_ID { get; set; }

        
        public int? CAT_ID { get; set; }

        [Required]
        public int quantity { get; set; }
        public string? Avatar { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự.")]
        public string? Name { get; set; }
        public string? Intro { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public string? Unit { get; set; }
        public double? Rate { get; set; }


        public string? Description { get; set; }
        public string? Details { get; set; }

        [ForeignKey("CAT_ID")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<Review>? Review { get; set; }
    }
}
