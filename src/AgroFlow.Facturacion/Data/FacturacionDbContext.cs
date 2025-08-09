using AgroFlow.Facturacion.Models;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Facturacion.Data;

public class FacturacionDbContext : DbContext
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options)
    {
    }

    public DbSet<Factura> Facturas { get; set; }
    public DbSet<DetalleFactura> DetallesFactura { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración para Factura
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.Property(f => f.FacturaId).HasDefaultValueSql("UUID()");
            entity.HasIndex(f => f.CosechaId).IsUnique(); // Una cosecha = una factura
            entity.ToTable(f => f.HasCheckConstraint("CK_Factura_Toneladas_Positive", "Toneladas > 0"));
            entity.ToTable(f => f.HasCheckConstraint("CK_Factura_Total_Positive", "Total > 0"));
        });

        // Configuración para DetalleFactura
        modelBuilder.Entity<DetalleFactura>(entity =>
        {
            entity.Property(d => d.DetalleId).HasDefaultValueSql("UUID()");
            entity.ToTable(d => d.HasCheckConstraint("CK_DetalleFactura_Cantidad_Positive", "Cantidad > 0"));
        });

        // Configurar relación uno a muchos
        modelBuilder.Entity<DetalleFactura>()
            .HasOne(d => d.Factura)
            .WithMany(f => f.Detalles)
            .HasForeignKey(d => d.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
