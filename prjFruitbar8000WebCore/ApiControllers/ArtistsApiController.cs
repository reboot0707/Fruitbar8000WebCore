using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("apis/v2/artists")]
    [ApiController]
    public class ArtistsApiController : ControllerBase
    {
        private readonly FruitBarDbContext _context;

        public ArtistsApiController(FruitBarDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有創作者。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/artists；無查詢參數、篩選或分頁，依 id 遞增排序。
        ///
        /// 每筆資料包含 id、artistName、artistType；查無資料時回傳空陣列。
        /// </remarks>
        /// <returns>ArtistsDTO 陣列。</returns>
        /// <response code="200">查詢成功，回傳 ArtistsDTO[]。查無資料時為空陣列。</response>
        /// <response code="204">僅在查詢結果為 null 的防禦分支回傳；正常 ToListAsync 不會進入此分支。</response>
        /// <response code="500">捕捉到查詢例外時回傳 ProblemDetails。</response>
        [HttpGet]
        [ProducesResponseType(typeof(ArtistsDTO[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            List<ArtistsDTO>? artists = new List<ArtistsDTO>();
            try
            {
                artists = await _context.TArtists
                    .OrderBy(x => x.FArtistId)
                    .Select(x => new ArtistsDTO()
                    {
                        id = x.FArtistId,
                        artistName = x.FArtistName,
                        artistType = x.FArtistType
                    })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                // TODO-NEXT: error handling to log error
                return Problem(ex.Message);
            }
            if (artists is null)
            {
                return NoContent();
            }
            return Ok(artists);
        }

        /// <summary>
        /// 依編號取得單一創作者。
        /// </summary>
        /// <remarks>
        /// GET /apis/v2/artists/{id}；回傳欄位為 id、artistName、artistType。
        /// </remarks>
        /// <param name="id">路徑中的創作者整數編號。</param>
        /// <returns>找到的 ArtistsDTO，或找不到資料的訊息字串。</returns>
        /// <response code="200">查詢成功，回傳 ArtistsDTO。</response>
        /// <response code="400">路徑 id 無法繫結為整數。</response>
        /// <response code="404">查無指定編號，回傳內容為 { "message": "Not Found" } 的字串。</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ArtistsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            ArtistsDTO? artist = await _context.TArtists
            .OrderBy(x => x.FArtistId)
            .Where(x => x.FArtistId == id)
            .Select(x => new ArtistsDTO()
            {
                id = x.FArtistId,
                artistName = x.FArtistName,
                artistType = x.FArtistType
            }).FirstOrDefaultAsync();
            if (artist is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            return Ok(artist);
        }

        /// <summary>
        /// 新增創作者。
        /// </summary>
        /// <remarks>
        /// POST /apis/v2/artists；Content-Type: application/json。
        ///
        /// JSON 本文：artistName 為必填名稱，artistType 可為 null。資料庫長度上限分別為 200 與 50 字元，DTO 未設定長度驗證。
        ///
        /// 本文 id 不用指定，新增後使用資料庫產生的編號。
        ///
        /// 成功使用 HTTP 200，回傳內容為 { "newAlbumId": "編號" } 的字串；編號值為字串。 現有欄位名稱仍為 newAlbumId，實際值是創作者編號。
        /// </remarks>
        /// <param name="artistsDTO">要新增的 ArtistsDTO。</param>
        /// <returns>包含新增編號的字串。</returns>
        /// <response code="200">新增成功，回傳內容為 { "newAlbumId": "編號" } 的字串，編號值為字串。此編號實際為創作者編號。</response>
        /// <response code="400">JSON 本文或模型驗證失敗。</response>
        /// <response code="500">儲存失敗，回傳 ProblemDetails。</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ArtistsDTO artistsDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var tobeAdd = new TArtist()
            {
                FArtistName = artistsDTO.artistName,
                FArtistType = artistsDTO.artistType
            };

            _context.TArtists.Add(tobeAdd);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            artistsDTO.id = tobeAdd.FArtistId;
            //Use Ok instead of Created
            return Ok($"{{ \"newAlbumId\": \"{artistsDTO.id}\" }}");
        }

        /// <summary>
        /// 更新指定創作者的全部可編輯欄位。
        /// </summary>
        /// <remarks>
        /// PUT /apis/v2/artists/{id}；Content-Type: application/json。
        ///
        /// JSON 本文：artistName 為必填名稱，artistType 可為 null。資料庫長度上限分別為 200 與 50 字元，DTO 未設定長度驗證。
        ///
        /// 以路徑 id 為準，本文 id 不參與比對；成功回傳時會填入實際編號。artistName、artistType 皆會覆寫，未提供的選填欄位會設為 null。
        /// </remarks>
        /// <param name="id">路徑中的創作者整數編號。</param>
        /// <param name="artistsDTO">更新內容，格式為 ArtistsDTO。</param>
        /// <returns>更新後的 DTO。</returns>
        /// <response code="200">更新成功，回傳 ArtistsDTO。id 已填入實際更新的編號。</response>
        /// <response code="400">路徑 id、JSON 本文或模型驗證失敗。</response>
        /// <response code="404">id 為 null 或查無資料，回傳 Not Found 訊息字串。</response>
        /// <response code="500">儲存失敗，回傳 ProblemDetails。</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ArtistsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int? id, [FromBody] ArtistsDTO artistsDTO)
        {
            if (id is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            TArtist? artist = await _context.TArtists
                .Where(x => x.FArtistId == id)
                .FirstOrDefaultAsync();
            if (artist is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            artist.FArtistName = artistsDTO.artistName;
            artist.FArtistType = artistsDTO.artistType;
            _context.TArtists.Update(artist);
            try
            {
                await _context.SaveChangesAsync();
                artistsDTO.id = artist.FArtistId;
            }
            catch (Exception ex)
            {
                return Problem(ex.GetType() + ": " + ex.Message);
            }
            return Ok(artistsDTO);
        }

        /// <summary>
        /// 刪除指定創作者，刪除前檢查歌曲關聯。
        /// </summary>
        /// <remarks>
        /// DELETE /apis/v2/artists/{id}；不需要請求本文。
        ///
        /// 直接刪除創作者資料；仍有歌曲關聯時呼叫 Forbid，不會移除歌曲或其關聯。
        ///
        /// 實作注意：目前 Forbid 的字串參數會被當成驗證方案名稱，並非回應本文；未註冊對應方案時可能拋出例外，不能保證回傳 403。
        /// </remarks>
        /// <param name="id">路徑中的創作者整數編號。</param>
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
                return NotFound(MsgDicionary.message404);
            }
            if (await new CheckNavigate(_context).IsArtistHaveSong((int)id))
            {
                return Forbid("{ \"message\": \"Still Have Songs related to this Artist.\" }");
            }
            TArtist? artist = await _context.TArtists
                .FirstOrDefaultAsync(x => x.FArtistId == id);
            if (artist is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            _context.TArtists.Remove(artist);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            return Ok(MsgDicionary.messagedeleted);
        }
    }
}
