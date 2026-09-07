using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("apis/v1/albums")]
    [ApiController]
    public class AlbumsApiController : ControllerBase
    {

        private readonly FruitBarDbContext _context;
        public AlbumsApiController(FruitBarDbContext context)
        {
            _context = context;
        }

        // GET: api/<AlbumsApiController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<AlbumsDTO>? albums = new List<AlbumsDTO>();
            try
            {
                albums = await _context.TAlbums
                    .OrderBy(x => x.FAlbumId)
                    .Select(x => new AlbumsDTO()
                    {
                        id = x.FAlbumId,
                        albumName = x.FAlbumName,
                        albumType = x.FAlbumType,
                        releaseDate = x.FReleaseDate
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

        // GET api/<AlbumsApiController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            AlbumsDTO? album = await _context.TAlbums
            .OrderBy(x => x.FAlbumId)
            .Where(x => x.FAlbumId == id)
            .Select(x => new AlbumsDTO()
            {
                id = x.FAlbumId,
                albumName = x.FAlbumName,
                albumType = x.FAlbumType,
                releaseDate = x.FReleaseDate
            }).FirstOrDefaultAsync();
            if (album is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            return Ok(album);
        }

        // POST api/<AlbumsApiController>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AlbumsDTO albumsDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var tobeAdd = new TAlbum()
            {
                FAlbumName = albumsDTO.albumName,
                FAlbumType = albumsDTO.albumType,
                FReleaseDate = albumsDTO.releaseDate
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

        // PUT api/<AlbumsApiController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int? id, [FromBody] AlbumsDTO albumsDTO)
        {
            if (id is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            TAlbum? album = await _context.TAlbums
                .Where(x => x.FAlbumId == id)
                .FirstOrDefaultAsync();
            if (album is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            album.FAlbumName = albumsDTO.albumName;
            album.FAlbumType = albumsDTO.albumType;
            album.FReleaseDate = albumsDTO.releaseDate;
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

        // DELETE api/<AlbumsApiController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound(MsgDicionary.message404);
            }
            if (await new CheckNavigate(_context).IsAlbumHaveSong((int)id))
            {
                return Forbid("{ \"message\": \"Still Have Songs related to this Album.\" }");
            }
            TAlbum? album = await _context.TAlbums
                .FirstOrDefaultAsync(x => x.FAlbumId == id);
            if (album is null)
            {
                return NotFound(MsgDicionary.message404);
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
            return Ok(MsgDicionary.messagedeleted);
        }
    }
}
