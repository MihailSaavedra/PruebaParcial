using AgroFlow.Central.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Conexión a PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
}

builder.Services.AddDbContext<CentralDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((ctx, cfg) => {
        // Lee el host de RabbitMQ desde appsettings.json
        var rabbitMqHost = builder.Configuration["RabbitMQHost"] ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h => {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        
        cfg.ConfigureEndpoints(ctx);
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
        Title = "AgroFlow Central API", 
        Version = "v1",
        Description = "API para gestión central de agricultores y cosechas"
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
    var context = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Base de datos verificada/creada exitosamente");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al verificar/crear la base de datos");
    }
}

// 7. Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroFlow Central API v1");
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
            logger.LogError(error.Error, "Error no controlado en la aplicación");
            
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

app.Logger.LogInformation("AgroFlow Central API iniciada correctamente");
app.Run();

// Definición de los contratos de eventos para claridad.
// Idealmente, esto estaría en una librería compartida.
public record NuevaCosechaEvent(
    Guid CosechaId,
    string Producto,
    decimal Toneladas,
    DateTime Timestamp
);

public record CosechaEnProcesoEvent(
    Guid CosechaId,
    Guid AgricultorId,
    string NombreAgricultor,
    string Producto,
    decimal Toneladas,
    DateTime Timestamp
);