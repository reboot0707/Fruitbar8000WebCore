namespace prjFruitbar8000WebCore.Models.DTOs;

public class GallerySongsDTO
{
    public int id { get; set; }

    public string? SongName { get; set; }

    public IEnumerable<ArtistsDTO> RelatedArtists { get; set; } = [];
    public IEnumerable<AlbumsDTO> RelatedAlbums { get; set; } = [];
}