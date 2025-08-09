using AgroFlow.Inventario.Controllers;
using AgroFlow.Inventario.Data;
using AgroFlow.Inventario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgroFlow.Inventario.Tests;

public class InsumosControllerTests : IDisposable
{
    private readonly InventarioDbContext _context;
    private readonly InsumosController _controller;
    private readonly Mock<ILogger<InsumosController>> _mockLogger;

    public InsumosControllerTests()
    {
        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InventarioDbContext(options);
        _mockLogger = new Mock<ILogger<InsumosController>>();
        _controller = new InsumosController(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetInsumos_ReturnsEmptyList_WhenNoInsumos()
    {
        // Act
        var result = await _controller.GetInsumos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var insumos = Assert.IsAssignableFrom<IEnumerable<Insumo>>(okResult.Value);
        Assert.Empty(insumos);
    }

    [Fact]
    public async Task GetInsumos_ReturnsInsumos_WhenInsumosExist()
    {
        // Arrange
        var insumo = new Insumo
        {
            InsumoId = Guid.NewGuid(),
            NombreInsumo = "Semilla de Arroz",
            Stock = 100,
            UnidadMedida = "kg",
            Categoria = "Semillas"
        };

        _context.Insumos.Add(insumo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetInsumos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var insumos = Assert.IsAssignableFrom<IEnumerable<Insumo>>(okResult.Value);
        Assert.Single(insumos);
        Assert.Equal("Semilla de Arroz", insumos.First().NombreInsumo);
    }

    [Fact]
    public async Task GetInsumosStockBajo_ReturnsInsumosWithLowStock()
    {
        // Arrange
        var insumoStockBajo = new Insumo
        {
            InsumoId = Guid.NewGuid(),
            NombreInsumo = "Fertilizante Bajo",
            Stock = 5, // Stock bajo
            UnidadMedida = "kg",
            Categoria = "Fertilizantes"
        };

        var insumoStockAlto = new Insumo
        {
            InsumoId = Guid.NewGuid(),
            NombreInsumo = "Fertilizante Alto",
            Stock = 50, // Stock alto
            UnidadMedida = "kg",
            Categoria = "Fertilizantes"
        };

        _context.Insumos.AddRange(insumoStockBajo, insumoStockAlto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetInsumosStockBajo(10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var insumos = Assert.IsAssignableFrom<IEnumerable<Insumo>>(okResult.Value);
        Assert.Single(insumos);
        Assert.Equal("Fertilizante Bajo", insumos.First().NombreInsumo);
    }

    [Fact]
    public async Task CreateInsumo_ReturnsCreatedResult_WithValidInsumo()
    {
        // Arrange
        var insumo = new Insumo
        {
            NombreInsumo = "Nuevo Insumo",
            Stock = 200,
            UnidadMedida = "L",
            Categoria = "Pesticidas"
        };

        // Act
        var result = await _controller.CreateInsumo(insumo);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdInsumo = Assert.IsType<Insumo>(createdResult.Value);
        Assert.Equal(insumo.NombreInsumo, createdInsumo.NombreInsumo);
        Assert.NotEqual(Guid.Empty, createdInsumo.InsumoId);
    }

    [Fact]
    public async Task CreateInsumo_ReturnsConflict_WhenInsumoAlreadyExists()
    {
        // Arrange
        var existingInsumo = new Insumo
        {
            InsumoId = Guid.NewGuid(),
            NombreInsumo = "Insumo Existente",
            Stock = 100,
            UnidadMedida = "kg"
        };

        _context.Insumos.Add(existingInsumo);
        await _context.SaveChangesAsync();

        var newInsumo = new Insumo
        {
            NombreInsumo = "Insumo Existente", // Mismo nombre
            Stock = 50,
            UnidadMedida = "kg"
        };

        // Act
        var result = await _controller.CreateInsumo(newInsumo);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStock_ReturnsNotFound_WhenInsumoDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.UpdateStock(id, 100);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStock_ReturnsBadRequest_WhenStockIsNegative()
    {
        // Arrange
        var insumo = new Insumo
        {
            InsumoId = Guid.NewGuid(),
            NombreInsumo = "Test Insumo",
            Stock = 100,
            UnidadMedida = "kg"
        };

        _context.Insumos.Add(insumo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.UpdateStock(insumo.InsumoId, -10);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
