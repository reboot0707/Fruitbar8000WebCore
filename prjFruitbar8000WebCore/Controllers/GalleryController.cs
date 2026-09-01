using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;
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
            List<GalleryListViewModel> queryList = new List<GalleryListViewModel>();

            queryList = new GalleryDataAccess().List(queryList, _context);

            return View(queryList);
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return RedirectToAction(nameof(List));
            }
            var selListArtist = await _context.TArtists
                .OrderBy(x => x.FArtistName)
                .Select(x => new SelectListItem()
                {
                    Value = x.FArtistId.ToString(),
                    Text = x.FArtistName
                }).ToListAsync();
            var selListAlbum = await _context.TAlbums
                .OrderBy(x => x.FAlbumName)
                .Select(x => new SelectListItem()
                {
                    Value = x.FAlbumId.ToString(),
                    Text = x.FAlbumName
                }).ToListAsync();
            var editSong = await _context.TSongs
                .Where(x => x.FSongId == id)
                .Include(x => x.TArtistsSongs)
                .Include(x => x.TSongsAlbums)
                .FirstOrDefaultAsync();
            if (editSong is null)
            {
                return RedirectToAction(nameof(List));
            }
            var artistsIdOfSong = editSong.TArtistsSongs.Select(x => x.FArtistId).ToList();
            var albumsIdOfSong = editSong.TSongsAlbums.Select(x => x.FAlbumId).ToList();


            var seletedArtists = selListArtist.Where(x => artistsIdOfSong.Contains(int.Parse(x.Value)));
            var selectedAlbums = selListAlbum.Where(x => albumsIdOfSong.Contains(int.Parse(x.Value)));
            foreach (var artitem in seletedArtists)
            {
                artitem.Selected = true;
            }
            foreach (var albumitem in selectedAlbums)
            {
                albumitem.Selected = true;
            }
            var infoEditSong = new GallerySongViewModel()
            {
                id = editSong.FSongId,
                SongName = editSong.FSongName,
                SelectedArtistIdList = artistsIdOfSong,
                SelectedAlbumIdList = albumsIdOfSong,
                OptionArtistIdList = selListArtist,
                OptionAlbumIdList = selListAlbum
            };
            return View(infoEditSong);
        }

        // FIXED: 修正差集同步邏輯, 目前邏輯會踩到中介表 FK 不能為空值的坑
        [HttpPost]
        public async Task<IActionResult> Edit(GallerySongViewModel gsvm)
        {
            if (gsvm is null)
            {
                return RedirectToAction(nameof(List));
            }
            if (gsvm.id is null || 
                // KNOWN ISSUE: 因應 index 選取邏輯一定要有對應關聯資料，暫不開放藉由 Controller 清空歌曲所有的創作者/專輯關聯
                gsvm.SelectedArtistIdList is null || 
                gsvm.SelectedAlbumIdList is null || 
                string.IsNullOrWhiteSpace(gsvm.SongName))
            {
                return View(gsvm);
            }
            // FIXED: 此查詢必須一併載入 TArtistsSongs 與 TSongsAlbums。導覽集合雖已初始化但未代表資料庫內容，
            // 因此下方的 existRelationInList 會把既有關聯誤判為不存在，最後 INSERT 時撞上複合唯一索引。
            var tobeUpdate = _context.TSongs
            .Include(x => x.TArtistsSongs)
            .Include(x => x.TSongsAlbums)
            .FirstOrDefault(x => x.FSongId == gsvm.id);
            if (tobeUpdate is null)
            {
                return View(gsvm);
            }
            tobeUpdate.FSongName = gsvm.SongName;
            
            List<TArtistsSong> artistTobeAdd = new List<TArtistsSong>();
            List<TArtistsSong> artistTobeRemoved = new List<TArtistsSong>();

            foreach (var artistid in gsvm.SelectedArtistIdList)
            {
                // 檢查是否已有重複關聯
                var existRelationInList = tobeUpdate.TArtistsSongs
                    .Where(x => x.FArtistId == artistid).FirstOrDefault();

                // 如果沒有重複關聯, 新增關聯
                if (existRelationInList is null)
                {
                    // 先存在 List 裡面，全部確認後直接對 DbSet 操作
                    artistTobeAdd.Add(new TArtistsSong()
                    {
                        FArtistId = artistid,
                        FSong = tobeUpdate // 導覽屬性, 導覽回更新歌曲物件本體
                    });
                }
            }
            // FIXED: 不可在 foreach 枚舉 TArtistsSongs 時修改同一集合，否則會拋出 Collection was modified；
            // 且關聯 FK 不可為 null，應由待刪除差集明確將中介實體標記為 Deleted，而非只切斷導覽關聯。
            artistTobeRemoved = tobeUpdate.TArtistsSongs.Where(x => !gsvm.SelectedArtistIdList.Contains(x.FArtistId)).ToList();

            _context.TArtistsSongs.AddRange(artistTobeAdd);
            _context.TArtistsSongs.RemoveRange(artistTobeRemoved);

            List<TSongsAlbum> albumsTobeAdd = new List<TSongsAlbum>();
            List<TSongsAlbum> albumsTobeRemoved = new List<TSongsAlbum>();

            var selectedAlbumList = await _context.TAlbums
                .Where(x => gsvm.SelectedAlbumIdList.Contains(x.FAlbumId))
                .Include(x => x.TSongsAlbums)
                .ToListAsync();  // 針對指定導覽屬性做 Eager Loading, 等等才查得到既有專輯內曲目編號

            foreach (var selectedAlbum in selectedAlbumList)
            {
                // 檢查是否已有重複關聯
                var existRelationInList = tobeUpdate.TSongsAlbums
                    .Where(x => x.FAlbumId == selectedAlbum.FAlbumId).FirstOrDefault();

                // 如果沒有重複關聯, 新增關聯
                if (existRelationInList is null)
                {
                    // FIXED: TSongsAlbums 沒有保證排序；目前逐項 while 的結果受列舉順序影響，
                    // 例如曲號 [2, 1] 會算出已被占用的 2，儲存時撞上 (AlbumId, TrackNumber) 唯一索引。
                    // 即使改為正確的空號計算，並行請求仍可能選到同一曲號，儲存時仍須處理唯一限制衝突。
                    var usedTrackIdinAlbum = selectedAlbum.TSongsAlbums
                        .OrderBy(x => x.FTrackNumber)
                        .Select(x => x.FTrackNumber)
                        .ToList();

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

                    albumsTobeAdd.Add(new TSongsAlbum()
                    {
                        FAlbumId = selectedAlbum.FAlbumId,
                        FTrackNumber = assumedTrackNumber,
                        FSong = tobeUpdate // 導覽屬性, 導覽回更新歌曲物件本體
                    });
                }
            }
            // FIXED: 不可在 foreach 枚舉 TSongsAlbums 時修改同一集合，否則會拋出 Collection was modified；
            // 且關聯 FK 不可為 null，應由待刪除差集明確將中介實體標記為 Deleted，而非只切斷導覽關聯。
            albumsTobeRemoved = tobeUpdate.TSongsAlbums.Where(x => !gsvm.SelectedAlbumIdList.Contains(x.FAlbumId)).ToList();

            _context.TSongsAlbums.AddRange(albumsTobeAdd);
            _context.TSongsAlbums.RemoveRange(albumsTobeRemoved);

            await _context.SaveChangesAsync();
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
