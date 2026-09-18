using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Un movimiento de almacén. Es el libro de la existencia: cada entrada,
    /// salida, reserva o ajuste deja aquí su renglón, con su documento de origen
    /// y quién lo hizo.
    ///
    /// Hasta ahora la existencia era un número que se sumaba y se restaba sin
    /// historial: no se podía auditar quién movió qué, reconstruir el saldo a una
    /// fecha, ni explicar un descuadre. Con el kardex,
    /// <see cref="Inventario.Existencia"/> deja de ser la verdad y pasa a ser un
    /// saldo que se puede reconciliar contra la suma de estos renglones.
    /// </summary>
    [Table("MovimientosInventario")]
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        [Required]
        public int IdAlmacen { get; set; }

        [Required]
        public TipoMovimientoInventario Tipo { get; set; }

        /// <summary>
        /// Cantidad con signo: positiva si entra al almacén, negativa si sale.
        /// El saldo de un material es la suma de este campo sobre los movimientos
        /// que afectan existencia.
        /// </summary>
        [Required]
        public decimal Cantidad { get; set; }

        /// <summary>Costo unitario del movimiento, tomado del lote cuando lo hay.</summary>
        public decimal? CostoUnitario { get; set; }

        /// <summary>Lote al que pertenece el movimiento, si aplica.</summary>
        public int? IdCompraDetalle { get; set; }

        // ===== DOCUMENTO DE ORIGEN =====

        /// <summary>Qué documento lo provocó: "CompraEPP", "EntregaEPP", "Ajuste"…</summary>
        [MaxLength(50)]
        public string? DocumentoTipo { get; set; }

        /// <summary>Identificador de ese documento, para poder volver a él.</summary>
        public int? DocumentoId { get; set; }

        /// <summary>Folio legible del documento, para el kardex impreso.</summary>
        [MaxLength(50)]
        public string? DocumentoFolio { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ===== AUDITORÍA =====

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? IdUsuario { get; set; }

        /// <summary>
        /// Saldo de existencia del material en ese almacén justo después de este
        /// movimiento. Se guarda para poder leer el kardex sin recalcular toda la
        /// historia en cada consulta.
        /// </summary>
        public decimal SaldoResultante { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdCompraDetalle")]
        public virtual CompraEPPDetalle? CompraDetalle { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        // ===== CALCULADAS =====

        /// <summary>
        /// Los movimientos de reserva no mueven el anaquel: apartan y sueltan, pero
        /// la pieza sigue ahí. Sólo los demás cuentan para el saldo de existencia.
        /// </summary>
        [NotMapped]
        public bool AfectaExistencia =>
            Tipo != TipoMovimientoInventario.Reserva &&
            Tipo != TipoMovimientoInventario.LiberacionReserva;

        [NotMapped]
        public bool EsEntrada => Cantidad > 0;

        [NotMapped]
        public decimal Importe => Math.Abs(Cantidad) * (CostoUnitario ?? 0);
    }

    /// <summary>
    /// Naturaleza del movimiento. El signo de la cantidad lo determina la
    /// operación, no el tipo, pero cada tipo tiene un sentido esperado.
    /// </summary>
    public enum TipoMovimientoInventario
    {
        /// <summary>Compra recibida en el almacén. Positivo.</summary>
        Entrada = 0,

        /// <summary>Entrega a un trabajador. Negativo.</summary>
        Salida = 1,

        /// <summary>Material apartado para una requisición autorizada. No mueve existencia.</summary>
        Reserva = 2,

        /// <summary>Reserva soltada por cancelación o rechazo. No mueve existencia.</summary>
        LiberacionReserva = 3,

        /// <summary>Corrección por conteo físico. Puede ser positivo o negativo.</summary>
        Ajuste = 4,

        /// <summary>Sale de un almacén hacia otro. Negativo.</summary>
        TraspasoSalida = 5,

        /// <summary>Entra desde otro almacén. Positivo.</summary>
        TraspasoEntrada = 6,

        /// <summary>Material devuelto por un trabajador. Positivo.</summary>
        Devolucion = 7,

        /// <summary>Baja por daño, extravío o caducidad. Negativo.</summary>
        Merma = 8,

        /// <summary>Cancelación de una compra: revierte su entrada. Negativo.</summary>
        CancelacionCompra = 9
    }
}
