namespace prjFruitbar8000WebCore.Models;

public partial class TAlbum
{
    public int FAlbumId { get; set; }

    public string FAlbumName { get; set; } = null!;

    public bool FIsDeleted { get; set; }

    public DateOnly? FReleaseDate { get; set; }

    public byte[]? FCoverPic { get; set; }

    public string? FAlbumType { get; set; }

    public virtual ICollection<TAlbumArtist> TAlbumArtists { get; set; } = new List<TAlbumArtist>();

    public virtual ICollection<TSongsAlbum> TSongsAlbums { get; set; } = new List<TSongsAlbum>();
}
