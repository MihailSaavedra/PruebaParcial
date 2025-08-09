# AgroFlow Central API

API central para la gestión de agricultores y cosechas en el sistema AgroFlow.

## Funcionalidades Implementadas

### ✅ Controladores API
- **AgricultorasController**: CRUD completo para agricultores
- **CosechasController**: CRUD completo para cosechas con gestión de estados

### ✅ Características
- Validación de modelos con Data Annotations
- Manejo de errores global
- Logging estructurado
- Configuración CORS para desarrollo
- Documentación automática con Swagger
- Integración con MassTransit/RabbitMQ
- Base de datos PostgreSQL con Entity Framework Core

## Endpoints Disponibles

### Agricultores (`/api/agricultoras`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/agricultoras` | Obtener todos los agricultores |
| GET | `/api/agricultoras/{id}` | Obtener agricultor por ID |
| POST | `/api/agricultoras` | Crear nuevo agricultor |
| PUT | `/api/agricultoras/{id}` | Actualizar agricultor |
| DELETE | `/api/agricultoras/{id}` | Eliminar agricultor |

### Cosechas (`/api/cosechas`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/cosechas` | Obtener todas las cosechas |
| GET | `/api/cosechas/{id}` | Obtener cosecha por ID |
| GET | `/api/cosechas/agricultor/{agricultorId}` | Obtener cosechas por agricultor |
| POST | `/api/cosechas` | Crear nueva cosecha |
| PUT | `/api/cosechas/{id}` | Actualizar cosecha |
| PUT | `/api/cosechas/{id}/estado` | Actualizar solo el estado |
| DELETE | `/api/cosechas/{id}` | Eliminar cosecha (solo si estado = REGISTRADA) |

## Estados de Cosecha

- `REGISTRADA`: Estado inicial
- `EN_PROCESO`: Cosecha en procesamiento
- `FACTURADA`: Cosecha facturada
- `COMPLETADA`: Proceso completado

## Configuración

### Base de Datos
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=agroflow_central;Username=admin;Password=admin"
  }
}
```

### RabbitMQ
```json
{
  "RabbitMQHost": "localhost",
  "RabbitMQ": {
    "Username": "guest",
    "Password": "guest",
    "Port": 5672
  }
}
```

## Ejecución

```bash
dotnet run
```

La API estará disponible en:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: http://localhost:5000 (en desarrollo)

## Eventos Publicados

### NuevaCosechaEvent
Se publica cuando se crea una nueva cosecha:
```json
{
  "cosechaId": "guid",
  "producto": "string",
  "toneladas": "decimal",
  "timestamp": "datetime"
}
```

## Estructura del Proyecto

```
src/AgroFlow.Central/
├── Controllers/          # Controladores API
├── Data/                # Contexto de base de datos
├── DTOs/                # Data Transfer Objects
├── Models/              # Modelos de dominio
├── Migrations/          # Migraciones de EF Core
└── Program.cs          # Configuración de la aplicación
```

## Dependencias

- .NET 9.0
- Entity Framework Core (PostgreSQL)
- MassTransit (RabbitMQ)
- Swashbuckle (Swagger)
- ASP.NET Core Web API

