
namespace prjFruitbar8000WebCore.Models.DTOs;

public class AlbumsDTO
{
    public int id { get; set; }

    public string albumName { get; set; } = null!;

    public DateOnly? releaseDate { get; set; }
    public string? albumType { get; set; }
}
