namespace AgroFlow.Inventario.Contracts;

// Este "record" define la estructura del mensaje que esperamos recibir
public record NuevaCosechaEvent(
    Guid CosechaId,
    string Producto,
    decimal Toneladas,
    DateTime Timestamp
);