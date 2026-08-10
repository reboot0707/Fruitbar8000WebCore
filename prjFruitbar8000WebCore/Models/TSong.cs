using System;
using System.Collections.Generic;

namespace prjFruitbar8000WebCore.Models;

public partial class TSong
{
    public int FSongId { get; set; }

    public string FSongName { get; set; } = null!;

    public bool FIsDeleted { get; set; }

    public string? FLyrics { get; set; }

    /// <summary>
    /// Unit: second
    /// </summary>
    public int? FDuration { get; set; }

    public virtual ICollection<TArtistsSong> TArtistsSongs { get; set; } = new List<TArtistsSong>();

    public virtual ICollection<TSongGenre> TSongGenres { get; set; } = new List<TSongGenre>();

    public virtual ICollection<TSongsAlbum> TSongsAlbums { get; set; } = new List<TSongsAlbum>();
}
