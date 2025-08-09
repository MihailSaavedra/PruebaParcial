using AgroFlow.Central.Controllers;
using AgroFlow.Central.Data;
using AgroFlow.Central.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgroFlow.Central.Tests;

public class AgricultoresControllerTests : IDisposable
{
    private readonly CentralDbContext _context;
    private readonly AgricultoresController _controller;
    private readonly Mock<ILogger<AgricultoresController>> _mockLogger;

    public AgricultoresControllerTests()
    {
        var options = new DbContextOptionsBuilder<CentralDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CentralDbContext(options);
        _mockLogger = new Mock<ILogger<AgricultoresController>>();
        _controller = new AgricultoresController(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAgricultores_ReturnsEmptyList_WhenNoAgricultores()
    {
        // Act
        var result = await _controller.GetAgricultores();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var agricultores = Assert.IsAssignableFrom<IEnumerable<Agricultor>>(okResult.Value);
        Assert.Empty(agricultores);
    }

    [Fact]
    public async Task GetAgricultores_ReturnsAgricultores_WhenAgricultoresExist()
    {
        // Arrange
        var agricultor = new Agricultor
        {
            AgricultorId = Guid.NewGuid(),
            Nombre = "Juan Pérez",
            Finca = "Finca La Esperanza",
            Ubicacion = "Valle del Cauca",
            Correo = "juan@example.com"
        };

        _context.Agricultores.Add(agricultor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAgricultores();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var agricultores = Assert.IsAssignableFrom<IEnumerable<Agricultor>>(okResult.Value);
        Assert.Single(agricultores);
        Assert.Equal("Juan Pérez", agricultores.First().Nombre);
    }

    [Fact]
    public async Task GetAgricultor_ReturnsNotFound_WhenAgricultorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.GetAgricultor(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAgricultor_ReturnsAgricultor_WhenAgricultorExists()
    {
        // Arrange
        var agricultor = new Agricultor
        {
            AgricultorId = Guid.NewGuid(),
            Nombre = "María García",
            Finca = "Finca El Progreso",
            Ubicacion = "Antioquia",
            Correo = "maria@example.com"
        };

        _context.Agricultores.Add(agricultor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAgricultor(agricultor.AgricultorId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAgricultor = Assert.IsType<Agricultor>(okResult.Value);
        Assert.Equal(agricultor.Nombre, returnedAgricultor.Nombre);
    }

    [Fact]
    public async Task CreateAgricultor_ReturnsCreatedResult_WithValidAgricultor()
    {
        // Arrange
        var agricultor = new Agricultor
        {
            Nombre = "Carlos López",
            Finca = "Finca San José",
            Ubicacion = "Cundinamarca",
            Correo = "carlos@example.com"
        };

        // Act
        var result = await _controller.CreateAgricultor(agricultor);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdAgricultor = Assert.IsType<Agricultor>(createdResult.Value);
        Assert.Equal(agricultor.Nombre, createdAgricultor.Nombre);
        Assert.NotEqual(Guid.Empty, createdAgricultor.AgricultorId);
    }

    [Fact]
    public async Task UpdateAgricultor_ReturnsNotFound_WhenAgricultorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agricultor = new Agricultor
        {
            AgricultorId = id,
            Nombre = "Test",
            Finca = "Test",
            Ubicacion = "Test",
            Correo = "test@example.com"
        };

        // Act
        var result = await _controller.UpdateAgricultor(id, agricultor);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAgricultor_ReturnsNotFound_WhenAgricultorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteAgricultor(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
