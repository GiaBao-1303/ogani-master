using ogani_master.Models;

namespace ogani_master.Areas.Admin.DTO
{
    public class BannerDTO : BaseModel
    {
        public int BAN_ID { get; set; }
        public required string Title { get; set; }
        public int? DisplayOrder { get; set; }
        public IFormFile? Image { get; set; }
        public string? Url { get; set; }
        public string? CurrentImage { get; set; }
    }
}
