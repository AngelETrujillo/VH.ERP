using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// La vuelta de una herramienta o de un equipo retornable.
    ///
    /// El EPP de consumo se entrega y se acabó: unos guantes no vuelven. Una
    /// herramienta sí, y hasta ahora el sistema no sabía distinguirlos: el
    /// rotomartillo salía del almacén y desaparecía del inventario para siempre,
    /// aunque estuviera a diez metros en manos de alguien.
    ///
    /// Cada devolución apunta a la entrega de la que salió, así que siempre se
    /// puede contestar qué se prestó, quién lo tiene y en qué estado volvió.
    /// </summary>
    [Table("DevolucionesEPP")]
    public class DevolucionEPP
    {
        [Key]
        public int IdDevolucion { get; set; }

        /// <summary>Entrega de la que salió lo que ahora vuelve.</summary>
        [Required]
        public int IdEntrega { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        public DateTime FechaDevolucion { get; set; } = DateTime.Now;

        /// <summary>Almacenista que la recibió de vuelta.</summary>
        [Required]
        public string IdUsuarioRecibe { get; set; } = string.Empty;

        [Required]
        public EstadoDevolucion Estado { get; set; } = EstadoDevolucion.Buena;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdEntrega")]
        public virtual EntregaEPP? Entrega { get; set; }

        [ForeignKey("IdUsuarioRecibe")]
        public virtual Usuario? UsuarioRecibe { get; set; }

        // ===== CALCULADAS =====

        /// <summary>
        /// Lo perdido no regresa al anaquel: no hay nada que volver a guardar.
        /// Lo dañado sí entra, porque sigue siendo del inventario aunque haya que
        /// darle mantenimiento o darlo de baja después.
        /// </summary>
        [NotMapped]
        public bool RegresaAlAlmacen => Estado != EstadoDevolucion.Perdida;
    }

    public enum EstadoDevolucion
    {
        /// <summary>Vuelve entera y se puede volver a prestar.</summary>
        Buena = 0,

        /// <summary>Vuelve golpeada: entra al almacén, pero hay que revisarla.</summary>
        Dañada = 1,

        /// <summary>No vuelve. Se pierde o se queda en la obra.</summary>
        Perdida = 2
    }
}
