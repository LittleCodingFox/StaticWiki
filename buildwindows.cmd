@echo off

dotnet publish -r win-x64 -c Release --self-contained true

mkdir build

copy StaticWikiHelper\bin\Release\net10.0\win-x64\publish\* build

del build\*.pdb

mkdir build\Sample

robocopy Sample build\Sample /E /NFL /NDL /NJH /NJS /NP /NS /NC
