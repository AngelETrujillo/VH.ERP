using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    // ===== REQUEST DTOs =====

    public record RequisicionEPPRequestDto(
        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén")]
        int IdAlmacen,

        [MaxLength(500, ErrorMessage = "La justificación no puede exceder 500 caracteres")]
        string? Justificacion,

        [Required(ErrorMessage = "Debe agregar al menos un material")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un material")]
        List<RequisicionEPPDetalleRequestDto> Detalles,

        DateTime? FechaRequerida = null
    );

    public record RequisicionEPPDetalleRequestDto(
        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material")]
        int IdMaterial,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadSolicitada,

        [MaxLength(20, ErrorMessage = "La talla no puede exceder 20 caracteres")]
        string? TallaSolicitada,

        /// <summary>Persona que recibe. Nulo para consumibles que se cargan a la obra.</summary>
        int? IdEmpleadoDestino = null,

        int? IdProyectoDestino = null,

        int? IdConceptoPartida = null
    );

    /// <summary>Autoriza o rechaza renglones concretos. Si no se indican, aplica a todos.</summary>
    public record AprobarRequisicionRequestDto(
        bool Aprobada,

        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        string? MotivoRechazo,

        List<int>? IdsRenglones = null
    );

    /// <summary>
    /// Entrega de todo lo que recibe UNA persona, con su firma. Un documento con
    /// renglones para varios obreros se surte con una petición por obrero.
    /// </summary>
    public record EntregarRequisicionRequestDto(
        [Required(ErrorMessage = "Debe indicar el empleado que recibe")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido")]
        int IdEmpleado,

        [Required(ErrorMessage = "La firma digital es obligatoria")]
        string FirmaDigital,

        string? FotoEvidencia,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones,

        [Required(ErrorMessage = "Debe especificar los materiales a entregar")]
        List<EntregarRequisicionDetalleDto> Detalles
    );

    public record EntregarRequisicionDetalleDto(
        [Required]
        int IdRequisicionDetalle,

        [Required(ErrorMessage = "Debe seleccionar un lote")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un lote válido")]
        int IdCompraDetalle,

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

        // Almacén
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        public string Justificacion { get; set; } = string.Empty;
        public DateTime? FechaRequerida { get; set; }

        // Estado (derivado de los renglones)
        public EstadoRequisicion EstadoRequisicion { get; set; }
        public string EstadoTexto => EstadoRequisicion switch
        {
            EstadoRequisicion.Pendiente => "Pendiente",
            EstadoRequisicion.Aprobada => "Aprobada",
            EstadoRequisicion.Rechazada => "Rechazada",
            EstadoRequisicion.Entregada => "Entregada",
            EstadoRequisicion.Cancelada => "Cancelada",
            EstadoRequisicion.Parcial => "Parcialmente entregada",
            _ => EstadoRequisicion.ToString()
        };
        public string EstadoClase => EstadoRequisicion switch
        {
            EstadoRequisicion.Pendiente => "warning",
            EstadoRequisicion.Aprobada => "info",
            EstadoRequisicion.Rechazada => "danger",
            EstadoRequisicion.Entregada => "success",
            EstadoRequisicion.Cancelada => "secondary",
            EstadoRequisicion.Parcial => "primary",
            _ => "secondary"
        };

        // Autorización
        public string? IdUsuarioAprueba { get; set; }
        public string? NombreUsuarioAprueba { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? MotivoRechazo { get; set; }

        // Auditoría
        public DateTime FechaSolicitud { get; set; }

        public List<RequisicionEPPDetalleResponseDto> Detalles { get; set; } = new();

        /// <summary>Firmas recogidas, una por persona que recibió.</summary>
        public List<RequisicionEntregaResponseDto> Entregas { get; set; } = new();

        // ===== CALCULADOS =====

        public int TotalMateriales => Detalles.Count;
        public decimal TotalCantidadSolicitada => Detalles.Sum(d => d.CantidadSolicitada);

        /// <summary>
        /// Renglones que todavía esperan que alguien los autorice o los rechace. La
        /// autorización es por renglón, así que un documento ya aprobado en parte
        /// puede seguir teniéndolos.
        /// </summary>
        public int RenglonesPorDecidir => Detalles.Count(d => d.EstadoRenglon == EstadoRenglonRequisicion.Solicitado);

        public bool TieneRenglonesPorDecidir => RenglonesPorDecidir > 0;

        /// <summary>Personas distintas que reciben algo en este documento.</summary>
        public int TotalEmpleados => Detalles
            .Where(d => d.IdEmpleadoDestino.HasValue)
            .Select(d => d.IdEmpleadoDestino!.Value)
            .Distinct()
            .Count();

        public int RenglonesSurtidos => Detalles.Count(d => d.Entregado);
        public int RenglonesPendientes => Detalles.Count(d => d.EstaPendiente);

        /// <summary>Resumen para listados: "Juan Martinez, Victor Garza".</summary>
        public string ResumenDestinatarios
        {
            get
            {
                var nombres = Detalles
                    .Where(d => d.IdEmpleadoDestino.HasValue)
                    .Select(d => d.NombreEmpleadoDestino)
                    .Distinct()
                    .ToList();

                if (nombres.Count == 0) return "Cargo a obra";
                if (nombres.Count <= 2) return string.Join(", ", nombres);
                return $"{nombres[0]}, {nombres[1]} y {nombres.Count - 2} más";
            }
        }

        /// <summary>Empleados con renglones autorizados que aún no han firmado.</summary>
        public List<int> EmpleadosPorSurtir => Detalles
            .Where(d => d.IdEmpleadoDestino.HasValue && d.EstaPendiente
                        && d.EstadoRenglon == EstadoRenglonRequisicion.Reservado)
            .Select(d => d.IdEmpleadoDestino!.Value)
            .Distinct()
            .ToList();
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

        // Destino
        public int? IdEmpleadoDestino { get; set; }
        public string NombreEmpleadoDestino { get; set; } = string.Empty;
        public string NumeroNominaDestino { get; set; } = string.Empty;
        public int? IdProyectoDestino { get; set; }
        public string? NombreProyectoDestino { get; set; }
        public int? IdConceptoPartida { get; set; }
        public string? DescripcionPartida { get; set; }

        // Estado
        public EstadoRenglonRequisicion EstadoRenglon { get; set; }
        public string EstadoRenglonTexto => EstadoRenglon switch
        {
            EstadoRenglonRequisicion.Solicitado => "Solicitado",
            EstadoRenglonRequisicion.Autorizado => "Autorizado",
            EstadoRenglonRequisicion.Rechazado => "Rechazado",
            EstadoRenglonRequisicion.Surtido => "Surtido",
            EstadoRenglonRequisicion.Cancelado => "Cancelado",
            _ => EstadoRenglon.ToString()
        };
        public string EstadoRenglonClase => EstadoRenglon switch
        {
            EstadoRenglonRequisicion.Solicitado => "warning",
            EstadoRenglonRequisicion.Autorizado => "info",
            EstadoRenglonRequisicion.Rechazado => "danger",
            EstadoRenglonRequisicion.Surtido => "success",
            EstadoRenglonRequisicion.Cancelado => "secondary",
            _ => "secondary"
        };
        public string? MotivoRechazo { get; set; }

        // Entrega
        public int? IdCompraDetalle { get; set; }
        public string? DescripcionLote { get; set; }
        public decimal? CantidadEntregada { get; set; }

        /// <summary>Firma que amparó este renglón, cuando ya se surtió.</summary>
        public int? IdRequisicionEntrega { get; set; }

        // Calculados
        public bool Entregado => EstadoRenglon == EstadoRenglonRequisicion.Surtido;
        public bool RequiereFirma => IdEmpleadoDestino.HasValue;
        public bool EstaPendiente =>
            EstadoRenglon != EstadoRenglonRequisicion.Rechazado &&
            EstadoRenglon != EstadoRenglonRequisicion.Cancelado &&
            EstadoRenglon != EstadoRenglonRequisicion.Surtido;

        /// <summary>A quién va: la persona, o la obra/partida cuando no hay persona.</summary>
        public string Destino => IdEmpleadoDestino.HasValue
            ? NombreEmpleadoDestino
            : (DescripcionPartida ?? NombreProyectoDestino ?? "Cargo a obra");
    }

    public class RequisicionEntregaResponseDto
    {
        public int IdRequisicionEntrega { get; set; }
        public int IdRequisicion { get; set; }

        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string NumeroNomina { get; set; } = string.Empty;

        public DateTime FechaEntrega { get; set; }
        public string IdUsuarioEntrega { get; set; } = string.Empty;
        public string NombreUsuarioEntrega { get; set; } = string.Empty;

        public string FirmaDigital { get; set; } = string.Empty;
        public string? FotoEvidencia { get; set; }
        public string? Observaciones { get; set; }
    }
}
