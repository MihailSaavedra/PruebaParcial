@echo off
echo ===============================================
echo    Compilando Sistema AgroFlow
echo ===============================================

echo.
echo [1/4] Compilando AgroFlow.Central...
cd src\AgroFlow.Central
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error compilando AgroFlow.Central
    pause
    exit /b 1
)
cd ..\..

echo.
echo [2/4] Compilando AgroFlow.Inventario...
cd src\AgroFlow.Inventario
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error compilando AgroFlow.Inventario
    pause
    exit /b 1
)
cd ..\..

echo.
echo [3/4] Compilando AgroFlow.Facturacion...
cd src\AgroFlow.Facturacion
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error compilando AgroFlow.Facturacion
    pause
    exit /b 1
)
cd ..\..

echo.
echo [4/4] Compilando AgroFlow.Gateway...
cd src\AgroFlow.Gateway
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error compilando AgroFlow.Gateway
    pause
    exit /b 1
)
cd ..\..

echo.
echo ===============================================
echo Compilación completada exitosamente!
echo ===============================================
echo.
pause
