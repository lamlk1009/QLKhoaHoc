using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models
{
    [Table("BaiHoc")]
    public class BaiHoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhoaHocId { get; set; }

        [Required, StringLength(200)]
        public string? TenBaiHoc { get; set; }

        [Required, StringLength(20)]
        public string LoaiNoiDung { get; set; } = "Video";

        public string? DuongDanNoiDung { get; set; }

        // Thứ tự trong khóa
        public int ThuTuHienThi { get; set; } = 0;

        // Thời lượng (phút) – dùng để hiển thị 45 phút, 60 phút...
        [Display(Name = "Thời lượng (phút)")]
        public int? ThoiLuong { get; set; }

        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        public virtual ICollection<TienDoHocTap>? TienDoHocTap { get; set; }
    }

}
