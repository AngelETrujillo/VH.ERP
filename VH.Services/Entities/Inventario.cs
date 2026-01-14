using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Inventario")]
    public class Inventario
    {
        [Key]
        public int IdInventario { get; set; }

        // Relación con Almacén
        public int IdAlmacen { get; set; }
        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        // Relación con Material
        public int IdMaterial { get; set; }
        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        // Datos de control
        public decimal Existencia { get; set; } // Decimal por si usas metros/litros
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }

        [MaxLength(100)]
        public string UbicacionPasillo { get; set; } = string.Empty; // Ej: "Estante 4-B"

        public DateTime FechaUltimoMovimiento { get; set; }
    }
}