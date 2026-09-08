using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Services;
// 
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("apis/v1/gallery/songs")]
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
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GalleryApiController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GalleryApiController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if(await new GalleryDataAccess().Delete(id, _context))
            {
                return Ok(MsgDicionary.messagedeleted);
            }
            else
            {
                // NEXT-TODO: expand returned info
                return Problem();
            }
            
        }
    }
}
