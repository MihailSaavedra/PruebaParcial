using System.ComponentModel.DataAnnotations;

namespace AgroFlow.Central.Models;

public class Agricultor
{
    [Key] // Marca esta propiedad como la Clave Primaria (Primary Key)
    public Guid AgricultorId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Finca { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    [Required]
    [EmailAddress] // Para validación de formato de email
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Propiedad de navegación: Un agricultor puede tener muchas cosechas
    public ICollection<Cosecha> Cosechas { get; set; } = new List<Cosecha>();
}