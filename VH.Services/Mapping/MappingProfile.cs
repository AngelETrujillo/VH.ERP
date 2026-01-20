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
            CreateMap<ProyectoRequestDto, Proyecto>();
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
            CreateMap<MaterialEPPRequestDto, MaterialEPP>();

            CreateMap<MaterialEPP, MaterialEPPResponseDto>()
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

            // ===== COMPRAS EPP (NUEVO) =====
            CreateMap<CompraEPPRequestDto, CompraEPP>()
                .ForMember(dest => dest.CantidadDisponible, opt => opt.MapFrom(src => src.CantidadComprada))
                .ForMember(dest => dest.NumeroDocumento, opt => opt.MapFrom(src => src.NumeroDocumento ?? string.Empty))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones ?? string.Empty));

            CreateMap<CompraEPP, CompraEPPResponseDto>()
                // Información del Material
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null
                        ? src.Material.UnidadMedida.Abreviatura
                        : string.Empty))
                // Información del Proveedor
                .ForMember(dest => dest.IdProveedor, opt => opt.MapFrom(src => src.IdProveedor))
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Proveedor != null ? src.Proveedor.Nombre : string.Empty))
                // Información del Almacén
                .ForMember(dest => dest.IdAlmacen, opt => opt.MapFrom(src => src.IdAlmacen))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty));

            CreateMap<CompraEPP, CompraEPPSimpleDto>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Proveedor != null ? src.Proveedor.Nombre : string.Empty))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src =>
                    $"Lote #{src.IdCompra} - {(src.Proveedor != null ? src.Proveedor.Nombre : "?")} - {src.CantidadDisponible} disponibles @ ${src.PrecioUnitario}"));

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
                // Información del Lote/Compra
                .ForMember(dest => dest.IdCompra, opt => opt.MapFrom(src => src.IdCompra))
                // Información del Material (desde la Compra)
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.IdMaterial : 0))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Material != null
                        ? src.Compra.Material.Nombre
                        : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Material != null && src.Compra.Material.UnidadMedida != null
                        ? src.Compra.Material.UnidadMedida.Abreviatura
                        : string.Empty))
                // Información del Proveedor (desde la Compra)
                .ForMember(dest => dest.IdProveedor, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.IdProveedor : 0))
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Proveedor != null
                        ? src.Compra.Proveedor.Nombre
                        : string.Empty))
                // Información del Almacén (desde la Compra)
                .ForMember(dest => dest.IdAlmacen, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.IdAlmacen : 0))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Compra != null && src.Compra.Almacen != null
                        ? src.Compra.Almacen.Nombre
                        : string.Empty))
                // Precio del lote
                .ForMember(dest => dest.PrecioUnitarioCompra, opt => opt.MapFrom(src =>
                    src.Compra != null ? src.Compra.PrecioUnitario : 0));
        }
    }
}