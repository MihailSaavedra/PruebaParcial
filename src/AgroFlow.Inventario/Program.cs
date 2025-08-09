using AgroFlow.Inventario.Consumers;
using AgroFlow.Inventario.Data;
using AgroFlow.Inventario.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 29)); // Especifica una versión de MySQL
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// 2. Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(config => {
    // Registra nuestro consumidor para que MassTransit sepa de su existencia
    config.AddConsumer<NuevaCosechaConsumer>();

    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(builder.Configuration["RabbitMQHost"] ?? "localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        // Configura el "Receive Endpoint" (la cola)
        // Esto crea la cola "cola_inventario" si no existe
        // y la suscribe para recibir mensajes de tipo 'NuevaCosechaEvent'.
        cfg.ReceiveEndpoint("cola_inventario", e => {
            e.ConfigureConsumer<NuevaCosechaConsumer>(ctx);
        });
    });
});

// 3. Configurar CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Agregar servicios estándar de API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AgroFlow Inventario API", 
        Version = "v1",
        Description = "API para gestión de inventario de insumos agrícolas"
    });
});

// 5. Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 6. Crear la base de datos si no existe (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        
        // Seed data inicial si no hay insumos
        if (!await context.Insumos.AnyAsync())
        {
            var insumosIniciales = new Insumo[]
            {
                new Insumo { InsumoId = Guid.NewGuid(), NombreInsumo = "Semilla Arroz L-23", Stock = 1000, UnidadMedida = "kg", Categoria = "Semillas" },
                new Insumo { InsumoId = Guid.NewGuid(), NombreInsumo = "Fertilizante N-PK", Stock = 500, UnidadMedida = "kg", Categoria = "Fertilizantes" },
                new Insumo { InsumoId = Guid.NewGuid(), NombreInsumo = "Semilla Maíz Híbrido", Stock = 800, UnidadMedida = "kg", Categoria = "Semillas" },
                new Insumo { InsumoId = Guid.NewGuid(), NombreInsumo = "Herbicida Glifosato", Stock = 200, UnidadMedida = "L", Categoria = "Pesticidas" },
                new Insumo { InsumoId = Guid.NewGuid(), NombreInsumo = "Fertilizante Orgánico", Stock = 300, UnidadMedida = "kg", Categoria = "Fertilizantes" }
            };
            
            context.Insumos.AddRange(insumosIniciales);
            await context.SaveChangesAsync();
            app.Logger.LogInformation("Datos iniciales de inventario creados");
        }
        
        app.Logger.LogInformation("Base de datos de inventario verificada/creada exitosamente");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al verificar/crear la base de datos de inventario");
    }
}

// 7. Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroFlow Inventario API v1");
        c.RoutePrefix = string.Empty; // Para que Swagger esté en la raíz
    });
    app.UseCors("DevelopmentPolicy");
}

// 8. Middleware de manejo de errores global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Error no controlado en la aplicación de inventario");
            
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Error interno del servidor",
                message = app.Environment.IsDevelopment() ? error.Error.Message : "Ha ocurrido un error inesperado"
            }));
        }
    });
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("AgroFlow Inventario API iniciada correctamente");
app.Run();