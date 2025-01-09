using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ogani_master.Models
{
    [Table("Menus")]

    public class Menu : BaseModel
    {

        [Key]
        [Display(Name ="STT")]
        public int MEN_ID { get; set; }
        [Display(Name = "Menu Cha")]
        public int? PARENT_ID { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Title cannot be longer than 255 characters.")]
        [Display(Name = "Menu")]
        public string Title { get; set; }
        [StringLength(255, ErrorMessage = "Url cannot be longer than 255 characters.")]
        [Display(Name = "Đường Dẫn")]
        public string Url { get; set; }
        //public Menu ParentMenu { get; set; }
        [ForeignKey("PARENT_ID")]
        public virtual ICollection<Menu>? children { get; set; }
    }
}
