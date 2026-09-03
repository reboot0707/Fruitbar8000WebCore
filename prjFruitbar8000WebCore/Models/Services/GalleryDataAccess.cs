using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.Models.Services;

public class GalleryDataAccess
{
    public GalleryDataAccess() {} //constructor

    // input: (1) empty "quertlistview" data, (2) DBcontext
    public List<GalleryListViewModel> List(List<GalleryListViewModel> querylistview, FruitBarDbContext inputContext)
    {     
        var songlistq = inputContext.TSongs
                .OrderBy(x => x.FSongName)
                .Select(x => new
                {
                    x.FSongId,
                    x.FSongName,
                    ArtistNames = x.TArtistsSongs
                        .OrderBy(y => y.FArtist.FArtistName)
                        .Select(y => y.FArtist.FArtistName),
                    AlbumNames = x.TSongsAlbums
                        .OrderBy(y => y.FAlbum.FAlbumName)
                        .Select(y => y.FAlbum.FAlbumName)
                });

        // NEXT-TODO: use "SelectMany" to replace multi-layer loop
        foreach (var song in songlistq)
        {
            foreach (var songalbum in song.AlbumNames)
            {
                List<string> ArtistNameList = new List<string>();
                foreach (var songartist in song.ArtistNames)
                {
                    ArtistNameList.Add(songartist);
                }
                GalleryListViewModel qvm = new GalleryListViewModel()
                {
                    id = song.FSongId,
                    SongName = song.FSongName,
                    ArtistNames = String.Join('、', ArtistNameList),
                    AlbumName = songalbum
                };
                querylistview.Add(qvm);
            }
        }
        return querylistview;
    }

    // binding with MVC View Component
    public async Task<GallerySongViewModel> GetCreate(FruitBarDbContext inputContext)
    {
        var qArtist = inputContext.TArtists.OrderBy(x => x.FArtistName);
        var qAlbum = inputContext.TAlbums.OrderBy(x => x.FAlbumName);

        GallerySongViewModel nsvm = new GallerySongViewModel()
        {
            SongName = String.Empty,
            SelectedArtistIdList = new List<int>(),
            SelectedAlbumIdList = new List<int>(),
            OptionArtistIdList = await qArtist.Select(x => new SelectListItem()
            {
                Value = x.FArtistId.ToString(),
                Text = x.FArtistName
            }).ToListAsync(),
            OptionAlbumIdList = await qAlbum.Select(x => new SelectListItem()
            {
                Value = x.FAlbumId.ToString(),
                Text = x.FAlbumName
            })
            .ToListAsync()
        };
        return nsvm;
    }

    public async Task PostCreate(GallerySongViewModel nsvmSent, FruitBarDbContext inputContext)
        {
            if ((nsvmSent is null)
            || string.IsNullOrWhiteSpace(nsvmSent.SongName))
            {
                return;
            }
            var createdSong = new TSong()
            {
                FSongName = nsvmSent.SongName,
            };
            if (nsvmSent.SelectedAlbumIdList is null || 
                nsvmSent.SelectedArtistIdList is null)
            {
                return;
            }
            foreach (int artistid in nsvmSent.SelectedArtistIdList)
            {
                createdSong.TArtistsSongs.Add(new TArtistsSong()
                {
                    FArtistId = artistid,
                });
            }

            // NEXT-TODO: 初步先讓專輯歌曲編號合法不重複, 後續研議改資料庫約束條件或是優化指定/檢查機制
            foreach (int albumid in nsvmSent.SelectedAlbumIdList)
            {
                int relatedAlbumid = albumid;
                var selectedAlbum = await inputContext.TAlbums
                    .Where(x => x.FAlbumId == relatedAlbumid)
                    .Include(x => x.TSongsAlbums)  // 針對指定導覽屬性做 Eager Loading, 等等才查得到既有專輯內曲目編號
                    .FirstOrDefaultAsync();
                if (selectedAlbum is null)
                {
                    continue;
                }
                int assumedTrackNumber = AssumedTrackNumberInAlbum(selectedAlbum);
                createdSong.TSongsAlbums.Add(new TSongsAlbum()
                {
                    FAlbumId = relatedAlbumid,
                    FTrackNumber = assumedTrackNumber
                });
            }
    inputContext.TSongs.Add(createdSong);
        inputContext.SaveChanges();
    }

    // binding with MVC View Component
    public async Task<GallerySongViewModel> GetEdit(TSong editSong, FruitBarDbContext inputContext)
    {
        var selListArtist = await inputContext.TArtists
            .OrderBy(x => x.FArtistName)
            .Select(x => new SelectListItem()
            {
                Value = x.FArtistId.ToString(),
                Text = x.FArtistName
            }).ToListAsync();
        var selListAlbum = await inputContext.TAlbums
            .OrderBy(x => x.FAlbumName)
            .Select(x => new SelectListItem()
            {
                Value = x.FAlbumId.ToString(),
                Text = x.FAlbumName
            }).ToListAsync();
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
        return infoEditSong;
    }

