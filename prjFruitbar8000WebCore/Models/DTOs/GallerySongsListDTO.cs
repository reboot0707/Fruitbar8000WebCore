namespace prjFruitbar8000WebCore.Models.DTOs;

public class GallerySongsListDTO
{
    public int id { get; set; }

    public string? SongName { get; set; }

    public IEnumerable<string> ArtistNames { get; set; } = [];
    public IEnumerable<string> AlbumNames { get; set; } = [];
}