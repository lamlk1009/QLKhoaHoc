using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoApp.Data;
using DemoApp.Models;

namespace DemoApp.Admins
{
    public class KhoaHocsController : Controller
    {
        private readonly AppDbContext _context;

        public KhoaHocsController(AppDbContext context)
        {
            _context = context;
        }
       
        // GET: KhoaHocs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.KhoaHoc.Include(k => k.DanhMuc).Include(k => k.user);
            return View(await appDbContext.ToListAsync());
        }

        // GET: KhoaHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.KhoaHoc == null)
            {
                return NotFound();
            }

            var khoaHoc = await _context.KhoaHoc
                .Include(k => k.DanhMuc)
                .Include(k => k.user)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (khoaHoc == null)
            {
                return NotFound();
            }

            return View(khoaHoc);
        }

        // GET: KhoaHocs/Create
        public IActionResult Create()
        {
            ViewData["DanhMucId"] = new SelectList(_context.Set<DanhMuc>(), "Id", "TenDanhMuc");
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId");
            return View();
        }

        // POST: KhoaHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaKhoaHoc,TenKhoaHoc,MoTaNgan,AnhBia,UserId,DanhMucId,CapDo,GiaTien,TrangThai,NgayTao")] KhoaHoc khoaHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(khoaHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.Set<DanhMuc>(), "Id", "TenDanhMuc", khoaHoc.DanhMucId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", khoaHoc.UserId);
            return View(khoaHoc);
        }

        // GET: KhoaHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.KhoaHoc == null)
            {
                return NotFound();
            }

            var khoaHoc = await _context.KhoaHoc.FindAsync(id);
            if (khoaHoc == null)
            {
                return NotFound();
            }
            ViewData["DanhMucId"] = new SelectList(_context.Set<DanhMuc>(), "Id", "TenDanhMuc", khoaHoc.DanhMucId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", khoaHoc.UserId);
            return View(khoaHoc);
        }

        // POST: KhoaHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaKhoaHoc,TenKhoaHoc,MoTaNgan,AnhBia,UserId,DanhMucId,CapDo,GiaTien,TrangThai,NgayTao")] KhoaHoc khoaHoc)
        {
            if (id != khoaHoc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khoaHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaHocExists(khoaHoc.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.Set<DanhMuc>(), "Id", "TenDanhMuc", khoaHoc.DanhMucId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", khoaHoc.UserId);
            return View(khoaHoc);
        }

        // GET: KhoaHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.KhoaHoc == null)
            {
                return NotFound();
            }

            var khoaHoc = await _context.KhoaHoc
                .Include(k => k.DanhMuc)
                .Include(k => k.user)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (khoaHoc == null)
            {
                return NotFound();
            }

            return View(khoaHoc);
        }

        // POST: KhoaHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.KhoaHoc == null)
            {
                return Problem("Entity set 'AppDbContext.KhoaHoc'  is null.");
            }
            var khoaHoc = await _context.KhoaHoc.FindAsync(id);
            if (khoaHoc != null)
            {
                _context.KhoaHoc.Remove(khoaHoc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult DanhSachKhoaHoc()
        {
            var khoaHocs = _context.KhoaHoc.ToList();
            return View(khoaHocs);
        }
        public IActionResult ChiTietKhoaHoc(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var khoaHoc = _context.KhoaHoc
                .Include(k => k.BaiHoc)
                .FirstOrDefault(k => k.Id == id);

            if (khoaHoc == null)
            {
                return NotFound();
            }

            khoaHoc.BaiHoc = khoaHoc.BaiHoc?
                .OrderBy(b => b.ThuTuHienThi)
                .ToList();

            return View(khoaHoc);
        }
        private bool KhoaHocExists(int id)
        {
          return (_context.KhoaHoc?.Any(e => e.Id == id)).GetValueOrDefault();
        }
      
       
    }
}
