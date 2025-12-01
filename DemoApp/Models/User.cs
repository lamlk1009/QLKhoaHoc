using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace DemoApp.Models
{
    [Table("Users")]
    public partial class User
    {
        [Key]
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? NumberPhone { get; set; }
        public string Address { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }

        public ICollection<TienDoHocTap>? TienDoHocTap { get; set; }
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual Role? Role { get; set; } = null!;
        public ICollection<KhoaHoc>? KhoaHoc { get; set; }
        public ICollection<DangKyKhoaHoc>? DangKyKhoaHocs { get; set; }

        // Thêm
        public ICollection<DiemDanh>? DiemDanhs { get; set; }
    }
}
