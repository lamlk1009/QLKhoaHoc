using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoApp.Data;
using DemoApp.Models;
using DemoApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Controllers
{
    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly AppDbContext _context;

        public MyCoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ================== LẤY USER HIỆN TẠI ==================
        private async Task<User?> GetCurrentUserAsync()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var identityName = User.Identity.Name; // Email hoặc Username

            return await _context.User
                .FirstOrDefaultAsync(u =>
                    u.Email == identityName || u.Username == identityName);
        }

        // ================== KHÓA HỌC CỦA TÔI ==================
        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var registrations = await _context.DangKyKhoaHoc
                .Where(dk => dk.UserId == currentUser.UserId
                             && dk.TrangThai == "DangHoc")
                .Include(dk => dk.KhoaHoc!)
                    .ThenInclude(kh => kh.BaiHoc)
                .ToListAsync();

            var tienDoList = await _context.TienDoHocTap
                .Where(td => td.UserId == currentUser.UserId)
                .ToListAsync();

            var viewModel = new List<MyCourseItemViewModel>();

            foreach (var dk in registrations)
            {
                var kh = dk.KhoaHoc!;
                var totalLessons = kh.BaiHoc?.Count ?? 0;

                var completedLessons = tienDoList
                    .Where(td => td.KhoaHocId == kh.Id && td.DaHoanThanh)
                    .Select(td => td.BaiHocId)
                    .Distinct()
                    .Count();

                int percent = totalLessons > 0
                    ? (int)Math.Round(completedLessons * 100.0 / totalLessons)
                    : 0;

                viewModel.Add(new MyCourseItemViewModel
                {
                    CourseId = kh.Id,
                    CourseTitle = kh.TenKhoaHoc,
                    ShortDescription = kh.MoTaNgan,
                    ThumbnailUrl = "/images/" + (kh.AnhBia ?? "default-course.jpg"),
                    Level = MapLevel(kh.CapDo),
                    TotalLessons = totalLessons,
                    ProgressPercent = percent,
                    StartDate = dk.NgayDangKy
                });
            }

            return View(viewModel);
        }

        private string MapLevel(string? capDo)
        {
            return capDo switch
            {
                "CoBan" => "Cơ bản",
                "TrungCap" => "Trung cấp",
                "NangCao" => "Nâng cao",
                _ => "All level"
            };
        }

        // ================== CHI TIẾT KHÓA HỌC ==================
        public async Task<IActionResult> Details(int id, int? activeLessonId = null)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.KhoaHoc
                .Include(k => k.BaiHoc)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (course == null)
                return NotFound();

            // (OPTION) Nếu chưa có buổi học nào thì create vài buổi demo theo số bài
            var hasSessions = await _context.BuoiHoc.AnyAsync(b => b.KhoaHocId == id);
            if (!hasSessions && course.BaiHoc != null && course.BaiHoc.Count > 0)
            {
                var orderedLessons = course.BaiHoc
                    .OrderBy(b => b.ThuTuHienThi)
                    .ThenBy(b => b.Id)
                    .ToList();

                var demoSessions = new List<BuoiHoc>();
                int order = 1;
                foreach (var les in orderedLessons)
                {
                    demoSessions.Add(new BuoiHoc
                    {
                        KhoaHocId = id,
                        TenBuoiHoc = $"Buổi {order}: {les.TenBaiHoc}",
                        ThuTuBuoi = order,
                        NgayHoc = DateTime.Today.AddDays(order - 1)
                    });
                    order++;
                }

                _context.BuoiHoc.AddRange(demoSessions);
                await _context.SaveChangesAsync();
            }

            var lessons = course.BaiHoc
                .OrderBy(b => b.ThuTuHienThi)
                .ThenBy(b => b.Id)
                .ToList();

            var totalLessons = lessons.Count;

            var progresses = await _context.TienDoHocTap
                .Where(t => t.UserId == currentUser.UserId && t.KhoaHocId == id)
                .ToListAsync();

            var completedIds = progresses
                .Where(t => t.DaHoanThanh)
                .Select(t => t.BaiHocId)
                .Distinct()
                .ToList();

            int completedCount = completedIds.Count;

            int progressPercent = totalLessons > 0
                ? (int)Math.Round(completedCount * 100.0 / totalLessons)
                : 0;

            // ====== ĐIỂM DANH + THỐNG KÊ ======
            var sessions = await _context.BuoiHoc
                .Where(b => b.KhoaHocId == id)
                .OrderBy(b => b.ThuTuBuoi)
                .ToListAsync();

            var attendances = await _context.DiemDanh
                .Where(d => d.UserId == currentUser.UserId && d.KhoaHocId == id)
                .Include(d => d.BuoiHoc)
                .ToListAsync();

            var allProgresses = await _context.TienDoHocTap
                .Where(t => t.UserId == currentUser.UserId && t.KhoaHocId == id)
                .ToListAsync();

            var stats = new CourseLearningStatsViewModel();

            if (allProgresses.Any())
            {
                stats.StartDate = allProgresses.Min(p => p.ThoiGianBatDau);
                stats.LastStudyDate = allProgresses.Max(p => p.ThoiGianBatDau);

                stats.StudyDays = allProgresses
                    .Select(p => p.ThoiGianBatDau.Value.Date)
                    .Distinct()
                    .Count();

                stats.TotalStudyMinutes = allProgresses.Sum(p => p.ThoiGianHoc);
            }

            stats.TotalSessions = sessions.Count;

            stats.AttendancePresent = attendances
                .Count(d => d.TrangThai == "present" || d.TrangThai == "late");

            stats.AttendanceRate = stats.TotalSessions > 0
                ? (int)Math.Round(stats.AttendancePresent * 100.0 / stats.TotalSessions)
                : 0;

            if (allProgresses.Any())
            {
                var studyDates = allProgresses
                    .Select(p => p.ThoiGianBatDau.Value.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                int streak = 0;
                DateTime cursor = DateTime.Today;

                while (studyDates.Contains(cursor))
                {
                    streak++;
                    cursor = cursor.AddDays(-1);
                }

                stats.CurrentStreakDays = streak;
                stats.StreakDays = streak;
            }

            stats.AvgScore = 0;

            var vm = new CourseLearningViewModel
            {
                KhoaHoc = course,
                Lessons = lessons,
                TotalLessons = totalLessons,
                CompletedLessons = completedCount,
                CompletedLessonIds = completedIds,
                ProgressPercent = progressPercent,
                Sessions = sessions,
                Attendances = attendances,
                Stats = stats
            };

            ViewBag.ActiveLessonId = activeLessonId;

            return View(vm);
        }

        // ================== HOÀN THÀNH BÀI HỌC (+ AUTO ĐIỂM DANH) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int courseId, int lessonId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var lesson = await _context.BaiHoc
                .FirstOrDefaultAsync(b => b.Id == lessonId && b.KhoaHocId == courseId);

            if (lesson == null)
                return NotFound();

            // BẮT BUỘC HOÀN THÀNH BÀI TRƯỚC
            var previousLesson = await _context.BaiHoc
                .Where(b => b.KhoaHocId == courseId && b.ThuTuHienThi < lesson.ThuTuHienThi)
                .OrderByDescending(b => b.ThuTuHienThi)
                .FirstOrDefaultAsync();

            if (previousLesson != null)
            {
                var prevDone = await _context.TienDoHocTap
                    .AnyAsync(t =>
                        t.UserId == currentUser.UserId &&
                        t.KhoaHocId == courseId &&
                        t.BaiHocId == previousLesson.Id &&
                        t.DaHoanThanh);

                if (!prevDone)
                {
                    TempData["Error"] = "Bạn cần hoàn thành bài học trước đó.";
                    return RedirectToAction("Details", new { id = courseId });
                }
            }

            // TIẾN ĐỘ
            var progress = await _context.TienDoHocTap
                .FirstOrDefaultAsync(t =>
                    t.UserId == currentUser.UserId &&
                    t.KhoaHocId == courseId &&
                    t.BaiHocId == lessonId);

            if (progress == null)
            {
                progress = new TienDoHocTap
                {
                    UserId = currentUser.UserId,
                    KhoaHocId = courseId,
                    BaiHocId = lessonId,
                    ThoiGianBatDau = DateTime.Now
                };
                _context.TienDoHocTap.Add(progress);
            }

            progress.DaHoanThanh = true;
            progress.TrangThaiHoc = "DaHoanThanh";
            progress.ThoiGianHoanThanh = DateTime.Now;
            progress.ThoiGianCapNhat = DateTime.Now;
            progress.TyLeHoanThanh = 100;
            progress.ThoiGianHoc = lesson.ThoiLuong.Value > 0 ? lesson.ThoiLuong.Value : 45;

            // ============ AUTO ĐIỂM DANH BUỔI HỌC TƯƠNG ỨNG ============
            // Quy ước: BuoiHoc.ThuTuBuoi trùng với BaiHoc.ThuTuHienThi
            var session = await _context.BuoiHoc
                .FirstOrDefaultAsync(b =>
                    b.KhoaHocId == courseId &&
                    b.ThuTuBuoi == lesson.ThuTuHienThi);

            if (session != null)
            {
                var attend = await _context.DiemDanh
                    .FirstOrDefaultAsync(d =>
                        d.UserId == currentUser.UserId &&
                        d.KhoaHocId == courseId &&
                        d.BuoiHocId == session.Id);

                if (attend == null)
                {
                    attend = new DiemDanh
                    {
                        UserId = currentUser.UserId,
                        KhoaHocId = courseId,
                        BuoiHocId = session.Id,
                        NgayDiemDanh = DateTime.Now,
                        TrangThai = "present"
                    };
                    _context.DiemDanh.Add(attend);
                }
                else
                {
                    attend.NgayDiemDanh = DateTime.Now;
                    attend.TrangThai = "present";
                    _context.DiemDanh.Update(attend);
                }
            }
            // ===========================================================

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = courseId });
        }

        // ================== ENROLL (HỌC NGAY) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (id <= 0)
                return RedirectToAction("DanhSachKhoaHoc", "KhoaHocs");

            var course = await _context.KhoaHoc.FindAsync(id);
            if (course == null)
                return NotFound();

            var dk = await _context.DangKyKhoaHoc
                .FirstOrDefaultAsync(x => x.UserId == currentUser.UserId && x.KhoaHocId == id);

            if (dk == null)
            {
                dk = new DangKyKhoaHoc
                {
                    UserId = currentUser.UserId,
                    KhoaHocId = id,
                    NgayDangKy = DateTime.Now,
                    TrangThai = "DangHoc"
                };
                _context.DangKyKhoaHoc.Add(dk);
            }
            else
            {
                dk.TrangThai = "DangHoc";
                dk.NgayDangKy = DateTime.Now;
                _context.DangKyKhoaHoc.Update(dk);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "MyCourses");
        }

        // ================== (OPTION) ĐIỂM DANH THỦ CÔNG MỘT BUỔI ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int courseId, int sessionId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var session = await _context.BuoiHoc
                .FirstOrDefaultAsync(b => b.Id == sessionId && b.KhoaHocId == courseId);

            if (session == null)
                return NotFound();

            var attend = await _context.DiemDanh
                .FirstOrDefaultAsync(d =>
                    d.UserId == currentUser.UserId &&
                    d.KhoaHocId == courseId &&
                    d.BuoiHocId == sessionId);

            if (attend == null)
            {
                attend = new DiemDanh
                {
                    UserId = currentUser.UserId,
                    KhoaHocId = courseId,
                    BuoiHocId = sessionId,
                    NgayDiemDanh = DateTime.Now,
                    TrangThai = "present"
                };
                _context.DiemDanh.Add(attend);
            }
            else
            {
                attend.NgayDiemDanh = DateTime.Now;
                attend.TrangThai = "present";
                _context.DiemDanh.Update(attend);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = courseId });
        }
    }
}
