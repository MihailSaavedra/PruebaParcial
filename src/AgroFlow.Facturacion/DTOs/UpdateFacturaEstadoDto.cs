using System.ComponentModel.DataAnnotations;

namespace AgroFlow.Facturacion.DTOs;

public class UpdateFacturaEstadoDto
{
    [Required]
    [RegularExpression("^(PENDIENTE|PAGADA|ANULADA)$", ErrorMessage = "Estado debe ser PENDIENTE, PAGADA o ANULADA")]
    public string Estado { get; set; } = string.Empty;

    public DateTime? FechaPago { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
