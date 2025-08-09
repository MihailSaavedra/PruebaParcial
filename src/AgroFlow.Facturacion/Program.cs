using AgroFlow.Facturacion.Consumers;
using AgroFlow.Facturacion.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Conexión a MariaDB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
}

var serverVersion = new MariaDbServerVersion(new Version(10, 11, 0));
builder.Services.AddDbContext<FacturacionDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// 2. Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(config => {
    // Registrar el consumer
    config.AddConsumer<CosechaEnProcesoConsumer>();

    config.UsingRabbitMq((ctx, cfg) => {
        var rabbitMqHost = builder.Configuration["RabbitMQHost"] ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h => {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // Configurar cola para recibir eventos de cosechas en proceso
        cfg.ReceiveEndpoint("cola_facturacion", e => {
            e.ConfigureConsumer<CosechaEnProcesoConsumer>(ctx);
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
        Title = "AgroFlow Facturación API", 
        Version = "v1",
        Description = "API para gestión de facturación de cosechas"
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
    var context = scope.ServiceProvider.GetRequiredService<FacturacionDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Base de datos de facturación verificada/creada exitosamente");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al verificar/crear la base de datos de facturación");
    }
}

// 7. Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroFlow Facturación API v1");
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
            logger.LogError(error.Error, "Error no controlado en la aplicación de facturación");
            
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

app.Logger.LogInformation("AgroFlow Facturación API iniciada correctamente");
app.Run();
