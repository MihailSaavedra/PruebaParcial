using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroFlow.Inventario.Models;

public class Insumo
{
    [Key]
    public Guid InsumoId { get; set; }

    [Required]
    [StringLength(100)]
    public string NombreInsumo { get; set; } = string.Empty;

    [Required]
    public int Stock { get; set; } = 0;

    [StringLength(10)]
    public string UnidadMedida { get; set; } = "kg";

    [StringLength(30)]
    public string? Categoria { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UltimaActualizacion { get; set; }
}