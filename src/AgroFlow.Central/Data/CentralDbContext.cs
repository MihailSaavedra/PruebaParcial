using AgroFlow.Central.Models;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Central.Data;

public class CentralDbContext : DbContext
{
    // El constructor permite que la configuración de la conexión sea inyectada desde Program.cs
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options)
    {
    }

    // Define las tablas que EF Core debe crear y gestionar
    public DbSet<Agricultor> Agricultores { get; set; }
    public DbSet<Cosecha> Cosechas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración adicional (opcional pero recomendada)
        // Asegura que los GUIDs se generen en la base de datos
        modelBuilder.Entity<Agricultor>().Property(a => a.AgricultorId).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<Cosecha>().Property(c => c.CosechaId).HasDefaultValueSql("gen_random_uuid()");

        // Definir una restricción CHECK para toneladas > 0
        modelBuilder.Entity<Cosecha>().ToTable(c => c.HasCheckConstraint("CK_Cosecha_Toneladas_Positive", "toneladas > 0"));
    }
}