using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
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
    }
}
