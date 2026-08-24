using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using prjFruitbar8000WebCore.Models.Entities;

namespace prjFruitbar8000WebCore.Models.Wraps;

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

    [DisplayName("專輯名稱")]
    public string FAlbumName
    {
        get { return _talbum.FAlbumName; }
        set { _talbum.FAlbumName = value; }
    }

    public bool FIsDeleted
    {
        get { return _talbum.FIsDeleted; }
        set { _talbum.FIsDeleted = value; }
    }

    [DisplayName("發行日期")]
    public DateOnly? FReleaseDate
    {
        get { return _talbum.FReleaseDate; }
        set { _talbum.FReleaseDate = value; }
    }

    public byte[]? FCoverPic
    {
        get { return _talbum.FCoverPic; }
        set { _talbum.FCoverPic = value; }
    }

    [DisplayName("專輯類型")]
    public string? FAlbumType
    {
        get { return _talbum.FAlbumType; }
        set { _talbum.FAlbumType = value; }
    }
}
