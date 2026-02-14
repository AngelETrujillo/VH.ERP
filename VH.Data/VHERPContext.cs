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

        // Catálogos Base
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ConceptoPartida> ConceptosPartidas { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Puesto> Puestos { get; set; }

        // Catálogos EPP
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<MaterialEPP> MaterialesEPP { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }

        // Transacciones EPP
        public DbSet<CompraEPP> ComprasEPP { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<EntregaEPP> EntregasEPP { get; set; }
        public DbSet<RequisicionEPP> RequisicionesEPP { get; set; }
        public DbSet<RequisicionEPPDetalle> RequisicionesEPPDetalle { get; set; }

        // Analytics
        public DbSet<ConfiguracionMaterialEPP> ConfiguracionesMaterialEPP { get; set; }
        public DbSet<AlertaConsumo> AlertasConsumo { get; set; }
        public DbSet<EstadisticaEmpleadoMensual> EstadisticasEmpleadoMensual { get; set; }
        public DbSet<EstadisticaProyectoMensual> EstadisticasProyectoMensual { get; set; }

        // Sistema
        public DbSet<LogActividad> LogsActividad { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarProyecto(modelBuilder);
            ConfigurarPuesto(modelBuilder);
            ConfigurarEmpleado(modelBuilder);
            ConfigurarProveedor(modelBuilder);
            ConfigurarMaterialEPP(modelBuilder);
            ConfigurarAlmacen(modelBuilder);
            ConfigurarCompraEPP(modelBuilder);
            ConfigurarInventario(modelBuilder);
            ConfigurarEntregaEPP(modelBuilder);
            ConfigurarRequisicionEPP(modelBuilder);
            ConfigurarRequisicionEPPDetalle(modelBuilder);
            ConfigurarConfiguracionMaterialEPP(modelBuilder);
            ConfigurarAlertaConsumo(modelBuilder);
            ConfigurarEstadisticaEmpleadoMensual(modelBuilder);
            ConfigurarEstadisticaProyectoMensual(modelBuilder);
            ConfigurarConceptoPartida(modelBuilder);
            ConfigurarLogActividad(modelBuilder);
            ConfigurarModulo(modelBuilder);
            ConfigurarRolPermiso(modelBuilder);
        }

        private void ConfigurarProyecto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proyecto>(entity =>
            {
                entity.HasKey(p => p.IdProyecto);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(p => p.TipoObra).HasMaxLength(100);
                entity.Property(p => p.PresupuestoTotal).HasPrecision(18, 2);
                entity.Property(p => p.PresupuestoEPPMensual).HasPrecision(18, 2);

                entity.HasMany(p => p.ConceptosPartidas)
                    .WithOne(cp => cp.Proyecto)
                    .HasForeignKey(cp => cp.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Empleados)
                    .WithOne(e => e.Proyecto)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Almacenes)
                    .WithOne(a => a.Proyecto)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarPuesto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Puesto>(entity =>
            {
                entity.HasKey(p => p.IdPuesto);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Descripcion).HasMaxLength(500);
                entity.HasIndex(p => p.Nombre).IsUnique();

                entity.HasMany(p => p.Empleados)
                    .WithOne(e => e.PuestoCatalogo)
                    .HasForeignKey(e => e.IdPuesto)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarEmpleado(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasKey(e => e.IdEmpleado);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoPaterno).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
                entity.Property(e => e.NumeroNomina).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Puesto).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PuntuacionRiesgoActual).HasPrecision(18, 2);

                entity.HasIndex(e => e.NumeroNomina).IsUnique();

                entity.HasOne(e => e.Proyecto)
                    .WithMany(p => p.Empleados)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.EntregasEPP)
                    .WithOne(ep => ep.Empleado)
                    .HasForeignKey(ep => ep.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarProveedor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(p => p.IdProveedor);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(p => p.RFC).IsRequired().HasMaxLength(13);
                entity.Property(p => p.Contacto).HasMaxLength(200);
                entity.Property(p => p.Telefono).HasMaxLength(20);
                entity.HasIndex(p => p.RFC).IsUnique();

                entity.HasMany(p => p.Compras)
                    .WithOne(c => c.Proveedor)
                    .HasForeignKey(c => c.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarMaterialEPP(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaterialEPP>(entity =>
            {
                entity.HasKey(m => m.IdMaterial);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Descripcion).HasMaxLength(500);
                entity.Property(m => m.CostoUnitarioEstimado).HasPrecision(18, 2);

                entity.HasOne(m => m.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(m => m.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Compras)
                    .WithOne(c => c.Material)
                    .HasForeignKey(c => c.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Inventarios)
                    .WithOne(i => i.Material)
                    .HasForeignKey(i => i.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Configuracion)
                    .WithOne(c => c.Material)
                    .HasForeignKey<ConfiguracionMaterialEPP>(c => c.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarAlmacen(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.HasKey(a => a.IdAlmacen);
                entity.Property(a => a.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Descripcion).HasMaxLength(500);
                entity.Property(a => a.Domicilio).HasMaxLength(300);
                entity.Property(a => a.TipoUbicacion).HasMaxLength(50);

                entity.HasOne(a => a.Proyecto)
                    .WithMany(p => p.Almacenes)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Inventarios)
                    .WithOne(i => i.Almacen)
                    .HasForeignKey(i => i.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Compras)
                    .WithOne(c => c.Almacen)
                    .HasForeignKey(c => c.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarCompraEPP(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompraEPP>(entity =>
            {
                entity.HasKey(c => c.IdCompra);
                entity.Property(c => c.FechaCompra).IsRequired();
                entity.Property(c => c.CantidadComprada).IsRequired().HasPrecision(18, 4);
                entity.Property(c => c.CantidadDisponible).IsRequired().HasPrecision(18, 4);
                entity.Property(c => c.PrecioUnitario).IsRequired().HasPrecision(18, 2);
                entity.Property(c => c.NumeroDocumento).HasMaxLength(50);
                entity.Property(c => c.Observaciones).HasMaxLength(500);

                entity.HasIndex(c => c.FechaCompra);
            });
        }

        private void ConfigurarInventario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventario>(entity =>
            {
                entity.HasKey(i => i.IdInventario);
                entity.Property(i => i.Existencia).HasPrecision(18, 4);
                entity.Property(i => i.StockMinimo).HasPrecision(18, 4);
                entity.Property(i => i.StockMaximo).HasPrecision(18, 4);
                entity.Property(i => i.UbicacionPasillo).HasMaxLength(100);

                entity.HasIndex(i => new { i.IdAlmacen, i.IdMaterial }).IsUnique();
            });
        }

        private void ConfigurarEntregaEPP(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntregaEPP>(entity =>
            {
                entity.HasKey(e => e.IdEntrega);
                entity.Property(e => e.FechaEntrega).IsRequired();
                entity.Property(e => e.CantidadEntregada).IsRequired().HasPrecision(18, 4);
                entity.Property(e => e.TallaEntregada).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                entity.HasIndex(e => e.FechaEntrega);

                entity.HasOne(e => e.Compra)
                    .WithMany()
                    .HasForeignKey(e => e.IdCompra)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarRequisicionEPP(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequisicionEPP>(entity =>
            {
                entity.HasKey(r => r.IdRequisicion);
                entity.Property(r => r.NumeroRequisicion).IsRequired().HasMaxLength(20);
                entity.Property(r => r.Justificacion).HasMaxLength(500);
                entity.Property(r => r.MotivoRechazo).HasMaxLength(500);
                entity.Property(r => r.FirmaDigital).HasMaxLength(500000);
                entity.Property(r => r.FotoEvidencia).HasMaxLength(300);
                entity.Property(r => r.Observaciones).HasMaxLength(500);
                entity.Property(r => r.EstadoRequisicion).IsRequired();

                entity.HasIndex(r => r.NumeroRequisicion).IsUnique();
                entity.HasIndex(r => r.FechaSolicitud);
                entity.HasIndex(r => r.EstadoRequisicion);
                entity.HasIndex(r => r.IdUsuarioSolicita);
                entity.HasIndex(r => r.IdEmpleadoRecibe);

                entity.HasOne(r => r.UsuarioSolicita)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioSolicita)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.EmpleadoRecibe)
                    .WithMany()
                    .HasForeignKey(r => r.IdEmpleadoRecibe)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Almacen)
                    .WithMany()
                    .HasForeignKey(r => r.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.UsuarioAprueba)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioAprueba)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.UsuarioEntrega)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuarioEntrega)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarRequisicionEPPDetalle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequisicionEPPDetalle>(entity =>
            {
                entity.HasKey(d => d.IdRequisicionDetalle);
                entity.Property(d => d.CantidadSolicitada).IsRequired().HasPrecision(18, 4);
                entity.Property(d => d.CantidadEntregada).HasPrecision(18, 4);
                entity.Property(d => d.TallaSolicitada).HasMaxLength(20);

                entity.HasOne(d => d.Requisicion)
                    .WithMany(r => r.Detalles)
                    .HasForeignKey(d => d.IdRequisicion)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Material)
                    .WithMany()
                    .HasForeignKey(d => d.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Compra)
                    .WithMany()
                    .HasForeignKey(d => d.IdCompra)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarConfiguracionMaterialEPP(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfiguracionMaterialEPP>(entity =>
            {
                entity.HasKey(c => c.IdConfiguracion);
                entity.Property(c => c.CantidadMaximaMensual).HasPrecision(18, 4);
                entity.Property(c => c.CantidadMaximaPorEntrega).HasPrecision(18, 4);

                entity.HasIndex(c => c.IdMaterial).IsUnique();
            });
        }

        private void ConfigurarAlertaConsumo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlertaConsumo>(entity =>
            {
                entity.HasKey(a => a.IdAlerta);
                entity.Property(a => a.Descripcion).IsRequired().HasMaxLength(1000);
                entity.Property(a => a.ValorEsperado).HasMaxLength(100);
                entity.Property(a => a.ValorReal).HasMaxLength(100);
                entity.Property(a => a.Desviacion).HasPrecision(18, 2);
                entity.Property(a => a.CostoEstimado).HasPrecision(18, 2);
                entity.Property(a => a.Observaciones).HasMaxLength(1000);

                entity.HasIndex(a => a.IdEmpleado);
                entity.HasIndex(a => a.EstadoAlerta);
                entity.HasIndex(a => a.FechaGeneracion);
                entity.HasIndex(a => a.Severidad);
                entity.HasIndex(a => a.IdProyecto);

                entity.HasOne(a => a.Empleado)
                    .WithMany(e => e.Alertas)
                    .HasForeignKey(a => a.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Material)
                    .WithMany(m => m.Alertas)
                    .HasForeignKey(a => a.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Proyecto)
                    .WithMany(p => p.Alertas)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Entrega)
                    .WithMany()
                    .HasForeignKey(a => a.IdEntrega)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Requisicion)
                    .WithMany()
                    .HasForeignKey(a => a.IdRequisicion)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.UsuarioReviso)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuarioReviso)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarEstadisticaEmpleadoMensual(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EstadisticaEmpleadoMensual>(entity =>
            {
                entity.HasKey(e => e.IdEstadistica);
                entity.Property(e => e.TotalUnidades).HasPrecision(18, 4);
                entity.Property(e => e.CostoTotal).HasPrecision(18, 2);
                entity.Property(e => e.PromedioDesviacionVidaUtil).HasPrecision(18, 2);
                entity.Property(e => e.PuntuacionRiesgo).HasPrecision(18, 2);

                entity.HasIndex(e => new { e.IdEmpleado, e.Anio, e.Mes }).IsUnique();
                entity.HasIndex(e => new { e.IdProyecto, e.Anio, e.Mes });
                entity.HasIndex(e => e.PuntuacionRiesgo);

                entity.HasOne(e => e.Empleado)
                    .WithMany(emp => emp.Estadisticas)
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Proyecto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarEstadisticaProyectoMensual(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EstadisticaProyectoMensual>(entity =>
            {
                entity.HasKey(e => e.IdEstadistica);
                entity.Property(e => e.TotalUnidades).HasPrecision(18, 4);
                entity.Property(e => e.CostoTotal).HasPrecision(18, 2);
                entity.Property(e => e.CostoPromedioPorEmpleado).HasPrecision(18, 2);
                entity.Property(e => e.PresupuestoAsignado).HasPrecision(18, 2);
                entity.Property(e => e.DesviacionPresupuesto).HasPrecision(18, 2);

                entity.HasIndex(e => new { e.IdProyecto, e.Anio, e.Mes }).IsUnique();

                entity.HasOne(e => e.Proyecto)
                    .WithMany(p => p.Estadisticas)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarConceptoPartida(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConceptoPartida>(entity =>
            {
                entity.HasKey(cp => cp.IdPartida);
                entity.Property(cp => cp.Descripcion).IsRequired().HasMaxLength(500);
                entity.Property(cp => cp.CantidadEstimada).HasPrecision(18, 4);

                entity.HasOne(cp => cp.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(cp => cp.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarLogActividad(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogActividad>(entity =>
            {
                entity.HasKey(l => l.IdLog);
                entity.Property(l => l.Accion).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Entidad).HasMaxLength(100);
                entity.Property(l => l.Descripcion).HasMaxLength(500);
                entity.Property(l => l.DireccionIP).HasMaxLength(50);

                entity.HasIndex(l => l.Fecha);
                entity.HasIndex(l => l.IdUsuario);

                entity.HasOne(l => l.Usuario)
                    .WithMany(u => u.LogsActividad)
                    .HasForeignKey(l => l.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarModulo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.HasKey(m => m.IdModulo);
                entity.Property(m => m.Codigo).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Descripcion).HasMaxLength(500);
                entity.Property(m => m.Icono).HasMaxLength(50);
                entity.Property(m => m.ControllerName).HasMaxLength(100);

                entity.HasIndex(m => m.Codigo).IsUnique();

                entity.HasOne(m => m.ModuloPadre)
                    .WithMany(m => m.SubModulos)
                    .HasForeignKey(m => m.IdModuloPadre)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarRolPermiso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.HasKey(rp => rp.IdRolPermiso);

                entity.HasIndex(rp => new { rp.IdRol, rp.IdModulo }).IsUnique();

                entity.HasOne(rp => rp.Rol)
                    .WithMany()
                    .HasForeignKey(rp => rp.IdRol)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Modulo)
                    .WithMany()
                    .HasForeignKey(rp => rp.IdModulo)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
