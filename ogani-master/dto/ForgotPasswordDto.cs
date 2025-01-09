using Microsoft.Build.Framework;

namespace ogani_master.dto
{
    public class ForgotPasswordDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string email { get; set; }
    }
}
