using System;
using System.ComponentModel.DataAnnotations;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    // ===== REQUEST =====

    public record AjusteInventarioRequestDto(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material")]
        int IdMaterial,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén")]
        int IdAlmacen,

        [Required(ErrorMessage = "Indique la existencia contada")]
        [Range(0, double.MaxValue, ErrorMessage = "La existencia contada no puede ser negativa")]
        decimal ExistenciaContada,

        [Required(ErrorMessage = "Debe explicar el motivo del ajuste")]
        [MaxLength(500)]
        string Motivo
    );

    public record TraspasoRequestDto(
        [Required]
        [Range(1, int.MaxValue)]
        int IdMaterial,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el almacén de origen")]
        int IdAlmacenOrigen,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el almacén de destino")]
        int IdAlmacenDestino,

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal Cantidad,

        [Required(ErrorMessage = "Debe indicar el motivo del traspaso")]
        [MaxLength(500)]
        string Motivo
    );

    public record MermaRequestDto(
        [Required]
        [Range(1, int.MaxValue)]
        int IdMaterial,

        [Required]
        [Range(1, int.MaxValue)]
        int IdAlmacen,

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal Cantidad,

        [Required(ErrorMessage = "Debe explicar el motivo de la baja")]
        [MaxLength(500)]
        string Motivo
    );

    // ===== RESPONSE =====

    public class MovimientoInventarioResponseDto
    {
        public int IdMovimiento { get; set; }

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        public TipoMovimientoInventario Tipo { get; set; }
        public string TipoTexto => Tipo switch
        {
            TipoMovimientoInventario.Entrada => "Entrada por compra",
            TipoMovimientoInventario.Salida => "Salida por entrega",
            TipoMovimientoInventario.Reserva => "Apartado",
            TipoMovimientoInventario.LiberacionReserva => "Apartado liberado",
            TipoMovimientoInventario.Ajuste => "Ajuste por conteo",
            TipoMovimientoInventario.TraspasoSalida => "Traspaso enviado",
            TipoMovimientoInventario.TraspasoEntrada => "Traspaso recibido",
            TipoMovimientoInventario.Devolucion => "Devolución",
            TipoMovimientoInventario.Merma => "Merma",
            TipoMovimientoInventario.CancelacionCompra => "Compra cancelada",
            _ => Tipo.ToString()
        };
        public string TipoClase => Tipo switch
        {
            TipoMovimientoInventario.Entrada => "success",
            TipoMovimientoInventario.TraspasoEntrada => "success",
            TipoMovimientoInventario.Devolucion => "success",
            TipoMovimientoInventario.Salida => "danger",
            TipoMovimientoInventario.TraspasoSalida => "danger",
            TipoMovimientoInventario.Merma => "danger",
            TipoMovimientoInventario.CancelacionCompra => "danger",
            TipoMovimientoInventario.Ajuste => "warning",
            _ => "secondary"
        };
        public string TipoIcono => Tipo switch
        {
            TipoMovimientoInventario.Entrada => "bi-box-arrow-in-down",
            TipoMovimientoInventario.Salida => "bi-box-arrow-up",
            TipoMovimientoInventario.Reserva => "bi-lock",
            TipoMovimientoInventario.LiberacionReserva => "bi-unlock",
            TipoMovimientoInventario.Ajuste => "bi-sliders",
            TipoMovimientoInventario.TraspasoSalida => "bi-arrow-right-circle",
            TipoMovimientoInventario.TraspasoEntrada => "bi-arrow-left-circle",
            TipoMovimientoInventario.Devolucion => "bi-arrow-return-left",
            TipoMovimientoInventario.Merma => "bi-trash",
            TipoMovimientoInventario.CancelacionCompra => "bi-x-circle",
            _ => "bi-dot"
        };

        public decimal Cantidad { get; set; }
        public decimal SaldoResultante { get; set; }
        public decimal? CostoUnitario { get; set; }
        public int? IdCompraDetalle { get; set; }

        public string? DocumentoTipo { get; set; }
        public int? DocumentoId { get; set; }
        public string? DocumentoFolio { get; set; }
        public string? Observaciones { get; set; }

        public DateTime Fecha { get; set; }
        public string? IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        public bool EsEntrada => Cantidad > 0;
        public bool AfectaExistencia => Tipo != TipoMovimientoInventario.Reserva
                                        && Tipo != TipoMovimientoInventario.LiberacionReserva;
        public decimal Importe => Math.Abs(Cantidad) * (CostoUnitario ?? 0);
    }

    /// <summary>
    /// Diferencia entre el saldo guardado y lo que suman los movimientos. Cada fila
    /// es existencia que no está explicada por el kardex.
    /// </summary>
    public class DescuadreDto
    {
        public int IdInventario { get; set; }
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;

        /// <summary>Lo que dice el registro de inventario.</summary>
        public decimal SaldoRegistrado { get; set; }

        /// <summary>Lo que suman los movimientos del kardex.</summary>
        public decimal SaldoKardex { get; set; }

        public decimal Diferencia => SaldoRegistrado - SaldoKardex;
    }
}
