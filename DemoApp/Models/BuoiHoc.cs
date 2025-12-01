using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoApp.Models
{
    [Table("BuoiHoc")]
    public class BuoiHoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhoaHocId { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Tên buổi học")]
        public string TenBuoiHoc { get; set; } = null!;

        [Display(Name = "Ngày học")]
        public DateTime NgayHoc { get; set; }

        [Display(Name = "Thời lượng (phút)")]
        public int? ThoiLuong { get; set; }

        [Display(Name = "Thứ tự buổi")]
        public int ThuTuBuoi { get; set; } = 0;

        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        public virtual ICollection<DiemDanh>? DiemDanhs { get; set; }
    }
}
