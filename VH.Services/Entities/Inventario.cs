using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Control de inventario por Material y Almacén.
    /// La existencia representa el total de unidades disponibles de ese material
    /// en ese almacén (suma de todos los lotes/compras menos las entregas).
    /// </summary>
    [Table("Inventarios")]
    public class Inventario
    {
        [Key]
        public int IdInventario { get; set; }

        /// <summary>
        /// Almacén donde se encuentra el material
        /// </summary>
        [Required]
        public int IdAlmacen { get; set; }

        /// <summary>
        /// Material del que se lleva el control
        /// </summary>
        [Required]
        public int IdMaterial { get; set; }

        /// <summary>
        /// Cantidad total disponible en este almacén.
        /// Se actualiza automáticamente con compras (+) y entregas (-).
        /// Es la suma de CantidadDisponible de todas las CompraEPP de este material en este almacén.
        /// </summary>
        public decimal Existencia { get; set; }

        /// <summary>
        /// Cantidad mínima antes de generar alerta de reabastecimiento
        /// </summary>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Cantidad máxima recomendada (alerta si se excede)
        /// </summary>
        public decimal StockMaximo { get; set; }

        /// <summary>
        /// Ubicación física dentro del almacén (ej: "Estante 4-B", "Pasillo 2")
        /// </summary>
        [MaxLength(100)]
        public string UbicacionPasillo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha del último movimiento (compra o entrega)
        /// </summary>
        public DateTime FechaUltimoMovimiento { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        // ===== PROPIEDADES CALCULADAS (No se guardan en BD) =====

        /// <summary>
        /// Indica si el stock está por debajo del mínimo
        /// </summary>
        [NotMapped]
        public bool BajoStock => Existencia <= StockMinimo;

        /// <summary>
        /// Indica si el stock excede el máximo
        /// </summary>
        [NotMapped]
        public bool SobreStock => Existencia >= StockMaximo;

        /// <summary>
        /// Estado del inventario para mostrar en UI
        /// </summary>
        [NotMapped]
        public string EstadoStock
        {
            get
            {
                if (Existencia <= 0) return "SinStock";
                if (BajoStock) return "Bajo";
                if (SobreStock) return "Excedido";
                return "Normal";
            }
        }
    }
}