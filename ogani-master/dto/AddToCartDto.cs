using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ogani_master.dto
{
    public class AddToCartDto
    {
        [Required]
        [IntegerValidator(MinValue = 1)]
        public int amount { get; set; }

        [Required]
        public int ProdID { get; set; }

        public string? offer { get; set; }
    }

}
