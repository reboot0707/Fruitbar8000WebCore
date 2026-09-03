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
    public async Task<NewSongViewModel> GetCreate(FruitBarDbContext inputContext)
    {
        var qArtist = inputContext.TArtists.OrderBy(x => x.FArtistName);
        var qAlbum = inputContext.TAlbums.OrderBy(x => x.FAlbumName);

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
        return nsvm;
    }

    public async Task PostCreate(NewSongViewModel nsvmSent, FruitBarDbContext inputContext)
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
                var selectedAlbum = await inputContext.TAlbums
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
            inputContext.TSongs.Add(createdSong);
            inputContext.SaveChanges();
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
}
