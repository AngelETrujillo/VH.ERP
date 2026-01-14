using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record AlmacenRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        string Nombre,

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion,

        [MaxLength(300, ErrorMessage = "El domicilio no puede exceder 300 caracteres")]
        string Domicilio,

        [MaxLength(50, ErrorMessage = "El tipo de ubicación no puede exceder 50 caracteres")]
        string TipoUbicacion,

        bool Activo,

        [Required(ErrorMessage = "El proyecto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proyecto válido")]
        int IdProyecto
    );

    // DTO para lectura
    public class AlmacenResponseDto
    {
        public int IdAlmacen { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Domicilio { get; set; } = string.Empty;
        public string TipoUbicacion { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Información del Proyecto
        public int IdProyecto { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
    }
}