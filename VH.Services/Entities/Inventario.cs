using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Inventarios")]
    public class Inventario
    {
        [Key]
        public int IdInventario { get; set; }
        [Required]
        public int IdAlmacen { get; set; }
        [Required]
        public int IdMaterial { get; set; }
        public decimal Existencia { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        [MaxLength(100)]
        public string UbicacionPasillo { get; set; } = string.Empty;
        public DateTime FechaUltimoMovimiento { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        // ===== PROPIEDADES CALCULADAS (No se guardan en BD) =====
        [NotMapped]
        public bool BajoStock => Existencia <= StockMinimo;
        [NotMapped]
        public bool SobreStock => Existencia >= StockMaximo;
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