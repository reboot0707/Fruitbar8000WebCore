namespace prjFruitbar8000WebCore.Models;

public partial class TArtist
{
    public int FArtistId { get; set; }

    public string FArtistName { get; set; } = null!;

    public bool FIsDeleted { get; set; }

    public string? FArtistType { get; set; }

    public virtual ICollection<TAlbumArtist> TAlbumArtists { get; set; } = new List<TAlbumArtist>();

    public virtual ICollection<TArtistsSong> TArtistsSongs { get; set; } = new List<TArtistsSong>();
}
