using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models
{
    [Table("DangKyKhoaHoc")]
    public class DangKyKhoaHoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int KhoaHocId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "DangHoc";

     
        [ForeignKey("UserId")]
        public virtual User? user { get; set; }

        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }
    }
}
