using System;
using System.Collections.Generic;

namespace prjFruitbar8000WebCore.Models;

public partial class TArtistsSong
{
    public int FId { get; set; }

    public int FSongId { get; set; }

    public int FArtistId { get; set; }

    public string? FCreditRoles { get; set; }

    public virtual TArtist FArtist { get; set; } = null!;

    public virtual TSong FSong { get; set; } = null!;
}
