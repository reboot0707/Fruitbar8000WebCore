using Microsoft.EntityFrameworkCore;

namespace prjFruitbar8000WebCore.Models.Services;

public class CheckNavigate
{
    private readonly FruitBarDbContext _inputContext;

    public CheckNavigate(FruitBarDbContext inputContext)
    {
        _inputContext = inputContext;
    } // constructor

    public async Task<bool> IsArtistHaveSong(int id)
    {
        var haveSong = await _inputContext.TArtistsSongs
            .AsNoTracking()
            .AnyAsync(x => x.FArtistId == id);
        if(!haveSong) return false;
        return true;
    }

        public async Task<bool> IsAlbumHaveSong(int id)
    {
        var haveSong = await _inputContext.TSongsAlbums
            .AsNoTracking()
            .AnyAsync(x => x.FAlbumId == id);
        if(!haveSong) return false;
        return true;
    }
}