    public async Task<bool> PostEdit(GallerySongViewModel gsvm, TSong tobeUpdate, FruitBarDbContext InputContext)
        {
            if(gsvm.id is null ||
                // KNOWN ISSUE: 因應 index 選取邏輯一定要有對應關聯資料，暫不開放藉由 Controller 清空歌曲所有的創作者/專輯關聯
                gsvm.SelectedArtistIdList is null ||
                gsvm.SelectedAlbumIdList is null || string.IsNullOrWhiteSpace(gsvm.SongName))
            {
                return false;
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

            InputContext.TArtistsSongs.AddRange(artistTobeAdd);
            InputContext.TArtistsSongs.RemoveRange(artistTobeRemoved);

            List<TSongsAlbum> albumsTobeAdd = new List<TSongsAlbum>();
            List<TSongsAlbum> albumsTobeRemoved = new List<TSongsAlbum>();

            var selectedAlbumList = await InputContext.TAlbums
                .Where(x => gsvm.SelectedAlbumIdList.Contains(x.FAlbumId))
                .Include(x => x.TSongsAlbums)
                .ToListAsync();  // 針對指定導覽屬性做 Eager Loading, 等等才查得到既有專輯內曲目編號

            foreach (var selectedAlbum in selectedAlbumList)
            {
                // 檢查是否已有重複關聯
                var existRelationInList = tobeUpdate.TSongsAlbums
                    .Where(x => x.FAlbumId == selectedAlbum.FAlbumId).FirstOrDefault();

                if (existRelationInList is not null)
                {
                    continue;
                }
                
                // 如果沒有重複關聯, 新增關聯
                int assumedTrackNumber = AssumedTrackNumberInAlbum(selectedAlbum);

                albumsTobeAdd.Add(new TSongsAlbum()
                {
                    FAlbumId = selectedAlbum.FAlbumId,
                    FTrackNumber = assumedTrackNumber,
                    FSong = tobeUpdate // 導覽屬性, 導覽回更新歌曲物件本體
                });
            }
            // FIXED: 不可在 foreach 枚舉 TSongsAlbums 時修改同一集合，否則會拋出 Collection was modified；
            // 且關聯 FK 不可為 null，應由待刪除差集明確將中介實體標記為 Deleted，而非只切斷導覽關聯。
            albumsTobeRemoved = tobeUpdate.TSongsAlbums.Where(x => !gsvm.SelectedAlbumIdList.Contains(x.FAlbumId)).ToList();

            InputContext.TSongsAlbums.AddRange(albumsTobeAdd);
            InputContext.TSongsAlbums.RemoveRange(albumsTobeRemoved);

            try
            {
                await InputContext.SaveChangesAsync();
                return true;
            }
            catch(Exception)
            {
                // NEXT-TODO: log error to log file
                return false;
            }
            
        }

    public async Task Delete(int? songId, FruitBarDbContext InputContext)
    {
        if(songId is null)
        { 
            return;
        }
        var songToBeDeleted = await InputContext.TSongs
            .Include(x => x.TSongsAlbums)
            .Include(x => x.TArtistsSongs)
            .FirstOrDefaultAsync(x => x.FSongId == songId);
        if (songToBeDeleted is null) // 開始查詢
        {
            return;
        }
        InputContext.RemoveRange(songToBeDeleted.TArtistsSongs);
        InputContext.RemoveRange(songToBeDeleted.TSongsAlbums);
        InputContext.Remove(songToBeDeleted);
        await InputContext.SaveChangesAsync();
        return;
    }

    // section for private methods
    private static int AssumedTrackNumberInAlbum(TAlbum selectedAlbum)
    {
        // FIXED: TSongsAlbums 沒有保證排序；目前逐項 while 的結果受列舉順序影響，
        // 例如曲號 [2, 1] 會算出已被占用的 2，儲存時撞上 (AlbumId, TrackNumber) 唯一索引。
        // 即使改為正確的空號計算，並行請求仍可能選到同一曲號，儲存時仍須處理唯一限制衝突。
        List<int> usedTrackIdinAlbum = selectedAlbum
            .TSongsAlbums
            .OrderBy(x => x.FTrackNumber)
            .Select(x => x.FTrackNumber)
            .ToList();
        int assumedTrackNumber = 1;
        foreach (int num in usedTrackIdinAlbum)
        {
            while (assumedTrackNumber == num)
            {
                assumedTrackNumber++;
            }
        }

        return assumedTrackNumber;
    }
}
