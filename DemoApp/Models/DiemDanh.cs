using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoApp.Models
{
    [Table("DiemDanh")]
    public class DiemDanh
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int KhoaHocId { get; set; }

        [Required]
        public int BuoiHocId { get; set; }

        [Display(Name = "Ngày điểm danh")]
        public DateTime NgayDiemDanh { get; set; } = DateTime.Now;

        /// <summary>
        /// present = Có mặt, late = Đi muộn, absent = Vắng mặt
        /// </summary>
        [Required, StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "present";

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        [ForeignKey("BuoiHocId")]
        public virtual BuoiHoc? BuoiHoc { get; set; }
    }
}
