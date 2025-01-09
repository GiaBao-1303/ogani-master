using Microsoft.Build.Framework;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.dto
{
    public class ChangeRoleDto
    {
        [Required]
        public int selectedRole { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
