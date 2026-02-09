using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    // ===== REQUEST DTOs =====

    public record RequisicionEPPRequestDto(
        [Required(ErrorMessage = "El empleado receptor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado")]
        int IdEmpleadoRecibe,

        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén")]
        int IdAlmacen,

        [MaxLength(500, ErrorMessage = "La justificación no puede exceder 500 caracteres")]
        string? Justificacion,

        [Required(ErrorMessage = "Debe agregar al menos un material")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un material")]
        List<RequisicionEPPDetalleRequestDto> Detalles
    );

    public record RequisicionEPPDetalleRequestDto(
        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material")]
        int IdMaterial,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadSolicitada,

        [MaxLength(20, ErrorMessage = "La talla no puede exceder 20 caracteres")]
        string? TallaSolicitada
    );

    public record AprobarRequisicionRequestDto(
        bool Aprobada,

        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        string? MotivoRechazo
    );

    public record EntregarRequisicionRequestDto(
        [Required(ErrorMessage = "La firma digital es obligatoria")]
        string FirmaDigital,

        string? FotoEvidencia,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones,

        [Required(ErrorMessage = "Debe especificar los detalles de entrega")]
        List<EntregarRequisicionDetalleDto> Detalles
    );

    public record EntregarRequisicionDetalleDto(
        [Required]
        int IdRequisicionDetalle,

        [Required(ErrorMessage = "Debe seleccionar un lote")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un lote válido")]
        int IdCompra,

        [Required(ErrorMessage = "La cantidad entregada es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadEntregada
    );

    // ===== RESPONSE DTOs =====

    public class RequisicionEPPResponseDto
    {
        public int IdRequisicion { get; set; }
        public string NumeroRequisicion { get; set; } = string.Empty;

        // Solicitante
        public string IdUsuarioSolicita { get; set; } = string.Empty;
        public string NombreUsuarioSolicita { get; set; } = string.Empty;

        // Empleado receptor
        public int IdEmpleadoRecibe { get; set; }
        public string NombreEmpleadoRecibe { get; set; } = string.Empty;
        public string NumeroNominaEmpleado { get; set; } = string.Empty;

        // Almacén
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        public string Justificacion { get; set; } = string.Empty;

        // Estado
        public EstadoRequisicion EstadoRequisicion { get; set; }
        public string EstadoTexto => EstadoRequisicion.ToString();
        public string EstadoClase => EstadoRequisicion switch
        {
            EstadoRequisicion.Pendiente => "warning",
            EstadoRequisicion.Aprobada => "info",
            EstadoRequisicion.Rechazada => "danger",
            EstadoRequisicion.Entregada => "success",
            EstadoRequisicion.Cancelada => "secondary",
            _ => "secondary"
        };

        // Aprobación
        public string? IdUsuarioAprueba { get; set; }
        public string? NombreUsuarioAprueba { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? MotivoRechazo { get; set; }

        // Entrega
        public string? IdUsuarioEntrega { get; set; }
        public string? NombreUsuarioEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? FirmaDigital { get; set; }
        public string? FotoEvidencia { get; set; }
        public string? Observaciones { get; set; }

        // Auditoría
        public DateTime FechaSolicitud { get; set; }

        // Detalles
        public List<RequisicionEPPDetalleResponseDto> Detalles { get; set; } = new();

        // Calculados
        public int TotalMateriales => Detalles.Count;
        public decimal TotalCantidadSolicitada => Detalles.Sum(d => d.CantidadSolicitada);
    }

    public class RequisicionEPPDetalleResponseDto
    {
        public int IdRequisicionDetalle { get; set; }
        public int IdRequisicion { get; set; }

        // Material
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }
        public string? TallaSolicitada { get; set; }

        // Entrega
        public int? IdCompra { get; set; }
        public string? DescripcionLote { get; set; }
        public decimal? CantidadEntregada { get; set; }

        // Calculado
        public bool Entregado => CantidadEntregada.HasValue && CantidadEntregada > 0;
    }
}