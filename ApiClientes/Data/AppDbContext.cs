using Microsoft.EntityFrameworkCore;
using ApiClientes.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiClientes.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🔹 Tablas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ReglaPrecio> ReglasPrecio { get; set; } // 👈 Agrega esta línea

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔗 Configuración explícita de relación Cliente ↔ ReglasPrecio
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.ReglaPrecio)
                .WithMany()
                .HasForeignKey(c => c.ReglaPrecioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.ReglaPrecioId)
                .IsRequired();

            // ✅ Cliente por defecto
            modelBuilder.Entity<Cliente>().HasData(new Cliente
            {
                Id = 1,
                Nombre = "Consumidor Final",
                NIT = "CF",
                Telefono = "",
                Direccion = "Ciudad",
                Activo = true,
                ReglaPrecioId = 3 // ID de “Cliente Final” (de la tabla ReglasPrecio)
            });
        }
    }
}