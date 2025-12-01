using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models
{
    [Table("TienDoHocTap")]
    public class TienDoHocTap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int KhoaHocId { get; set; }

        [Required]
        public int BaiHocId { get; set; }

        [Display(Name = "Trạng thái hoàn thành")]
        public bool DaHoanThanh { get; set; } = false;

        [Display(Name = "Thời gian bắt đầu")]
        public DateTime? ThoiGianBatDau { get; set; }

        [Display(Name = "Thời gian hoàn thành")]
        public DateTime? ThoiGianHoanThanh { get; set; }

        [Display(Name = "Thời gian cập nhật")]
        public DateTime ThoiGianCapNhat { get; set; } = DateTime.Now;
                [Display(Name = "Tỷ lệ hoàn thành")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TyLeHoanThanh { get; set; } = 0;

        [Display(Name = "Thời gian học (phút)")]
        public int ThoiGianHoc { get; set; } = 0;

        [StringLength(20)]
        [Display(Name = "Trạng thái học")]
        public string TrangThaiHoc { get; set; } = "DangHoc"; // DangHoc, TamDung, DaHoanThanh

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        [ForeignKey("BaiHocId")]
        public virtual BaiHoc? BaiHoc { get; set; }
    }
}
