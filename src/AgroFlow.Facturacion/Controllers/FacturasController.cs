using AgroFlow.Facturacion.Data;
using AgroFlow.Facturacion.DTOs;
using AgroFlow.Facturacion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Facturacion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturacionDbContext _context;
    private readonly ILogger<FacturasController> _logger;

    public FacturasController(FacturacionDbContext context, ILogger<FacturasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/facturas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Factura>>> GetFacturas()
    {
        try
        {
            var facturas = await _context.Facturas
                .Include(f => f.Detalles)
                .OrderByDescending(f => f.FechaCreacion)
                .ToListAsync();

            return Ok(facturas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener facturas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/facturas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Factura>> GetFactura(Guid id)
    {
        try
        {
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null)
            {
                return NotFound($"Factura con ID {id} no encontrada");
            }

            return Ok(factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener factura {FacturaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/facturas/agricultor/{agricultorId}
    [HttpGet("agricultor/{agricultorId}")]
    public async Task<ActionResult<IEnumerable<Factura>>> GetFacturasByAgricultor(Guid agricultorId)
    {
        try
        {
            var facturas = await _context.Facturas
                .Include(f => f.Detalles)
                .Where(f => f.AgricultorId == agricultorId)
                .OrderByDescending(f => f.FechaCreacion)
                .ToListAsync();

            return Ok(facturas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener facturas del agricultor {AgricultorId}", agricultorId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/facturas/cosecha/{cosechaId}
    [HttpGet("cosecha/{cosechaId}")]
    public async Task<ActionResult<Factura>> GetFacturaByCosecha(Guid cosechaId)
    {
        try
        {
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.CosechaId == cosechaId);

            if (factura == null)
            {
                return NotFound($"No se encontró factura para la cosecha {cosechaId}");
            }

            return Ok(factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener factura de la cosecha {CosechaId}", cosechaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/facturas
    [HttpPost]
    public async Task<ActionResult<Factura>> CreateFactura(CreateFacturaDto dto)
    {
        try
        {
            // Verificar si ya existe una factura para esta cosecha
            var facturaExistente = await _context.Facturas
                .FirstOrDefaultAsync(f => f.CosechaId == dto.CosechaId);

            if (facturaExistente != null)
            {
                return Conflict($"Ya existe una factura para la cosecha {dto.CosechaId}");
            }

            // Calcular valores
            var subtotal = dto.Toneladas * dto.PrecioPorTonelada;
            var porcentajeImpuesto = 19.0m; // IVA 19%
            var montoImpuesto = subtotal * (porcentajeImpuesto / 100);
            var total = subtotal + montoImpuesto;

            var factura = new Factura
            {
                FacturaId = Guid.NewGuid(),
                CosechaId = dto.CosechaId,
                AgricultorId = dto.AgricultorId,
                NombreAgricultor = dto.NombreAgricultor,
                Producto = dto.Producto,
                Toneladas = dto.Toneladas,
                PrecioPorTonelada = dto.PrecioPorTonelada,
                Subtotal = subtotal,
                PorcentajeImpuesto = porcentajeImpuesto,
                MontoImpuesto = montoImpuesto,
                Total = total,
                Estado = "PENDIENTE",
                Observaciones = dto.Observaciones,
                FechaCreacion = DateTime.UtcNow
            };

            // Crear detalle
            var detalle = new DetalleFactura
            {
                DetalleId = Guid.NewGuid(),
                FacturaId = factura.FacturaId,
                Concepto = $"Venta de {dto.Producto}",
                Cantidad = dto.Toneladas,
                PrecioUnitario = dto.PrecioPorTonelada,
                Subtotal = subtotal,
                UnidadMedida = "Ton"
            };

            _context.Facturas.Add(factura);
            _context.DetallesFactura.Add(detalle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Factura creada: {FacturaId}", factura.FacturaId);

            // Incluir detalles en la respuesta
            factura.Detalles = new List<DetalleFactura> { detalle };

            return CreatedAtAction(nameof(GetFactura), new { id = factura.FacturaId }, factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear factura");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/facturas/{id}/estado
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateFacturaEstado(Guid id, UpdateFacturaEstadoDto dto)
    {
        try
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound($"Factura con ID {id} no encontrada");
            }

            // Validar transiciones de estado
            if (factura.Estado == "ANULADA")
            {
                return BadRequest("No se puede modificar una factura anulada");
            }

            if (factura.Estado == "PAGADA" && dto.Estado != "ANULADA")
            {
                return BadRequest("Solo se puede anular una factura pagada");
            }

            factura.Estado = dto.Estado;
            factura.Observaciones = dto.Observaciones;

            if (dto.Estado == "PAGADA" && dto.FechaPago.HasValue)
            {
                factura.FechaPago = dto.FechaPago.Value;
            }
            else if (dto.Estado == "PAGADA")
            {
                factura.FechaPago = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Estado de factura {FacturaId} actualizado a {Estado}", id, dto.Estado);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de factura {FacturaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/facturas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFactura(Guid id)
    {
        try
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound($"Factura con ID {id} no encontrada");
            }

            // Solo se pueden eliminar facturas pendientes
            if (factura.Estado != "PENDIENTE")
            {
                return BadRequest("Solo se pueden eliminar facturas en estado PENDIENTE");
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Factura eliminada: {FacturaId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar factura {FacturaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/facturas/estadisticas
    [HttpGet("estadisticas")]
    public async Task<ActionResult<object>> GetEstadisticas()
    {
        try
        {
            var totalFacturas = await _context.Facturas.CountAsync();
            var facturasPendientes = await _context.Facturas.CountAsync(f => f.Estado == "PENDIENTE");
            var facturasPagadas = await _context.Facturas.CountAsync(f => f.Estado == "PAGADA");
            var facturasAnuladas = await _context.Facturas.CountAsync(f => f.Estado == "ANULADA");
            
            var montoTotal = await _context.Facturas
                .Where(f => f.Estado == "PAGADA")
                .SumAsync(f => f.Total);

            var montoMesActual = await _context.Facturas
                .Where(f => f.Estado == "PAGADA" && 
                           f.FechaPago.HasValue &&
                           f.FechaPago.Value.Month == DateTime.Now.Month &&
                           f.FechaPago.Value.Year == DateTime.Now.Year)
                .SumAsync(f => f.Total);

            return Ok(new
            {
                TotalFacturas = totalFacturas,
                FacturasPendientes = facturasPendientes,
                FacturasPagadas = facturasPagadas,
                FacturasAnuladas = facturasAnuladas,
                MontoTotalPagado = montoTotal,
                MontoMesActual = montoMesActual
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
