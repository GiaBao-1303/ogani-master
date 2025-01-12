using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace ogani_master.Models
{
    [Table("Categories")]
    public class Category:BaseModel
    {
        [Key]
        public int? CAT_ID { get; set; }
        public required string Name { get; set; }
        [IntegerValidator(MinValue = 0)]
        public int DisplayOrder { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
