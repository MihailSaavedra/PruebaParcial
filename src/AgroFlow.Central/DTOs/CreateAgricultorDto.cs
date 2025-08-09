using System.ComponentModel.DataAnnotations;

namespace AgroFlow.Central.DTOs;

public class CreateAgricultorDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la finca es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre de la finca no puede exceder los 100 caracteres")]
    public string Finca { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria")]
    [StringLength(100, ErrorMessage = "La ubicación no puede exceder los 100 caracteres")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres")]
    public string Correo { get; set; } = string.Empty;
}

public class UpdateAgricultorDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la finca es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre de la finca no puede exceder los 100 caracteres")]
    public string Finca { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria")]
    [StringLength(100, ErrorMessage = "La ubicación no puede exceder los 100 caracteres")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres")]
    public string Correo { get; set; } = string.Empty;
}

