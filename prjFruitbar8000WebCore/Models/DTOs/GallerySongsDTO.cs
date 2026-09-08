namespace prjFruitbar8000WebCore.Models.DTOs;

public class GallerySongsDTO
{
    public int id { get; set; }

    public string? songName { get; set; }

    public IEnumerable<string> artistNames { get; set; } = [];
    public IEnumerable<string> albumNames { get; set; } = [];
}
