@echo off
echo ===============================================
echo    Ejecutando Tests de AgroFlow
echo ===============================================

echo.
echo [1/3] Ejecutando tests de AgroFlow.Central...
cd tests\AgroFlow.Central.Tests
dotnet test --logger "console;verbosity=normal"
if %errorlevel% neq 0 (
    echo ERROR: Tests de AgroFlow.Central fallaron
    cd ..\..
    pause
    exit /b 1
)
cd ..\..

echo.
echo [2/3] Ejecutando tests de AgroFlow.Inventario...
cd tests\AgroFlow.Inventario.Tests
dotnet test --logger "console;verbosity=normal"
if %errorlevel% neq 0 (
    echo ERROR: Tests de AgroFlow.Inventario fallaron
    cd ..\..
    pause
    exit /b 1
)
cd ..\..

echo.
echo [3/3] Ejecutando todos los tests con cobertura...
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"

echo.
echo ===============================================
echo Tests completados exitosamente!
echo ===============================================
echo.
pause
