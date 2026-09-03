using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        
        public ArtistsApiController(FruitBarDbContext context)
        {
            _context = context;
        }
        
        // GET
        [HttpGet]
        public List<ArtistsDTO> Get()
        {
            List<ArtistsDTO>? artists = _context.TArtists
            .OrderBy(x => x.FArtistId)
            .Select(x => new ArtistsDTO()
            {
                FArtistId = x.FArtistId,
                FArtistName = x.FArtistName,
                FArtistType = x.FArtistType
            })
            .ToList();
            return artists;
        }

        // GET api/v1/artists/5
        [HttpGet("{id}")]
        public ArtistsDTO? Get(int id)
        {
            ArtistsDTO? artist = _context.TArtists
            .Where(x => x.FArtistId == id)
            .Select(x => new ArtistsDTO()
            {
                FArtistId = x.FArtistId,
                FArtistName = x.FArtistName,
                FArtistType = x.FArtistType
            }).FirstOrDefault();
            return artist;
        }

        // POST api/<ArtistsApiController>
        [HttpPost]
        public void Create(ArtistsDTO artistsDTO)
        {
            if (!ModelState.IsValid)
            {
                return;
            }
            var tobeAdd = new TArtist()
            {
                FArtistName = artistsDTO.FArtistName,
                FArtistType = artistsDTO.FArtistType
            };
            
            _context.Add(tobeAdd);
            _context.SaveChangesAsync();
        }

        // PUT api/<ArtistsApiController>/5
        [HttpPut("{id}")]
        public void Update(int id, ArtistsDTO artistsDTO)
        {
        }

        // DELETE api/<ArtistsApiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
