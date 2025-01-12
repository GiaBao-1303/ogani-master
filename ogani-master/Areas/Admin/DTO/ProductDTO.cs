using ogani_master.Models;
using System.ComponentModel.DataAnnotations;
using System.Configuration;


namespace ogani_master.Areas.Admin.DTO
{
    public class ProductDTO : BaseModel
    {
        public int PRO_ID { get; set; }
        public int CAT_ID { get; set; }

        [Required(ErrorMessage = "Số lượng sản phẩm là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải là số dương.")]
        public int quantity { get; set; }

        public IFormFile? Avatar { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự.")]
        public required string Name { get; set; }
        public string? Intro { get; set; }
        [Required]
        [IntegerValidator(MinValue = 0)]
        public required decimal Price { get; set; }
        [Required]
        [IntegerValidator(MinValue = 0)]
        public required decimal DiscountPrice { get; set; }
        [Required]
        [IntegerValidator(MinValue = 1)]
        public string Unit { get; set; }
        public double? Rate { get; set; }
        public string? Description { get; set; }
        public string? Details { get; set; }
    }

    public class DeleteProductViewModel
    {
        public int? PRO_ID { get; set; }
    }

}
