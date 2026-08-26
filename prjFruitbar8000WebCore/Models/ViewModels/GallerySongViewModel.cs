using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace prjFruitbar8000WebCore.Models.ViewModels;

public class GallerySongViewModel
{
    [ScaffoldColumn(false)]
    public int? id { get; set; }

    [Required]
    [DisplayName("樂曲名稱")]
    public string? SongName { get; set; }

    [Required]
    [DisplayName("樂曲創作者 (可用Ctrl/Shift進行多選)")]
    public List<int>? SelectedArtistIdList { get; set; }

    [Required]
    [DisplayName("收錄專輯 (可用Ctrl/Shift進行多選)")]
    public List<int>? SelectedAlbumIdList { get; set; }

    public List<SelectListItem>? OptionArtistIdList { get; set; } // 列表選項
    public List<SelectListItem>? OptionAlbumIdList { get; set; } // 列表選項
}
