@echo off
echo ===============================================
echo    Deteniendo Sistema AgroFlow
echo ===============================================

echo.
echo [1/2] Deteniendo servicios .NET...
taskkill /f /im dotnet.exe 2>nul
echo Servicios .NET detenidos.

echo.
echo [2/2] Deteniendo servicios Docker...
docker-compose down
echo Servicios Docker detenidos.

echo.
echo ===============================================
echo Sistema AgroFlow detenido correctamente!
echo ===============================================
echo.
pause
