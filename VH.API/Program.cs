// VH.API/Program.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VH.Data;
using VH.Data.Repositories;
using VH.Services.Interfaces;
using VH.Services.Mapping;
using VH.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURACIÓN DE SERVICIOS =====

// 1. Agregar Controllers con opciones JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VH.ERP API - Sistema de Gestión EPP",
        Version = "v1",
        Description = "API para el sistema ERP de gestión de equipos de protección personal"
    });
});

// 3. Entity Framework - SQL Server
builder.Services.AddDbContext<VHERPContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// 5. Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 6. Servicios de Negocio
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IConceptoPartidaService, ConceptoPartidaService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMaterialEPPService, MaterialEPPService>();
builder.Services.AddScoped<IEntregaEPPService, EntregaEPPService>();
builder.Services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
builder.Services.AddScoped<IAlmacenService, AlmacenService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

// 7. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7266", "http://localhost:5210")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== CONFIGURACIÓN DEL PIPELINE =====

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VH.ERP API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthorization();

// ¡IMPORTANTE! Esto mapea todos los controllers
app.MapControllers();

app.Run();