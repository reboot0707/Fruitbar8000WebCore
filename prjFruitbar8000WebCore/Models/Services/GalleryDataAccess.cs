using Microsoft.EntityFrameworkCore;
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

    public void Delete(int? songId, FruitBarDbContext InputContext)
    {
        if(songId is null)
        { 
            return;
        }
        var songToBeDeleted = InputContext.TSongs
            .Include(x => x.TSongsAlbums)
            .Include(x => x.TArtistsSongs)
            .FirstOrDefault(x => x.FSongId == songId);
        if (songToBeDeleted is null) // 開始查詢
        {
            return;
        }
        InputContext.RemoveRange(songToBeDeleted.TArtistsSongs);
        InputContext.RemoveRange(songToBeDeleted.TSongsAlbums);
        InputContext.Remove(songToBeDeleted);
        InputContext.SaveChanges();
        return;
    }
}
