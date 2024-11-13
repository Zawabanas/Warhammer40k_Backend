using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarHammer40K.Entities;
using WarHammer40K.DTOs;
using System.IO;
using AutoMapper;

namespace WarHammer40K.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CharactersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST: Crear un nuevo personaje
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Post([FromForm] CharacterDTO characterDTO)
        {
            // Validar si el personaje ya existe
            var characterExists = await _context.Characters.AnyAsync(c => c.Name == characterDTO.Name);
            if (characterExists) return BadRequest($"El personaje '{characterDTO.Name}' ya existe.");

            var character = _mapper.Map<Character>(characterDTO);

            // Guardar la imagen si se proporciona
            if (characterDTO.Image != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(characterDTO.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await characterDTO.Image.CopyToAsync(stream);
                }

                character.Imagen = $"/images/{fileName}";
            }

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            return Ok(character);
        }

        // GET: Listar todos los personajes
        //[HttpGet]
        //public async Task<ActionResult<List<Character>>> Get()
        //{
        //    return await _context.Characters.Include(c => c.Faction).ToListAsync();
        //}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharacters([FromQuery] string? name, [FromQuery] int? factionId)
        {
            var query = _context.Characters.Include(c => c.Faction).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            if (factionId.HasValue)
            {
                query = query.Where(c => c.FactionId == factionId.Value);
            }

            var characters = await query.ToListAsync();
            return Ok(characters);
        }


        // GET: Obtener un personaje por ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Character>> Get(int id)
        {
            var character = await _context.Characters.Include(c => c.Faction).FirstOrDefaultAsync(c => c.Id == id);
            if (character == null) return NotFound($"No se encontró el personaje con ID {id}");

            return Ok(character);
        }

        // PUT: Actualizar un personaje
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Put(int id, [FromForm] CharacterDTO characterDTO)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null) return NotFound($"No se encontró el personaje con ID {id}");

            _mapper.Map(characterDTO, character);

            // Si hay una nueva imagen, actualizarla
            if (characterDTO.Image != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(characterDTO.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await characterDTO.Image.CopyToAsync(stream);
                }

                character.Imagen = $"/images/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(character);
        }

        // DELETE: Eliminar un personaje
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null) return NotFound($"No se encontró el personaje con ID {id}");

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return Ok($"Personaje con ID {id} eliminado.");
        }
    }
}
