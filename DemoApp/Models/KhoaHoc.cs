using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models
{
    [Table("KhoaHoc")]
    public class KhoaHoc
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Mã Khóa Học")]
        public string MaKhoaHoc { get; set; }

        [Required, StringLength(200)]

        [Display(Name = "Tên Khóa Học")]
        public string TenKhoaHoc { get; set; }

        [Display(Name = "Mô tả ngắn ")]
        public string? MoTaNgan { get; set; }



        [Display(Name = "Ảnh bìa")]
        public string? AnhBia { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? DanhMucId { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Cấp độ")]
        public string CapDo { get; set; } = "CoBan";

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá Tiền ")]
        public decimal GiaTien { get; set; } = 0;

        [Required, StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "BanNhap";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? user { get; set; }

        [ForeignKey("DanhMucId")]
        public virtual DanhMuc? DanhMuc { get; set; }

        // Quan hệ hiện có
        public virtual ICollection<TienDoHocTap>? TienDoHocTap { get; set; }
        public virtual ICollection<BaiHoc>? BaiHoc { get; set; }
        public virtual ICollection<DangKyKhoaHoc>? DangKyKhoaHoc { get; set; }

        // Thêm cho phần quản lý buổi học & điểm danh
        public virtual ICollection<BuoiHoc>? BuoiHocs { get; set; }
        public virtual ICollection<DiemDanh>? DiemDanhs { get; set; }
    }

}
