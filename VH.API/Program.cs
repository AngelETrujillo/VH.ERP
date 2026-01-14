using AutoMapper; 
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VH.Data;
using VH.Data.Repositories;
using VH.Services.DTOs; 
using VH.Services.Entities;
using VH.Services.Interfaces;
using VH.Services.Mapping; 
using VH.Services.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VHERPContext>(options =>
    options.UseSqlServer(connectionString));



#region Scoped



builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IConceptoPartidaService, ConceptoPartidaService>();

builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMaterialEPPService, MaterialEPPService>();
builder.Services.AddScoped<IEntregaEPPService, EntregaEPPService>();
builder.Services.AddScoped<IProyectoEPPReaderService, ProyectoEPPReaderService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#endregion

#region Mapeo de Endpoints para Proyectos
////////////////////////////////////////////
// Mapeo de Endpoints para Proyectos
////////////////////////////////////////////

app.MapGet("/api/proyectos", async (IProyectoService proyectoService, IMapper mapper) =>
{
    var proyectos = await proyectoService.GetAllProyectosAsync();
    var proyectosDto = mapper.Map<IEnumerable<ProyectoResponseDto>>(proyectos);
    return Results.Ok(proyectosDto);
})
.WithName("GetAllProyectos");

app.MapGet("/api/proyectos/{id}", async (int id, IProyectoService proyectoService, IMapper mapper) =>
{
    var proyecto = await proyectoService.GetProyectoByIdAsync(id);
    if (proyecto is null)
    {
        return Results.NotFound();
    }
    var proyectoDto = mapper.Map<ProyectoResponseDto>(proyecto);
    return Results.Ok(proyectoDto);
})
.WithName("GetProyectoById");

app.MapPost("/api/proyectos", async (ProyectoRequestDto proyectoDto, IProyectoService proyectoService, IMapper mapper) =>
{
    // 1. Mapear DTO de entrada a Entidad
    var proyecto = mapper.Map<Proyecto>(proyectoDto);

    var nuevoProyecto = await proyectoService.CreateProyectoAsync(proyecto);

    // 2. Mapear Entidad de salida a DTO de respuesta
    var nuevoProyectoDto = mapper.Map<ProyectoResponseDto>(nuevoProyecto);

    return Results.Created($"/api/proyectos/{nuevoProyectoDto.IdProyecto}", nuevoProyectoDto);
})
.WithName("CreateProyecto");

