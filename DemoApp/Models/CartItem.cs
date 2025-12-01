namespace DemoApp.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        // Khóa học trong giỏ
        public int KhoaHocId { get; set; }
        public virtual KhoaHoc KhoaHoc { get; set; } = null!;

        // Giá tại thời điểm thêm vào giỏ
        public decimal Price { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
