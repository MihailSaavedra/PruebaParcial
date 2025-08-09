using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroFlow.Facturacion.Models;

public class DetalleFactura
{
    [Key]
    public Guid DetalleId { get; set; }

    [Required]
    public Guid FacturaId { get; set; }

    [ForeignKey("FacturaId")]
    public Factura? Factura { get; set; }

    [Required]
    [StringLength(100)]
    public string Concepto { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Cantidad { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrecioUnitario { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [StringLength(10)]
    public string UnidadMedida { get; set; } = "Ton";
}
