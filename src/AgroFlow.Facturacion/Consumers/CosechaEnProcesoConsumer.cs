using AgroFlow.Facturacion.Contracts;
using AgroFlow.Facturacion.Data;
using AgroFlow.Facturacion.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Facturacion.Consumers;

public class CosechaEnProcesoConsumer : IConsumer<CosechaEnProcesoEvent>
{
    private readonly FacturacionDbContext _dbContext;
    private readonly ILogger<CosechaEnProcesoConsumer> _logger;

    public CosechaEnProcesoConsumer(FacturacionDbContext dbContext, ILogger<CosechaEnProcesoConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CosechaEnProcesoEvent> context)
    {
        var evento = context.Message;
        _logger.LogInformation("--> Evento de cosecha en proceso recibido: ID {CosechaId}", evento.CosechaId);

        try
        {
            // Verificar si ya existe una factura para esta cosecha
            var facturaExistente = await _dbContext.Facturas
                .FirstOrDefaultAsync(f => f.CosechaId == evento.CosechaId);

            if (facturaExistente != null)
            {
                _logger.LogWarning("Ya existe una factura para la cosecha {CosechaId}", evento.CosechaId);
                return;
            }

            // Calcular precios según el producto (lógica de negocio)
            var precioPorTonelada = CalcularPrecioPorTonelada(evento.Producto);
            var subtotal = evento.Toneladas * precioPorTonelada;
            var porcentajeImpuesto = 19.0m; // IVA 19%
            var montoImpuesto = subtotal * (porcentajeImpuesto / 100);
            var total = subtotal + montoImpuesto;

            // Crear la factura
            var factura = new Factura
            {
                FacturaId = Guid.NewGuid(),
                CosechaId = evento.CosechaId,
                AgricultorId = evento.AgricultorId,
                NombreAgricultor = evento.NombreAgricultor,
                Producto = evento.Producto,
                Toneladas = evento.Toneladas,
                PrecioPorTonelada = precioPorTonelada,
                Subtotal = subtotal,
                PorcentajeImpuesto = porcentajeImpuesto,
                MontoImpuesto = montoImpuesto,
                Total = total,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow
            };

            // Crear detalle de factura
            var detalle = new DetalleFactura
            {
                DetalleId = Guid.NewGuid(),
                FacturaId = factura.FacturaId,
                Concepto = $"Venta de {evento.Producto}",
                Cantidad = evento.Toneladas,
                PrecioUnitario = precioPorTonelada,
                Subtotal = subtotal,
                UnidadMedida = "Ton"
            };

            _dbContext.Facturas.Add(factura);
            _dbContext.DetallesFactura.Add(detalle);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("<-- Factura creada exitosamente para la cosecha {CosechaId}. Total: ${Total}", 
                evento.CosechaId, total);

            // Publicar evento de factura creada (para otros servicios)
            await context.Publish(new FacturaCreatedEvent(
                factura.FacturaId,
                factura.CosechaId,
                factura.AgricultorId,
                factura.Total,
                DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear factura para la cosecha {CosechaId}", evento.CosechaId);
            throw;
        }
    }

    private static decimal CalcularPrecioPorTonelada(string producto)
    {
        // Lógica de precios por producto (esto podría venir de una tabla de configuración)
        return producto.ToUpper() switch
        {
            "ARROZ" => 2500000m, // $2,500,000 COP por tonelada
            "MAIZ" => 1800000m,  // $1,800,000 COP por tonelada
            "SOYA" => 3200000m,  // $3,200,000 COP por tonelada
            "CAFE" => 8500000m,  // $8,500,000 COP por tonelada
            _ => 2000000m        // Precio por defecto
        };
    }
}

// Evento que se publica cuando se crea una factura
public record FacturaCreatedEvent(
    Guid FacturaId,
    Guid CosechaId,
    Guid AgricultorId,
    decimal Total,
    DateTime Timestamp
);
