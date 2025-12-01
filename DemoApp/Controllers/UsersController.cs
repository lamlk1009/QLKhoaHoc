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

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)              
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Set<Role>(), "RoleId", "RoleId", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,NumberPhone,Address,Username,RoleId")] User user, string? NewPassword)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                  
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        user.Password = NewPassword;
                    }
                    

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật thành công!";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Lỗi khi cập nhật!";
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoleId = new SelectList(_context.Role, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'AppDbContext.User'  is null.");
            }
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.User?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
