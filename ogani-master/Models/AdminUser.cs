using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("AdminUsers")]

    public class AdminUser : BaseModel
    {
        [Key]
      
        public int USE_ID { get; set; }

        public required string Username { get; set; }

        public required string Password { get; set; }

        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
