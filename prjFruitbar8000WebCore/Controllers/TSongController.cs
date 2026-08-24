
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models;

public class TSongController : Controller
{
    private readonly FruitBarDbContext _context;

    public TSongController(FruitBarDbContext context)
    {
        _context = context;
    }

    // GET: TSONGS
    public async Task<IActionResult> Index()
    {
        return View(await _context.TSongs.ToListAsync());
    }

    // GET: TSONGS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tsong = await _context.TSongs
            .FirstOrDefaultAsync(m => m.FSongId == id);
        if (tsong == null)
        {
            return NotFound();
        }

        return View(tsong);
    }

    // GET: TSONGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TSONGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FSongId,FSongName,FIsDeleted,FLyrics,FDuration,TArtistsSongs,TSongGenres,TSongsAlbums")] TSong tsong)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tsong);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tsong);
    }

    // GET: TSONGS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tsong = await _context.TSongs.FindAsync(id);
        if (tsong == null)
        {
            return NotFound();
        }
        return View(tsong);
    }

    // POST: TSONGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("FSongId,FSongName,FIsDeleted,FLyrics,FDuration,TArtistsSongs,TSongGenres,TSongsAlbums")] TSong tsong)
    {
        if (id != tsong.FSongId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tsong);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TSongExists(tsong.FSongId))
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
        return View(tsong);
    }

    // GET: TSONGS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tsong = await _context.TSongs
            .FirstOrDefaultAsync(m => m.FSongId == id);
        if (tsong == null)
        {
            return NotFound();
        }

        return View(tsong);
    }

    // POST: TSONGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var tsong = await _context.TSongs.FindAsync(id);
        if (tsong != null)
        {
            _context.TSongs.Remove(tsong);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TSongExists(int? fsongid)
    {
        return _context.TSongs.Any(e => e.FSongId == fsongid);
    }
}
