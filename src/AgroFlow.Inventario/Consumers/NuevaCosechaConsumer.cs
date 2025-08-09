// Importamos las librerías necesarias
using AgroFlow.Inventario.Contracts; // Para poder usar nuestro 'NuevaCosechaEvent'
using AgroFlow.Inventario.Data;     // Para poder usar nuestro 'InventarioDbContext'
using MassTransit;                  // La librería principal para la mensajería
using Microsoft.EntityFrameworkCore;  // Para usar 'ExecuteUpdateAsync'

namespace AgroFlow.Inventario.Consumers;

/// <summary>
/// Esta clase es nuestro consumidor. Implementa la interfaz IConsumer<T>,
/// donde T es el tipo de mensaje que queremos consumir. MassTransit se encargará
/// de que, cuando un mensaje de tipo 'NuevaCosechaEvent' llegue a la cola correcta,
/// se cree una instancia de esta clase y se llame al método 'Consume'.
/// </summary>
public class NuevaCosechaConsumer : IConsumer<NuevaCosechaEvent>
{
    // Propiedades privadas para guardar las dependencias que necesitamos
    private readonly InventarioDbContext _dbContext;
    private readonly ILogger<NuevaCosechaConsumer> _logger;

    // --- El Constructor ---
    // Usamos Inyección de Dependencias para recibir las herramientas que necesitamos.
    // Cuando MassTransit cree esta clase, automáticamente le pasará el DbContext y el Logger.
    public NuevaCosechaConsumer(InventarioDbContext dbContext, ILogger<NuevaCosechaConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // --- El Método Consume ---
    // Este es el método que se ejecuta cada vez que se recibe un mensaje.
    public async Task Consume(ConsumeContext<NuevaCosechaEvent> context)
    {
        // 1. Extraemos el mensaje del 'contexto' de MassTransit.
        var evento = context.Message;
        _logger.LogInformation("--> Evento de nueva cosecha recibido: ID {CosechaId}", evento.CosechaId);

        // 2. Aplicamos la lógica de negocio especificada en el PDF.
        // Fórmula: 5kg de semilla por tonelada + 2kg de fertilizante por tonelada
        var semillaNecesaria = (int)(evento.Toneladas * 5);
        var fertilizanteNecesario = (int)(evento.Toneladas * 2);

        _logger.LogInformation("Calculando insumos necesarios: {Semilla}kg de semilla y {Fertilizante}kg de fertilizante.", semillaNecesaria, fertilizanteNecesario);

        try
        {
            // 3. Actualizamos la base de datos de forma eficiente.
            // En lugar de traer los datos, modificarlos y guardarlos (3 pasos),
            // 'ExecuteUpdateAsync' envía una única instrucción UPDATE a la base de datos.
            // Es más rápido y seguro para operaciones de este tipo.
            await _dbContext.Insumos
                .Where(insumo => insumo.NombreInsumo == "Semilla Arroz L-23")
                .ExecuteUpdateAsync(updates => updates.SetProperty(prop => prop.Stock, prop => prop.Stock - semillaNecesaria));

            await _dbContext.Insumos
                .Where(insumo => insumo.NombreInsumo == "Fertilizante N-PK")
                .ExecuteUpdateAsync(updates => updates.SetProperty(prop => prop.Stock, prop => prop.Stock - fertilizanteNecesario));

            _logger.LogInformation("<-- Stock de insumos actualizado correctamente para la cosecha {CosechaId}.", evento.CosechaId);

            // Futuro Paso: Aquí es donde publicarías un nuevo evento, como 'InventarioAjustado',
            // para notificar a otros servicios (como el de facturación) que tu trabajo ha terminado.
            // await context.Publish(new InventarioAjustadoEvent(...));
        }
        catch (Exception ex)
        {
            // Es crucial capturar errores. ¿Qué pasa si el insumo no existe o la DB está caída?
            _logger.LogError(ex, "Error al actualizar el stock para la cosecha {CosechaId}", evento.CosechaId);
            // Lanzar la excepción de nuevo le dice a MassTransit que el mensaje falló
            // y puede intentar procesarlo de nuevo o moverlo a una cola de errores.
            throw;
        }
    }
}