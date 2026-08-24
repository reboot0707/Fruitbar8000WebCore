using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.Controllers
{
    public class SongsController : Controller
    {
        private readonly FruitBarDbContext _context;

        public SongsController(FruitBarDbContext context)
        {
            _context = context;
        }

        // GET: SongsController
        public async Task<IActionResult> Index()
        {
            var qs = _context.TSongs;
            List<CSongsWrap> songslist = new List<CSongsWrap>() { };

            songslist = await qs.Select(x => new CSongsWrap(x)).ToListAsync();

            return View(songslist);
        }

        public IActionResult Create()
        {
            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://learn.microsoft.com/zh-tw/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/implementing-basic-crud-functionality-with-the-entity-framework-in-asp-net-mvc-application#overpost.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FSongName,FLyrics,FDuration")] CSongsWrap sw)
        {
            var newsongdata = sw.tsong;
            if (newsongdata == null || !ModelState.IsValid)
            {
                return View(sw);
            }
            _context.Add(newsongdata);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            TSong? resultNow = await _context.TSongs.FirstOrDefaultAsync(x => x.FSongId == id);

            if (resultNow == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var resultNowShow = new CSongsWrap()
            {
                tsong = resultNow
            };
            return View(resultNowShow);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("FSongId,FSongName,FLyrics,FDuration")] CSongsWrap sw)
        {
            var updatedsongdata = sw.tsong;
            if (updatedsongdata == null || !ModelState.IsValid)
            {
                return View(sw);
            }
            try
            {
                _context.Update(updatedsongdata);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TSongExists(updatedsongdata.FSongId))
                {
                    return View(sw);
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            TSong? resultNow = await _context.TSongs.FirstOrDefaultAsync(x => x.FSongId == id);

            if (resultNow == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Remove(resultNow);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TSongExists(int? fsongid)
        {
            return _context.TSongs.Any(e => e.FSongId == fsongid);
        }
    }
}
