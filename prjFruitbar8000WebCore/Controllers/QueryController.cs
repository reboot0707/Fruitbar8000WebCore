using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.Controllers
{
    public class QueryController : Controller
    {
        private readonly FruitBarDbContext _context;
        public QueryController(FruitBarDbContext context)
        {
            _context = context;
        }

        // GET: QueryController
        public async Task<IActionResult> Index()
        {
            // 修改前作邏輯, 參考某音樂平台, 先用專輯為單位列表
            List<QueryViewModel> querylistview = new List<QueryViewModel>();

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
                        QueryViewModel qvm = new QueryViewModel()
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
    }
}
