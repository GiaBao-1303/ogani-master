using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("BaseModels")]

    public class BaseModel
    {
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
      
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
       
        public string? UpdatedBy { get; set; }
    }
}
