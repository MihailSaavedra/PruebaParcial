using AgroFlow.Central.Data;
using AgroFlow.Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgricultorasController : ControllerBase
{
    private readonly CentralDbContext _context;
    private readonly ILogger<AgricultorasController> _logger;

    public AgricultorasController(CentralDbContext context, ILogger<AgricultorasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/agricultoras
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agricultor>>> GetAgricultores()
    {
        try
        {
            var agricultores = await _context.Agricultores
                .Include(a => a.Cosechas)
                .ToListAsync();
            
            return Ok(agricultores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los agricultores");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/agricultoras/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Agricultor>> GetAgricultor(Guid id)
    {
        try
        {
            var agricultor = await _context.Agricultores
                .Include(a => a.Cosechas)
                .FirstOrDefaultAsync(a => a.AgricultorId == id);

            if (agricultor == null)
            {
                return NotFound($"Agricultor con ID {id} no encontrado");
            }

            return Ok(agricultor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el agricultor con ID {AgricultorId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/agricultoras
    [HttpPost]
    public async Task<ActionResult<Agricultor>> CreateAgricultor(Agricultor agricultor)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Asegurar que se genere un nuevo ID
            agricultor.AgricultorId = Guid.NewGuid();
            agricultor.FechaRegistro = DateTime.UtcNow;

            _context.Agricultores.Add(agricultor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Agricultor creado exitosamente: {AgricultorId}", agricultor.AgricultorId);

            return CreatedAtAction(nameof(GetAgricultor), 
                new { id = agricultor.AgricultorId }, agricultor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el agricultor");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/agricultoras/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAgricultor(Guid id, Agricultor agricultor)
    {
        try
        {
            if (id != agricultor.AgricultorId)
            {
                return BadRequest("El ID no coincide con el agricultor");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAgricultor = await _context.Agricultores.FindAsync(id);
            if (existingAgricultor == null)
            {
                return NotFound($"Agricultor con ID {id} no encontrado");
            }

            // Actualizar solo los campos permitidos
            existingAgricultor.Nombre = agricultor.Nombre;
            existingAgricultor.Finca = agricultor.Finca;
            existingAgricultor.Ubicacion = agricultor.Ubicacion;
            existingAgricultor.Correo = agricultor.Correo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Agricultor actualizado exitosamente: {AgricultorId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el agricultor con ID {AgricultorId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/agricultoras/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgricultor(Guid id)
    {
        try
        {
            var agricultor = await _context.Agricultores.FindAsync(id);
            if (agricultor == null)
            {
                return NotFound($"Agricultor con ID {id} no encontrado");
            }

            _context.Agricultores.Remove(agricultor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Agricultor eliminado exitosamente: {AgricultorId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el agricultor con ID {AgricultorId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

