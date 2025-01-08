using System.ComponentModel.DataAnnotations;

namespace ogani_master.dto
{
    public class ResetPasswordDto
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$", ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string newPassword { get; set; }

        [Required]
        [Compare("newPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmPassword { get; set; }

        [Required]
        public string token { get; set; }
    }
}
