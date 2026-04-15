#!/bin/sh

dotnet publish -r linux-x64 -c Release --self-contained true

mkdir build

cp -Rf StaticWikiHelper/bin/Release/net10.0/linux-x64/publish/* build

rm -Rf build/*.pdb

mkdir build/Sample

cp -Rf Sample build/Sample
