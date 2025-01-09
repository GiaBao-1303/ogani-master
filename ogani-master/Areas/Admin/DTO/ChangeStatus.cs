using System.ComponentModel.DataAnnotations;

namespace ogani_master.Areas.Admin.dto
{
    public class ChangeStatus
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [Range(0, 1)]
        public int selectedStatus { get; set; }
    }
}
