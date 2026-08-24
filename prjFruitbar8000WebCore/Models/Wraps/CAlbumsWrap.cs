using System.ComponentModel.DataAnnotations;
using prjFruitbar8000WebCore.Models.Entities;

namespace prjFruitbar8000WebCore;

public class CAlbumsWrap
{
    private TAlbum _talbum;
    public TAlbum talbum
    {
        get { return _talbum; }
        set { _talbum = value; }
    }

    public CAlbumsWrap()
    {
        _talbum = new TAlbum();
    }

    public CAlbumsWrap(TAlbum talbum)
    {
        _talbum = talbum;
    }

    [Key]
    public int FAlbumId
    {
        get { return _talbum.FAlbumId; }
        set { _talbum.FAlbumId = value; }
    }

    // TODO: 依照 FAlbumId 完成剩下的 TAlbum <--> CAlbumsWrap 對應屬性, 導覽屬性應該先不用
}
