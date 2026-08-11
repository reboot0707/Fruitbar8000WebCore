namespace prjFruitbar8000WebCore.Models;

public partial class TSongsAlbum
{
    public int FId { get; set; }

    public int FAlbumId { get; set; }

    public int FSongId { get; set; }

    public int FTrackNumber { get; set; }

    public virtual TAlbum FAlbum { get; set; } = null!;

    public virtual TSong FSong { get; set; } = null!;
}