app.MapPut("/api/proyectos/{id}", async (int id, ProyectoRequestDto proyectoDto, IProyectoService proyectoService, IMapper mapper) =>
{
    // 1. Obtener la entidad existente
    var proyectoExistente = await proyectoService.GetProyectoByIdAsync(id);
    if (proyectoExistente is null)
    {
        return Results.NotFound();
    }

    // 2. Mapear DTO de entrada sobre la Entidad existente (AutoMapper actualiza la entidad)
    mapper.Map(proyectoDto, proyectoExistente);
    proyectoExistente.IdProyecto = id; // Aseguramos que el ID sea el correcto

    var success = await proyectoService.UpdateProyectoAsync(proyectoExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateProyecto"); ;

app.MapDelete("/api/proyectos/{id}", async (int id, IProyectoService proyectoService) =>
{
    var success = await proyectoService.DeleteProyectoAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProyecto");

#endregion

#region Mapeo de Endpoints para ConceptosPartida
////////////////////////////////////////////
// Mapeo de Endpoints para ConceptosPartida
////////////////////////////////////////////

app.MapGet("/api/proyectos/{idProyecto}/partidas", async (int idProyecto, IConceptoPartidaService partidaService, IMapper mapper) =>
{
    var partidas = await partidaService.GetPartidasByProyectoAsync(idProyecto);
    var partidasDto = mapper.Map<IEnumerable<ConceptoPartidaResponseDto>>(partidas);
    return Results.Ok(partidasDto);
})
.WithName("GetPartidasByProyecto");

//app.MapGet("/api/partidas/{idPartida}", async (int idPartida, IConceptoPartidaService partidaService, IMapper mapper) =>
//{
//    var partida = await partidaService.GetPartidaByIdAsync(idPartida);
//    if (partida is null)
//    {
//        return Results.NotFound();
//    }
//    var partidaDto = mapper.Map<ConceptoPartidaResponseDto>(partida);
//    return Results.Ok(partidaDto);
//})
//.WithName("GetPartidaById");

app.MapGet("/api/proyectos/{idProyecto}/partidas/{idPartida}", async (int idProyecto, int idPartida, IConceptoPartidaService partidaService, IMapper mapper) =>
{
    var partida = await partidaService.GetPartidaByIdAsync(idPartida);
    if (partida is null)
    {
        return Results.NotFound();
    }
    var partidaDto = mapper.Map<ConceptoPartidaResponseDto>(partida);
    return Results.Ok(partidaDto);
})
.WithName("GetPartidaByProyectoAndId"); // Cambia el nombre también

app.MapPost("/api/proyectos/{idProyecto}/partidas", async (int idProyecto, ConceptoPartidaRequestDto partidaDto, IConceptoPartidaService partidaService, IMapper mapper) =>
{
    // Mapear DTO de entrada a Entidad
    var partida = mapper.Map<ConceptoPartida>(partidaDto);

    var nuevaPartida = await partidaService.CreatePartidaAsync(idProyecto, partida);

    if (nuevaPartida == null)
    {
        return Results.NotFound(new { Message = $"Proyecto con ID {idProyecto} no encontrado." });
    }

    // Mapear Entidad de salida a DTO de respuesta
    var nuevaPartidaDto = mapper.Map<ConceptoPartidaResponseDto>(nuevaPartida);

    return Results.Created($"/api/partidas/{nuevaPartidaDto.IdPartida}", nuevaPartidaDto);
})
.WithName("CreateConceptoPartida");

app.MapPut("/api/proyectos/{idProyecto}/partidas/{idPartida}", async (int idProyecto, int idPartida, ConceptoPartidaRequestDto partidaDto, IConceptoPartidaService partidaService, IMapper mapper) =>
{
    var partidaExistente = await partidaService.GetPartidaByIdAsync(idPartida);
    if (partidaExistente is null)
    {
        return Results.NotFound();
    }

    // Mapear DTO de entrada sobre la Entidad existente (AutoMapper actualiza la entidad)
    mapper.Map(partidaDto, partidaExistente);
    // Opcional: partidaExistente.IdProyecto = idProyecto; si el servicio lo requiere
    partidaExistente.IdPartida = idPartida; // Asegurar que el ID sea correcto

    var success = await partidaService.UpdatePartidaAsync(partidaExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateConceptoPartidaByProyecto"); // Cambia el nombre también

//app.MapPut("/api/partidas/{idPartida}", async (int idPartida, ConceptoPartidaRequestDto partidaDto, IConceptoPartidaService partidaService, IMapper mapper) =>
//{
//    var partidaExistente = await partidaService.GetPartidaByIdAsync(idPartida);
//    if (partidaExistente is null)
//    {
//        return Results.NotFound();
//    }

//    // Mapear DTO de entrada sobre la Entidad existente (AutoMapper actualiza la entidad)
//    mapper.Map(partidaDto, partidaExistente);
//    partidaExistente.IdPartida = idPartida; // Asegurar que el ID sea correcto

//    var success = await partidaService.UpdatePartidaAsync(partidaExistente);
//    return success ? Results.NoContent() : Results.NotFound();
//})
//.WithName("UpdateConceptoPartida");

app.MapDelete("/api/proyectos/{idProyecto}/partidas/{idPartida}", async (int idProyecto, int idPartida, IConceptoPartidaService partidaService) =>
{
    // La lógica de DeletePartidaAsync probablemente solo necesita idPartida
    var success = await partidaService.DeletePartidaAsync(idPartida);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteConceptoPartidaByProyecto"); // Cambia el nombre también

//app.MapDelete("/api/partidas/{idPartida}", async (int idPartida, IConceptoPartidaService partidaService) =>
//{
//    var success = await partidaService.DeletePartidaAsync(idPartida);
//    return success ? Results.NoContent() : Results.NotFound();
//})
//.WithName("DeleteConceptoPartida");

#endregion

#region Mapeo de Endpoints para Empleados EPP
////////////////////////////////////////////
// Mapeo de Endpoints para Empleados EPP
////////////////////////////////////////////

app.MapGet("/api/empleados", async (IEmpleadoService service, IMapper mapper) =>
{
    var empleados = await service.GetAllEmpleadosAsync();
    return Results.Ok(mapper.Map<IEnumerable<EmpleadoResponseDto>>(empleados));
})
.WithName("GetAllEmpleados");

app.MapGet("/api/empleados/{id}", async (int id, IEmpleadoService service, IMapper mapper) =>
{
    var empleado = await service.GetEmpleadoByIdAsync(id);
    if (empleado is null) return Results.NotFound();

    return Results.Ok(mapper.Map<EmpleadoResponseDto>(empleado));
})
.WithName("GetEmpleadoById");

app.MapPost("/api/empleados", async (EmpleadoRequestDto dto, IEmpleadoService service, IMapper mapper) =>
{
    var empleado = mapper.Map<Empleado>(dto);
    var nuevoEmpleado = await service.CreateEmpleadoAsync(empleado);
    var responseDto = mapper.Map<EmpleadoResponseDto>(nuevoEmpleado);

    return Results.Created($"/api/empleados/{responseDto.IdEmpleado}", responseDto);
})
.WithName("CreateEmpleado");

app.MapPut("/api/empleados/{id}", async (int id, EmpleadoRequestDto dto, IEmpleadoService service, IMapper mapper) =>
{
    var empleadoExistente = await service.GetEmpleadoByIdAsync(id);
    if (empleadoExistente is null) return Results.NotFound();

    // Mapeamos el DTO sobre la entidad existente para actualizar
    mapper.Map(dto, empleadoExistente);
    empleadoExistente.IdEmpleado = id;

    var success = await service.UpdateEmpleadoAsync(empleadoExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateEmpleado");

app.MapDelete("/api/empleados/{id}", async (int id, IEmpleadoService service) =>
{
    var success = await service.DeleteEmpleadoAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEmpleado");

#endregion

#region Mapeo de Endpoints para Proveedores
////////////////////////////////////////////
// Mapeo de Endpoints para Proveedores
////////////////////////////////////////////

app.MapGet("/api/proveedores", async (IProveedorService service, IMapper mapper) =>
{
    var proveedores = await service.GetAllProveedoresAsync();
    return Results.Ok(mapper.Map<IEnumerable<ProveedorResponseDto>>(proveedores));
})
.WithName("GetAllProveedores");

app.MapGet("/api/proveedores/{id}", async (int id, IProveedorService service, IMapper mapper) =>
{
    var proveedor = await service.GetProveedorByIdAsync(id);
    if (proveedor is null) return Results.NotFound();

    return Results.Ok(mapper.Map<ProveedorResponseDto>(proveedor));
})
.WithName("GetProveedorById");

app.MapPost("/api/proveedores", async (ProveedorRequestDto dto, IProveedorService service, IMapper mapper) =>
{
    var proveedor = mapper.Map<Proveedor>(dto);
    var nuevoProveedor = await service.CreateProveedorAsync(proveedor);
    var responseDto = mapper.Map<ProveedorResponseDto>(nuevoProveedor);

    return Results.Created($"/api/proveedores/{responseDto.IdProveedor}", responseDto);
})
.WithName("CreateProveedor");

app.MapPut("/api/proveedores/{id}", async (int id, ProveedorRequestDto dto, IProveedorService service, IMapper mapper) =>
{
    var proveedorExistente = await service.GetProveedorByIdAsync(id);
    if (proveedorExistente is null) return Results.NotFound();

    mapper.Map(dto, proveedorExistente);
    proveedorExistente.IdProveedor = id;

    var success = await service.UpdateProveedorAsync(proveedorExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateProveedor");

app.MapDelete("/api/proveedores/{id}", async (int id, IProveedorService service) =>
{
    var success = await service.DeleteProveedorAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProveedor");

#endregion

#region Mapeo de Endpoints para Materiales EPP
////////////////////////////////////////////
// Mapeo de Endpoints para Materiales EPP
////////////////////////////////////////////

app.MapGet("/api/materiales", async (IMaterialEPPService service, IMapper mapper) =>
{
    var materiales = await service.GetAllMaterialesEPPAsync();
    return Results.Ok(mapper.Map<IEnumerable<MaterialEPPResponseDto>>(materiales));
})
.WithName("GetAllMaterialesEPP");

app.MapGet("/api/materiales/{id}", async (int id, IMaterialEPPService service, IMapper mapper) =>
{
    var material = await service.GetMaterialEPPByIdAsync(id);
    if (material is null) return Results.NotFound();

    return Results.Ok(mapper.Map<MaterialEPPResponseDto>(material));
})
.WithName("GetMaterialEPPById");

app.MapPost("/api/materiales", async (MaterialEPPRequestDto dto, IMaterialEPPService service, IMapper mapper) =>
{
    var material = mapper.Map<MaterialEPP>(dto);
    var nuevoMaterial = await service.CreateMaterialEPPAsync(material);
    var responseDto = mapper.Map<MaterialEPPResponseDto>(nuevoMaterial);

    return Results.Created($"/api/materiales/{responseDto.IdMaterial}", responseDto);
})
.WithName("CreateMaterialEPP");

app.MapPut("/api/materiales/{id}", async (int id, MaterialEPPRequestDto dto, IMaterialEPPService service, IMapper mapper) =>
{
    var materialExistente = await service.GetMaterialEPPByIdAsync(id);
    if (materialExistente is null) return Results.NotFound();

    mapper.Map(dto, materialExistente);
    materialExistente.IdMaterial = id;

    var success = await service.UpdateMaterialEPPAsync(materialExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateMaterialEPP");

app.MapDelete("/api/materiales/{id}", async (int id, IMaterialEPPService service) =>
{
    var success = await service.DeleteMaterialEPPAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteMaterialEPP");

#endregion

#region Mapeo de Endpoints para Entregas EPP
////////////////////////////////////////////
// Mapeo de Endpoints para Entregas EPP
////////////////////////////////////////////

// GET: Obtener todas las entregas o filtrar por empleado (ruta anidada opcional)
app.MapGet("/api/entregas", async (IEntregaEPPService service, IMapper mapper, [FromQuery] int? idEmpleado) =>
{
    var entregas = await service.GetEntregasAsync(idEmpleado);
    return Results.Ok(mapper.Map<IEnumerable<EntregaEPPResponseDto>>(entregas));
})
.WithName("GetAllEntregasEPP");

app.MapGet("/api/entregas/{id}", async (int id, IEntregaEPPService service, IMapper mapper) =>
{
    var entrega = await service.GetEntregaEPPByIdAsync(id);
    if (entrega is null) return Results.NotFound();

    return Results.Ok(mapper.Map<EntregaEPPResponseDto>(entrega));
})
.WithName("GetEntregaEPPById");

app.MapPost("/api/entregas", async (EntregaEPPRequestDto dto, IEntregaEPPService service, IMapper mapper) =>
{
    var entrega = mapper.Map<EntregaEPP>(dto);
    var nuevaEntrega = await service.CreateEntregaEPPAsync(entrega);
    var responseDto = mapper.Map<EntregaEPPResponseDto>(nuevaEntrega);

    return Results.Created($"/api/entregas/{responseDto.IdEntrega}", responseDto);
})
.WithName("CreateEntregaEPP");

app.MapPut("/api/entregas/{id}", async (int id, EntregaEPPRequestDto dto, IEntregaEPPService service, IMapper mapper) =>
{
    var entregaExistente = await service.GetEntregaEPPByIdAsync(id);
    if (entregaExistente is null) return Results.NotFound();

    mapper.Map(dto, entregaExistente);
    entregaExistente.IdEntrega = id;

    var success = await service.UpdateEntregaEPPAsync(entregaExistente);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateEntregaEPP");

app.MapDelete("/api/entregas/{id}", async (int id, IEntregaEPPService service) =>
{
    var success = await service.DeleteEntregaEPPAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEntregaEPP");

#endregion

// -------------------------------------------------------------

//app.Run();

app.Run();
