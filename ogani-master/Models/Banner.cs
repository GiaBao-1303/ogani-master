using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Banners")]

    public class Banner: BaseModel
    {
        [Key]
        public int BAN_ID { get; set; }
        public string? Title { get; set; }
        public string? Image { get; set; }
        public string? Url { get; set; }
        public int DisplayOrder { get; set; }
    }
}
