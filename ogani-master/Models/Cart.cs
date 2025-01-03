using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Carts")]
    public class Cart : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ORDD_ID { get; set; } 

        [Required]
        public int UserId { get; set; } 

        [Required]
        public int PRO_ID { get; set; } 

        [Required]
        public int Quantity { get; set; } 

        [Required]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Column(TypeName = "money")]
        public decimal? DiscountPrice { get; set; }

        [ForeignKey("PRO_ID")]
        public virtual Product? Product { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
