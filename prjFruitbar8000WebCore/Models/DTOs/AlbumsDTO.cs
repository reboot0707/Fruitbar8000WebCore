
namespace prjFruitbar8000WebCore.Models.DTOs;

public class AlbumsDTO
{
    public int id { get; set; }

    public string AlbumName { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }
    public string? AlbumType { get; set; }
}
