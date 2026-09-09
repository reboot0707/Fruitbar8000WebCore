namespace prjFruitbar8000WebCore.Models.DTOs;

public class GallerySongsDTO
{
    public int id { get; set; }

    public string? SongName { get; set; }

    public IEnumerable<int> ArtistIds { get; set; } = [];
    public IEnumerable<int> AlbumIds { get; set; } = [];
}