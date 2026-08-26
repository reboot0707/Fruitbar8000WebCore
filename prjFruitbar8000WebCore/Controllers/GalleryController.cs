using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.Controllers
{
    public class GalleryController : Controller
    {
        private readonly FruitBarDbContext _context;
        public GalleryController(FruitBarDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        // GET: QueryController
        public async Task<IActionResult> List()
        {
            // 修改前作邏輯, 參考某音樂平台, 先用專輯為單位列表
            List<GalleryViewModel> querylistview = new List<GalleryViewModel>();

            var songlistq = _context.TSongs.Select(x => new
            {
                SongName = x.FSongName,
                ArtistNames = x.TArtistsSongs.Select(y => y.FArtist.FArtistName),
                AlbumNames = x.TSongsAlbums.Select(y => y.FAlbum.FAlbumName)
            });

            // NEXT-TODO: use "SelectMany" to replace multi-layer loop
            foreach (var song in songlistq)
            {
                foreach (var songalbum in song.AlbumNames)
                {
                    foreach (var songartist in song.ArtistNames)
                    {
                        GalleryViewModel qvm = new GalleryViewModel()
                        {
                            SongName = song.SongName,
                            ArtistName = songartist,
                            AlbumName = songalbum
                        };
                        querylistview.Add(qvm);
                    }
                }
            }
            return View(querylistview);
        }

        public async Task<IActionResult> Create()
        {
            var qArtist = _context.TArtists.OrderBy(x => x.FArtistName);
            var qAlbum = _context.TAlbums.OrderBy(x => x.FAlbumName);

            NewSongViewModel nsvm = new NewSongViewModel()
            {
                SongName = String.Empty,
                ArtistId = null,
                AlbumId = null,
                ArtistList = await qArtist.Select(x => new SelectListItem()
                {
                    Value = x.FArtistId.ToString(),
                    Text = x.FArtistName
                }).ToListAsync(),
                AlbumList = await qAlbum.Select(x => new SelectListItem()
                {
                    Value = x.FAlbumId.ToString(),
                    Text = x.FAlbumName
                })
                .ToListAsync()
            };
            return View(nsvm);
        }
    }
}
