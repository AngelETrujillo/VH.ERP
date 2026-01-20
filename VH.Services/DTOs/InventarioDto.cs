using System;
using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    /// <summary>
    /// DTO para crear o actualizar configuración de inventario.
    /// Nota: La existencia se calcula automáticamente desde las compras y entregas.
    /// Este DTO solo configura los parámetros de control (stock mínimo, máximo, ubicación).
    /// </summary>
    public record InventarioRequestDto(
        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        int IdAlmacen,

        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido")]
        int IdMaterial,

        [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
        decimal StockMinimo,

        [Range(0, double.MaxValue, ErrorMessage = "El stock máximo debe ser mayor o igual a 0")]
        decimal StockMaximo,

        [MaxLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
        string? UbicacionPasillo
    );

    /// <summary>
    /// DTO para mostrar información de inventario con alertas de stock
    /// </summary>
    public class InventarioResponseDto
    {
        public int IdInventario { get; set; }

        // Información del Almacén
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

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

        // ===== INDICADORES DE ALERTA =====

        /// <summary>
        /// True si la existencia está en o por debajo del stock mínimo
        /// </summary>
        public bool BajoStock => Existencia <= StockMinimo;

        /// <summary>
        /// True si la existencia está en o por encima del stock máximo
        /// </summary>
        public bool SobreStock => Existencia >= StockMaximo && StockMaximo > 0;

        /// <summary>
        /// True si no hay existencias
        /// </summary>
        public bool SinStock => Existencia <= 0;

        /// <summary>
        /// Estado del inventario: "SinStock", "Bajo", "Normal", "Excedido"
        /// </summary>
        public string EstadoStock
        {
            get
            {
                if (SinStock) return "SinStock";
                if (BajoStock) return "Bajo";
                if (SobreStock) return "Excedido";
                return "Normal";
            }
        }

        /// <summary>
        /// Clase CSS para mostrar el color apropiado según el estado
        /// </summary>
        public string ClaseEstado
        {
            get
            {
                return EstadoStock switch
                {
                    "SinStock" => "danger",
                    "Bajo" => "warning",
                    "Excedido" => "info",
                    _ => "success"
                };
            }
        }

        /// <summary>
        /// Mensaje descriptivo del estado para mostrar al usuario
        /// </summary>
        public string MensajeAlerta
        {
            get
            {
                return EstadoStock switch
                {
                    "SinStock" => "⚠️ SIN STOCK - Requiere reabastecimiento inmediato",
                    "Bajo" => "⚠️ Stock bajo - Considere reabastecer",
                    "Excedido" => "ℹ️ Stock excedido - Considere redistribuir",
                    _ => "✅ Stock normal"
                };
            }
        }
    }

    /// <summary>
    /// DTO para resumen de alertas de inventario (Dashboard)
    /// </summary>
    public class AlertaInventarioDto
    {
        public int IdInventario { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string NombreAlmacen { get; set; } = string.Empty;
        public decimal Existencia { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
        public string MensajeAlerta { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
    }
}