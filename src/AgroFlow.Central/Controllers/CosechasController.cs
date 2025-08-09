using AgroFlow.Central.Data;
using AgroFlow.Central.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CosechasController : ControllerBase
{
    private readonly CentralDbContext _context;
    private readonly ILogger<CosechasController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public CosechasController(
        CentralDbContext context, 
        ILogger<CosechasController> logger,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    // GET: api/cosechas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cosecha>>> GetCosechas()
    {
        try
        {
            var cosechas = await _context.Cosechas
                .Include(c => c.Agricultor)
                .ToListAsync();
            
            return Ok(cosechas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las cosechas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/cosechas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Cosecha>> GetCosecha(Guid id)
    {
        try
        {
            var cosecha = await _context.Cosechas
                .Include(c => c.Agricultor)
                .FirstOrDefaultAsync(c => c.CosechaId == id);

            if (cosecha == null)
            {
                return NotFound($"Cosecha con ID {id} no encontrada");
            }

            return Ok(cosecha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la cosecha con ID {CosechaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/cosechas/agricultor/{agricultorId}
    [HttpGet("agricultor/{agricultorId}")]
    public async Task<ActionResult<IEnumerable<Cosecha>>> GetCosechasByAgricultor(Guid agricultorId)
    {
        try
        {
            var cosechas = await _context.Cosechas
                .Include(c => c.Agricultor)
                .Where(c => c.AgricultorId == agricultorId)
                .ToListAsync();

            return Ok(cosechas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las cosechas del agricultor {AgricultorId}", agricultorId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/cosechas
    [HttpPost]
    public async Task<ActionResult<Cosecha>> CreateCosecha(Cosecha cosecha)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el agricultor existe
            var agricultor = await _context.Agricultores.FindAsync(cosecha.AgricultorId);
            if (agricultor == null)
            {
                return BadRequest($"Agricultor con ID {cosecha.AgricultorId} no encontrado");
            }

            // Validar que las toneladas sean positivas
            if (cosecha.Toneladas <= 0)
            {
                return BadRequest("Las toneladas deben ser mayor a 0");
            }

            // Asegurar que se genere un nuevo ID
            cosecha.CosechaId = Guid.NewGuid();
            cosecha.CreadoEn = DateTime.UtcNow;
            cosecha.Estado = "REGISTRADA";

            _context.Cosechas.Add(cosecha);
            await _context.SaveChangesAsync();

            // Publicar evento de nueva cosecha
            var nuevaCosechaEvent = new NuevaCosechaEvent(
                cosecha.CosechaId,
                cosecha.Producto,
                cosecha.Toneladas,
                DateTime.UtcNow
            );

            await _publishEndpoint.Publish(nuevaCosechaEvent);

            _logger.LogInformation("Cosecha creada y evento publicado exitosamente: {CosechaId}", cosecha.CosechaId);

            return CreatedAtAction(nameof(GetCosecha), 
                new { id = cosecha.CosechaId }, cosecha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la cosecha");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/cosechas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCosecha(Guid id, Cosecha cosecha)
    {
        try
        {
            if (id != cosecha.CosechaId)
            {
                return BadRequest("El ID no coincide con la cosecha");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCosecha = await _context.Cosechas.FindAsync(id);
            if (existingCosecha == null)
            {
                return NotFound($"Cosecha con ID {id} no encontrada");
            }

            // Validar que las toneladas sean positivas
            if (cosecha.Toneladas <= 0)
            {
                return BadRequest("Las toneladas deben ser mayor a 0");
            }

            // Actualizar solo los campos permitidos
            existingCosecha.Producto = cosecha.Producto;
            existingCosecha.Toneladas = cosecha.Toneladas;
            existingCosecha.Estado = cosecha.Estado;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cosecha actualizada exitosamente: {CosechaId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la cosecha con ID {CosechaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/cosechas/{id}/estado
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstadoCosecha(Guid id, [FromBody] string nuevoEstado)
    {
        try
        {
            var cosecha = await _context.Cosechas
                .Include(c => c.Agricultor)
                .FirstOrDefaultAsync(c => c.CosechaId == id);
            
            if (cosecha == null)
            {
                return NotFound($"Cosecha con ID {id} no encontrada");
            }

            // Validar estados permitidos
            var estadosPermitidos = new[] { "REGISTRADA", "EN_PROCESO", "FACTURADA", "COMPLETADA" };
            if (!estadosPermitidos.Contains(nuevoEstado.ToUpper()))
            {
                return BadRequest($"Estado '{nuevoEstado}' no válido. Estados permitidos: {string.Join(", ", estadosPermitidos)}");
            }

            var estadoAnterior = cosecha.Estado;
            cosecha.Estado = nuevoEstado.ToUpper();
            await _context.SaveChangesAsync();

            // Si el estado cambió a EN_PROCESO, publicar evento para facturación
            if (cosecha.Estado == "EN_PROCESO" && estadoAnterior != "EN_PROCESO")
            {
                var cosechaEnProcesoEvent = new CosechaEnProcesoEvent(
                    cosecha.CosechaId,
                    cosecha.AgricultorId,
                    cosecha.Agricultor?.Nombre ?? "Desconocido",
                    cosecha.Producto,
                    cosecha.Toneladas,
                    DateTime.UtcNow
                );

                await _publishEndpoint.Publish(cosechaEnProcesoEvent);
                _logger.LogInformation("Evento CosechaEnProceso publicado para cosecha {CosechaId}", id);
            }

            _logger.LogInformation("Estado de cosecha actualizado: {CosechaId} -> {NuevoEstado}", id, nuevoEstado);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el estado de la cosecha con ID {CosechaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/cosechas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCosecha(Guid id)
    {
        try
        {
            var cosecha = await _context.Cosechas.FindAsync(id);
            if (cosecha == null)
            {
                return NotFound($"Cosecha con ID {id} no encontrada");
            }

            // Solo permitir eliminación si está en estado REGISTRADA
            if (cosecha.Estado != "REGISTRADA")
            {
                return BadRequest($"No se puede eliminar una cosecha en estado '{cosecha.Estado}'. Solo se pueden eliminar cosechas en estado 'REGISTRADA'");
            }

            _context.Cosechas.Remove(cosecha);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cosecha eliminada exitosamente: {CosechaId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la cosecha con ID {CosechaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

