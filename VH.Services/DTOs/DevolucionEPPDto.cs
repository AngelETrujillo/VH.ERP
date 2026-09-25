using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    /// <summary>
    /// Una herramienta que salió del almacén y todavía no vuelve.
    /// </summary>
    public class PrestamoDto
    {
        public int IdEntrega { get; set; }
        public DateTime FechaEntrega { get; set; }

        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string NumeroNomina { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public string? Talla { get; set; }

        public int IdCompraDetalle { get; set; }
        public string? LoteProveedor { get; set; }

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        public decimal CantidadEntregada { get; set; }
        public decimal CantidadDevuelta { get; set; }

        /// <summary>Lo que esa persona todavía tiene en su poder.</summary>
        public decimal Pendiente => Math.Max(0, CantidadEntregada - CantidadDevuelta);

        public int DiasFuera => (int)(DateTime.Today - FechaEntrega.Date).TotalDays;
    }

    public record RegistrarDevolucionRequestDto(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la entrega")]
        int IdEntrega,

        [Required(ErrorMessage = "Indique cuánto vuelve")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal Cantidad,

        [Required(ErrorMessage = "Indique en qué estado vuelve")]
        EstadoDevolucion Estado,

        [MaxLength(500)]
        string? Observaciones
    );

    public class DevolucionResponseDto
    {
        public int IdDevolucion { get; set; }
        public int IdEntrega { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaDevolucion { get; set; }

        public EstadoDevolucion Estado { get; set; }
        public string EstadoTexto => Estado switch
        {
            EstadoDevolucion.Buena => "En buen estado",
            EstadoDevolucion.Dañada => "Dañada",
            EstadoDevolucion.Perdida => "Perdida",
            _ => Estado.ToString()
        };
        public string EstadoClase => Estado switch
        {
            EstadoDevolucion.Buena => "success",
            EstadoDevolucion.Dañada => "warning",
            EstadoDevolucion.Perdida => "danger",
            _ => "secondary"
        };

        public string? Observaciones { get; set; }
        public string NombreUsuarioRecibe { get; set; } = string.Empty;

        public string NombreEmpleado { get; set; } = string.Empty;
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
    }
}
