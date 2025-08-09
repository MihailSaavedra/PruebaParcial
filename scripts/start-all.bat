@echo off
echo ===============================================
echo    Iniciando Sistema AgroFlow
echo ===============================================

echo.
echo [1/5] Iniciando servicios de infraestructura...
start "Docker Services" cmd /k "docker-compose up -d && echo Servicios de base de datos iniciados. && timeout /t 5"

echo.
echo [2/5] Esperando que las bases de datos estén listas...
timeout /t 15

echo.
echo [3/5] Iniciando microservicio Central...
start "AgroFlow Central" cmd /k "cd src\AgroFlow.Central && dotnet run"

echo.
echo [4/5] Iniciando microservicio Inventario...
start "AgroFlow Inventario" cmd /k "cd src\AgroFlow.Inventario && dotnet run"

echo.
echo [5/5] Iniciando microservicio Facturación...
start "AgroFlow Facturacion" cmd /k "cd src\AgroFlow.Facturacion && dotnet run"

echo.
echo [6/6] Iniciando API Gateway...
timeout /t 10
start "AgroFlow Gateway" cmd /k "cd src\AgroFlow.Gateway && dotnet run"

echo.
echo ===============================================
echo Sistema AgroFlow iniciado correctamente!
echo.
echo URLs disponibles:
echo - Frontend: http://localhost:8080
echo - API Gateway: http://localhost:8080/swagger
echo - Central API: http://localhost:5000
echo - Inventario API: http://localhost:5001  
echo - Facturación API: http://localhost:5098
echo - RabbitMQ Management: http://localhost:15672
echo ===============================================
echo.
pause
