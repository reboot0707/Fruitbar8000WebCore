using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("apis/v2/albums")]
    [ApiController]
    public class AlbumsApiController : ControllerBase
    {

        private readonly FruitBarDbContext _context;
        public AlbumsApiController(FruitBarDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有專輯。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/albums；無查詢參數、篩選或分頁，依 id 遞增排序。
        ///
        /// 每筆資料包含 id、albumName、albumType、releaseDate；查無資料時回傳空陣列。
        /// </remarks>
        /// <returns>AlbumsDTO 陣列。</returns>
        /// <response code="200">查詢成功，回傳 AlbumsDTO[]。查無資料時為空陣列。</response>
        /// <response code="204">僅在查詢結果為 null 的防禦分支回傳；正常 ToListAsync 不會進入此分支。</response>
        /// <response code="500">捕捉到查詢例外時回傳 ProblemDetails。</response>
        [HttpGet]
        [ProducesResponseType(typeof(AlbumsDTO[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List()
        {
            List<AlbumsDTO>? albums = new List<AlbumsDTO>();
            try
            {
                albums = await _context.TAlbums
                    .OrderBy(x => x.FAlbumId)
                    .Select(x => new AlbumsDTO()
                    {
                        id = x.FAlbumId,
                        AlbumName = x.FAlbumName,
                        AlbumType = x.FAlbumType,
                        ReleaseDate = x.FReleaseDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // TODO-NEXT: error handling to log error
                return Problem(ex.Message);
            }
            if (albums is null)
            {
                return NoContent();
            }
            return Ok(albums);
        }

        /// <summary>
        /// 依編號取得單一專輯。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/albums/{id}；回傳欄位為 id、albumName、albumType、releaseDate。
        /// </remarks>
        /// <param name="id">路徑中的專輯整數編號。</param>
        /// <returns>找到的 AlbumsDTO，或找不到資料的訊息字串。</returns>
        /// <response code="200">查詢成功，回傳 AlbumsDTO。</response>
        /// <response code="400">路徑 id 無法繫結為整數。</response>
        /// <response code="404">查無指定編號，回傳內容為 { "message": "Not Found" } 的字串。</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlbumsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List(int id)
        {
            AlbumsDTO? album = await _context.TAlbums
            .OrderBy(x => x.FAlbumId)
            .Where(x => x.FAlbumId == id)
            .Select(x => new AlbumsDTO()
            {
                id = x.FAlbumId,
                AlbumName = x.FAlbumName,
                AlbumType = x.FAlbumType,
                ReleaseDate = x.FReleaseDate
            }).FirstOrDefaultAsync();
            if (album is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            return Ok(album);
        }

        /// <summary>
        /// 新增專輯。
        /// </summary>
        /// <remarks>
        /// POST /apis/v2/albums；Content-Type: application/json。
        ///
        /// JSON 本文：albumName 為必填名稱，albumType 可為 null。資料庫長度上限分別為 200 與 50 字元，DTO 未設定長度驗證。 releaseDate 可為 null，日期格式為 yyyy-MM-dd。
        ///
        /// 本文 id 不用指定，新增後使用資料庫產生的編號。
        ///
        /// 成功使用 HTTP 200，回傳內容為 { "newAlbumId": "編號" } 的字串；編號值為字串。
        /// </remarks>
        /// <param name="albumsDTO">要新增的 AlbumsDTO。</param>
        /// <returns>包含新增編號的字串。</returns>
        /// <response code="200">新增成功，回傳內容為 { "newAlbumId": "編號" } 的字串，編號值為字串。</response>
        /// <response code="400">JSON 本文或模型驗證失敗。</response>
        /// <response code="500">儲存失敗，回傳 ProblemDetails。</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AlbumsDTO albumsDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var tobeAdd = new TAlbum()
            {
                FAlbumName = albumsDTO.AlbumName,
                FAlbumType = albumsDTO.AlbumType,
                FReleaseDate = albumsDTO.ReleaseDate
            };
            _context.TAlbums.Add(tobeAdd);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            albumsDTO.id = tobeAdd.FAlbumId;
            //Use Ok instead of Created
            return Ok($"{{ \"newAlbumId\": \"{albumsDTO.id}\" }}");
        }

        /// <summary>
        /// 更新指定專輯的全部可編輯欄位。
        /// </summary>
        /// <remarks>
        /// PUT /apis/v2/albums/{id}；Content-Type: application/json。
        ///
        /// JSON 本文：albumName 為必填名稱，albumType 可為 null。資料庫長度上限分別為 200 與 50 字元，DTO 未設定長度驗證。 releaseDate 可為 null，日期格式為 yyyy-MM-dd。
        ///
        /// 以路徑 id 為準，本文 id 不參與比對；成功回傳時會填入實際編號。albumName、albumType、releaseDate 皆會覆寫，未提供的選填欄位會設為 null。
        /// </remarks>
        /// <param name="id">路徑中的專輯整數編號。</param>
        /// <param name="albumsDTO">更新內容，格式為 AlbumsDTO。</param>
        /// <returns>更新後的 DTO。</returns>
        /// <response code="200">更新成功，回傳 AlbumsDTO。id 已填入實際更新的編號。</response>
        /// <response code="400">路徑 id、JSON 本文或模型驗證失敗。</response>
        /// <response code="404">id 為 null 或查無資料，回傳 Not Found 訊息字串。</response>
        /// <response code="500">儲存失敗，回傳 ProblemDetails。</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AlbumsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int? id, [FromBody] AlbumsDTO albumsDTO)
        {
            if (id is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            TAlbum? album = await _context.TAlbums
                .Where(x => x.FAlbumId == id)
                .FirstOrDefaultAsync();
            if (album is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            album.FAlbumName = albumsDTO.AlbumName;
            album.FAlbumType = albumsDTO.AlbumType;
            album.FReleaseDate = albumsDTO.ReleaseDate;
            _context.TAlbums.Update(album);
            try
            {
                await _context.SaveChangesAsync();
                albumsDTO.id = album.FAlbumId;
            }
            catch (Exception ex)
            {
                return Problem(ex.GetType() + ": " + ex.Message);
            }
            return Ok(albumsDTO);
        }

        /// <summary>
        /// 刪除指定專輯，刪除前檢查歌曲關聯。
        /// </summary>
        /// <remarks>
        /// DELETE /apis/v2/albums/{id}；不需要請求本文。
        ///
        /// 直接刪除專輯資料；仍有歌曲關聯時呼叫 Forbid，不會移除歌曲或其關聯。
        ///
        /// 實作注意：目前 Forbid 的字串參數會被當成驗證方案名稱，並非回應本文；未註冊對應方案時可能拋出例外，不能保證回傳 403。
        /// </remarks>
        /// <param name="id">路徑中的專輯整數編號。</param>
        /// <returns>成功時回傳內容為 { "message": "Deleted" } 的字串。</returns>
        /// <response code="200">刪除成功，回傳內容為 { "message": "Deleted" } 的字串。</response>
        /// <response code="400">路徑 id 無法繫結為整數。</response>
        /// <response code="404">id 為 null 或查無資料，回傳 Not Found 訊息字串。</response>
        /// <response code="500">儲存失敗時回傳 ProblemDetails；Forbid 執行例外另由全域錯誤處理。</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            if (await new CheckNavigate(_context).IsAlbumHaveSong((int)id))
            {
                return Forbid("{ \"message\": \"Still Have Songs related to this Album.\" }");
            }
            TAlbum? album = await _context.TAlbums
                .FirstOrDefaultAsync(x => x.FAlbumId == id);
            if (album is null)
            {
                return NotFound(MsgDicionary.Message404);
            }
            _context.TAlbums.Remove(album);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            return Ok(MsgDicionary.MessageDeleted);
        }
    }
}
