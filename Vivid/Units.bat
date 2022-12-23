@echo off
dotnet build Vivid.csproj -c Units
echo Optimization is disabled
echo.
.\bin\Units\net7.0\Vivid
echo.
echo 'Optimization level 1'
echo.
.\bin\Units\net7.0\Vivid -O1
echo.
echo 'Optimization level 2'
echo.
.\bin\Units\net7.0\Vivid -O2

.\Clear.bat