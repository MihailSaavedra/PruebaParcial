namespace AgroFlow.Facturacion.Contracts;

public record CosechaEnProcesoEvent(
    Guid CosechaId,
    Guid AgricultorId,
    string NombreAgricultor,
    string Producto,
    decimal Toneladas,
    DateTime Timestamp
);
