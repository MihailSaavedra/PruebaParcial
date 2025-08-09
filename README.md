# 🌾 AgroFlow - Sistema de Gestión Agrícola
:)
AgroFlow es un sistema completo de gestión agrícola basado en microservicios que permite administrar agricultores, cosechas, inventario de insumos y facturación de manera integrada.

## 🏗️ Arquitectura del Sistema

### Microservicios

- **🏢 AgroFlow.Central** - Gestión de agricultores y cosechas (PostgreSQL)
- **📦 AgroFlow.Inventario** - Control de inventario de insumos (MySQL)  
- **💰 AgroFlow.Facturacion** - Sistema de facturación (MariaDB)
- **🌐 AgroFlow.Gateway** - API Gateway y frontend web

### Tecnologías

- **.NET 9** - Framework principal
- **Entity Framework Core** - ORM para bases de datos
- **MassTransit + RabbitMQ** - Mensajería entre microservicios
- **YARP** - Reverse proxy para API Gateway
- **Docker** - Contenedores para infraestructura
- **HTML/CSS/JavaScript** - Frontend web sin frameworks

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Git

### Instalación

1. **Clonar el repositorio**
   ```bash
   git clone <url-del-repo>
   cd AgroFlow
   ```

2. **Configurar el entorno de desarrollo**
   ```bash
   scripts\setup-dev.bat
   ```

3. **Iniciar el sistema completo**
   ```bash
   scripts\start-all.bat
   ```

4. **Acceder a la aplicación**
   - **Frontend**: http://localhost:8080
   - **API Gateway**: http://localhost:8080/swagger

## 📋 URLs del Sistema

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **Frontend Web** | http://localhost:8080 | Interfaz principal del usuario |
| **API Gateway** | http://localhost:8080/swagger | Documentación unificada |
| **Central API** | http://localhost:5000 | Gestión de agricultores y cosechas |
| **Inventario API** | http://localhost:5001 | Control de inventario |
| **Facturación API** | http://localhost:5098 | Sistema de facturación |
| **RabbitMQ Management** | http://localhost:15672 | Panel de administración de colas |

### Credenciales por defecto

- **RabbitMQ**: usuario: `guest`, contraseña: `guest`
- **PostgreSQL**: usuario: `admin`, contraseña: `admin`
- **MySQL**: usuario: `root`, contraseña: `admin`
- **MariaDB**: usuario: `root`, contraseña: `admin`

## 🔄 Flujo de Trabajo

### 1. Gestión de Agricultores
- Registro de agricultores con información de finca y contacto
- Actualización de datos y gestión de registros

### 2. Registro de Cosechas
- Crear nuevas cosechas asociadas a agricultores
- Seguimiento de estados: `REGISTRADA` → `EN_PROCESO` → `FACTURADA` → `COMPLETADA`

### 3. Control de Inventario
- Gestión automática de insumos cuando se registran cosechas
- Alertas de stock bajo
- Ajuste manual de inventarios

### 4. Facturación Automática
- Generación automática de facturas cuando cosechas pasan a `EN_PROCESO`
- Cálculo de precios por producto
- Gestión de estados de pago

## 🔧 Comandos Útiles

### Desarrollo
```bash
# Compilar todo el sistema
scripts\build-all.bat

# Iniciar servicios
scripts\start-all.bat

# Detener servicios  
scripts\stop-all.bat

# Reiniciar solo Docker
docker-compose down && docker-compose up -d
```

### Base de Datos
```bash
# Aplicar migraciones Central
cd src\AgroFlow.Central
dotnet ef database update

# Crear nueva migración
dotnet ef migrations add NombreMigracion
```

## 📊 Estructura del Proyecto

```
AgroFlow/
├── src/
│   ├── AgroFlow.Central/          # Microservicio principal
│   ├── AgroFlow.Inventario/       # Gestión de inventario
│   ├── AgroFlow.Facturacion/      # Sistema de facturación
│   └── AgroFlow.Gateway/          # API Gateway + Frontend
├── scripts/                       # Scripts de automatización
├── docker-compose.yml            # Configuración de infraestructura
└── README.md                     # Este archivo
```

## 🌟 Características Principales

### ✅ Implementado
- ✅ **Microservicios completos** con APIs REST
- ✅ **Frontend web responsive** sin frameworks
- ✅ **Mensajería asíncrona** entre servicios
- ✅ **API Gateway** con proxy reverso
- ✅ **Bases de datos múltiples** (PostgreSQL, MySQL, MariaDB)
- ✅ **Gestión de estados** de cosechas
- ✅ **Facturación automática** basada en eventos
- ✅ **Control de inventario** automático
- ✅ **Scripts de automatización** para desarrollo

### 🔄 Eventos del Sistema

1. **Nueva Cosecha** → Actualiza inventario de insumos
2. **Cosecha EN_PROCESO** → Genera factura automáticamente  
3. **Inventario Bajo** → Alertas en dashboard

## 🐛 Solución de Problemas

### Error de conexión a base de datos
```bash
# Verificar que Docker esté ejecutándose
docker ps

# Reiniciar servicios Docker
docker-compose down && docker-compose up -d
```

### Puerto ocupado
- Verificar que los puertos 5000, 5001, 5098, 8080 estén disponibles
- Cambiar puertos en `launchSettings.json` si es necesario

### Error de compilación
```bash
# Limpiar y restaurar paquetes
dotnet clean
dotnet restore
dotnet build
```

## 🤝 Contribución

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## 👨‍💻 Autor

Desarrollado con ❤️ para la gestión agrícola moderna.

---

**¿Necesitas ayuda?** Abre un issue en el repositorio o consulta la documentación de cada microservicio.
