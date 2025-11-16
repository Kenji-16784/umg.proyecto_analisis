using ApiProductos.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiProductos.Data
{
    public class AppDbContext : DbContext
    {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options) { }

        // ===== TABLAS PRINCIPALES =====
        //Agregamos la referencia de la bbdd de la tarjeta
        public DbSet<GiftCard> GiftCards { get; set; } = null!;

        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Presentacion> Presentaciones => Set<Presentacion>();
        public DbSet<Unidad> Unidades => Set<Unidad>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<ReglaPrecio> ReglasPrecio => Set<ReglaPrecio>();
        public DbSet<Compra> Compras => Set<Compra>();
        public DbSet<CompraDetalle> ComprasDetalles => Set<CompraDetalle>();
        public DbSet<StockProducto> StockProductos => Set<StockProducto>();
        public DbSet<Descuento> Descuentos => Set<Descuento>();
        public DbSet<DescuentoCliente> DescuentosClientes => Set<DescuentoCliente>();
        public DbSet<DescuentoProducto> DescuentosProductos => Set<DescuentoProducto>();
        public DbSet<Sucursal> Sucursales => Set<Sucursal>();
          
        // ===== TABLAS DE VENTAS =====
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentasDetalles => Set<VentaDetalle>();
        public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            // ===== UNIDADES =====
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<Unidad>(entity =>
            {
                entity.Property(u => u.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Abreviatura).IsRequired().HasMaxLength(10);
                entity.Property(u => u.Activo).HasDefaultValue(true);
            });

            // ===== PRODUCTOS =====
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Descripcion).HasMaxLength(200);
                entity.Property(p => p.Activo).HasDefaultValue(true);

                entity.HasMany(p => p.Presentaciones)
                      .WithOne(pr => pr.Producto)
                      .HasForeignKey(pr => pr.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PRESENTACIONES =====
            modelBuilder.Entity<Presentacion>(entity =>
            {
                entity.Property(p => p.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(p => p.FactorConversion).HasPrecision(10, 2);

                entity.HasOne(p => p.Producto)
                      .WithMany(p => p.Presentaciones)
                      .HasForeignKey(p => p.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Unidad)
                      .WithMany()
                      .HasForeignKey(p => p.UnidadId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== COMPRAS =====
            modelBuilder.Entity<Compra>(entity =>
            {
                entity.Property(c => c.NumeroFactura).HasMaxLength(50);
                entity.Property(c => c.Observaciones).HasMaxLength(500);
                entity.Property(c => c.Iva).HasPrecision(10, 2);
                entity.Property(c => c.TotalCompra).HasPrecision(12, 2);

                entity.HasOne(c => c.Proveedor)
                      .WithMany()
                      .HasForeignKey(c => c.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Sucursal)
                      .WithMany()
                      .HasForeignKey("SucursalId")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== COMPRA DETALLE =====
            modelBuilder.Entity<CompraDetalle>(entity =>
            {
                entity.Property(d => d.Cantidad).HasPrecision(10, 2);
                entity.Property(d => d.PrecioCompra).HasPrecision(10, 2);
                entity.Property(d => d.PrecioVenta).HasPrecision(10, 2);
                entity.Property(d => d.Subtotal).HasPrecision(10, 2);
                entity.Property(d => d.Lote).HasMaxLength(50).IsRequired(false);
                entity.Property(d => d.Observaciones).HasMaxLength(500);
                entity.Property(d => d.CodigoAlterno).HasMaxLength(100);

                entity.HasOne(d => d.Compra)
                      .WithMany(c => c.Detalles)
                      .HasForeignKey(d => d.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Unidad)
                      .WithMany()
                      .HasForeignKey(d => d.UnidadId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Presentacion)
                      .WithMany()
                      .HasForeignKey(d => d.PresentacionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== STOCK =====
            modelBuilder.Entity<StockProducto>(entity =>
            {
                entity.Property(s => s.Costo).HasPrecision(10, 2);
                entity.Property(s => s.PrecioVenta).HasPrecision(10, 2);
                entity.Property(s => s.StockActual).HasPrecision(12, 2);

                entity.HasIndex(s => new { s.ProductoId, s.ProveedorId, s.Lote }).IsUnique();

                entity.HasOne(s => s.Producto)
                      .WithMany()
                      .HasForeignKey(s => s.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Proveedor)
                      .WithMany()
                      .HasForeignKey(s => s.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== SUCURSAL =====
            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.Property(s => s.Nombre).IsRequired().HasMaxLength(100);
            });

            // ===== VENTAS =====
// ===== VENTAS =====
modelBuilder.Entity<Venta>(entity =>
{
    entity.Property(v => v.NumeroFactura).HasMaxLength(50);
    entity.Property(v => v.MetodoPago).HasMaxLength(50);
    entity.Property(v => v.Observaciones).HasMaxLength(500);
    entity.Property(v => v.Subtotal).HasPrecision(12, 2);
    entity.Property(v => v.Iva).HasPrecision(12, 2);
    entity.Property(v => v.Total).HasPrecision(12, 2);

    entity.HasOne(v => v.Cliente)
          .WithMany()
          .HasForeignKey(v => v.ClienteId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(v => v.ReglaPrecio)
          .WithMany()
          .HasForeignKey(v => v.ReglaPrecioId)
          .OnDelete(DeleteBehavior.Restrict);

    // ✅ NUEVA RELACIÓN CON SUCURSAL
    entity.HasOne(v => v.Sucursal)
          .WithMany()
          .HasForeignKey(v => v.SucursalId)
          .OnDelete(DeleteBehavior.Restrict);
});

                  // ===== VENTA DETALLE =====
                  modelBuilder.Entity<VentaDetalle>(entity =>
                  {
                        entity.Property(d => d.Cantidad).HasPrecision(10, 2);
                        entity.Property(d => d.PrecioUnitario).HasPrecision(10, 2);
                        entity.Property(d => d.Subtotal).HasPrecision(10, 2);

                        entity.HasOne(d => d.Venta)
                        .WithMany(v => v.Detalles)
                        .HasForeignKey(d => d.VentaId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(d => d.Producto)
                        .WithMany()
                        .HasForeignKey(d => d.ProductoId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });
            
            // ===== MOVIMIENTOS STOCK =====
modelBuilder.Entity<MovimientoStock>(entity =>
{
    entity.Property(m => m.Cantidad).HasPrecision(12, 2);
    entity.Property(m => m.PrecioUnitario).HasPrecision(10, 2);
    entity.Property(m => m.Observaciones).HasMaxLength(500);

    entity.HasOne(m => m.Producto)
          .WithMany()
          .HasForeignKey(m => m.ProductoId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(m => m.Sucursal)
          .WithMany()
          .HasForeignKey(m => m.SucursalId)
          .OnDelete(DeleteBehavior.Restrict);
});
        }
    }
}