namespace prjFruitbar8000WebCore.Models.DTOs;

public class GallerySongsDTO
{
    public int id { get; set; }

    public string? songName { get; set; }

    public IEnumerable<int> artistIds { get; set; } = [];
    public IEnumerable<int> albumIds { get; set; } = [];
}