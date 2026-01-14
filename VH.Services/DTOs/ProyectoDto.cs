using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para lectura
    public record ProyectoResponseDto(
        int IdProyecto,
        string Nombre,
        string TipoObra,
        DateTime FechaInicio,
        decimal PresupuestoTotal
    );

    // DTO para creación/actualización
    public record ProyectoRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        string Nombre,

        [MaxLength(100, ErrorMessage = "El tipo de obra no puede exceder 100 caracteres")]
        string TipoObra,

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        DateTime FechaInicio,

        [Range(0, double.MaxValue, ErrorMessage = "El presupuesto debe ser mayor o igual a 0")]
        decimal PresupuestoTotal
    );
}