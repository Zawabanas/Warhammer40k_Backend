using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarHammer40K.Entities;
using WarHammer40K.DTOs;
using AutoMapper;
using System.IO;

namespace WarHammer40K.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UnitsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/units
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnits([FromQuery] string? name, [FromQuery] string? type, [FromQuery] int? powerLevel)
        {
            var query = _context.Units.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.Name.Contains(name));
            if (!string.IsNullOrEmpty(type))
                query = query.Where(u => u.Type.Contains(type));
            if (powerLevel.HasValue)
                query = query.Where(u => u.PowerLevel == powerLevel);

            var units = await query.Include(u => u.Faction).ToListAsync();
            return Ok(units);
        }

        // GET: api/units/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetUnit(int id)
        {
            var unit = await _context.Units.Include(u => u.Faction).FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null) return NotFound();
            return Ok(unit);
        }

        // POST: api/units
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> PostUnit([FromForm] UnitDTO unitDto)
        {
            var unit = _mapper.Map<Unit>(unitDto);

            if (unitDto.Image != null && unitDto.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(unitDto.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await unitDto.Image.CopyToAsync(stream);
                }
                unit.Imagen = $"/images/{fileName}";
            }

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnit), new { id = unit.Id }, unit);
        }

        // PUT: api/units/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateUnit(int id, [FromForm] UnitDTO unitDto)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            _mapper.Map(unitDto, unit);

            if (unitDto.Image != null && unitDto.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(unitDto.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await unitDto.Image.CopyToAsync(stream);
                }
                unit.Imagen = $"/images/{fileName}";
            }

            await _context.SaveChangesAsync();
            return Ok(unit);
        }

        // DELETE: api/units/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
