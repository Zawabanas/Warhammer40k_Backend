using WarHammer40K.Entities;
using WarHammer40K.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace WarHammer40K.Controllers
{
   
        [ApiController]
        [Route("api/[controller]")]
        public class FactionsController : Controller
        {
            private readonly ApplicationDbContext context;

            public FactionsController(ApplicationDbContext context)
            {
                this.context = context;
            }

            [HttpPost]
            [Consumes("multipart/form-data")]
            public async Task<ActionResult> Post([FromForm] FactionDTO factionsDto)
            {
                // Validar si el faction ya existe
                var factionExiste = await context.Factions.AnyAsync(x => x.Name == factionsDto.Name);
                if (factionExiste)
                {
                    return BadRequest($"La faccion '{factionsDto.Name}' ya existe.");
                }

                // Crear el objeto Faction
                var faction = new Faction
                {
                    Name = factionsDto.Name,
                    Description = factionsDto.Description
                };

                // Procesar la imagen si se proporciona
                if (factionsDto.Image != null && factionsDto.Image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(uploadsFolder); // Crear carpeta si no existe

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(factionsDto.Image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Guardar la imagen en el servidor
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await factionsDto.Image.CopyToAsync(stream);
                    }

                    // Asignar la URL de la imagen en el faccion
                    faction.Imagen = $"/images/{fileName}";
                }

                // Guardar la nueva faccion en la base de datos
                context.Factions.Add(faction);
                await context.SaveChangesAsync();

                return Ok(faction);
            }


        // GET: api/faccion
        //[HttpGet]
        //public async Task<ActionResult<List<Faction>>> Get()
        //{
        //    // Obtener todas las facciones
        //    return await context.Factions.ToListAsync();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Faction>>> Get([FromQuery] string? name, [FromQuery] string? description)
        {
            var query = context.Factions.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(f => f.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(f => f.Description.Contains(description));
            }

            var factions = await query.ToListAsync();
            return Ok(factions);
        }

        // GET: api/faccion/{id}
        [HttpGet("{id:int}")]
         public async Task<ActionResult<Faction>> Get(int id)
         {
             // Buscar la faccion por ID
             var faction = await context.Factions.FindAsync(id);
             if (faction == null)
             {
                 return NotFound($"No se encontró la faccion con ID {id}");
             }

             return Ok(faction);
         }




        // PUT: api/factions/{id}
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Put(int id, [FromForm] FactionDTO factionDto)
        {
            // Validar si el ID en la URL coincide con el ID del DTO de la facción enviada
            var faction = await context.Factions.FindAsync(id);
            if (faction == null)
            {
                return NotFound($"No se encontró la facción con ID {id}");
            }

            // Actualizar los campos de la facción
            faction.Name = factionDto.Name;
            faction.Description = factionDto.Description;

            // Procesar la nueva imagen si se proporciona
            if (factionDto.Image != null && factionDto.Image.Length > 0)
            {
                // Crear la carpeta si no existe
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                // Generar un nombre de archivo único
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(factionDto.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Guardar la nueva imagen en el servidor
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await factionDto.Image.CopyToAsync(stream);
                }

                // Actualizar la URL de la imagen en la entidad Faction
                faction.Imagen = $"/images/{fileName}";
            }

            // Guardar los cambios en la base de datos
            context.Factions.Update(faction);
            await context.SaveChangesAsync();

            return Ok(faction);
        }


        // DELETE: api/faccion/{id}
        [HttpDelete("{id:int}")]
            public async Task<ActionResult> Delete(int id)
            {
                // Buscar el faccion por ID
                var faction = await context.Factions.FindAsync(id);
                if (faction == null)
                {
                    return NotFound($"No se encontró el faccion con ID {id}");
                }

                // Eliminar el faccion
                context.Factions.Remove(faction);
                await context.SaveChangesAsync();

                return Ok($"Faccion con ID {id} eliminado.");
            }
            [HttpPost("{id:int}/upload-image")]
            [Consumes("multipart/form-data")] // Indicar a Swagger que es un formulario con datos binarios
            public async Task<ActionResult> UploadImage(int id, IFormFile image)
            {
                var faction = await context.Factions.FindAsync(id);
                if (faction == null)
                {
                    return NotFound($"No se encontró la faccion con ID {id}");
                }

                if (image == null || image.Length == 0)
                {
                    return BadRequest("No se ha proporcionado una imagen válida.");
                }

                // Guardar la imagen en el servidor
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder); // Crear carpeta si no existe

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Actualizar la URL de la imagen en la faccion
                faction.Imagen = $"/images/{fileName}";
                await context.SaveChangesAsync();

                return Ok(new { faction.Imagen });
            }

        }
    
}
