using System.Security.Claims;
using DemoApp.Data;
using DemoApp.Models;
using DemoApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Tổng khóa học
            var totalCourses = await _context.KhoaHoc.CountAsync();

            // Tổng học viên (giả sử Role.Name == "User")
            var totalStudents = await _context.User
                .Include(u => u.Role)
                .CountAsync(u => u.Role.RoleName == "User");

            // Thống kê đăng ký theo khóa học
            var stats = await _context.DangKyKhoaHoc
                .Include(d => d.KhoaHoc)
                .GroupBy(d => new { d.KhoaHocId, d.KhoaHoc.TenKhoaHoc })
                .Select(g => new CourseRegistrationStat
                {
                    CourseId = g.Key.KhoaHocId,
                    CourseName = g.Key.TenKhoaHoc,
                    Registrations = g.Count()
                })
                .OrderByDescending(x => x.Registrations)
                .ToListAsync();

            var totalRegs = stats.Sum(x => x.Registrations);

            foreach (var s in stats)
            {
                s.Percentage = totalRegs == 0
                    ? 0
                    : (double)s.Registrations / totalRegs * 100.0;
            }

            var vm = new AdminDashboardViewModel
            {
                TotalCourses = totalCourses,
                TotalStudents = totalStudents,
                TotalRegistrations = totalRegs,
                RegistrationStats = stats
            };

            return View(vm);
        
        }

        // Quản lý khóa học
        public IActionResult Courses()
        {
            var courses = _context.KhoaHoc
               .Include(k => k.DanhMuc)
               .Include(k => k.user)
               .Include(k => k.DangKyKhoaHoc)
               .ToList();

            ViewBag.DanhMucList = _context.DanhMuc.ToList();

            return View(courses);
        }

        // GET: /admin/courses/create  → Hiện form thêm khóa học
        [HttpGet("admin/courses/create")]
        public IActionResult CreateCourse()
        {
            // Danh mục để show dropdown
            ViewBag.DanhMucList = _context.DanhMuc.ToList();

            // Có thể set sẵn giá trị mặc định
            var model = new KhoaHoc
            {
                TrangThai = "BanNhap",
                CapDo = "CoBan",
                NgayTao = DateTime.Now
            };

            // View: Views/Admin/CreateCourse.cshtml
            return View("CreateCourse", model);
        }

        // POST: /admin/courses/create  → Nhận form, lưu DB
        [HttpPost("admin/courses/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(KhoaHoc model, IFormFile AnhBiaFile)
        {
            ViewBag.DanhMucList = _context.DanhMuc.ToList();

            // Gán UserId theo user đang đăng nhập (nếu chưa có)
            if (model.UserId == 0)
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int uid))
                {
                    model.UserId = uid;
                }
                else
                {
                    ModelState.AddModelError("", "Không lấy được thông tin giảng viên (UserId).");
                }
            }

            // 👉 CHECK TRÙNG MÃ KHÓA HỌC (CREATE)
            bool maTrung = await _context.KhoaHoc
                .AnyAsync(k => k.MaKhoaHoc == model.MaKhoaHoc);

            if (maTrung)
            {
                ModelState.AddModelError("MaKhoaHoc", "Mã khóa học này đã tồn tại, hãy chọn mã khác.");
            }

            // Nếu có lỗi => trả lại form
            if (!ModelState.IsValid)
            {
                return View("CreateCourse", model);
            }

            // Upload ảnh bìa (nếu có chọn file)
            if (AnhBiaFile != null && AnhBiaFile.Length > 0)
            {
                string folder = Path.Combine("wwwroot", "images", "courses");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(AnhBiaFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhBiaFile.CopyToAsync(stream);
                }

                model.AnhBia = "/images/courses/" + fileName;
            }

            model.NgayTao = DateTime.Now;
            model.GiaTien = 0;

            _context.KhoaHoc.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm khóa học thành công!";
            return RedirectToAction(nameof(Courses));
        }
        // =============== EDIT COURSE (GET) ===============
        [HttpGet("admin/courses/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.KhoaHoc
                .Include(k => k.DanhMuc)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (course == null) return NotFound();

            ViewBag.DanhMucList = await _context.DanhMuc.ToListAsync();


            return View(course);
        }
        // =============== EDIT COURSE (POST) ===============
        [HttpPost("admin/courses/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhoaHoc model)
        {
            var course = await _context.KhoaHoc.FirstOrDefaultAsync(k => k.Id == id);
            if (course == null) return NotFound();

            // 👉 CHECK TRÙNG MÃ KHÓA HỌC (EDIT) – loại trừ chính nó
            bool maTrung = await _context.KhoaHoc
                .AnyAsync(k => k.Id != id && k.MaKhoaHoc == model.MaKhoaHoc);

            if (maTrung)
            {
                ModelState.AddModelError("MaKhoaHoc", "Mã khóa học này đã tồn tại, hãy chọn mã khác.");
            }

            if (!ModelState.IsValid)
            {
                // Cần load lại danh mục để dropdown không bị null
                ViewBag.DanhMucList = await _context.DanhMuc.ToListAsync();
                return View(model);
            }

            // update thuộc tính
            course.TenKhoaHoc = model.TenKhoaHoc;
            course.MaKhoaHoc = model.MaKhoaHoc;
            course.MoTaNgan = model.MoTaNgan;
            
            course.GiaTien = model.GiaTien;
            course.CapDo = model.CapDo;
            course.DanhMucId = model.DanhMucId;
            course.TrangThai = model.TrangThai;
            course.AnhBia = model.AnhBia;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật khóa học thành công!";
            return RedirectToAction(nameof(Courses));
        }
        // =============== DELETE COURSE ===============
        [HttpPost("admin/courses/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.KhoaHoc
                .Include(k => k.DangKyKhoaHoc)
                .Include(k => k.BaiHoc) // nếu có bảng bài học
                .FirstOrDefaultAsync(k => k.Id == id);

            if (course == null) return NotFound();

            // Nếu có đăng ký khóa học thì xóa luôn
            if (course.DangKyKhoaHoc != null)
                _context.DangKyKhoaHoc.RemoveRange(course.DangKyKhoaHoc);

            // Nếu có bài học thì xóa luôn
            if (course.BaiHoc != null)
                _context.BaiHoc.RemoveRange(course.BaiHoc);

            _context.KhoaHoc.Remove(course);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Quản lý người dùng
        public IActionResult Users() => View();

        // Đơn hàng
        public IActionResult Orders() => View();

        // Doanh thu
        public IActionResult Revenue() => View();

        // Đánh giá
        public IActionResult Reviews() => View();
    }
}
