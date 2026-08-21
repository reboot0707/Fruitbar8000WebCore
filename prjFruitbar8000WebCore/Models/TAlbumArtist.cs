using System;
using System.Collections.Generic;

namespace prjFruitbar8000WebCore.Models;

public partial class TAlbumArtist
{
    public int FId { get; set; }

    public int FAlbumId { get; set; }

    public int FArtistId { get; set; }

    public string? FCreditRoles { get; set; }

    public virtual TAlbum FAlbum { get; set; } = null!;

    public virtual TArtist FArtist { get; set; } = null!;
}
