using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    // ===== BANDEJA DE FALTANTES =====

    /// <summary>
    /// Una línea de la bandeja del Comprador: cuánto falta de un material en un
    /// almacén, y quién lo está esperando.
    /// </summary>
    public class FaltanteDto
    {
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        /// <summary>Suma de los renglones autorizados sin cobertura para este par.</summary>
        public decimal Demanda { get; set; }

        public decimal Existencia { get; set; }
        public decimal Comprometido { get; set; }

        /// <summary>Pedido a un proveedor y aún sin recibir en este almacén.</summary>
        public decimal EnTransito { get; set; }

        /// <summary>Lo que hay que comprar: demanda menos disponible menos tránsito.</summary>
        public decimal Faltante { get; set; }

        /// <summary>Último precio pagado por este material, como referencia.</summary>
        public decimal UltimoPrecio { get; set; }
        public int? IdUltimoProveedor { get; set; }
        public string? UltimoProveedor { get; set; }

        /// <summary>Renglones concretos que este faltante cubriría.</summary>
        public List<FaltanteRenglonDto> Renglones { get; set; } = new();

        public decimal Disponible => Existencia - Comprometido;
        public decimal ImporteEstimado => Faltante * UltimoPrecio;

        /// <summary>Días que lleva esperando el renglón más antiguo.</summary>
        public int DiasEsperaMaximo => Renglones.Count == 0 ? 0 : Renglones.Max(r => r.DiasEspera);
    }

    public class FaltanteRenglonDto
    {
        public int IdRequisicionDetalle { get; set; }
        public int IdRequisicion { get; set; }
        public string NumeroRequisicion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }

        public int? IdEmpleadoDestino { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRequerida { get; set; }

        public int DiasEspera => (int)(DateTime.Today - FechaSolicitud.Date).TotalDays;

        /// <summary>La fecha requerida ya pasó o es hoy.</summary>
        public bool EsUrgente => FechaRequerida.HasValue && FechaRequerida.Value.Date <= DateTime.Today;
    }

    // ===== REQUEST =====

    /// <summary>
    /// Genera una orden de compra a partir de líneas de la bandeja. El Comprador
    /// elige el proveedor y ajusta el precio; el sistema amarra cada renglón con
    /// las requisiciones que viene a cubrir.
    /// </summary>
    public record GenerarOrdenCompraRequestDto(
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        int IdProveedor,

        [Required(ErrorMessage = "Debe incluir al menos un material")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un material")]
        List<GenerarOrdenCompraLineaDto> Lineas,

        DateTime? FechaEntregaEstimada = null,

        [MaxLength(500)]
        string? Observaciones = null,

        [MaxLength(3)]
        string Moneda = "MXN"
    );

    public record GenerarOrdenCompraLineaDto(
        [Required]
        [Range(1, int.MaxValue)]
        int IdMaterial,

        [Required]
        [Range(1, int.MaxValue)]
        int IdAlmacenDestino,

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal Cantidad,

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        decimal PrecioUnitario
    );

    public record CancelarOrdenCompraRequestDto(
        [Required(ErrorMessage = "Debe indicar el motivo")]
        [MaxLength(500)]
        string Motivo
    );

    // ===== RESPONSE =====

    public class OrdenCompraResponseDto
    {
        public int IdOrdenCompra { get; set; }
        public string Folio { get; set; } = string.Empty;

        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }

        public string IdUsuarioEmite { get; set; } = string.Empty;
        public string NombreUsuarioEmite { get; set; } = string.Empty;

        public EstadoOrdenCompra Estado { get; set; }
        public string EstadoTexto => Estado switch
        {
            EstadoOrdenCompra.Emitida => "Emitida",
            EstadoOrdenCompra.ParcialmenteRecibida => "Parcialmente recibida",
            EstadoOrdenCompra.Recibida => "Recibida",
            EstadoOrdenCompra.Cancelada => "Cancelada",
            _ => Estado.ToString()
        };
        public string EstadoClase => Estado switch
        {
            EstadoOrdenCompra.Emitida => "info",
            EstadoOrdenCompra.ParcialmenteRecibida => "primary",
            EstadoOrdenCompra.Recibida => "success",
            EstadoOrdenCompra.Cancelada => "secondary",
            _ => "secondary"
        };

        public string Moneda { get; set; } = "MXN";
        public string? Observaciones { get; set; }
        public string? MotivoCancelacion { get; set; }

        public List<OrdenCompraDetalleResponseDto> Detalles { get; set; } = new();

        // Calculados
        public decimal Total => Detalles.Sum(d => d.Importe);
        public int TotalRenglones => Detalles.Count;
        public decimal TotalPiezas => Detalles.Sum(d => d.CantidadPedida);

        /// <summary>Almacenes distintos a los que va esta orden.</summary>
        public int TotalDestinos => Detalles.Select(d => d.IdAlmacenDestino).Distinct().Count();

        public bool PuedeCancelarse => Estado == EstadoOrdenCompra.Emitida
                                       && Detalles.All(d => d.CantidadRecibida == 0);

        /// <summary>Resumen para listados: "Cascos, Botas y 2 más".</summary>
        public string ResumenMateriales
        {
            get
            {
                if (Detalles.Count == 0) return "Sin renglones";
                if (Detalles.Count <= 2) return string.Join(", ", Detalles.Select(d => d.NombreMaterial));
                return $"{Detalles[0].NombreMaterial}, {Detalles[1].NombreMaterial} y {Detalles.Count - 2} más";
            }
        }
    }

    public class OrdenCompraDetalleResponseDto
    {
        public int IdOrdenCompraDetalle { get; set; }
        public int IdOrdenCompra { get; set; }

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public int IdAlmacenDestino { get; set; }
        public string NombreAlmacenDestino { get; set; } = string.Empty;

        public decimal CantidadPedida { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal PrecioUnitarioPactado { get; set; }
        public string? Observaciones { get; set; }

        /// <summary>Requisiciones que este renglón viene a cubrir.</summary>
        public List<CoberturaResponseDto> Coberturas { get; set; } = new();

        public decimal Importe => CantidadPedida * PrecioUnitarioPactado;
        public decimal EnTransito => Math.Max(0, CantidadPedida - CantidadRecibida);
        public bool Completo => CantidadRecibida >= CantidadPedida;
    }

    public class CoberturaResponseDto
    {
        public int IdRequisicionCobertura { get; set; }
        public int IdRequisicionDetalle { get; set; }
        public string NumeroRequisicion { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public OrigenCobertura Origen { get; set; }
    }
}
