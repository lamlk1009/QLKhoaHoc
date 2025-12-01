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

        // GET: /admin/lessons?khoahocId=5
        public async Task<IActionResult> Lessons(int? khoahocId)  // đúng: chữ thường
        {
            ViewBag.KhoaHocList = await _context.KhoaHoc
                .Select(k => new { k.Id, k.TenKhoaHoc })
                .OrderBy(k => k.TenKhoaHoc)
                .ToListAsync();

            ViewBag.SelectedKhoaHocId = khoahocId;  // đúng

            var query = _context.BaiHoc.Include(b => b.KhoaHoc).AsQueryable();

            if (khoahocId.HasValue)  // đúng
                query = query.Where(b => b.KhoaHocId == khoahocId.Value);

            var lessons = await query.OrderBy(b => b.ThuTuHienThi).ToListAsync();
            return View(lessons);
        }

        // GET: Thêm bài học
        public async Task<IActionResult> CreateLesson(int? khoahocId)
        {
            if (!khoahocId.HasValue)
            {
                return RedirectToAction("Index");
            }

            // Lấy thông tin khóa học
            var khoaHoc = await _context.KhoaHoc.FindAsync(khoahocId.Value);
            if (khoaHoc == null)
            {
                return NotFound();
            }

            ViewBag.KhoaHocList = await _context.KhoaHoc
                .Select(k => new { k.Id, k.TenKhoaHoc })
                .ToListAsync();

            ViewBag.KhoaHocTen = khoaHoc.TenKhoaHoc;
            ViewBag.SelectedKhoaHocId = khoahocId.Value;

            // Tự động lấy số thứ tự tiếp theo
            var maxOrder = await _context.BaiHoc
                .Where(b => b.KhoaHocId == khoahocId.Value)
                .MaxAsync(b => (int?)b.ThuTuHienThi) ?? 0;

            var model = new BaiHoc
            {
                KhoaHocId = khoahocId.Value,
                LoaiNoiDung = "Video",
                ThuTuHienThi = maxOrder + 1
            };

            return View(model);
        }

        // POST: Thêm bài học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(BaiHoc model, IFormFile? videoFile)
        {
            // Loại bỏ validation cho KhoaHoc navigation property
            ModelState.Remove("KhoaHoc");
            ModelState.Remove("TienDoHocTap");

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload file nếu có (ưu tiên file upload hơn YouTube link)
                    if (videoFile != null && videoFile.Length > 0)
                    {
                        var folder = Path.Combine("wwwroot", "videos", "lessons");
                        Directory.CreateDirectory(folder);
                        var fileName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                        var path = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await videoFile.CopyToAsync(stream);
                        }

                        model.DuongDanNoiDung = "/videos/lessons/" + fileName;
                    }
                    // Nếu không có file upload, kiểm tra DuongDanNoiDung từ form (YouTube link)
                    else if (string.IsNullOrEmpty(model.DuongDanNoiDung))
                    {
                        ModelState.AddModelError("DuongDanNoiDung", "Vui lòng chọn video từ YouTube hoặc upload file");
                        ViewBag.KhoaHocList = await _context.KhoaHoc
                            .Select(k => new { k.Id, k.TenKhoaHoc })
                            .ToListAsync();

                        var khoaHoc = await _context.KhoaHoc.FindAsync(model.KhoaHocId);
                        ViewBag.KhoaHocTen = khoaHoc?.TenKhoaHoc;
                        ViewBag.SelectedKhoaHocId = model.KhoaHocId;

                        return View(model);
                    }

                    // Thêm bài học vào database
                    _context.BaiHoc.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thêm bài học thành công!";
                    return RedirectToAction("Index", new { khoahocId = model.KhoaHocId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Debug: In ra lỗi validation
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine("Validation Error: " + error);
                }
            }

            // Nếu có lỗi, load lại form
            ViewBag.KhoaHocList = await _context.KhoaHoc
                .Select(k => new { k.Id, k.TenKhoaHoc })
                .ToListAsync();

            var kh = await _context.KhoaHoc.FindAsync(model.KhoaHocId);
            ViewBag.KhoaHocTen = kh?.TenKhoaHoc;
            ViewBag.SelectedKhoaHocId = model.KhoaHocId;

            return View(model);
        }

        // GET: Sửa bài học
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.BaiHoc.FindAsync(id);
            if (lesson == null) return NotFound();

            ViewBag.KhoaHocList = await _context.KhoaHoc.Select(k => new { k.Id, k.TenKhoaHoc }).ToListAsync();
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(BaiHoc model, IFormFile? videoFile)
        {
            if (ModelState.IsValid)
            {
                if (videoFile != null && videoFile.Length > 0)
                {
                    var folder = Path.Combine("wwwroot", "videos", "lessons");
                    Directory.CreateDirectory(folder);
                    var fileName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                    var path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await videoFile.CopyToAsync(stream);
                    model.DuongDanNoiDung = "/videos/lessons/" + fileName;
                }

                _context.BaiHoc.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Lessons", new { khoahocId = model.KhoaHocId });
            }
            ViewBag.KhoaHocList = await _context.KhoaHoc.Select(k => new { k.Id, k.TenKhoaHoc }).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteLesson(int id)
        {
            try
            {
                var lesson = _context.BaiHoc.FirstOrDefault(x => x.Id == id);
                if (lesson == null)
                    return Json(new { success = false, msg = "Không tìm thấy" });

                _context.BaiHoc.Remove(lesson);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
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
