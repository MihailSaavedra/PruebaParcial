using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar YARP (Yet Another Reverse Proxy)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Configurar Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("DefaultPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

// 4. Servicios básicos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AgroFlow API Gateway", 
        Version = "v1",
        Description = "Gateway unificado para todos los servicios de AgroFlow"
    });
});

// 5. Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 6. Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroFlow Gateway API v1");
        c.RoutePrefix = "swagger";
    });
}

// 7. Middleware
app.UseCors("AllowAll");
app.UseRateLimiter();

// 8. Servir archivos estáticos del frontend
app.UseDefaultFiles();
app.UseStaticFiles();

// 9. Middleware personalizado para logging de requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path} from {RemoteIp}", 
        context.Request.Method, 
        context.Request.Path, 
        context.Connection.RemoteIpAddress);
    
    await next();
});

// 10. Mapear rutas del proxy
app.MapReverseProxy();

// 11. Ruta de health check
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Services = new {
        Central = "http://localhost:5000",
        Inventario = "http://localhost:5001", 
        Facturacion = "http://localhost:5098"
    }
});

// 12. Ruta de información
app.MapGet("/info", () => new {
    Name = "AgroFlow API Gateway",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Uptime = DateTime.UtcNow
});

app.Logger.LogInformation("AgroFlow API Gateway iniciado correctamente");
app.Run();
