using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Users")]

    public class User:BaseModel
    {
        public int Id { get; set; }

        public required string Username { get; set; }

        public required string Password { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
