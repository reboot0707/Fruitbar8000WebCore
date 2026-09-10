using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Services;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("apis/v2/gallery/songs")]
    [ApiController]
    public class GallerySongsApiController : ControllerBase
    {
        private readonly FruitBarDbContext _context;
        public GallerySongsApiController(FruitBarDbContext context)
        {
            _context = context;
            // NEXT-TODO: 將 dataAccess 綁定 ViewModel 的方法拆成泛型, 再直接用 Service 裡面的方法

        }
        
        /// <summary>
        /// 取得歌曲清單及其關聯創作者、專輯資料。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/gallery/songs；無查詢參數或分頁。回傳 GallerySongsDTO 陣列，欄位為 id、songName、relatedArtists、relatedAlbums。
        ///
        /// relatedArtists 為物件陣列，每筆包含 id、artistName、artistType；relatedAlbums 為物件陣列，每筆包含 id、albumName、releaseDate、albumType。現行查詢未填入 releaseDate，該欄位回傳 null。
        ///
        /// 歌曲依 id 遞增排序，每首歌曲一筆；沒有創作者或專輯關聯的歌曲仍會出現，對應集合為空陣列。兩個關聯集合各自依創作者、專輯 id 遞增排序。
        /// </remarks>
        /// <returns>GallerySongsDTO 陣列。</returns>
        /// <response code="200">查詢成功，回傳 GallerySongsDTO[]。查無資料時為空陣列。</response>
        [HttpGet]
        [ProducesResponseType(typeof(GallerySongsDTO[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var queryList = await new GalleryDataAccess().ListApi(_context);

            return Ok(queryList);
        }

        /// <summary>
        /// 依編號取得單一歌曲及其關聯創作者、專輯資料。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/gallery/songs/{id}；回傳欄位為 id、songName、relatedArtists、relatedAlbums。
        ///
        /// relatedArtists 每筆包含 id、artistName、artistType；relatedAlbums 每筆包含 id、albumName、releaseDate、albumType。現行查詢未填入 releaseDate，該欄位回傳 null。
        ///
        /// 沒有創作者或專輯關聯的歌曲也可取得，對應集合為空陣列。兩個關聯集合各自依創作者、專輯 id 遞增排序。
        /// </remarks>
        /// <param name="id">路徑中的歌曲整數編號。</param>
        /// <returns>GallerySongsDTO 或找不到資料的訊息字串。</returns>
        /// <response code="200">查詢成功，回傳 GallerySongsDTO。</response>
        /// <response code="400">路徑 id 無法繫結為整數。</response>
        /// <response code="404">查無歌曲，回傳內容為 { "message": "Not Found" } 的字串。</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GallerySongsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List(int id)
        {        
            GallerySongsDTO? qResult = await new GalleryDataAccess().ListApiById(id, _context);
            
            if(qResult is null)
            {
                return NotFound(MsgDicionary.Message404);
            }

            return Ok(qResult);
        }

        /// <summary>
        /// 新增歌曲並建立創作者及專輯關聯。
        /// </summary>
        /// <remarks>
        /// POST /apis/v2/gallery/songs；Content-Type: application/json。
        ///
        /// songName 不可為 null、空字串或全空白；資料庫長度上限為 200 字元，DTO 未設定長度驗證。本文 id 不用指定，由資料庫產生。
        ///
        /// relatedArtists、relatedAlbums 為物件陣列，不可為 null，省略時預設為空陣列；空陣列表示不建立該類關聯。
        ///
        /// relatedArtists 每筆包含 id、artistName、artistType；relatedAlbums 每筆包含 id、albumName、releaseDate、albumType。artistName、albumName 為非 Nullable 欄位，依目前模型驗證設定不可省略或傳入 null；artistType、albumType、releaseDate 可省略或為 null。
        ///
        /// 寫入關聯時只使用各物件的 id，不會新增或修改創作者、專輯的名稱、類型或發行日期。創作者編號須存在；不存在的專輯編號會被略過。各集合內的 id 應避免重複。新增專輯關聯時自動分配從 1 起最小可用曲目編號。
        ///
        /// 成功回傳以 JSON 序列化的 DTO 字串，包含新 id 與原請求資料，未重新查詢關聯內容；RelatedAlbums 可能仍包含已被略過的專輯。字串內使用 DTO 原始屬性名稱：id、SongName、RelatedArtists、RelatedAlbums，內層亦保留 ArtistName、AlbumName 等原始大小寫。
        /// </remarks>
        /// <param name="newGSong">歌曲名稱與 relatedArtists、relatedAlbums 關聯物件陣列；每筆以 id 指定關聯對象，並提供模型驗證所需的名稱欄位。</param>
        /// <returns>包含新增歌曲資訊的 JSON 字串。</returns>
        /// <response code="200">新增成功，回傳包含新 id 與原請求欄位的 GallerySongsDTO JSON 字串。</response>
        /// <response code="400">JSON 本文或模型驗證失敗，例如關聯集合為 null，或關聯物件缺少必要的名稱欄位。</response>
        /// <response code="404">進入方法後本文為 null 或歌曲名稱為 null、空字串、全空白。</response>
        /// <response code="500">服務回報儲存失敗時回傳 ProblemDetails。</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] GallerySongsDTO newGSong)
        {
            if(newGSong is null
            || newGSong.SongName is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            ResultDTO result = await new GalleryDataAccess().PostCreateApi(newGSong, _context);

            if(result.IsSuccess)
            {
                return Ok(result.StatusMessage); // return new song info json
            }
            if(result.StatusMessage == "NotFound")
            {
                return NotFound(MsgDicionary.Message404);
            }
            return Problem(result.StatusMessage);
        }

        /// <summary>
        /// 更新歌曲名稱並取代創作者及專輯關聯集合。
        /// </summary>
        /// <remarks>
        /// PUT /apis/v2/gallery/songs/{id}；Content-Type: application/json。
        ///
        /// 本文 id 可省略或為 0；非 0 時必須與路徑 id 一致。songName 不可為 null、空字串或全空白，資料庫長度上限為 200 字元，DTO 未設定長度驗證。
        ///
        /// relatedArtists、relatedAlbums 為物件陣列，不可為 null；省略時預設為空陣列，空陣列會清空對應關聯。保留仍被選取的關聯、移除未選取的關聯並新增缺少的關聯，並非僅附加。
        ///
        /// relatedArtists 每筆包含 id、artistName、artistType；relatedAlbums 每筆包含 id、albumName、releaseDate、albumType。artistName、albumName 為非 Nullable 欄位，依目前模型驗證設定不可省略或傳入 null；artistType、albumType、releaseDate 可省略或為 null。
        ///
        /// 更新關聯時只使用各物件的 id，不會修改創作者、專輯的名稱、類型或發行日期。
        ///
        /// 創作者編號須存在且應避免重複；不存在的專輯編號會被略過。新專輯關聯自動分配從 1 起最小可用曲目編號，既有關聯保留曲目編號。
        ///
        /// 成功回傳原請求 DTO，欄位為 id、songName、relatedArtists、relatedAlbums，未重新查詢；本文 id 若省略仍回傳 0，relatedAlbums 也可能包含被略過的專輯。關聯物件的名稱、類型與發行日期為請求值，不代表資料庫目前內容。
        /// </remarks>
        /// <param name="id">路徑中的歌曲整數編號，作為實際更新目標。</param>
        /// <param name="newInfoSong">完整更新內容：id、songName、relatedArtists、relatedAlbums；關聯集合須提供欲保留的全部關聯物件，並包含模型驗證所需的名稱欄位。</param>
        /// <returns>請求中的 GallerySongsDTO。</returns>
        /// <response code="200">更新成功，回傳原請求 GallerySongsDTO；省略的 id 仍為 0，relatedAlbums 可能包含被略過的專輯。</response>
        /// <response code="400">路徑 id、JSON 本文或模型驗證失敗，例如關聯集合為 null，或關聯物件缺少必要的名稱欄位。</response>
        /// <response code="404">本文 id 不符、查無歌曲，或進入服務後檢查發現名稱無效或關聯集合為 null；回傳內容為 { "message": "Not Found" } 的字串。</response>
        /// <response code="500">服務回報儲存失敗時回傳 ProblemDetails。</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GallerySongsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] GallerySongsDTO newInfoSong)
        {
            // default value of int is zero, 暗示 payload 部分 id 可以不填, 但不能填錯.
            if(id != newInfoSong.id && newInfoSong.id != 0)
            {
                return NotFound(MsgDicionary.Message404);
            }
            // Note: (Why use `.Include()`) 
            // 此查詢必須一併載入 TArtistsSongs 與 TSongsAlbums。導覽集合雖已初始化但未代表資料庫內容，
            // 因此 PostEdit() 的 existRelationInList 會把既有關聯誤判為不存在，最後 INSERT 時撞上複合唯一索引。
            var songToBeUpdated = _context.TSongs
                .Include(x => x.TArtistsSongs)
                .Include(x => x.TSongsAlbums)
                .FirstOrDefault(x => x.FSongId == id);
            if (songToBeUpdated is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            ResultDTO result = await new GalleryDataAccess()
                .PostEditApi(newInfoSong, songToBeUpdated, _context);
            if(result.IsSuccess)
            {
                return Ok(newInfoSong);
            }
            if(result.StatusMessage == "Not Found")
            {
                return NotFound(MsgDicionary.Message404);
            }
            return Problem(result.StatusMessage);
        }

        /// <summary>
        /// 刪除指定歌曲及其創作者、專輯關聯。
        /// </summary>
        /// <remarks>
        /// DELETE /apis/v2/gallery/songs/{id}；不需要請求本文。
        ///
        /// 先移除歌曲的創作者與專輯中介資料，再刪除歌曲；創作者與專輯本身保留。
        ///
        /// 現行實作將服務回報的所有失敗統一轉為 Problem；查無歌曲時也回傳 500，detail 為 Not Found。
        /// </remarks>
        /// <param name="id">路徑中的歌曲整數編號。</param>
        /// <returns>成功時回傳內容為 { "message": "Deleted" } 的字串。</returns>
        /// <response code="200">刪除成功，回傳內容為 { "message": "Deleted" } 的字串。</response>
        /// <response code="400">路徑 id 無法繫結為整數。</response>
        /// <response code="500">查無歌曲或服務回報儲存失敗，回傳 ProblemDetails。</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await new GalleryDataAccess().Delete(id, _context);
            if(result.IsSuccess)
            {
                return Ok(MsgDicionary.MessageDeleted);
            }
            else
            {
                // NEXT-TODO: expand returned info
                return Problem(result.StatusMessage);
            }
            
        }
    }
}
