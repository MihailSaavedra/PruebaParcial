@echo off
echo ===============================================
echo    Configuración de Desarrollo AgroFlow
echo ===============================================

echo.
echo [1/6] Verificando Docker...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker no está instalado o no está en el PATH
    echo Por favor instale Docker Desktop desde: https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)
echo Docker encontrado ✓

echo.
echo [2/6] Verificando .NET 9...
dotnet --version | findstr "9." >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 9 no está instalado
    echo Por favor instale .NET 9 desde: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)
echo .NET 9 encontrado ✓

echo.
echo [3/6] Restaurando paquetes NuGet...
dotnet restore

echo.
echo [4/6] Iniciando servicios de infraestructura...
docker-compose up -d

echo.
echo [5/6] Esperando que los servicios estén listos...
timeout /t 20

echo.
echo [6/6] Aplicando migraciones de base de datos...
echo Configurando base de datos Central (PostgreSQL)...
cd src\AgroFlow.Central
dotnet ef database update 2>nul
cd ..\..

echo.
echo ===============================================
echo Configuración de desarrollo completada!
echo.
echo Servicios disponibles:
echo - PostgreSQL: localhost:5432 (Central)
echo - MySQL: localhost:3306 (Inventario)  
echo - MariaDB: localhost:3307 (Facturación)
echo - RabbitMQ: localhost:5672 (AMQP)
echo - RabbitMQ Management: http://localhost:15672
echo.
echo Para iniciar el sistema ejecute: scripts\start-all.bat
echo ===============================================
echo.
pause
