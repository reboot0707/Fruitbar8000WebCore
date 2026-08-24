using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using prjFruitbar8000WebCore.Models.Entities;

namespace prjFruitbar8000WebCore.Models.Wraps;

public class CSongsWrap
{
    private TSong _tsong;

    public TSong tsong
    {
        get { return _tsong; }
        set { _tsong = value; }
    }

    public CSongsWrap()
    {
        _tsong = new TSong();
    }

    public CSongsWrap(TSong tsong)
    {
        _tsong = tsong;
    }

    [Key]
    public int FSongId
    {
        get { return _tsong.FSongId; }
        set { _tsong.FSongId = value; }
    }

    [DisplayName("歌曲名稱")]
    public string FSongName
    {
        get { return _tsong.FSongName; }
        set { _tsong.FSongName = value; }
    }

    public bool FIsDeleted
    {
        get { return _tsong.FIsDeleted; }
        set { _tsong.FIsDeleted = value; }
    }

    [DisplayName("歌詞")]
    public string? FLyrics
    {
        get { return _tsong.FLyrics; }
        set { _tsong.FLyrics = value; }
    }

    [DisplayName("歌曲長度(秒)")]
    public int? FDuration
    {
        get { return _tsong.FDuration; }
        set { _tsong.FDuration = value; }
    }
}
