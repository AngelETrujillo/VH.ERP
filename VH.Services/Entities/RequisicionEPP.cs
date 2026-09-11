using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VH.Services.Entities
{
    /// <summary>
    /// Pedido de material de una obra. Puede cubrir a varios empleados: cada
    /// <see cref="RequisicionEPPDetalle"/> lleva su propio destino y su propio
    /// estado, y cada persona que recibe firma su propia
    /// <see cref="RequisicionEntrega"/>.
    ///
    /// La cabecera ya no tiene estado propio: lo deriva de sus renglones.
    /// </summary>
    [Table("RequisicionesEPP")]
    public class RequisicionEPP
    {
        [Key]
        public int IdRequisicion { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroRequisicion { get; set; } = string.Empty;

        // ===== SOLICITANTE =====

        [Required]
        public string IdUsuarioSolicita { get; set; } = string.Empty;

        [Required]
        public int IdAlmacen { get; set; }

        [MaxLength(500)]
        public string? Justificacion { get; set; }

        /// <summary>Cuándo se necesita el material. Ordena la atención de faltantes.</summary>
        public DateTime? FechaRequerida { get; set; }

        // ===== ESTADO =====

        /// <summary>
        /// Estado del documento. Se recalcula a partir de los renglones cada vez que
        /// alguno cambia; no se fija a mano.
        /// </summary>
        [Required]
        public EstadoRequisicion EstadoRequisicion { get; set; } = EstadoRequisicion.Pendiente;

        // ===== AUTORIZACIÓN =====

        public string? IdUsuarioAprueba { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        // ===== AUDITORÍA =====

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdUsuarioSolicita")]
        public virtual Usuario? UsuarioSolicita { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdUsuarioAprueba")]
        public virtual Usuario? UsuarioAprueba { get; set; }

        public virtual ICollection<RequisicionEPPDetalle> Detalles { get; set; } = new List<RequisicionEPPDetalle>();

        /// <summary>Una por cada persona que recibió material de este documento.</summary>
        public virtual ICollection<RequisicionEntrega> Entregas { get; set; } = new List<RequisicionEntrega>();

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>Personas distintas que reciben algo en este documento.</summary>
        [NotMapped]
        public int TotalEmpleados => Detalles
            .Where(d => d.IdEmpleadoDestino.HasValue)
            .Select(d => d.IdEmpleadoDestino!.Value)
            .Distinct()
            .Count();

        [NotMapped]
        public int RenglonesSurtidos => Detalles.Count(d => d.EstaSurtido);

        [NotMapped]
        public int RenglonesPendientes => Detalles.Count(d => d.EstaPendiente);

        /// <summary>
        /// Estado que corresponde al documento según sus renglones. Es lo que se
        /// guarda en <see cref="EstadoRequisicion"/> tras cada movimiento.
        /// </summary>
        public EstadoRequisicion CalcularEstado()
        {
            if (Detalles == null || Detalles.Count == 0)
                return EstadoRequisicion.Pendiente;

            // Ningún renglón vivo: el documento terminó de una u otra forma.
            if (Detalles.All(d => d.EstadoRenglon == EstadoRenglonRequisicion.Cancelado))
                return EstadoRequisicion.Cancelada;

            if (Detalles.All(d => d.EstadoRenglon == EstadoRenglonRequisicion.Rechazado))
                return EstadoRequisicion.Rechazada;

            if (Detalles.All(d => d.EstadoRenglon is EstadoRenglonRequisicion.Surtido
                                  or EstadoRenglonRequisicion.Rechazado
                                  or EstadoRenglonRequisicion.Cancelado))
                return EstadoRequisicion.Entregada;

            // Algo ya se surtió pero queda pendiente.
            if (Detalles.Any(d => d.EstaSurtido))
                return EstadoRequisicion.Parcial;

            // Autorizado, reservado, esperando compra, pedido o recibido: el
            // documento está aprobado y lo que falta es que el almacén lo resuelva.
            if (Detalles.Any(d => d.EstadoRenglon is EstadoRenglonRequisicion.Autorizado
                                  or EstadoRenglonRequisicion.Reservado
                                  or EstadoRenglonRequisicion.PorComprar
                                  or EstadoRenglonRequisicion.EnOrdenCompra
                                  or EstadoRenglonRequisicion.Recibido))
                return EstadoRequisicion.Aprobada;

            return EstadoRequisicion.Pendiente;
        }
    }
}
