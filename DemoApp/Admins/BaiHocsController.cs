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
    public class BaiHocsController : Controller
    {
        private readonly AppDbContext _context;

        public BaiHocsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BaiHocs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.BaiHoc.Include(b => b.KhoaHoc);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BaiHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BaiHoc == null)
            {
                return NotFound();
            }

            var baiHoc = await _context.BaiHoc
                .Include(b => b.KhoaHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiHoc == null)
            {
                return NotFound();
            }

            return View(baiHoc);
        }

        // GET: BaiHocs/Create
        public IActionResult Create()
        {
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo");
            return View();
        }

        // POST: BaiHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KhoaHocId,TenBaiHoc,LoaiNoiDung,DuongDanNoiDung,ThuTuHienThi")] BaiHoc baiHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(baiHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", baiHoc.KhoaHocId);
            return View(baiHoc);
        }

        // GET: BaiHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BaiHoc == null)
            {
                return NotFound();
            }

            var baiHoc = await _context.BaiHoc.FindAsync(id);
            if (baiHoc == null)
            {
                return NotFound();
            }
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", baiHoc.KhoaHocId);
            return View(baiHoc);
        }

        // POST: BaiHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KhoaHocId,TenBaiHoc,LoaiNoiDung,DuongDanNoiDung,ThuTuHienThi")] BaiHoc baiHoc)
        {
            if (id != baiHoc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baiHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiHocExists(baiHoc.Id))
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
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", baiHoc.KhoaHocId);
            return View(baiHoc);
        }

        // GET: BaiHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BaiHoc == null)
            {
                return NotFound();
            }

            var baiHoc = await _context.BaiHoc
                .Include(b => b.KhoaHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiHoc == null)
            {
                return NotFound();
            }

            return View(baiHoc);
        }

        // POST: BaiHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BaiHoc == null)
            {
                return Problem("Entity set 'AppDbContext.BaiHoc'  is null.");
            }
            var baiHoc = await _context.BaiHoc.FindAsync(id);
            if (baiHoc != null)
            {
                _context.BaiHoc.Remove(baiHoc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaiHocExists(int id)
        {
          return (_context.BaiHoc?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
