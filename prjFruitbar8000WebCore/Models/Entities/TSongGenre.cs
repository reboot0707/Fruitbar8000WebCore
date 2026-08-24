using System;
using System.Collections.Generic;

namespace prjFruitbar8000WebCore.Models.Entities;

public partial class TSongGenre
{
    public int FId { get; set; }

    public int FSongId { get; set; }

    public int FGenreId { get; set; }

    public virtual TGenre FGenre { get; set; } = null!;

    public virtual TSong FSong { get; set; } = null!;
}
