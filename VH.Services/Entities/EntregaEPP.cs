using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("EntregasEPP")]
    public class EntregaEPP
    {
        [Key]
        public int IdEntrega { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====

        /// <summary>
        /// Quién recibe, cuando el material va a una persona.
        ///
        /// Opcional desde que el almacén puede despachar a una obra: 20 kg de
        /// silicón que se van a la barda no los recibe nadie en particular, y
        /// exigir un nombre obligaba a que alguien firmara por material que nunca
        /// tuvo en las manos.
        /// </summary>
        public int? IdEmpleado { get; set; }

        /// <summary>Obra a la que se carga el consumo, cuando no va a una persona.</summary>
        public int? IdProyectoDestino { get; set; }

        /// <summary>
        /// Partida del presupuesto a la que se carga. Más fino que la obra: permite
        /// comparar después lo estimado contra lo realmente consumido.
        /// </summary>
        public int? IdConceptoPartida { get; set; }

        /// <summary>Lote del que salió el material: un renglón de compra.</summary>
        [Required]
        public int IdCompraDetalle { get; set; }

        /// <summary>
        /// Renglón de la requisición que esta salida surte.
        ///
        /// La firma sola no alcanza para saberlo: hay requisiciones con dos o tres
        /// renglones del mismo material —para personas distintas, o en tallas
        /// distintas— y sin este enlace no hay forma de decir cuál de ellos cubrió
        /// cada salida, ni de verificar el acumulado que guarda el renglón.
        ///
        /// Nulo en las entregas sueltas y en el consumo cargado directo a la obra.
        /// </summary>
        public int? IdRequisicionDetalle { get; set; }

        /// <summary>
        /// Acto de entrega que ampara esta salida, cuando vino de una requisición.
        ///
        /// Una firma puede amparar varias salidas: si lo pedido está repartido en
        /// dos lotes, se descuenta de cada uno por separado pero la persona firma
        /// una sola vez. Sin este enlace la ficha tenía que adivinar qué cubrió
        /// cada firma agrupando por trabajador, y con dos entregas al mismo obrero
        /// mostraba el acumulado del renglón en ambas.
        ///
        /// Nulo en las entregas sueltas, que no nacen de ninguna requisición.
        /// </summary>
        public int? IdRequisicionEntrega { get; set; }

        // ===== DATOS DE LA ENTREGA =====
        [Required]
        public DateTime FechaEntrega { get; set; }
        [Required]
        public decimal CantidadEntregada { get; set; }
        [MaxLength(20)]
        public string TallaEntregada { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdProyectoDestino")]
        public virtual Proyecto? ProyectoDestino { get; set; }

        [ForeignKey("IdConceptoPartida")]
        public virtual ConceptoPartida? ConceptoPartida { get; set; }

        [ForeignKey("IdCompraDetalle")]
        public virtual CompraEPPDetalle? CompraDetalle { get; set; }

        [ForeignKey("IdRequisicionEntrega")]
        public virtual RequisicionEntrega? RequisicionEntrega { get; set; }

        [ForeignKey("IdRequisicionDetalle")]
        public virtual RequisicionEPPDetalle? RequisicionDetalle { get; set; }

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>
        /// Una salida siempre tiene destino: o una persona, o una obra. Ninguno de
        /// los dos significa material que salió del almacén sin que nadie sepa
        /// adónde, que es justo lo que el kardex vino a evitar.
        /// </summary>
        [NotMapped]
        public bool TieneDestino => IdEmpleado.HasValue || IdProyectoDestino.HasValue;

        /// <summary>Va a una persona, así que necesita su firma.</summary>
        [NotMapped]
        public bool RequiereFirma => IdEmpleado.HasValue;
    }
}