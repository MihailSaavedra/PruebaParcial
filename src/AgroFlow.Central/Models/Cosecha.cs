using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroFlow.Central.Models;

public class Cosecha
{
    [Key]
    public Guid CosechaId { get; set; }

    [Required]
    public Guid AgricultorId { get; set; } // Clave foránea

    [ForeignKey("AgricultorId")] // Establece la relación
    public Agricultor? Agricultor { get; set; }

    [Required]
    [StringLength(50)]
    public string Producto { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Toneladas { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "REGISTRADA"; // Valor por defecto

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public Guid? FacturaId { get; set; } // Nullable, se llena después
}