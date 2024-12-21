using ogani_master.Models;

namespace ogani_master.Areas.Admin.DTO
{
    public class ProductDTO : BaseModel
    {
        public int PRO_ID { get; set; }
        public int CAT_ID { get; set; }
        public IFormFile? Avatar { get; set; }
        public required string Name { get; set; }
        public string? Intro { get; set; }
        public required decimal Price { get; set; }
        public required decimal DiscountPrice { get; set; }
        public string? Unit { get; set; }
        public double? Rate { get; set; }
        public string? Description { get; set; }
        public string? Details { get; set; }
    }
}
