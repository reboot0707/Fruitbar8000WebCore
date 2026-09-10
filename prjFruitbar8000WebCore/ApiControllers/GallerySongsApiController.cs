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
        /// 取得歌曲清單及其創作者、專輯關聯編號。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/gallery/songs；無查詢參數或分頁。回傳欄位為 id、songName、artistIds（創作者編號陣列）、albumIds（專輯編號陣列）。
        ///
        /// 現行 ListApi 依專輯關聯展開：沒有專輯的歌曲不會出現，有多張專輯的歌曲會重複出現，每筆仍包含該歌曲的完整關聯編號陣列。
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
        /// 依編號取得單一歌曲及其關聯編號。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/gallery/songs/{id}；回傳欄位為 id、songName、artistIds（創作者編號陣列）、albumIds（專輯編號陣列）。
        ///
        /// 直接依歌曲編號查詢，沒有專輯關聯的歌曲也可取得。
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
        /// artistIds、albumIds 為整數陣列，省略時預設為空陣列。創作者編號須存在；不存在的專輯編號會被略過。新增專輯關聯時自動分配從 1 起最小可用曲目編號。
        ///
        /// 成功回傳以 JSON 序列化的 DTO 字串，包含新 id 與原請求的欄位；albumIds 可能仍包含已被略過的編號，並非重新查詢的結果。
        ///
        /// </remarks>
        /// <param name="newGSong">歌曲名稱與欲建立的關聯編號；關聯編號應避免重複。</param>
        /// <returns>包含新增歌曲資訊的 JSON 字串。</returns>
        /// <response code="200">新增成功，回傳包含新 id 與原請求欄位的 GallerySongsDTO JSON 字串。</response>
        /// <response code="400">JSON 本文或模型驗證失敗。</response>
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
        /// artistIds、albumIds 不可為 null；省略時預設為空陣列，空陣列會清空對應關聯。保留仍被選取的關聯、移除未選取的關聯並新增缺少的關聯，並非僅附加。
        ///
        /// 創作者編號須存在且應避免重複；不存在的專輯編號會被略過。新專輯關聯自動分配從 1 起最小可用曲目編號，既有關聯保留曲目編號。
        ///
        /// 成功回傳原請求 DTO，未重新查詢；本文 id 若省略仍回傳 0，albumIds 也可能包含被略過的編號。
        /// </remarks>
        /// <param name="id">路徑中的歌曲整數編號，作為實際更新目標。</param>
        /// <param name="newInfoSong">完整更新內容：id、songName、artistIds、albumIds。</param>
        /// <returns>請求中的 GallerySongsDTO。</returns>
        /// <response code="200">更新成功，回傳 GallerySongsDTO。內容為原請求 DTO；省略的 id 仍為 0，albumIds 可能包含被略過的編號。</response>
        /// <response code="400">路徑 id、JSON 本文或模型驗證失敗。</response>
        /// <response code="404">本文 id 不符、查無歌曲，或服務檢查發現名稱無效或關聯集合為 null。</response>
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
