using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record EntregaEPPRequestDto(
        [Required(ErrorMessage = "El empleado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido")]
        int IdEmpleado,

        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido")]
        int IdMaterial,

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        int IdProveedor,

        [Required(ErrorMessage = "La fecha de entrega es obligatoria")]
        DateTime FechaEntrega,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadEntregada,

        [MaxLength(20, ErrorMessage = "La talla no puede exceder 20 caracteres")]
        string TallaEntregada,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string Observaciones
    );

    // DTO para lectura - Versión simplificada para evitar referencias circulares
    public class EntregaEPPResponseDto
    {
        public int IdEntrega { get; set; }
        public DateTime FechaEntrega { get; set; }
        public decimal CantidadEntregada { get; set; }
        public string TallaEntregada { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        // Información resumida del Empleado
        public int IdEmpleado { get; set; }
        public string NombreCompletoEmpleado { get; set; } = string.Empty;
        public string NumeroNominaEmpleado { get; set; } = string.Empty;

        // Información resumida del Material
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedidaMaterial { get; set; } = string.Empty;

        // Información resumida del Proveedor
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
    }
}