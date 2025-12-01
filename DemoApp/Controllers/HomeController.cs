    using System.Diagnostics;
using System.Threading.Tasks;
using DemoApp.Data;
using DemoApp.Models;
    using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Controllers
    {
        public class HomeController : Controller
        {
        private readonly AppDbContext _context;
            private readonly ILogger<HomeController> _logger;

            public HomeController(ILogger<HomeController> logger, AppDbContext context)
            {
                _logger = logger;
                _context = context;
            }

            public IActionResult Index()
            {
                return View();
            }

            //public IActionResult DanhSachKhoaHoc()
            //{
            //    var khoaHocs = _context.KhoaHoc.ToList();
            //    return View(khoaHocs);
            //}
        //public IActionResult ChiTietKhoaHoc(int id)
        //{
        //    var khoaHoc = _context.KhoaHoc
        //        .Include(k => k.BaiHoc)
        //        .Include(k => k.user)
        //        .Include(k => k.DanhMuc)
        //        .FirstOrDefault(k => k.Id == id);

        //    if (khoaHoc == null)
        //    {
        //        return NotFound();
        //    }

        //    khoaHoc.BaiHoc = khoaHoc.BaiHoc?
        //        .OrderBy(b => b.ThuTuHienThi)
        //        .ToList();

        //    return View(khoaHoc);
        //}
        public IActionResult Privacy()
            {
                return View();
            }
            
      
            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
