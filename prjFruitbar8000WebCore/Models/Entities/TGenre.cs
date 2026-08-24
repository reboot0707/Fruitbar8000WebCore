using System;
using System.Collections.Generic;

namespace prjFruitbar8000WebCore.Models.Entities;

public partial class TGenre
{
    public int FGenreId { get; set; }

    public string FGenreName { get; set; } = null!;

    public bool FIsDeleted { get; set; }

    public virtual ICollection<TSongGenre> TSongGenres { get; set; } = new List<TSongGenre>();
}
