using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ogani_master.dto
{
    public class OrderDto
    {
        [Required]
        public int ProdId { get; set; }

        [Required]
        [IntegerValidator(MinValue = 1)]
        public int amount { get; set; }
    }
}
