using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    /// <summary>
    /// DTO para registrar una nueva compra de material EPP
    /// </summary>
    public record CompraEPPRequestDto(
        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido")]
        int IdMaterial,

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        int IdProveedor,

        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        int IdAlmacen,

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        DateTime FechaCompra,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadComprada,

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        decimal PrecioUnitario,

        [MaxLength(50, ErrorMessage = "El número de documento no puede exceder 50 caracteres")]
        string? NumeroDocumento,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones
    );

    /// <summary>
    /// DTO para mostrar información de una compra de material EPP
    /// </summary>
    public class CompraEPPResponseDto
    {
        public int IdCompra { get; set; }

        // Información del Material
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedidaMaterial { get; set; } = string.Empty;

        // Información del Proveedor
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;

        // Información del Almacén
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        // Datos de la Compra
        public DateTime FechaCompra { get; set; }
        public decimal CantidadComprada { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        // Propiedades Calculadas
        public decimal CostoTotal => CantidadComprada * PrecioUnitario;
        public bool TieneDisponible => CantidadDisponible > 0;
        public decimal PorcentajeDisponible => CantidadComprada > 0
            ? Math.Round((CantidadDisponible / CantidadComprada) * 100, 2)
            : 0;
    }

    /// <summary>
    /// DTO simplificado para dropdowns de selección de lote/compra
    /// Usado en el formulario de entregas para seleccionar de qué lote sale el material
    /// </summary>
    public class CompraEPPSimpleDto
    {
        public int IdCompra { get; set; }
        public string Descripcion { get; set; } = string.Empty; // Ej: "Lote #5 - Proveedor X - 50 disponibles"
        public decimal CantidadDisponible { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public DateTime FechaCompra { get; set; }
    }
}