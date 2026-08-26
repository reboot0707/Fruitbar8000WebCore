using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace prjFruitbar8000WebCore.Models.ViewModels;

public class NewSongViewModel
{
    [DisplayName("樂曲名稱")]
    public string? SongName { get; set; }

    [Required]
    [DisplayName("樂曲創作者")]
    public int? ArtistId { get; set; }

    [Required]
    [DisplayName("收錄專輯")]
    public int? AlbumId { get; set; }

    // NEXT-TODO: 使用新式 C# 語法糖初始化 ( ` = [ ]; ` )
    public List<SelectListItem>? ArtistList { get; set; }

    // NEXT-TODO: 使用新式 C# 語法糖初始化 ( ` = [ ]; ` )
    public List<SelectListItem>? AlbumList { get; set; }
}
