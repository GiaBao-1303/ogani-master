using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ogani_master.dto
{
    public class UserRegistrationV1Dto
    {

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [IntegerValidator(MinValue = 0, MaxValue = 1)]
        public int Gender { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Phone number must be at least 10 digits and contain only numbers.")]
        public string Phone { get; set; }

        [Required]
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address { get; set; }

        public string? otpCode { get; set; }

        public DateTime? otp_expired { get; set; }
    }
}
