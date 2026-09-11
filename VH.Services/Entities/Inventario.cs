using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Existencia de un material en un almacén.
    ///
    /// <see cref="Existencia"/> es lo que hay físicamente y
    /// <see cref="Comprometido"/> lo que ya está apartado para requisiciones
    /// autorizadas. Lo que puede prometerse a alguien nuevo es la diferencia:
    /// sin esa distinción, dos obreros que piden el último casco el mismo día
    /// reciben ambos un "hay stock" y el segundo se queda esperando sin que
    /// nadie compre nada.
    /// </summary>
    [Table("Inventarios")]
    public class Inventario
    {
        [Key]
        public int IdInventario { get; set; }
        [Required]
        public int IdAlmacen { get; set; }
        [Required]
        public int IdMaterial { get; set; }

        /// <summary>Lo que hay en el anaquel.</summary>
        public decimal Existencia { get; set; }

        /// <summary>Parte de la existencia ya apartada para renglones autorizados.</summary>
        public decimal Comprometido { get; set; }

        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        [MaxLength(100)]
        public string UbicacionPasillo { get; set; } = string.Empty;
        public DateTime FechaUltimoMovimiento { get; set; }

        /// <summary>
        /// Control de concurrencia. Reservar y surtir son lectura-modificación-
        /// escritura sobre la misma fila; sin esto dos almacenistas simultáneos
        /// pueden apartar la misma pieza.
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        // ===== PROPIEDADES CALCULADAS (No se guardan en BD) =====

        /// <summary>Lo que todavía puede prometerse a alguien.</summary>
        [NotMapped]
        public decimal Disponible => Existencia - Comprometido;

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
