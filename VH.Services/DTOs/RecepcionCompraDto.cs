using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VH.Services.DTOs
{
    // ===== REQUEST =====

    /// <summary>
    /// Lo que el almacenista captura al descargar el camión: una orden, un
    /// almacén, la factura y lo que contó de cada renglón.
    /// </summary>
    public record RecibirOrdenRequestDto(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la orden de compra")]
        int IdOrdenCompra,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el almacén que recibe")]
        int IdAlmacen,

        [Required(ErrorMessage = "La fecha de recepción es obligatoria")]
        DateTime FechaRecepcion,

        [MaxLength(50, ErrorMessage = "El número de factura no puede exceder 50 caracteres")]
        string? NumeroFactura,

        [MaxLength(36, ErrorMessage = "El UUID del CFDI no puede exceder 36 caracteres")]
        string? UuidCFDI,

        /// <summary>IVA de la factura recibida. El subtotal sale de lo aceptado.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "El IVA no puede ser negativo")]
        decimal Iva,

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        string? Observaciones,

        [Required(ErrorMessage = "Debe recibir al menos un renglón")]
        [MinLength(1, ErrorMessage = "Debe recibir al menos un renglón")]
        List<RecibirLineaRequestDto> Lineas
    );

    public record RecibirLineaRequestDto(
        [Required]
        [Range(1, int.MaxValue)]
        int IdOrdenCompraDetalle,

        [Required(ErrorMessage = "Indique cuánto llegó")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad recibida debe ser mayor a 0")]
        decimal CantidadRecibida,

        [Required(ErrorMessage = "Indique cuánto se acepta")]
        [Range(0, double.MaxValue, ErrorMessage = "La cantidad aceptada no puede ser negativa")]
        decimal CantidadAceptada,

        [Required(ErrorMessage = "Indique el precio realmente pagado")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        decimal PrecioUnitarioReal,

        [MaxLength(20)]
        string? Talla,

        DateTime? FechaCaducidad,

        [MaxLength(50)]
        string? LoteProveedor,

        [MaxLength(500)]
        string? MotivoRechazo
    );

    // ===== PREPARACIÓN DE LA PANTALLA =====

    /// <summary>
    /// Lo que hay que poner enfrente del almacenista antes de que cuente: qué
    /// renglones de esa orden van a ese almacén, cuánto falta por llegar, a qué
    /// precio se pactó y quién lleva esperando ese material.
    /// </summary>
    public class PreparacionRecepcionDto
    {
        public int IdOrdenCompra { get; set; }
        public string Folio { get; set; } = string.Empty;
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        public List<PreparacionRenglonDto> Renglones { get; set; } = new();

        public bool TienePendientes => Renglones.Any(r => r.Pendiente > 0);
        public decimal TotalPendiente => Renglones.Sum(r => r.Pendiente);
        public int TotalEsperando => Renglones.Sum(r => r.Esperan.Count);
    }

    public class PreparacionRenglonDto
    {
        public int IdOrdenCompraDetalle { get; set; }

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public bool RequiereTalla { get; set; }
        public bool ControlaCaducidad { get; set; }

        public decimal CantidadPedida { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal PrecioUnitarioPactado { get; set; }

        /// <summary>Lo que sigue en camino y puede recibirse hoy.</summary>
        public decimal Pendiente => Math.Max(0, CantidadPedida - CantidadRecibida);

        public bool Completo => Pendiente <= 0;

        /// <summary>Quiénes están esperando este material, del más antiguo al más nuevo.</summary>
        public List<EsperandoDto> Esperan { get; set; } = new();
    }

    /// <summary>Un renglón de requisición que espera este material.</summary>
    public class EsperandoDto
    {
        public int IdRequisicionDetalle { get; set; }
        public int IdRequisicion { get; set; }
        public string NumeroRequisicion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public int? IdEmpleadoDestino { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public int DiasEsperando => (int)(DateTime.Today - FechaSolicitud.Date).TotalDays;

        /// <summary>Verdadero si esta recepción ya alcanza para cubrirlo.</summary>
        public bool CubiertoAhora { get; set; }
    }

    // ===== RESPONSE =====

    public class RecepcionCompraResponseDto
    {
        public int IdRecepcion { get; set; }
        public string Folio { get; set; } = string.Empty;

        public int IdOrdenCompra { get; set; }
        public string FolioOrdenCompra { get; set; } = string.Empty;
        public string NombreProveedor { get; set; } = string.Empty;

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        public DateTime FechaRecepcion { get; set; }
        public string IdUsuarioRecibe { get; set; } = string.Empty;
        public string NombreUsuarioRecibe { get; set; } = string.Empty;

        public string? NumeroFactura { get; set; }
        public string? UuidCFDI { get; set; }
        public string? Observaciones { get; set; }

        public int? IdCompra { get; set; }

        public List<RecepcionCompraDetalleResponseDto> Detalles { get; set; } = new();

        public int TotalRenglones => Detalles.Count;
        public decimal TotalRecibido => Detalles.Sum(d => d.CantidadRecibida);
        public decimal TotalAceptado => Detalles.Sum(d => d.CantidadAceptada);
        public decimal TotalRechazado => Detalles.Sum(d => d.CantidadRechazada);
        public decimal Importe => Detalles.Sum(d => d.Importe);
        public bool HuboRechazo => Detalles.Any(d => d.CantidadRechazada > 0);
        public bool HuboDiferenciaPrecio => Detalles.Any(d => d.DiferenciaPrecio != 0);

        public string ResumenMateriales =>
            Detalles.Count == 0
                ? ""
                : string.Join(", ", Detalles.Take(3).Select(d => d.NombreMaterial))
                  + (Detalles.Count > 3 ? $" y {Detalles.Count - 3} más" : "");
    }

    public class RecepcionCompraDetalleResponseDto
    {
        public int IdRecepcionDetalle { get; set; }
        public int IdOrdenCompraDetalle { get; set; }

        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public decimal CantidadRecibida { get; set; }
        public decimal CantidadAceptada { get; set; }
        public decimal CantidadRechazada => CantidadRecibida - CantidadAceptada;

        public decimal PrecioUnitarioReal { get; set; }
        public decimal PrecioUnitarioPactado { get; set; }
        public decimal DiferenciaPrecio => PrecioUnitarioReal - PrecioUnitarioPactado;
        public decimal Importe => CantidadAceptada * PrecioUnitarioReal;

        public string? Talla { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string? LoteProveedor { get; set; }
        public string? MotivoRechazo { get; set; }

        public int? IdCompraDetalle { get; set; }
    }

    /// <summary>
    /// Una orden con material todavía en camino a un almacén: lo que aparece en la
    /// bandeja del almacenista.
    /// </summary>
    public class OrdenPorRecibirDto
    {
        public int IdOrdenCompra { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string NombreProveedor { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }

        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        public int RenglonesPendientes { get; set; }
        public decimal PiezasPendientes { get; set; }
        public decimal ImportePendiente { get; set; }
        public int PersonasEsperando { get; set; }
        public string ResumenMateriales { get; set; } = string.Empty;

        /// <summary>Ya llegó algo de esta orden a este almacén.</summary>
        public bool Parcial { get; set; }

        public int DiasDesdeEmision => (int)(DateTime.Today - FechaEmision.Date).TotalDays;

        /// <summary>Días de retraso contra la fecha prometida. Cero si aún no vence.</summary>
        public int DiasRetraso => FechaEntregaEstimada.HasValue
            ? Math.Max(0, (int)(DateTime.Today - FechaEntregaEstimada.Value.Date).TotalDays)
            : 0;
    }
}
