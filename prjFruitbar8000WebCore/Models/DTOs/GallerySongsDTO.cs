namespace prjFruitbar8000WebCore.Models.DTOs;

/// <summary>
/// GET 歌曲查詢結果，包含創作者、專輯關聯物件。
/// </summary>
public class GallerySongsDTO
{
    /// <summary>
    /// 資料庫中的歌曲編號。
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// 資料庫中的歌曲名稱。
    /// </summary>
    public string? SongName { get; set; }

    /// <summary>
    /// 關聯創作者物件陣列，包含 id、artistName、artistType，依創作者編號遞增排序；沒有關聯時為空陣列。
    /// </summary>
    public IEnumerable<ArtistsDTO> RelatedArtists { get; set; } = [];

    /// <summary>
    /// 關聯專輯物件陣列，包含 id、albumName、releaseDate、albumType，依專輯編號遞增排序；沒有關聯時為空陣列。現行歌曲查詢未填入 releaseDate，回傳 null。
    /// </summary>
    public IEnumerable<AlbumsDTO> RelatedAlbums { get; set; } = [];
}

/// <summary>
/// POST 新增與 PUT 更新歌曲資料，以既有創作者與專輯的整數編號指定關聯。
/// </summary>
public class GallerySongsWriteDTO
{
    /// <summary>
    /// POST 時可省略，傳入值不作為新歌曲編號；成功回應的 JSON 字串會填入資料庫產生的編號。PUT 時可省略或為 0，非 0 時須與路徑編號一致；成功回應保留請求值，省略時為 0。
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// 歌曲名稱，不可為 null、空字串或全空白；資料庫上限為 200 字元，DTO 未設定長度驗證。
    /// </summary>
    public string? SongName { get; set; }

    /// <summary>
    /// 既有創作者編號陣列，編號須存在且應避免重複。不可為 null；POST 省略或空陣列表示不建立創作者關聯；PUT 須提供欲保留的完整集合，省略或空陣列會清空創作者關聯。
    /// </summary>
    public IEnumerable<int> RelatedArtistIds { get; set; } = [];

    /// <summary>
    /// 既有專輯編號陣列，應避免重複，不存在的編號會被略過。不可為 null；POST 省略或空陣列表示不建立專輯關聯；PUT 須提供欲保留的完整集合，省略或空陣列會清空專輯關聯。成功回應仍保留原請求中的編號。
    /// </summary>
    public IEnumerable<int> RelatedAlbumIds { get; set; } = [];
}
