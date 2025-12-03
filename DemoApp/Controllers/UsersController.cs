using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoApp.Data;
using DemoApp.Models;

namespace DemoApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        // GET: /my-courses  → Trang "Khóa học của tôi"
        [HttpGet("/my-courses")]
        public async Task<IActionResult> UserCourses()
        {
            var userName = User.Identity!.Name;

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == userName || u.Email == userName);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myRegistrations = await _context.DangKyKhoaHoc
                .Include(d => d.KhoaHoc)
                    .ThenInclude(k => k.DanhMuc)
                .Include(d => d.KhoaHoc)
                    .ThenInclude(k => k.BaiHoc)
                .Where(d => d.UserId == user.UserId)
                .ToListAsync();

            var total = myRegistrations.Count;
            var completed = myRegistrations.Count(x => x.TrangThai == "HoanThanh");
            var inProgress = myRegistrations.Count(x => x.TrangThai == "DangHoc");

            ViewBag.TotalCourses = total;
            ViewBag.CompletedCourses = completed;
            ViewBag.InProgressCourses = inProgress;

            // View: /Views/User/UserCourses.cshtml
            return View("UserCourses", myRegistrations);
        }
        // ============================================
        // 2) DANH SÁCH KHÓA HỌC - CHO USER ĐĂNG KÝ
        // GET: /user/courses
        // ============================================
        [HttpGet("/user/courses")]
        public async Task<IActionResult> CourseList()
        {
            var courses = await _context.KhoaHoc
                .Include(c => c.DanhMuc)
                .ToListAsync();

            return View("CourseListWithRegister", courses);
        }
        // POST: /user/courses/register/{courseId} → đăng ký khóa học
        [HttpPost("/user/courses/register/{courseId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCourse(int courseId)
        {
            var userName = User.Identity!.Name;

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == userName || u.Email == userName);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.KhoaHoc.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var existing = await _context.DangKyKhoaHoc
                .FirstOrDefaultAsync(d => d.UserId == user.UserId && d.KhoaHocId == courseId);

            if (existing != null)
            {
                TempData["InfoMessage"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction(nameof(UserCourses));
            }

            var dk = new DangKyKhoaHoc
            {
                UserId = user.UserId,
                KhoaHocId = courseId,
                NgayDangKy = DateTime.Now,
                TrangThai = "DangHoc"
            };

            _context.DangKyKhoaHoc.Add(dk);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký khóa học thành công!";
            return RedirectToAction(nameof(UserCourses));
        }
        // GET: Users
        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveMenu = "users"; // ĐỂ MENU ACTIVE ĐÚNG!

            var users = await _context.User
                .Include(u => u.Role)  // Lấy tên vai trò
                .ToListAsync();

            return View(users);
        }
        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Set<Role>(), "RoleId", "RoleId");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("FullName,Email,NumberPhone,Address,Username,Password,RoleId")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);

            ViewBag.RoleId = new SelectList(_context.Role, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Hiển thị form Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
                return NotFound();

            // Load danh sách Role cho dropdown
            ViewBag.RoleId = new SelectList(_context.Role, "RoleId", "RoleName", user.RoleId);

            return View(user);
        }

        // POST: Xử lý submit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model)
        {
            if (id != model.UserId)
                return NotFound();

            // Kiểm tra trùng Username (trừ user hiện tại)
            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.UserId != id);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username đã tồn tại!");
            }

            // Kiểm tra trùng Email (trừ user hiện tại)
            var existingEmail = await _context.User
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.UserId != id);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoleId = new SelectList(_context.Role, "RoleId", "RoleName", model.RoleId);
                return View(model);
            }

            try
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                    return NotFound();

                // Cập nhật thông tin
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.NumberPhone = model.NumberPhone;
                user.Address = model.Address;
                user.Username = model.Username;
                user.RoleId = model.RoleId;

                // Chỉ cập nhật password nếu người dùng nhập mới
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password; // Nên hash password trước khi lưu
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật user thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                ViewBag.RoleId = new SelectList(_context.Role, "RoleId", "RoleName", model.RoleId);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Không cho xóa chính mình
            var currentUser = User.Identity?.Name;
            if (user.Username == currentUser || user.Email == currentUser)
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: Users/Delete/5 → Xóa thật sự
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra lại không cho xóa chính mình
            var currentUser = User.Identity?.Name;
            if (user.Username == currentUser || user.Email == currentUser)
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa người dùng \"{user.Username}\" thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = $"Không thể xóa \"{user.Username}\" vì có dữ liệu liên quan (đăng ký khóa học, bình luận, v.v.)";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.User?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
