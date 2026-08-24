using Microsoft.AspNetCore.Mvc;
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

            songslist = qs.Select(x => new CSongsWrap(x)).ToList();

            return View(songslist);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CSongsWrap sw)
        {
            var newsongdata = sw.tsong;
            if (newsongdata == null)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.Add(newsongdata);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
