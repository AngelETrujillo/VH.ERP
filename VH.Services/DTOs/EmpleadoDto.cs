using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record EmpleadoRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        string Nombre,

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MaxLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
        string ApellidoPaterno,

        [MaxLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
        string ApellidoMaterno,

        [Required(ErrorMessage = "El número de nómina es obligatorio")]
        [MaxLength(20, ErrorMessage = "El número de nómina no puede exceder 20 caracteres")]
        string NumeroNomina,

        [Required(ErrorMessage = "El puesto es obligatorio")]
        [MaxLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
        string Puesto,

        bool Activo,

        [Required(ErrorMessage = "El proyecto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proyecto válido")]
        int IdProyecto
    );

    // DTO para lectura
    public class EmpleadoResponseDto
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();
        public string NumeroNomina { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Información del Proyecto
        public int IdProyecto { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
    }
}