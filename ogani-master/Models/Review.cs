using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ogani_master.Models
{
    [Table("Reviews")]

    public class Review:BaseModel
    {
        [Key]
        public int REV_ID { get; set; }
        public required int MEM_ID { get; set; }
        public required int PRO_ID { get; set; }
        public required double Rate { get; set; }
        public required string Contents { get; set; }
        public DateTime ReviewDate { get; set; }

        [ForeignKey("PRO_ID")]
        public virtual Product? Product { get; set; }

		[ForeignKey("MEM_ID")]
		public virtual User User { get; set; }
    }
}
