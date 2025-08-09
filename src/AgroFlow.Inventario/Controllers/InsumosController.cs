using AgroFlow.Inventario.Data;
using AgroFlow.Inventario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Inventario.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsumosController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly ILogger<InsumosController> _logger;

    public InsumosController(InventarioDbContext context, ILogger<InsumosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/insumos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Insumo>>> GetInsumos()
    {
        try
        {
            var insumos = await _context.Insumos
                .OrderBy(i => i.NombreInsumo)
                .ToListAsync();

            return Ok(insumos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener insumos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/insumos/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Insumo>> GetInsumo(Guid id)
    {
        try
        {
            var insumo = await _context.Insumos.FindAsync(id);

            if (insumo == null)
            {
                return NotFound($"Insumo con ID {id} no encontrado");
            }

            return Ok(insumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener insumo {InsumoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/insumos/categoria/{categoria}
    [HttpGet("categoria/{categoria}")]
    public async Task<ActionResult<IEnumerable<Insumo>>> GetInsumosByCategoria(string categoria)
    {
        try
        {
            var insumos = await _context.Insumos
                .Where(i => i.Categoria == categoria)
                .OrderBy(i => i.NombreInsumo)
                .ToListAsync();

            return Ok(insumos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener insumos de categoría {Categoria}", categoria);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/insumos/stock-bajo
    [HttpGet("stock-bajo")]
    public async Task<ActionResult<IEnumerable<Insumo>>> GetInsumosStockBajo([FromQuery] int limite = 10)
    {
        try
        {
            var insumos = await _context.Insumos
                .Where(i => i.Stock <= limite)
                .OrderBy(i => i.Stock)
                .ToListAsync();

            return Ok(insumos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener insumos con stock bajo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/insumos
    [HttpPost]
    public async Task<ActionResult<Insumo>> CreateInsumo(Insumo insumo)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un insumo con el mismo nombre
            var insumoExistente = await _context.Insumos
                .FirstOrDefaultAsync(i => i.NombreInsumo.ToLower() == insumo.NombreInsumo.ToLower());

            if (insumoExistente != null)
            {
                return Conflict($"Ya existe un insumo con el nombre '{insumo.NombreInsumo}'");
            }

            insumo.InsumoId = Guid.NewGuid();
            _context.Insumos.Add(insumo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Insumo creado: {InsumoId} - {NombreInsumo}", insumo.InsumoId, insumo.NombreInsumo);

            return CreatedAtAction(nameof(GetInsumo), new { id = insumo.InsumoId }, insumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear insumo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/insumos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInsumo(Guid id, Insumo insumo)
    {
        try
        {
            if (id != insumo.InsumoId)
            {
                return BadRequest("El ID no coincide con el insumo");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingInsumo = await _context.Insumos.FindAsync(id);
            if (existingInsumo == null)
            {
                return NotFound($"Insumo con ID {id} no encontrado");
            }

            // Actualizar campos
            existingInsumo.NombreInsumo = insumo.NombreInsumo;
            existingInsumo.Stock = insumo.Stock;
            existingInsumo.UnidadMedida = insumo.UnidadMedida;
            existingInsumo.Categoria = insumo.Categoria;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Insumo actualizado: {InsumoId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar insumo {InsumoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/insumos/{id}/stock
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int nuevoStock)
    {
        try
        {
            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null)
            {
                return NotFound($"Insumo con ID {id} no encontrado");
            }

            if (nuevoStock < 0)
            {
                return BadRequest("El stock no puede ser negativo");
            }

            var stockAnterior = insumo.Stock;
            insumo.Stock = nuevoStock;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock actualizado para {NombreInsumo}: {StockAnterior} -> {NuevoStock}", 
                insumo.NombreInsumo, stockAnterior, nuevoStock);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar stock del insumo {InsumoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/insumos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInsumo(Guid id)
    {
        try
        {
            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null)
            {
                return NotFound($"Insumo con ID {id} no encontrado");
            }

            _context.Insumos.Remove(insumo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Insumo eliminado: {InsumoId} - {NombreInsumo}", id, insumo.NombreInsumo);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar insumo {InsumoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/insumos/estadisticas
    [HttpGet("estadisticas")]
    public async Task<ActionResult<object>> GetEstadisticas()
    {
        try
        {
            var totalInsumos = await _context.Insumos.CountAsync();
            var totalStock = await _context.Insumos.SumAsync(i => i.Stock);
            var insumosStockBajo = await _context.Insumos.CountAsync(i => i.Stock <= 10);
            var categorias = await _context.Insumos
                .Where(i => !string.IsNullOrEmpty(i.Categoria))
                .GroupBy(i => i.Categoria)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalInsumos = totalInsumos,
                TotalStock = totalStock,
                InsumosStockBajo = insumosStockBajo,
                CategoriasPorInsumo = categorias
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
