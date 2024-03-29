#!/bin/bash
dotnet test Vivid.csproj -c Units /p:IncludeTestAssembly=true /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:./coverage.cobertura.xml -targetdir:./Reports/ -sourcedirs:$PWD -filefilters:"-*Phases*\*;-*Unit*\*;-*Debug*;-*ElfFormat*;-*StaticLibraryFormat*;-*StaticLibraryImporter*;-*ObjectExporter*;-*GarbageCollector*"