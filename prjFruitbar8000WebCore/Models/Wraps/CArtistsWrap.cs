using System.ComponentModel.DataAnnotations;
using prjFruitbar8000WebCore.Models.Entities;

namespace prjFruitbar8000WebCore.Models.Wraps;

public class CArtistsWrap
{
    private TArtist _tartist;
    public TArtist tartist
    {
        get { return _tartist; }
        set { _tartist = value; }
    }

    public CArtistsWrap()
    {
        _tartist = new TArtist();
    }
    public CArtistsWrap(TArtist tartist)
    {
        _tartist = tartist;
    }

    [Key]
    public int FArtistId
    {
        get { return _tartist.FArtistId; }
        set { _tartist.FArtistId = value; }
    }

    public string FArtistName
    {
        get { return _tartist.FArtistName; }
        set { _tartist.FArtistName = value; }
    }

    public bool FIsDeleted
    {
        get { return _tartist.FIsDeleted; }
        set { _tartist.FIsDeleted = value; }
    }

    public string? FArtistType
    {
        get { return _tartist.FArtistType; }
        set { _tartist.FArtistType = value; }
    }
}
