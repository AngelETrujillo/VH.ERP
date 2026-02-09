using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VH.Services.Entities;

namespace VH.Data
{
    public class VHERPContext : IdentityDbContext<Usuario, Rol, string>
    {
        public VHERPContext(DbContextOptions<VHERPContext> options) : base(options)
        {
        }

        // ===== DB SETS (Tablas de la BD) =====
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ConceptoPartida> ConceptosPartidas { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<MaterialEPP> MaterialesEPP { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<CompraEPP> ComprasEPP { get; set; }  // ← NUEVA TABLA
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<EntregaEPP> EntregasEPP { get; set; }
        public DbSet<RequisicionEPP> RequisicionesEPP { get; set; }
        public DbSet<RequisicionEPPDetalle> RequisicionesEPPDetalle { get; set; }
        public DbSet<LogActividad> LogsActividad { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CONFIGURACIÓN DE PROYECTO =====
            modelBuilder.Entity<Proyecto>(entity =>
            {
                entity.HasKey(p => p.IdProyecto);

                entity.Property(p => p.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.TipoObra)
                    .HasMaxLength(100);

                entity.Property(p => p.PresupuestoTotal)
                    .HasPrecision(18, 2);

                // Relación: Proyecto -> ConceptosPartidas
                entity.HasMany(p => p.ConceptosPartidas)
                    .WithOne(cp => cp.Proyecto)
                    .HasForeignKey(cp => cp.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Proyecto -> Empleados
                entity.HasMany(p => p.Empleados)
                    .WithOne(e => e.Proyecto)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Proyecto -> Almacenes
                entity.HasMany(p => p.Almacenes)
                    .WithOne(a => a.Proyecto)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE UNIDAD MEDIDA =====
            modelBuilder.Entity<UnidadMedida>(entity =>
            {
                entity.HasKey(u => u.IdUnidadMedida);

                entity.Property(u => u.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Abreviatura)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(u => u.Descripcion)
                    .HasMaxLength(500);

                // Índices únicos para evitar duplicados
                entity.HasIndex(u => u.Nombre).IsUnique();
                entity.HasIndex(u => u.Abreviatura).IsUnique();
            });

            // ===== CONFIGURACIÓN DE CONCEPTO PARTIDA =====
            modelBuilder.Entity<ConceptoPartida>(entity =>
            {
                entity.HasKey(cp => cp.IdPartida);

                entity.Property(cp => cp.Descripcion)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(cp => cp.CantidadEstimada)
                    .HasPrecision(18, 4);

                // Relación: ConceptoPartida -> Proyecto
                entity.HasOne(cp => cp.Proyecto)
                    .WithMany(p => p.ConceptosPartidas)
                    .HasForeignKey(cp => cp.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: ConceptoPartida -> UnidadMedida
                entity.HasOne(cp => cp.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(cp => cp.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE EMPLEADO =====
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasKey(e => e.IdEmpleado);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ApellidoPaterno)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ApellidoMaterno)
                    .HasMaxLength(100);

                entity.Property(e => e.NumeroNomina)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Puesto)
                    .IsRequired()
                    .HasMaxLength(100);

                // Índice único para número de nómina
                entity.HasIndex(e => e.NumeroNomina).IsUnique();

                // Relación: Empleado -> Proyecto
                entity.HasOne(e => e.Proyecto)
                    .WithMany(p => p.Empleados)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Empleado -> EntregasEPP
                entity.HasMany(e => e.EntregasEPP)
                    .WithOne(ep => ep.Empleado)
                    .HasForeignKey(ep => ep.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE PROVEEDOR =====
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(p => p.IdProveedor);

                entity.Property(p => p.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.RFC)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.Property(p => p.Contacto)
                    .HasMaxLength(200);

                entity.Property(p => p.Telefono)
                    .HasMaxLength(20);

                // Índice único para RFC
                entity.HasIndex(p => p.RFC).IsUnique();

                // Relación: Proveedor -> ComprasEPP
                entity.HasMany(p => p.Compras)
                    .WithOne(c => c.Proveedor)
                    .HasForeignKey(c => c.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE MATERIAL EPP =====
            modelBuilder.Entity<MaterialEPP>(entity =>
            {
                entity.HasKey(m => m.IdMaterial);

                entity.Property(m => m.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(m => m.Descripcion)
                    .HasMaxLength(500);

                entity.Property(m => m.CostoUnitarioEstimado)
                    .HasPrecision(18, 2);

                // Relación: MaterialEPP -> UnidadMedida
                entity.HasOne(m => m.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(m => m.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: MaterialEPP -> ComprasEPP
                entity.HasMany(m => m.Compras)
                    .WithOne(c => c.Material)
                    .HasForeignKey(c => c.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: MaterialEPP -> Inventarios
                entity.HasMany(m => m.Inventarios)
                    .WithOne(i => i.Material)
                    .HasForeignKey(i => i.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE ALMACEN =====
            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.HasKey(a => a.IdAlmacen);

                entity.Property(a => a.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(a => a.Descripcion)
                    .HasMaxLength(500);

                entity.Property(a => a.Domicilio)
                    .HasMaxLength(300);

                entity.Property(a => a.TipoUbicacion)
                    .HasMaxLength(50);

                // Relación: Almacen -> Proyecto
                entity.HasOne(a => a.Proyecto)
                    .WithMany(p => p.Almacenes)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Almacen -> Inventarios
                entity.HasMany(a => a.Inventarios)
                    .WithOne(i => i.Almacen)
                    .HasForeignKey(i => i.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Almacen -> ComprasEPP
                entity.HasMany(a => a.Compras)
                    .WithOne(c => c.Almacen)
                    .HasForeignKey(c => c.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE COMPRA EPP (NUEVA) =====
            modelBuilder.Entity<CompraEPP>(entity =>
            {
                entity.HasKey(c => c.IdCompra);

                entity.Property(c => c.FechaCompra)
                    .IsRequired();

                entity.Property(c => c.CantidadComprada)
                    .IsRequired()
                    .HasPrecision(18, 4);

                entity.Property(c => c.CantidadDisponible)
                    .IsRequired()
                    .HasPrecision(18, 4);

                entity.Property(c => c.PrecioUnitario)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(c => c.NumeroDocumento)
                    .HasMaxLength(50);

                entity.Property(c => c.Observaciones)
                    .HasMaxLength(500);

                // Relación: CompraEPP -> Material
                entity.HasOne(c => c.Material)
                    .WithMany(m => m.Compras)
                    .HasForeignKey(c => c.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: CompraEPP -> Proveedor
                entity.HasOne(c => c.Proveedor)
                    .WithMany(p => p.Compras)
                    .HasForeignKey(c => c.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: CompraEPP -> Almacen
                entity.HasOne(c => c.Almacen)
                    .WithMany(a => a.Compras)
                    .HasForeignKey(c => c.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para búsquedas frecuentes
                entity.HasIndex(c => c.FechaCompra);
                entity.HasIndex(c => new { c.IdMaterial, c.IdAlmacen }); // Búsqueda de stock por material y almacén
            });

            // ===== CONFIGURACIÓN DE INVENTARIO =====
            modelBuilder.Entity<Inventario>(entity =>
            {
                entity.HasKey(i => i.IdInventario);

                entity.Property(i => i.Existencia)
                    .HasPrecision(18, 4);

                entity.Property(i => i.StockMinimo)
                    .HasPrecision(18, 4);

                entity.Property(i => i.StockMaximo)
                    .HasPrecision(18, 4);

                entity.Property(i => i.UbicacionPasillo)
                    .HasMaxLength(100);

                // Índice compuesto único: un material solo puede tener un registro de inventario por almacén
                entity.HasIndex(i => new { i.IdAlmacen, i.IdMaterial }).IsUnique();

                // Relación: Inventario -> Almacen
                entity.HasOne(i => i.Almacen)
                    .WithMany(a => a.Inventarios)
                    .HasForeignKey(i => i.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Inventario -> Material
                entity.HasOne(i => i.Material)
                    .WithMany(m => m.Inventarios)
                    .HasForeignKey(i => i.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE ENTREGA EPP =====
            modelBuilder.Entity<EntregaEPP>(entity =>
            {
                entity.HasKey(e => e.IdEntrega);

                entity.Property(e => e.FechaEntrega)
                    .IsRequired();

                entity.Property(e => e.CantidadEntregada)
                    .IsRequired()
                    .HasPrecision(18, 4);

                entity.Property(e => e.TallaEntregada)
                    .HasMaxLength(20);

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500);

                // Relación: EntregaEPP -> Empleado
                entity.HasOne(e => e.Empleado)
                    .WithMany(emp => emp.EntregasEPP)
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: EntregaEPP -> CompraEPP (de qué lote sale el material)
                entity.HasOne(e => e.Compra)
                    .WithMany()
                    .HasForeignKey(e => e.IdCompra)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice para búsquedas por fecha
                entity.HasIndex(e => e.FechaEntrega);
            });

            // ===== CONFIGURACIÓN DE REQUISICION EPP =====
            modelBuilder.Entity<RequisicionEPP>(entity =>
            {
                entity.HasKey(r => r.IdRequisicion);

                entity.Property(r => r.NumeroRequisicion)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(r => r.Justificacion)
                    .HasMaxLength(500);

                entity.Property(r => r.MotivoRechazo)
                    .HasMaxLength(500);

                entity.Property(r => r.FirmaDigital)
                    .HasMaxLength(500000);

                entity.Property(r => r.FotoEvidencia)
                    .HasMaxLength(300);

                entity.Property(r => r.Observaciones)
                    .HasMaxLength(500);

                entity.Property(r => r.EstadoRequisicion)
                    .IsRequired();

                // Índice único para número de requisición
                entity.HasIndex(r => r.NumeroRequisicion).IsUnique();

                // Relación: RequisicionEPP -> Usuario Solicita
                entity.HasOne(r => r.UsuarioSolicita)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioSolicita)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: RequisicionEPP -> Empleado Recibe
                entity.HasOne(r => r.EmpleadoRecibe)
                    .WithMany()
                    .HasForeignKey(r => r.IdEmpleadoRecibe)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: RequisicionEPP -> Almacén
                entity.HasOne(r => r.Almacen)
                    .WithMany()
                    .HasForeignKey(r => r.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: RequisicionEPP -> Usuario Aprueba
                entity.HasOne(r => r.UsuarioAprueba)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioAprueba)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: RequisicionEPP -> Usuario Entrega
                entity.HasOne(r => r.UsuarioEntrega)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioEntrega)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para búsquedas frecuentes
                entity.HasIndex(r => r.FechaSolicitud);
                entity.HasIndex(r => r.EstadoRequisicion);
                entity.HasIndex(r => r.IdUsuarioSolicita);
                entity.HasIndex(r => r.IdEmpleadoRecibe);
            });

            // ===== CONFIGURACIÓN DE REQUISICION EPP DETALLE =====
            modelBuilder.Entity<RequisicionEPPDetalle>(entity =>
            {
                entity.HasKey(d => d.IdRequisicionDetalle);

                entity.Property(d => d.CantidadSolicitada)
                    .IsRequired()
                    .HasPrecision(18, 4);

                entity.Property(d => d.CantidadEntregada)
                    .HasPrecision(18, 4);

                entity.Property(d => d.TallaSolicitada)
                    .HasMaxLength(20);

                // Relación: Detalle -> Requisición
                entity.HasOne(d => d.Requisicion)
                    .WithMany(r => r.Detalles)
                    .HasForeignKey(d => d.IdRequisicion)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación: Detalle -> Material
                entity.HasOne(d => d.Material)
                    .WithMany()
                    .HasForeignKey(d => d.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación: Detalle -> Compra (lote)
                entity.HasOne(d => d.Compra)
                    .WithMany()
                    .HasForeignKey(d => d.IdCompra)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice para búsquedas
                entity.HasIndex(d => new { d.IdRequisicion, d.IdMaterial });
            });

            // ===== CONFIGURACIÓN DE LOG ACTIVIDAD =====
            modelBuilder.Entity<LogActividad>(entity =>
            {
                entity.HasKey(l => l.IdLog);

                entity.Property(l => l.Accion).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Entidad).HasMaxLength(100);
                entity.Property(l => l.Descripcion).HasMaxLength(500);
                entity.Property(l => l.DireccionIP).HasMaxLength(50);

                entity.HasOne(l => l.Usuario)
                    .WithMany(u => u.LogsActividad)
                    .HasForeignKey(l => l.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => l.Fecha);
                entity.HasIndex(l => l.IdUsuario);
            });

            // ===== CONFIGURACIÓN DE MODULO =====
            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.HasKey(m => m.IdModulo);

                entity.Property(m => m.Codigo).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Descripcion).HasMaxLength(200);
                entity.Property(m => m.Icono).HasMaxLength(50);
                entity.Property(m => m.ControllerName).HasMaxLength(100);

                entity.HasIndex(m => m.Codigo).IsUnique();

                entity.HasOne(m => m.ModuloPadre)
                    .WithMany(m => m.SubModulos)
                    .HasForeignKey(m => m.IdModuloPadre)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE ROL PERMISO =====
            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.HasKey(rp => rp.IdRolPermiso);

                entity.HasIndex(rp => new { rp.IdRol, rp.IdModulo }).IsUnique();

                entity.HasOne(rp => rp.Rol)
                    .WithMany(r => r.RolPermisos)
                    .HasForeignKey(rp => rp.IdRol)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Modulo)
                    .WithMany(m => m.RolPermisos)
                    .HasForeignKey(rp => rp.IdModulo)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}