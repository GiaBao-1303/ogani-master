using System.ComponentModel.DataAnnotations;

namespace ogani_master.dto
{
    public class OrderDto
    {
        [Required]
        public int ProdId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be at least 1.")]
        public int amount { get; set; }
    }
}
