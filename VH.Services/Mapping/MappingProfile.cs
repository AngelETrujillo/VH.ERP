using AutoMapper;
using VH.Services.Entities;
using VH.Services.DTOs;

namespace VH.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== PROYECTOS =====
            CreateMap<Proyecto, ProyectoResponseDto>();
            CreateMap<ProyectoRequestDto, Proyecto>()
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

            CreateMap<Proyecto, ProyectoResponseDto>();

            CreateMap<Proyecto, ProyectoSimpleResponseDto>();

            // ===== CONCEPTOS PARTIDAS =====
            CreateMap<ConceptoPartida, ConceptoPartidaResponseDto>()
                .ForMember(dest => dest.IdUnidadMedida, opt => opt.MapFrom(src => src.IdUnidadMedida))
                .ForMember(dest => dest.NombreUnidadMedida, opt => opt.MapFrom(src =>
                    src.UnidadMedida != null ? src.UnidadMedida.Nombre : string.Empty))
                .ForMember(dest => dest.AbreviaturaUnidadMedida, opt => opt.MapFrom(src =>
                    src.UnidadMedida != null ? src.UnidadMedida.Abreviatura : string.Empty));

            CreateMap<ConceptoPartidaRequestDto, ConceptoPartida>();

            // ===== EMPLEADOS =====
            CreateMap<Empleado, EmpleadoResponseDto>()
                .ForMember(dest => dest.IdProyecto, opt => opt.MapFrom(src => src.IdProyecto))
                .ForMember(dest => dest.NombreProyecto, opt => opt.MapFrom(src =>
                    src.Proyecto != null ? src.Proyecto.Nombre : string.Empty));

            CreateMap<EmpleadoRequestDto, Empleado>();

            // ===== PROVEEDORES =====
            CreateMap<ProveedorRequestDto, Proveedor>();
            CreateMap<Proveedor, ProveedorResponseDto>();

            // ===== UNIDADES DE MEDIDA =====
            CreateMap<UnidadMedidaRequestDto, UnidadMedida>();
            CreateMap<UnidadMedida, UnidadMedidaResponseDto>();

            // ===== MATERIALES EPP =====
            CreateMap<MaterialRequestDto, Material>();

            CreateMap<Material, MaterialResponseDto>()
                .ForMember(dest => dest.IdUnidadMedida, opt => opt.MapFrom(src => src.IdUnidadMedida))
                .ForMember(dest => dest.NombreUnidadMedida, opt => opt.MapFrom(src =>
                    src.UnidadMedida != null ? src.UnidadMedida.Nombre : string.Empty))
                .ForMember(dest => dest.AbreviaturaUnidadMedida, opt => opt.MapFrom(src =>
                    src.UnidadMedida != null ? src.UnidadMedida.Abreviatura : string.Empty))
                .ForMember(dest => dest.StockGlobal, opt => opt.MapFrom(src =>
                    src.Inventarios != null ? src.Inventarios.Sum(i => i.Existencia) : 0));

            // ===== ALMACENES =====
            CreateMap<AlmacenRequestDto, Almacen>();

            CreateMap<Almacen, AlmacenResponseDto>()
                .ForMember(dest => dest.IdProyecto, opt => opt.MapFrom(src => src.IdProyecto))
                .ForMember(dest => dest.NombreProyecto, opt => opt.MapFrom(src =>
                    src.Proyecto != null ? src.Proyecto.Nombre : string.Empty));

            // ===== COMPRAS EPP: cabecera y renglones =====
            CreateMap<CompraEPPRequestDto, CompraEPP>()
                .ForMember(dest => dest.NumeroDocumento, opt => opt.MapFrom(src => src.NumeroDocumento ?? string.Empty))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones ?? string.Empty))
                // Subtotal y Total los calcula el servicio a partir de los renglones.
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore());

            CreateMap<CompraEPPDetalleRequestDto, CompraEPPDetalle>()
                // Un lote nace completo; el servicio iguala disponible a cantidad.
                .ForMember(dest => dest.CantidadDisponible, opt => opt.MapFrom(src => src.Cantidad));

            CreateMap<CompraEPP, CompraEPPResponseDto>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));

            CreateMap<CompraEPPDetalle, CompraEPPDetalleResponseDto>()
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null
                        ? src.Material.UnidadMedida.Abreviatura
                        : string.Empty))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty));

            // El lote que se elige al surtir es un renglón de compra.
            CreateMap<CompraEPPDetalle, CompraEPPSimpleDto>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Proveedor != null ? src.Compra.Proveedor.Nombre : string.Empty))
                .ForMember(dest => dest.FechaCompra, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.FechaCompra : DateTime.MinValue))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src =>
                    $"Lote #{src.IdCompraDetalle} - {(src.Compra != null && src.Compra.Proveedor != null ? src.Compra.Proveedor.Nombre : "?")} - {src.CantidadDisponible} disponibles @ ${src.PrecioUnitario}"));

            CreateMap<CompraEPPDetalle, HistorialPrecioDto>()
                .ForMember(dest => dest.FechaCompra, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.FechaCompra : DateTime.MinValue))
                .ForMember(dest => dest.NumeroDocumento, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.NumeroDocumento : string.Empty))
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Proveedor != null ? src.Compra.Proveedor.Nombre : string.Empty))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty));

            // ===== INVENTARIOS =====
            CreateMap<InventarioRequestDto, Inventario>()
                .ForMember(dest => dest.Existencia, opt => opt.Ignore()) // Se calcula automáticamente
                .ForMember(dest => dest.FechaUltimoMovimiento, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UbicacionPasillo, opt => opt.MapFrom(src => src.UbicacionPasillo ?? string.Empty));

            CreateMap<Inventario, InventarioResponseDto>()
                // Información del Almacén
                .ForMember(dest => dest.IdAlmacen, opt => opt.MapFrom(src => src.IdAlmacen))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty))
                .ForMember(dest => dest.NombreProyecto, opt => opt.MapFrom(src =>
                    src.Almacen != null && src.Almacen.Proyecto != null
                        ? src.Almacen.Proyecto.Nombre
                        : string.Empty))
                // Información del Material
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null
                        ? src.Material.UnidadMedida.Abreviatura
                        : string.Empty));

            CreateMap<Inventario, AlertaInventarioDto>()
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null
                        ? src.Material.UnidadMedida.Abreviatura
                        : string.Empty))
                .ForMember(dest => dest.EstadoStock, opt => opt.MapFrom(src => src.EstadoStock))
                .ForMember(dest => dest.MensajeAlerta, opt => opt.MapFrom(src =>
                    src.EstadoStock == "SinStock" ? "⚠️ SIN STOCK - Requiere reabastecimiento inmediato" :
                    src.EstadoStock == "Bajo" ? "⚠️ Stock bajo - Considere reabastecer" :
                    src.EstadoStock == "Excedido" ? "ℹ️ Stock excedido - Considere redistribuir" :
                    "✅ Stock normal"));

            // ===== ENTREGAS EPP (ACTUALIZADO) =====
            CreateMap<EntregaEPPRequestDto, EntregaEPP>()
                .ForMember(dest => dest.TallaEntregada, opt => opt.MapFrom(src => src.TallaEntregada ?? string.Empty))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones ?? string.Empty));

            CreateMap<EntregaEPP, EntregaEPPResponseDto>()
                // Información del Empleado
                .ForMember(dest => dest.IdEmpleado, opt => opt.MapFrom(src => src.IdEmpleado))
                .ForMember(dest => dest.NombreCompletoEmpleado, opt => opt.MapFrom(src =>
                    src.Empleado != null
                        ? $"{src.Empleado.Nombre} {src.Empleado.ApellidoPaterno} {src.Empleado.ApellidoMaterno}".Trim()
                        : string.Empty))
                .ForMember(dest => dest.NumeroNominaEmpleado, opt => opt.MapFrom(src =>
                    src.Empleado != null ? src.Empleado.NumeroNomina : string.Empty))
                // El lote del que salió: un renglón de compra
                .ForMember(dest => dest.IdCompraDetalle, opt => opt.MapFrom(src => src.IdCompraDetalle))
                .ForMember(dest => dest.IdCompra, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null ? src.CompraDetalle.IdCompra : 0))
                // Material (desde el lote)
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null ? src.CompraDetalle.IdMaterial : 0))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null && src.CompraDetalle.Material != null
                        ? src.CompraDetalle.Material.Nombre
                        : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null && src.CompraDetalle.Material != null && src.CompraDetalle.Material.UnidadMedida != null
                        ? src.CompraDetalle.Material.UnidadMedida.Abreviatura
                        : string.Empty))
                // Proveedor (desde la cabecera de la compra)
                .ForMember(dest => dest.IdProveedor, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null && src.CompraDetalle.Compra != null ? src.CompraDetalle.Compra.IdProveedor : 0))
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null && src.CompraDetalle.Compra != null && src.CompraDetalle.Compra.Proveedor != null
                        ? src.CompraDetalle.Compra.Proveedor.Nombre
                        : string.Empty))
                // Almacén (del renglón)
                .ForMember(dest => dest.IdAlmacen, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null ? src.CompraDetalle.IdAlmacen : 0))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null && src.CompraDetalle.Almacen != null
                        ? src.CompraDetalle.Almacen.Nombre
                        : string.Empty))
                // Precio del lote
                .ForMember(dest => dest.PrecioUnitarioCompra, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null ? src.CompraDetalle.PrecioUnitario : 0));

            // ===== REQUISICIONES EPP =====
            CreateMap<RequisicionEPP, RequisicionEPPResponseDto>()
                .ForMember(dest => dest.Justificacion, opt => opt.MapFrom(src => src.Justificacion ?? string.Empty))
                .ForMember(dest => dest.NombreUsuarioSolicita, opt => opt.MapFrom(src =>
                    src.UsuarioSolicita != null ? src.UsuarioSolicita.NombreCompleto : string.Empty))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty))
                .ForMember(dest => dest.NombreUsuarioAprueba, opt => opt.MapFrom(src =>
                    src.UsuarioAprueba != null ? src.UsuarioAprueba.NombreCompleto : null));

            CreateMap<RequisicionEntrega, RequisicionEntregaResponseDto>()
                .ForMember(dest => dest.NombreEmpleado, opt => opt.MapFrom(src =>
                    src.Empleado != null ? src.Empleado.NombreCompleto : string.Empty))
                .ForMember(dest => dest.NumeroNomina, opt => opt.MapFrom(src =>
                    src.Empleado != null ? src.Empleado.NumeroNomina : string.Empty))
                .ForMember(dest => dest.NombreUsuarioEntrega, opt => opt.MapFrom(src =>
                    src.UsuarioEntrega != null ? src.UsuarioEntrega.NombreCompleto : string.Empty));

            CreateMap<RequisicionEPPDetalle, RequisicionEPPDetalleResponseDto>()
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null ? src.Material.UnidadMedida.Abreviatura : string.Empty))
                .ForMember(dest => dest.DescripcionLote, opt => opt.MapFrom(src =>
                    src.CompraDetalle != null
                        ? $"Lote #{src.CompraDetalle.IdCompraDetalle} - {(src.CompraDetalle.Talla ?? "")}"
                        : null))
                // Destino del renglón: la persona, o la obra/partida
                .ForMember(dest => dest.NombreEmpleadoDestino, opt => opt.MapFrom(src =>
                    src.EmpleadoDestino != null ? src.EmpleadoDestino.NombreCompleto : string.Empty))
                .ForMember(dest => dest.NumeroNominaDestino, opt => opt.MapFrom(src =>
                    src.EmpleadoDestino != null ? src.EmpleadoDestino.NumeroNomina : string.Empty))
                .ForMember(dest => dest.NombreProyectoDestino, opt => opt.MapFrom(src =>
                    src.ProyectoDestino != null ? src.ProyectoDestino.Nombre : null))
                .ForMember(dest => dest.DescripcionPartida, opt => opt.MapFrom(src =>
                    src.ConceptoPartida != null ? src.ConceptoPartida.Descripcion : null));

            CreateMap<RequisicionEPPRequestDto, RequisicionEPP>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<RequisicionEPPDetalleRequestDto, RequisicionEPPDetalle>();
        }
    }
}