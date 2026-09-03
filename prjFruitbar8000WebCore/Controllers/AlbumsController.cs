using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.Controllers;

public class AlbumsController : Controller
{
    private readonly FruitBarDbContext _context;

    public AlbumsController(FruitBarDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<CAlbumsWrap> albums = await _context.TAlbums
            .Select(album => new CAlbumsWrap(album))
            .ToListAsync();

        return View(albums);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FAlbumName,FReleaseDate,FAlbumType")] CAlbumsWrap albumWrap)
    {
        if (!ModelState.IsValid)
        {
            return View(albumWrap);
        }

        _context.Add(albumWrap.talbum);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        TAlbum? album = await _context.TAlbums
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.FAlbumId == id);

        if (album is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new CAlbumsWrap(album));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("FAlbumId,FAlbumName,FReleaseDate,FAlbumType")] CAlbumsWrap albumWrap)
    {
        if (!ModelState.IsValid)
        {
            return View(albumWrap);
        }

        TAlbum? album = await _context.TAlbums
            .FirstOrDefaultAsync(item => item.FAlbumId == albumWrap.FAlbumId);

        if (album is null)
        {
            return RedirectToAction(nameof(Index));
        }

        album.FAlbumName = albumWrap.FAlbumName;
        album.FReleaseDate = albumWrap.FReleaseDate;
        album.FAlbumType = albumWrap.FAlbumType;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TAlbumExists(albumWrap.FAlbumId))
            {
                return RedirectToAction(nameof(Index));
            }

            throw;
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

        if (await new CheckNavigate(_context).IsAlbumHaveSong((int)id))
        {
            return RedirectToAction(nameof(Index));
        }

        TAlbum? album = await _context.TAlbums
            .FirstOrDefaultAsync(item => item.FAlbumId == id);

        if (album is null)
        {
            return RedirectToAction(nameof(Index));
        }

        _context.Remove(album);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool TAlbumExists(int albumId)
    {
        return _context.TAlbums.Any(item => item.FAlbumId == albumId);
    }
}
