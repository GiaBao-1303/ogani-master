using System.ComponentModel.DataAnnotations;

namespace ogani_master.dto
{
    public class UpdateProfileDto
    {

        public IFormFile? ProfilePictureUrl { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Gender is not valid")]
        public int Gender { get; set; }

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; }

        [Required]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; }
    }
}
