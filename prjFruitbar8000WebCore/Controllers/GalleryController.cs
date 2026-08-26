using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Entities;
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
            // 修改前作邏輯, 參考某音樂平台, 先用歌曲為單位列表
            List<GalleryViewModel> querylistview = new List<GalleryViewModel>();

            var songlistq = _context.TSongs.Select(x => new
            {
                x.FSongId,
                x.FSongName,
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
                            id = song.FSongId,
                            SongName = song.FSongName,
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

        [HttpPost]
        public async Task<IActionResult> Create(NewSongViewModel nsvmSent)
        {
            if ((nsvmSent is null)
            || string.IsNullOrWhiteSpace(nsvmSent.SongName))
            {
                return RedirectToAction(nameof(List));
            }

            var createdSong = new TSong()
            {
                FSongName = nsvmSent.SongName,
            };
            if (nsvmSent.ArtistId is not null)
            {
                createdSong.TArtistsSongs.Add(new TArtistsSong()
                {
                    FArtistId = (int)nsvmSent.ArtistId,
                });
            }

            // NEXT-TODO: 初步先讓專輯歌曲編號合法不重複, 後續研議改資料庫約束條件或是優化指定/檢查機制
            if (nsvmSent.AlbumId is not null)
            {
                int relatedAlbumid = (int)nsvmSent.AlbumId;
                //List<int>
                var selectedAlbum = await _context.TAlbums
                    .Where(x => x.FAlbumId == relatedAlbumid)
                    .Include(x => x.TSongsAlbums)  // 針對指定導覽屬性做 Eager Loading, 等等才查得到既有專輯內曲目編號
                    .FirstOrDefaultAsync();
                if (selectedAlbum is not null)
                {
                    var usedTrackIdinAlbum = selectedAlbum
                        .TSongsAlbums.Select(x => x.FTrackNumber).ToList();

                    int assumedTrackNumber = 1;
                    if (usedTrackIdinAlbum.Count() > 0)
                    {
                        foreach (var num in usedTrackIdinAlbum)
                        {
                            while (assumedTrackNumber == num)
                            {
                                assumedTrackNumber++;
                            }
                        }
                    }
                    createdSong.TSongsAlbums.Add(new TSongsAlbum()
                    {
                        FAlbumId = relatedAlbumid,
                        FTrackNumber = assumedTrackNumber
                    });
                }
            }
            _context.TSongs.Add(createdSong);
            _context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return RedirectToAction(nameof(List));
            }
            var songToBeDeleted = await _context.TSongs
                .Include(x => x.TSongsAlbums)
                .Include(x => x.TArtistsSongs)
                .FirstOrDefaultAsync(x => x.FSongId == id);
            if (songToBeDeleted is null) // 開始查詢
            {
                return RedirectToAction(nameof(List));
            }
            _context.RemoveRange(songToBeDeleted.TArtistsSongs);
            _context.RemoveRange(songToBeDeleted.TSongsAlbums);
            _context.Remove(songToBeDeleted);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}
