using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Settings")]

    public class Setting:BaseModel
    {
        [Key]
        public int SET_ID { get; set; }
        
        public required string Name { get; set; }
        public required string Value { get; set; }
    }
}
