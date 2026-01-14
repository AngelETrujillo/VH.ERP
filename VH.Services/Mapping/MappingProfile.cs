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
                .ForMember(dest => dest.NombreUnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida != null ? src.UnidadMedida.Nombre : string.Empty))
                .ForMember(dest => dest.AbreviaturaUnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida != null ? src.UnidadMedida.Abreviatura : string.Empty));

            CreateMap<ConceptoPartidaRequestDto, ConceptoPartida>();

            // ===== EMPLEADOS =====
            CreateMap<Empleado, EmpleadoResponseDto>()
                .ForMember(dest => dest.IdProyecto, opt => opt.MapFrom(src => src.IdProyecto))
                .ForMember(dest => dest.NombreProyecto, opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.Nombre : string.Empty));

            CreateMap<EmpleadoRequestDto, Empleado>();

            // ===== PROVEEDORES =====
            CreateMap<ProveedorRequestDto, Proveedor>();
            CreateMap<Proveedor, ProveedorResponseDto>();

            // ===== MATERIALES EPP =====
            CreateMap<MaterialEPPRequestDto, MaterialEPP>();

            // En la sección de MATERIALES EPP, reemplazar:
            CreateMap<MaterialEPP, MaterialEPPResponseDto>()
                .ForMember(dest => dest.IdUnidadMedida, opt => opt.MapFrom(src => src.IdUnidadMedida))
                .ForMember(dest => dest.NombreUnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida != null ? src.UnidadMedida.Nombre : string.Empty))
                .ForMember(dest => dest.AbreviaturaUnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida != null ? src.UnidadMedida.Abreviatura : string.Empty))
                .ForMember(dest => dest.StockGlobal, opt => opt.MapFrom(src =>
                    src.Inventarios != null ? src.Inventarios.Sum(i => i.Existencia) : 0));

            // ===== ENTREGAS EPP =====
            CreateMap<EntregaEPPRequestDto, EntregaEPP>();

            CreateMap<EntregaEPP, EntregaEPPResponseDto>()
                // Mapeo de Empleado
                .ForMember(dest => dest.IdEmpleado, opt => opt.MapFrom(src => src.IdEmpleado))
                .ForMember(dest => dest.NombreCompletoEmpleado, opt => opt.MapFrom(src =>
                    src.Empleado != null
                        ? $"{src.Empleado.Nombre} {src.Empleado.ApellidoPaterno} {src.Empleado.ApellidoMaterno}".Trim()
                        : string.Empty))
                .ForMember(dest => dest.NumeroNominaEmpleado, opt => opt.MapFrom(src =>
                    src.Empleado != null ? src.Empleado.NumeroNomina : string.Empty))

                // Mapeo de Material
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.MaterialEPP != null ? src.MaterialEPP.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.MaterialEPP != null && src.MaterialEPP.UnidadMedida != null
                        ? src.MaterialEPP.UnidadMedida.Abreviatura
                        : string.Empty))

                // Mapeo de Proveedor
                .ForMember(dest => dest.IdProveedor, opt => opt.MapFrom(src => src.IdProveedor))
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src =>
                    src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));

            // ===== UNIDADES DE MEDIDA =====
            CreateMap<UnidadMedidaRequestDto, UnidadMedida>();
            CreateMap<UnidadMedida, UnidadMedidaResponseDto>();

            // ===== ALMACENES =====
            CreateMap<AlmacenRequestDto, Almacen>();
            CreateMap<Almacen, AlmacenResponseDto>()
                .ForMember(dest => dest.IdProyecto, opt => opt.MapFrom(src => src.IdProyecto))
                .ForMember(dest => dest.NombreProyecto, opt => opt.MapFrom(src =>
                    src.Proyecto != null ? src.Proyecto.Nombre : string.Empty));

            // ===== INVENTARIOS =====
            CreateMap<InventarioRequestDto, Inventario>()
                .ForMember(dest => dest.FechaUltimoMovimiento, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<Inventario, InventarioResponseDto>()
                // Mapeo de Almacén
                .ForMember(dest => dest.IdAlmacen, opt => opt.MapFrom(src => src.IdAlmacen))
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src =>
                    src.Almacen != null ? src.Almacen.Nombre : string.Empty))

                // Mapeo de Material
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.NombreMaterial, opt => opt.MapFrom(src =>
                    src.Material != null ? src.Material.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedidaMaterial, opt => opt.MapFrom(src =>
                    src.Material != null && src.Material.UnidadMedida != null
                        ? src.Material.UnidadMedida.Abreviatura
                        : string.Empty));
        }
    }
}