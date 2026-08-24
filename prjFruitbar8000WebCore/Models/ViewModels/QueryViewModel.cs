using System.ComponentModel;

namespace prjFruitbar8000WebCore.Models.ViewModels;

public class QueryViewModel
{
    public int id { get; set; }
    [DisplayName("樂曲名稱")]
    public string? SongName { get; set; }
    [DisplayName("樂曲創作者")]
    public string? ArtistName { get; set; }
    [DisplayName("收錄專輯")]
    public string? AlbumName { get; set; }
}
