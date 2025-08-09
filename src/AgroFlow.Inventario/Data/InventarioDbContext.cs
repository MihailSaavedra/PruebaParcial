using AgroFlow.Inventario.Models;
using Microsoft.EntityFrameworkCore;

namespace AgroFlow.Inventario.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    public DbSet<Insumo> Insumos { get; set; }
}