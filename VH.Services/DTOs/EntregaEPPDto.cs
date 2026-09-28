using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    /// <summary>
    /// DTO para registrar una nueva entrega de EPP.
    /// Ahora requiere seleccionar el lote/compra específico de donde sale el material.
    /// </summary>
    public record EntregaEPPRequestDto(
        [Required(ErrorMessage = "El empleado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido")]
        int IdEmpleado,

        [Required(ErrorMessage = "El lote es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un lote válido")]
        int IdCompraDetalle,

        [Required(ErrorMessage = "La fecha de entrega es obligatoria")]
        DateTime FechaEntrega,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadEntregada,

        [MaxLength(20, ErrorMessage = "La talla no puede exceder 20 caracteres")]
        string? TallaEntregada,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones
    );

    /// <summary>
    /// DTO para mostrar información de una entrega de EPP.
    /// Incluye información del lote/compra de donde salió el material.
    /// </summary>
    public class EntregaEPPResponseDto
    {
        public int IdEntrega { get; set; }
        public DateTime FechaEntrega { get; set; }
        public decimal CantidadEntregada { get; set; }
        public string TallaEntregada { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        // Información del Empleado. Viene en cero cuando la salida no fue a una
        // persona sino a una obra; para saberlo se usa EsConsumoDeObra.
        public int IdEmpleado { get; set; }
        public string NombreCompletoEmpleado { get; set; } = string.Empty;
        public string NumeroNominaEmpleado { get; set; } = string.Empty;

        // Destino de obra, cuando el material se consumió en la construcción en
        // vez de entregarse a alguien. Sin estos campos el listado de entregas
        // mostraba esas salidas con la columna del trabajador en blanco.
        public int? IdProyectoDestino { get; set; }
        public string? NombreProyectoDestino { get; set; }
        public int? IdConceptoPartida { get; set; }
        public string? DescripcionPartida { get; set; }

        public bool EsConsumoDeObra => IdProyectoDestino.HasValue;

        /// <summary>A quién o a qué se fue: la persona, o la obra y su partida.</summary>
        public string Destino => EsConsumoDeObra
            ? (DescripcionPartida ?? NombreProyectoDestino ?? "Cargo a obra")
            : NombreCompletoEmpleado;

        // Información del Lote/Compra
        public int IdCompraDetalle { get; set; }
        public int IdCompra { get; set; }

        // Información del Material (obtenida desde la Compra)
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedidaMaterial { get; set; } = string.Empty;

        // Información del Proveedor (obtenida desde la Compra)
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;

        // Información del Almacén (obtenida desde la Compra)
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        // Información adicional del lote
        public decimal PrecioUnitarioCompra { get; set; }
        public decimal CostoTotalEntrega => CantidadEntregada * PrecioUnitarioCompra;
    }
}