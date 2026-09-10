using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.Models.Services;

public class GalleryDataAccess
{
    public GalleryDataAccess() { } //constructor

    // input: (1) empty "quertlistview" data, (2) DBcontext
    public async Task<List<GalleryListViewModel>> List(FruitBarDbContext inputContext)
    {
        IQueryable<GallerySongsDTO> songlistq = GetGallerySongsRaw(inputContext);

        IQueryable<GalleryListViewModel> querylistview = songlistq.SelectMany(song => song.RelatedAlbums,
            (song, relatedAlbum) => new GalleryListViewModel
            {
                id = song.id,
                SongName = song.SongName,
                ArtistNames = string.Join('、', song.RelatedArtists.Select(x => x.ArtistName)),
                AlbumName = relatedAlbum.AlbumName
            });
        return await querylistview.ToListAsync();
    }

    public async Task<List<GallerySongsDTO>> ListApi(FruitBarDbContext inputContext)
    {
        IQueryable<GallerySongsDTO> songlistq = GetGallerySongsRaw(inputContext);

        return await songlistq.ToListAsync();
    }

    public async Task<GallerySongsDTO?> ListApiById(int id,
        FruitBarDbContext inputContext)
    {
        IQueryable<GallerySongsDTO> songlistq = GetGallerySongsRaw(inputContext);
        
        var qResult = songlistq
            .Where(x => x.id == id);

        return await qResult.FirstOrDefaultAsync();
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

    public async Task PostCreate(GallerySongViewModel nsvmSent,
        FruitBarDbContext inputContext)
    {
        if ((nsvmSent is null)
        || string.IsNullOrWhiteSpace(nsvmSent.SongName))
        {
            return;
        }
        if (nsvmSent.SelectedAlbumIdList is null ||
            nsvmSent.SelectedArtistIdList is null)
        {
            return;
        }
        GallerySongsDTO nsDTO = new GallerySongsDTO
        {
            SongName = nsvmSent.SongName,
            RelatedArtists = inputContext.TArtists
                .Where(x => nsvmSent.SelectedArtistIdList.Contains(x.FArtistId))
                .Select(x => new ArtistsDTO()
                {
                    id = x.FArtistId,
                    ArtistName = x.FArtistName,
                    ArtistType = x.FArtistType
                }),
            RelatedAlbums = inputContext.TAlbums
                .Where(x => nsvmSent.SelectedAlbumIdList.Contains(x.FAlbumId))
                .Select(x => new AlbumsDTO()
                {
                    id = x.FAlbumId,
                    AlbumName = x.FAlbumName,
                    AlbumType = x.FAlbumType
                }),
        };
        ResultDTO result = await CreateGallerySongCommon(nsDTO, inputContext);
    }

    public async Task<ResultDTO> PostCreateApi(GallerySongsDTO nsDTO,
        FruitBarDbContext inputContext)
    {
        if ((nsDTO is null)
        || string.IsNullOrWhiteSpace(nsDTO.SongName))
        {
            return new ResultDTO() { IsSuccess = false, StatusMessage = "NotFound" };
        }
        ResultDTO result = await CreateGallerySongCommon(nsDTO, inputContext);
        return result;
    }

    // binding with MVC View Component
    public async Task<GallerySongViewModel> GetEdit(TSong editSong,
        FruitBarDbContext inputContext)
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

    public async Task<ResultDTO> PostEdit(GallerySongViewModel gsvm,
        TSong tobeUpdate,
        FruitBarDbContext InputContext)
    {
        if (gsvm.id is null ||
            // KNOWN ISSUE: 因應 index 選取邏輯一定要有對應關聯資料，暫不開放藉由 Controller 清空歌曲所有的創作者/專輯關聯
            gsvm.SelectedArtistIdList is null ||
            gsvm.SelectedAlbumIdList is null ||
            string.IsNullOrWhiteSpace(gsvm.SongName))
        {
            return new ResultDTO()
            { 
                IsSuccess = false,
                StatusMessage = "Not Found"
            };
        }

        tobeUpdate.FSongName = gsvm.SongName;

        UpdateArtistsSong(gsvm.SelectedArtistIdList, tobeUpdate, InputContext);
        await UpdateSongAlbums(gsvm.SelectedAlbumIdList, tobeUpdate, InputContext);

        try
        {
            await InputContext.SaveChangesAsync();
            return new ResultDTO()
            { 
                IsSuccess = true,
            };
        }
        catch (Exception ex)
        {
            // NEXT-TODO: log error to log file
            return new ResultDTO()
            { 
                IsSuccess = false,
                StatusMessage = ex.Message
            };
        }
    }

    public async Task<ResultDTO> PostEditApi(GallerySongsDTO gsDTO,
        TSong tobeUpdate,
        FruitBarDbContext InputContext)
    {
        if (gsDTO.RelatedArtists is null ||
            gsDTO.RelatedAlbums is null ||
            string.IsNullOrWhiteSpace(gsDTO.SongName))
        {
            return new ResultDTO(){ IsSuccess = false, StatusMessage = "Not Found"};
        }

        tobeUpdate.FSongName = gsDTO.SongName;

        UpdateArtistsSong(
            gsDTO.RelatedArtists.Select(x => x.id).AsEnumerable(),
            tobeUpdate, InputContext);
        await UpdateSongAlbums(
            gsDTO.RelatedAlbums.Select(x => x.id).AsEnumerable(),
            tobeUpdate, InputContext);

        try
        {
            await InputContext.SaveChangesAsync();
            return new ResultDTO(){ IsSuccess = true };
        }
        catch (Exception ex)
        {
            // NEXT-TODO: log error to log file
            return new ResultDTO(){ IsSuccess = false, StatusMessage = ex.Message};
        }
    }

    public async Task<ResultDTO> Delete(int? songId,
        FruitBarDbContext InputContext)
    {
        if (songId is null)
        {
            return new ResultDTO() { IsSuccess = false, StatusMessage = "Not Found"};
        }
        var songToBeDeleted = await InputContext.TSongs
            .Include(x => x.TSongsAlbums)
            .Include(x => x.TArtistsSongs)
            .FirstOrDefaultAsync(x => x.FSongId == songId);
        if (songToBeDeleted is null) // 開始查詢
        {
            return new ResultDTO() { IsSuccess = false, StatusMessage = "Not Found"};
        }
        InputContext.RemoveRange(songToBeDeleted.TArtistsSongs);
        InputContext.RemoveRange(songToBeDeleted.TSongsAlbums);
        InputContext.Remove(songToBeDeleted);
        try
        {
            await InputContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // NEXT-TODO: expand returned info
            return new ResultDTO() { IsSuccess = false, StatusMessage = ex.Message};
        }
        return new ResultDTO() { IsSuccess = true };
    }

    ///////////////// section for private methods ///////////////////////

    private static IQueryable<GallerySongsDTO> GetGallerySongsRaw(FruitBarDbContext inputContext)
    {
        return inputContext.TSongs
                .OrderBy(x => x.FSongId)
                .Select(x => new GallerySongsDTO
                {
                    id = x.FSongId,
                    SongName = x.FSongName,
                    RelatedArtists = x.TArtistsSongs
                        .OrderBy(y => y.FArtist.FArtistId)
                        .Select(y => new ArtistsDTO()
                        {
                            id = y.FArtistId,
                            ArtistName = y.FArtist.FArtistName,
                            ArtistType = y.FArtist.FArtistType
                        }),
                    RelatedAlbums = x.TSongsAlbums
                        .OrderBy(y => y.FAlbum.FAlbumId)
                        .Select(y => new AlbumsDTO()
                        {
                            id = y.FAlbumId,
                            AlbumName = y.FAlbum.FAlbumName,
                            AlbumType = y.FAlbum.FAlbumType
                        })
                });
    }

    private static async Task<ResultDTO> CreateGallerySongCommon(GallerySongsDTO nsDTO,
        FruitBarDbContext inputContext)
    {
        ResultDTO resultDTO = new ResultDTO();

        var createdSong = new TSong()
        {
            FSongName = nsDTO.SongName!, // assume nsDTO.songName has value when calling this
        };
        foreach (int artistid in nsDTO.RelatedArtists.Select(x => x.id))
        {
            createdSong.TArtistsSongs.Add(new TArtistsSong()
            {
                FArtistId = artistid,
            });
        }
        // NEXT-TODO: 初步先讓專輯歌曲編號合法不重複, 後續研議改資料庫約束條件或是優化指定/檢查機制
        foreach (int albumid in nsDTO.RelatedAlbums.Select(x => x.id))
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
        try
        {
            inputContext.SaveChanges();
            nsDTO.id = createdSong.FSongId;
            resultDTO.IsSuccess = true;
            resultDTO.StatusMessage = JsonSerializer.Serialize(nsDTO);
        }
        catch (Exception ex)
        {
            resultDTO.IsSuccess = false;
            resultDTO.StatusMessage = ex.Message;
        }
        return resultDTO;
    }


    private static async Task UpdateSongAlbums(IEnumerable<int> selectedId, TSong tobeUpdate, FruitBarDbContext InputContext)
    {
        List<TSongsAlbum> albumsTobeAdd = new List<TSongsAlbum>();
        List<TSongsAlbum> albumsTobeRemoved = new List<TSongsAlbum>();

        var selectedAlbumList = await InputContext.TAlbums
            .Where(x => selectedId.Contains(x.FAlbumId))
            .Include(x => x.TSongsAlbums)
            .ToListAsync();  // 針對指定導覽屬性做 Eager Loading, 等等才查得到既有專輯內曲目編號

        foreach (var selectedAlbum in selectedAlbumList)
        {
            // 檢查是否已有重複關聯
            bool alreadyLinked = tobeUpdate.TSongsAlbums
                .Any(x => x.FAlbumId == selectedAlbum.FAlbumId);
            if (alreadyLinked)
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
        albumsTobeRemoved = tobeUpdate.TSongsAlbums.Where(x => !selectedId.Contains(x.FAlbumId)).ToList();

        InputContext.TSongsAlbums.AddRange(albumsTobeAdd);
        InputContext.TSongsAlbums.RemoveRange(albumsTobeRemoved);
    }

    private static void UpdateArtistsSong(IEnumerable<int> selectedId, TSong tobeUpdate, FruitBarDbContext InputContext)
    {
        List<TArtistsSong> artistTobeAdd = new List<TArtistsSong>();
        List<TArtistsSong> artistTobeRemoved = new List<TArtistsSong>();

        foreach (var artistId in selectedId)
        {
            // 檢查是否已有重複關聯
            bool alreadyLinked = tobeUpdate.TArtistsSongs
                .Any(x => x.FArtistId == artistId);

            // 如果沒有重複關聯, 新增關聯
            if (alreadyLinked)
            {
                continue;
            }
            // 先存在 List 裡面，全部確認後直接對 DbSet 操作
            artistTobeAdd.Add(new TArtistsSong()
            {
                FArtistId = artistId,
                FSong = tobeUpdate // 導覽屬性, 導覽回更新歌曲物件本體
            });
        }
        // FIXED: 不可在 foreach 枚舉 TArtistsSongs 時修改同一集合，否則會拋出 Collection was modified；
        // 且關聯 FK 不可為 null，應由待刪除差集明確將中介實體標記為 Deleted，而非只切斷導覽關聯。
        artistTobeRemoved = tobeUpdate.TArtistsSongs
            .Where(x => !selectedId.Contains(x.FArtistId))
            .ToList();

        InputContext.TArtistsSongs.AddRange(artistTobeAdd);
        InputContext.TArtistsSongs.RemoveRange(artistTobeRemoved);
    }

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
