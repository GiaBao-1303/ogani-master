using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ogani_master.dto
{
	public class UserSignInDto
	{
		[Required]
		[MaxLength(100)]
		public string Username { get; set; }

		[Required]
		[MaxLength(20)]
		public string Password { get; set; }
	}
}
