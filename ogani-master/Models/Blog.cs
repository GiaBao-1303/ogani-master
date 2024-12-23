using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Blogs")]
    public class Blog
    {
        [Key]
        public int BlogId { get; set; } // ID của bài viết
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public string? Image { get; set; } // Hình ảnh bài viết
        public DateTime CreatedAt { get; set; } // Ngày tạo
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật
    }
}