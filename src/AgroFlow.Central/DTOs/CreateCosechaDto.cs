using System.ComponentModel.DataAnnotations;

namespace AgroFlow.Central.DTOs;

public class CreateCosechaDto
{
    [Required(ErrorMessage = "El ID del agricultor es obligatorio")]
    public Guid AgricultorId { get; set; }

    [Required(ErrorMessage = "El producto es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre del producto no puede exceder los 50 caracteres")]
    public string Producto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Las toneladas son obligatorias")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Las toneladas deben ser mayor a 0")]
    public decimal Toneladas { get; set; }
}

public class UpdateCosechaDto
{
    [Required(ErrorMessage = "El producto es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre del producto no puede exceder los 50 caracteres")]
    public string Producto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Las toneladas son obligatorias")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Las toneladas deben ser mayor a 0")]
    public decimal Toneladas { get; set; }

    [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
    public string Estado { get; set; } = "REGISTRADA";
}

public class UpdateEstadoCosechaDto
{
    [Required(ErrorMessage = "El estado es obligatorio")]
    [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
    public string Estado { get; set; } = string.Empty;
}

