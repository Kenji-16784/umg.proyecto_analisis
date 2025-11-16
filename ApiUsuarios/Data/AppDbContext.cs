using Microsoft.EntityFrameworkCore;
using ApiUsuarios.Models;

namespace ApiUsuarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔗 Relación Usuario → Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 Relación Usuario → Sucursal
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Sucursal)
                .WithMany(s => s.Usuarios)
                .HasForeignKey(u => u.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🧱 Semillas de Roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador", Activo = true },
                new Rol { Id = 2, Nombre = "Supervisor", Activo = true },
                new Rol { Id = 3, Nombre = "Cajero", Activo = true }
            );

            // 🧱 Semillas de Sucursales (ejemplo)
            modelBuilder.Entity<Sucursal>().HasData(
                new Sucursal { Id = 1, Nombre = "Sucursal Central", Direccion = "Zona 1, Ciudad de Guatemala", Activo = true },
                new Sucursal { Id = 2, Nombre = "Sucursal Oriente", Direccion = "Zacapa", Activo = true }
            );
        }
    }
}