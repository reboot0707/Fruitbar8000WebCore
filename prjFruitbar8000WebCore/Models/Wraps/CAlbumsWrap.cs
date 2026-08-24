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

    public string? FAlbumType
    {
        get { return _talbum.FAlbumType; }
        set { _talbum.FAlbumType = value; }
    }
}
