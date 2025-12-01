using System.ComponentModel.DataAnnotations.Schema;

namespace DemoApp.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        // User sở hữu giỏ hàng
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Danh sách item trong giỏ
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Tổng tiền (không map DB)
        [NotMapped]
        public decimal TotalPrice => Items.Sum(i => i.Price);

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
