using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
	[Table("Users")]
	public class User : BaseModel
	{
		[Key]
		public int UserId { get; set; }

        [Required]
        public string ProfilePictureUrl { get; set; }

        [Required]
        public int Role { get; set; }

        [Required]
		[MaxLength(100)]
		public string UserName { get; set; }

		[Required]
		[MaxLength(100)]
		public string Password { get; set; }

		[MaxLength(100)]
		public string FirstName { get; set; }

		[MaxLength(100)]
		public string LastName { get; set; }

		public bool Gender { get; set; }

		[MaxLength(15)]
		public string Phone { get; set; }

		[MaxLength(200)]
		public string Email { get; set; }

		[MaxLength(300)]
		public string Address { get; set; }

		public string? token { get; set; }
		public DateTime? token_expired { get; set; }

		public int Status { get; set; }
	}
}
