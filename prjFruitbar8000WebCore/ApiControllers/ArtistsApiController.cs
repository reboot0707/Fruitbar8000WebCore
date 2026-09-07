using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Services;

namespace prjFruitbar8000WebCore.ApiControllers
{
    [Route("api/v1/artists")]
    [ApiController]
    public class ArtistsApiController : ControllerBase
    {
        // TODO: 移動到共用區域
        private readonly string message404 = "{ \"message\": \"Not Found\" }";
        private readonly string messagedeleted = "{ \"message\": \"Deleted\" }";
        private readonly FruitBarDbContext _context;

        public ArtistsApiController(FruitBarDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
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

        // GET api/v1/artists/5
        [HttpGet("{id}")]
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
                return NotFound(message404);
            }
            return Ok(artist);
        }

        // POST api/<ArtistsApiController>
        [HttpPost]
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

        // PUT api/<ArtistsApiController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int? id, [FromBody] ArtistsDTO artistsDTO)
        {
            if (id is null)
            {
                return NotFound(message404);
            }
            TArtist? artist = await _context.TArtists
                .Where(x => x.FArtistId == id)
                .FirstOrDefaultAsync();
            if (artist is null)
            {
                return NotFound(message404);
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

        // DELETE api/<ArtistsApiController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound(message404);
            }
            if (await new CheckNavigate(_context).IsArtistHaveSong((int)id))
            {
                return Forbid("{ \"message\": \"Still Have Songs related to this Artist.\" }");
            }
            TArtist? artist = await _context.TArtists
                .FirstOrDefaultAsync(x => x.FArtistId == id);
            if (artist is null)
            {
                return NotFound(message404);
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
            return Ok(messagedeleted);
        }
    }
}
