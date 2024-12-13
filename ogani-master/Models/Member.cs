using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Members")]

    public class Member:BaseModel
    {
        [Key]
        public int MEM_ID { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string LastName { get; set; }

        public required string FirstName { get; set; }
        public bool Gender { get; set; }
        public string? Phone { get; set; }
        public required string Email { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}
