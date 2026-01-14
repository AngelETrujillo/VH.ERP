using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record InventarioRequestDto(
        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        int IdAlmacen,

        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido")]
        int IdMaterial,

        [Required(ErrorMessage = "La existencia es obligatoria")]
        [Range(0, double.MaxValue, ErrorMessage = "La existencia debe ser mayor o igual a 0")]
        decimal Existencia,

        [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
        decimal StockMinimo,

        [Range(0, double.MaxValue, ErrorMessage = "El stock máximo debe ser mayor o igual a 0")]
        decimal StockMaximo,

        [MaxLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
        string UbicacionPasillo
    );

    // DTO para lectura
    public class InventarioResponseDto
    {
        public int IdInventario { get; set; }

        // Información del Almacén
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        // Información del Material
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedidaMaterial { get; set; } = string.Empty;

        // Datos de Inventario
        public decimal Existencia { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public string UbicacionPasillo { get; set; } = string.Empty;
        public DateTime FechaUltimoMovimiento { get; set; }

        // Indicador de estado
        public bool BajoStock => Existencia <= StockMinimo;
        public bool SobreStock => Existencia >= StockMaximo;
    }
}