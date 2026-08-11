using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;

namespace prjFruitbar8000WebCore.Controllers
{
    public class TSongsController : Controller
    {
        private readonly FruitBarDbv15Context _context;

        public TSongsController(FruitBarDbv15Context context)
        {
            _context = context;
        }

        // GET: TSongs
        public async Task<IActionResult> Index()
        {
            return View(await _context.TSongs.ToListAsync());
        }

        // GET: TSongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tSong = await _context.TSongs
                .FirstOrDefaultAsync(m => m.FSongId == id);
            if (tSong == null)
            {
                return NotFound();
            }

            return View(tSong);
        }

        // GET: TSongs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TSongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FSongId,FSongName,FIsDeleted,FLyrics,FDuration")] TSong tSong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tSong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tSong);
        }

        // GET: TSongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tSong = await _context.TSongs.FindAsync(id);
            if (tSong == null)
            {
                return NotFound();
            }
            return View(tSong);
        }

        // POST: TSongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FSongId,FSongName,FIsDeleted,FLyrics,FDuration")] TSong tSong)
        {
            if (id != tSong.FSongId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tSong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TSongExists(tSong.FSongId))
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
            return View(tSong);
        }

        // GET: TSongs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tSong = await _context.TSongs
                .FirstOrDefaultAsync(m => m.FSongId == id);
            if (tSong == null)
            {
                return NotFound();
            }

            return View(tSong);
        }

        // POST: TSongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tSong = await _context.TSongs.FindAsync(id);
            if (tSong != null)
            {
                _context.TSongs.Remove(tSong);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TSongExists(int id)
        {
            return _context.TSongs.Any(e => e.FSongId == id);
        }
    }
}
