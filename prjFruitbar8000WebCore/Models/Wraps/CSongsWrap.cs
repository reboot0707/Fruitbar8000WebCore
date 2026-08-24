using System.ComponentModel.DataAnnotations;
using prjFruitbar8000WebCore.Models.Entities;

namespace prjFruitbar8000WebCore.Models.Wraps;

public class CSongWrap
{
    private TSong _tsong;

    public TSong tsong
    {
        get { return _tsong; }
        set { _tsong = value; }
    }

    public CSongWrap()
    {
        _tsong = new TSong();
    }

    public CSongWrap(TSong tsong)
    {
        _tsong = tsong;
    }

    [Key]
    public int FSongId
    {
        get { return _tsong.FSongId; }
        set { _tsong.FSongId = value; }
    }

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

    public string? FLyrics
    {
        get { return _tsong.FLyrics; }
        set { _tsong.FLyrics = value; }
    }

    public int? FDuration
    {
        get { return _tsong.FDuration; }
        set { _tsong.FDuration = value; }
    }
}
