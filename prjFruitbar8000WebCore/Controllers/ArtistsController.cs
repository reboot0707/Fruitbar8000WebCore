using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.Controllers;

public class ArtistsController : Controller
{
    private readonly FruitBarDbContext _context;

    public ArtistsController(FruitBarDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<CArtistsWrap> artists = await _context.TArtists
            .Select(artist => new CArtistsWrap(artist))
            .ToListAsync();

        return View(artists);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FArtistName,FArtistType")] CArtistsWrap artistWrap)
    {
        if (!ModelState.IsValid)
        {
            return View(artistWrap);
        }

        _context.Add(artistWrap.tartist);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        TArtist? artist = await _context.TArtists
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.FArtistId == id);

        if (artist is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new CArtistsWrap(artist));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("FArtistId,FArtistName,FArtistType")] CArtistsWrap artistWrap)
    {
        if (!ModelState.IsValid)
        {
            return View(artistWrap);
        }

        TArtist? artist = await _context.TArtists
            .FirstOrDefaultAsync(item => item.FArtistId == artistWrap.FArtistId);

        if (artist is null)
        {
            return RedirectToAction(nameof(Index));
        }

        artist.FArtistName = artistWrap.FArtistName;
        artist.FArtistType = artistWrap.FArtistType;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // TODO: log error
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }
        if (await new CheckNavigate(_context).IsArtistHaveSong((int)id))
        {
            return RedirectToAction(nameof(Index));
        }

        TArtist? artist = await _context.TArtists
            .FirstOrDefaultAsync(item => item.FArtistId == id);

        if (artist is null)
        {
            return RedirectToAction(nameof(Index));
        }

        _context.Remove(artist);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
