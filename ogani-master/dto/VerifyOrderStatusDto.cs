using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ogani_master.dto
{
    public class VerifyOrderStatusDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int typeStatus { get; set; }
    }
}
