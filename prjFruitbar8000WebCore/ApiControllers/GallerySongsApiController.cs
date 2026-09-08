using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Services;
// 
using prjFruitbar8000WebCore.Models.ViewModels;

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
        
        // GET: api/<GalleryApiController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var queryList = await new GalleryDataAccess().ListApi(_context);

            return Ok(queryList);
        }

        // GET api/<GalleryApiController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {        
            GallerySongsDTO? qResult = await new GalleryDataAccess().ListApiById(id, _context);
            
            if(qResult is null)
            {
                return NotFound(MsgDicionary.message404);
            }

            return Ok(qResult);
        }

        // POST api/<GalleryApiController>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GallerySongsDTO newGSong)
        {
            if(newGSong is null
            || newGSong.songName is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            ResultDTO result = await new GalleryDataAccess().PostCreateApi(newGSong, _context);

            if(result.isSuccess)
            {
                return Ok();
            }
            if(result.statusMessage == "NotFound")
            {
                return NotFound(MsgDicionary.message404);
            }
            return Problem(result.statusMessage);
        }

        // PUT api/<GalleryApiController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GallerySongsDTO newInfoSong)
        {
            if(id != newInfoSong.id)
            {
                return NotFound(MsgDicionary.message404);
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
                return NotFound(MsgDicionary.message404);
            }
            ResultDTO result = await new GalleryDataAccess()
                .PostEditApi(newInfoSong, songToBeUpdated, _context);
            if(result.isSuccess)
            {
                return Ok(newInfoSong);
            }
            if(result.statusMessage == "Not Found")
            {
                return NotFound(MsgDicionary.message404);
            }
            return Problem(result.statusMessage);
        }

        // DELETE api/<GalleryApiController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await new GalleryDataAccess().Delete(id, _context);
            if(result.isSuccess)
            {
                return Ok(MsgDicionary.messagedeleted);
            }
            else
            {
                // NEXT-TODO: expand returned info
                return Problem(result.statusMessage);
            }
            
        }
    }
}
