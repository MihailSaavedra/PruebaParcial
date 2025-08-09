using System.ComponentModel.DataAnnotations;

namespace AgroFlow.Facturacion.DTOs;

public class CreateFacturaDto
{
    [Required]
    public Guid CosechaId { get; set; }

    [Required]
    public Guid AgricultorId { get; set; }

    [Required]
    [StringLength(100)]
    public string NombreAgricultor { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Producto { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Las toneladas deben ser mayor a 0")]
    public decimal Toneladas { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio por tonelada debe ser mayor a 0")]
    public decimal PrecioPorTonelada { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
