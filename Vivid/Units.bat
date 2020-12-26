@echo off
dotnet build Vivid.csproj -c Units
echo Optimization is disabled
echo.
.\bin\Units\netcoreapp3.1\Vivid
echo.
echo Optimization is enabled
echo.
.\bin\Units\netcoreapp3.1\Vivid -O

del Unit_*.asm
del Unit_*.obj
del Unit_*.dll
del Unit_*.exe