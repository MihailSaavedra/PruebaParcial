using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroFlow.Facturacion.Models;

public class Factura
{
    [Key]
    public Guid FacturaId { get; set; }

    [Required]
    public Guid CosechaId { get; set; } // Referencia a la cosecha

    [Required]
    public Guid AgricultorId { get; set; } // Referencia al agricultor

    [Required]
    [StringLength(100)]
    public string NombreAgricultor { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Producto { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Toneladas { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrecioPorTonelada { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal PorcentajeImpuesto { get; set; } = 19.0m; // IVA 19%

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontoImpuesto { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, PAGADA, ANULADA

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaPago { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    // Propiedad de navegación
    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
}
