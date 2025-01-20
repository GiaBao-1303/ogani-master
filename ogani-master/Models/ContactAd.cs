using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Contacts")]
    public class ContactAd
    {
        [Key]
        public int ContactId { get; set; }  // ID của liên hệ

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } // Tên người liên hệ

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } // Email của người liên hệ

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? PhoneNumber { get; set; } // Số điện thoại (tuỳ chọn)

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } // Nội dung tin nhắn

        public DateTime SentAt { get; set; } = DateTime.UtcNow; // Ngày gửi tin nhắn

        public string Status { get; set; } = "Unseen";// Trạng thái tin nhắn (mặc định là "Unread")
    }
}
