using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoApp.Data;
using DemoApp.Models;

namespace DemoApp.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy UserId bằng Claims (Identity)
        private int? GetCurrentUserId()
        {
            // Claim mặc định khi login bằng Identity
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(idStr, out int userId))
                return userId;

            return null;
        }

        private async Task<Cart?> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.KhoaHoc)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.KhoaHoc)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    Items = new List<CartItem>()
                };
            }

            return View(cart);
        }

        // POST: /Cart/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.KhoaHoc.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var cart = await GetOrCreateCartAsync(userId.Value);

            var existing = cart.Items.FirstOrDefault(i => i.KhoaHocId == id);
            if (existing == null)
            {
                cart.Items.Add(new CartItem
                {
                    KhoaHocId = id,
                    Price = course.GiaTien,   // thuộc tính giá của anh
                    AddedAt = DateTime.Now
                });
            }

            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == id && ci.Cart.UserId == userId.Value);

            if (item == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.KhoaHoc)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // Tạo đăng ký khóa học cho từng item trong giỏ
            foreach (var item in cart.Items)
            {
                // Nếu đã đăng ký rồi thì bỏ qua (tránh trùng)
                bool alreadyRegistered = await _context.DangKyKhoaHoc
                    .AnyAsync(d => d.UserId == userId.Value && d.KhoaHocId == item.KhoaHocId);

                if (!alreadyRegistered)
                {
                    var dk = new DangKyKhoaHoc
                    {
                        UserId = userId.Value,
                        KhoaHocId = item.KhoaHocId,
                        NgayDangKy = DateTime.Now,
                        TrangThai = "Pending" // chờ admin duyệt
                    };

                    _context.DangKyKhoaHoc.Add(dk);
                }
            }

            // Xóa giỏ hàng sau khi tạo đăng ký
            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Có thể chuyển sang trang thông báo
            return View("CheckoutSuccess"); // tạo view này đơn giản: "Đăng ký thành công, vui lòng chờ admin xác nhận"
        }

    }
}
