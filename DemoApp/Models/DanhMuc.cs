using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string TenDanhMuc { get; set; }
        public int ThuTuHienThi { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<KhoaHoc>? KhoaHoc { get; set; }
    }
}
