using Microsoft.EntityFrameworkCore;
using VH.Services.Entities;

namespace VH.Data
{
    public class VHERPContext : DbContext
    {
        public VHERPContext(DbContextOptions<VHERPContext> options) : base(options)
        {
        }

        // ===== DB SETS (Tablas de la DB) =====
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ConceptoPartida> ConceptosPartidas { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<MaterialEPP> MaterialesEPP { get; set; }
        public DbSet<EntregaEPP> EntregasEPP { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }

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

                // Relaciones
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

                // Índice único para evitar duplicados
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

                // Relación con Proyecto
                entity.HasOne(cp => cp.Proyecto)
                    .WithMany(p => p.ConceptosPartidas)
                    .HasForeignKey(cp => cp.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con UnidadMedida
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

                // Relación con Proyecto
                entity.HasOne(e => e.Proyecto)
                    .WithMany(p => p.Empleados)
                    .HasForeignKey(e => e.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con EntregasEPP
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

                // Relación con EntregasEPP
                entity.HasMany(p => p.EntregasEPP)
                    .WithOne(ep => ep.Proveedor)
                    .HasForeignKey(ep => ep.IdProveedor)
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

                // Relación con UnidadMedida
                entity.HasOne(m => m.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(m => m.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Inventarios
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

                // Relación con Proyecto
                entity.HasOne(a => a.Proyecto)
                    .WithMany(p => p.Almacenes)
                    .HasForeignKey(a => a.IdProyecto)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Inventarios
                entity.HasMany(a => a.Inventarios)
                    .WithOne(i => i.Almacen)
                    .HasForeignKey(i => i.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);
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

                // Índice compuesto único (un material solo puede estar una vez por almacén)
                entity.HasIndex(i => new { i.IdAlmacen, i.IdMaterial }).IsUnique();

                // Relación con Almacen
                entity.HasOne(i => i.Almacen)
                    .WithMany(a => a.Inventarios)
                    .HasForeignKey(i => i.IdAlmacen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Material
                entity.HasOne(i => i.Material)
                    .WithMany(m => m.Inventarios)
                    .HasForeignKey(i => i.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CONFIGURACIÓN DE ENTREGA EPP =====
            modelBuilder.Entity<EntregaEPP>(entity =>
            {
                entity.HasKey(e => e.IdEntrega);

                entity.Property(e => e.CantidadEntregada)
                    .HasPrecision(18, 4);

                entity.Property(e => e.TallaEntregada)
                    .HasMaxLength(20);

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500);

                // Relación con Empleado
                entity.HasOne(e => e.Empleado)
                    .WithMany(emp => emp.EntregasEPP)
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con MaterialEPP
                entity.HasOne(e => e.MaterialEPP)
                    .WithMany()
                    .HasForeignKey(e => e.IdMaterial)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Proveedor
                entity.HasOne(e => e.Proveedor)
                    .WithMany(p => p.EntregasEPP)
                    .HasForeignKey(e => e.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice para búsquedas por fecha
                entity.HasIndex(e => e.FechaEntrega);
            });
        }
    }
}