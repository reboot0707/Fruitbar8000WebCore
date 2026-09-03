using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.DTOs;
using prjFruitbar8000WebCore.Models.Entities;
using prjFruitbar8000WebCore.Models.Wraps;

namespace prjFruitbar8000WebCore.ApiControllers
{   
    [Route("api/v1/artists")]
    [ApiController]
    public class ArtistsApiController : ControllerBase
    {
        private readonly FruitBarDbContext _context;
        private readonly string message404 = "{ \"message\": \"Not Found\" }";
        private readonly string messagedeleted = "{ \"message\": \"Deleted\" }";
        
        public ArtistsApiController(FruitBarDbContext context)
        {
            _context = context;
        }
        
        // GET
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<ArtistsDTO>? artists = await _context.TArtists
            .OrderBy(x => x.FArtistId)
            .Select(x => new ArtistsDTO()
            {
                id = x.FArtistId,
                artistName = x.FArtistName,
                artistType = x.FArtistType
            })
            .ToListAsync();
            return Ok(artists);
        }

        // GET api/v1/artists/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ArtistsDTO? artist = await _context.TArtists
            .Where(x => x.FArtistId == id)
            .Select(x => new ArtistsDTO()
            {
                id = x.FArtistId,
                artistName = x.FArtistName,
                artistType = x.FArtistType
            }).FirstOrDefaultAsync();
            if(artist is null)
            {
                return NotFound("{ \"message\": \"Not Found\" }");   
            }
            return Ok(artist);
        }

        // POST api/<ArtistsApiController>
        [HttpPost]
        public async Task<IActionResult> Create(ArtistsDTO artistsDTO)
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
            return CreatedAtAction(
                nameof(Get),
                new { id = artistsDTO.id },
                artistsDTO
            );
        }

        // PUT api/<ArtistsApiController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArtistsDTO artistsDTO)
        {
            TArtist? artist = await _context.TArtists
                .Where(x => x.FArtistId == id)
                .FirstOrDefaultAsync();
            if(artist is null)
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
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
            return Ok(artistsDTO);
        }

        // DELETE api/<ArtistsApiController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            TArtist? artist = await _context.TArtists
                .Where(x => x.FArtistId == id)
                .FirstOrDefaultAsync();
            if(artist is null)
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
