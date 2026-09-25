using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // ===== REQUEST =====

    /// <summary>
    /// Alta de una compra completa: la factura del proveedor con todos sus renglones.
    /// Antes había que capturar una "compra" por material, sin nada que las uniera.
    /// </summary>
    public record CompraEPPRequestDto(
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        int IdProveedor,

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        DateTime FechaCompra,

        [MaxLength(50, ErrorMessage = "El número de documento no puede exceder 50 caracteres")]
        string? NumeroDocumento,

        [MaxLength(36, ErrorMessage = "El UUID del CFDI no puede exceder 36 caracteres")]
        string? UuidCFDI,

        [Range(0, double.MaxValue, ErrorMessage = "El IVA no puede ser negativo")]
        decimal Iva,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones,

        [Required(ErrorMessage = "Debe agregar al menos un material")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un material")]
        List<CompraEPPDetalleRequestDto> Detalles,

        [MaxLength(3)]
        string Moneda = "MXN"
    );

    public record CompraEPPDetalleRequestDto(
        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido")]
        int IdMaterial,

        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        int IdAlmacen,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal Cantidad,

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        decimal PrecioUnitario,

        [MaxLength(20, ErrorMessage = "La talla no puede exceder 20 caracteres")]
        string? Talla,

        DateTime? FechaCaducidad
    );

    /// <summary>Edición de la cabecera. Los renglones ya consumidos no se tocan aquí.</summary>
    public record CompraEPPUpdateDto(
        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        DateTime FechaCompra,

        [MaxLength(50)]
        string? NumeroDocumento,

        [MaxLength(36)]
        string? UuidCFDI,

        [Range(0, double.MaxValue)]
        decimal Iva,

        [MaxLength(500)]
        string? Observaciones
    );

    // ===== RESPONSE =====

    public class CompraEPPResponseDto
    {
        public int IdCompra { get; set; }

        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;

        public DateTime FechaCompra { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? UuidCFDI { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = "MXN";

        public string Observaciones { get; set; } = string.Empty;

        public List<CompraEPPDetalleResponseDto> Detalles { get; set; } = new();

        // Calculados
        public int TotalRenglones => Detalles.Count;
        public decimal TotalUnidades => Detalles.Sum(d => d.Cantidad);
        public bool TieneDisponible => Detalles.Any(d => d.CantidadDisponible > 0);

        /// <summary>Se puede cancelar mientras ningún renglón se haya consumido.</summary>
        public bool PuedeCancelarse => Detalles.All(d => d.CantidadDisponible == d.Cantidad);

        /// <summary>Resumen para listados: "Cascos, Guantes y 2 más".</summary>
        public string ResumenMateriales
        {
            get
            {
                if (Detalles.Count == 0) return "Sin renglones";
                if (Detalles.Count <= 2) return string.Join(", ", Detalles.Select(d => d.NombreMaterial));
                return $"{Detalles[0].NombreMaterial}, {Detalles[1].NombreMaterial} y {Detalles.Count - 2} más";
            }
        }
    }

    public class CompraEPPDetalleResponseDto
    {
        public int IdCompraDetalle { get; set; }
        public int IdCompra { get; set; }

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedidaMaterial { get; set; } = string.Empty;

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? Talla { get; set; }
        public DateTime? FechaCaducidad { get; set; }

        // Calculados
        public decimal CostoTotal => Cantidad * PrecioUnitario;
        public decimal CantidadEntregada => Cantidad - CantidadDisponible;
        public bool TieneDisponible => CantidadDisponible > 0;
        public bool EstaCaducado => FechaCaducidad.HasValue && FechaCaducidad.Value.Date < DateTime.Today;
        public decimal PorcentajeDisponible => Cantidad > 0
            ? Math.Round((CantidadDisponible / Cantidad) * 100, 2)
            : 0;
    }

    /// <summary>
    /// Lote para el selector de entrega. Un lote es un renglón de compra, así que
    /// el identificador es IdCompraDetalle.
    /// </summary>
    public class CompraEPPSimpleDto
    {
        public int IdCompraDetalle { get; set; }
        public int IdCompra { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string? Talla { get; set; }
    }

    /// <summary>Un renglón del historial de precios de un material.</summary>
    public class HistorialPrecioDto
    {
        public int IdCompraDetalle { get; set; }
        public int IdCompra { get; set; }
        public DateTime FechaCompra { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreProveedor { get; set; } = string.Empty;
        public string NombreAlmacen { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CostoTotal => Cantidad * PrecioUnitario;
    }
}
