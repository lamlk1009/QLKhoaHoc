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
    public class DangKyKhoaHocsController : Controller
    {
        private readonly AppDbContext _context;

        public DangKyKhoaHocsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DangKyKhoaHocs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DangKyKhoaHoc.Include(d => d.KhoaHoc).Include(d => d.user);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DangKyKhoaHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DangKyKhoaHoc == null)
            {
                return NotFound();
            }

            var dangKyKhoaHoc = await _context.DangKyKhoaHoc
                .Include(d => d.KhoaHoc)
                .Include(d => d.user)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangKyKhoaHoc == null)
            {
                return NotFound();
            }

            return View(dangKyKhoaHoc);
        }

        // GET: DangKyKhoaHocs/Create
        public IActionResult Create()
        {
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo");
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId");
            return View();
        }

        // POST: DangKyKhoaHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,KhoaHocId,NgayDangKy,TrangThai")] DangKyKhoaHoc dangKyKhoaHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dangKyKhoaHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", dangKyKhoaHoc.KhoaHocId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", dangKyKhoaHoc.UserId);
            return View(dangKyKhoaHoc);
        }

        // GET: DangKyKhoaHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DangKyKhoaHoc == null)
            {
                return NotFound();
            }

            var dangKyKhoaHoc = await _context.DangKyKhoaHoc.FindAsync(id);
            if (dangKyKhoaHoc == null)
            {
                return NotFound();
            }
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", dangKyKhoaHoc.KhoaHocId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", dangKyKhoaHoc.UserId);
            return View(dangKyKhoaHoc);
        }

        // POST: DangKyKhoaHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,KhoaHocId,NgayDangKy,TrangThai")] DangKyKhoaHoc dangKyKhoaHoc)
        {
            if (id != dangKyKhoaHoc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dangKyKhoaHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangKyKhoaHocExists(dangKyKhoaHoc.Id))
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
            ViewData["KhoaHocId"] = new SelectList(_context.KhoaHoc, "Id", "CapDo", dangKyKhoaHoc.KhoaHocId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", dangKyKhoaHoc.UserId);
            return View(dangKyKhoaHoc);
        }

        // GET: DangKyKhoaHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DangKyKhoaHoc == null)
            {
                return NotFound();
            }

            var dangKyKhoaHoc = await _context.DangKyKhoaHoc
                .Include(d => d.KhoaHoc)
                .Include(d => d.user)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangKyKhoaHoc == null)
            {
                return NotFound();
            }

            return View(dangKyKhoaHoc);
        }

        // POST: DangKyKhoaHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DangKyKhoaHoc == null)
            {
                return Problem("Entity set 'AppDbContext.DangKyKhoaHoc'  is null.");
            }
            var dangKyKhoaHoc = await _context.DangKyKhoaHoc.FindAsync(id);
            if (dangKyKhoaHoc != null)
            {
                _context.DangKyKhoaHoc.Remove(dangKyKhoaHoc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DangKyKhoaHocExists(int id)
        {
          return (_context.DangKyKhoaHoc?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
