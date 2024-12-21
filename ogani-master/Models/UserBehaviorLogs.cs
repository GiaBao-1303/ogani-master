using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("UserBehaviorLogs")] 
    public class UserBehaviorLog
    {
        [Key] 
        public int ID { get; set; }

        [Required] 
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Action { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required] 
        [StringLength(255)] 
        public string Page { get; set; }
    }
}
