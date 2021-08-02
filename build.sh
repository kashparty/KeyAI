#!/bin/bash
rm -r bin/Release

dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained

cd bin/Release/netcoreapp3.1/win-x64
mv publish win-x64-contained
zip -r win-x64-contained.zip win-x64-contained

cd ../linux-x64
mv publish linux-x64-contained
zip -r linux-x64-contained.zip linux-x64-contained

cd ../../
mv netcoreapp3.1/win-x64/win-x64-contained.zip windows-x64-contained.zip
mv netcoreapp3.1/linux-x64/linux-x64-contained.zip linux-x64-contained.zip

rm -r netcoreapp3.1
cd ../../

dotnet publish -c Release -r win-x64 --self-contained false
dotnet publish -c Release -r linux-x64 --self-contained false

cd bin/Release/netcoreapp3.1/win-x64
mv publish win-x64
zip -r win-x64.zip win-x64

cd ../linux-x64
mv publish linux-x64
zip -r linux-x64.zip linux-x64

cd ../../
mv netcoreapp3.1/win-x64/win-x64.zip windows-x64.zip
mv netcoreapp3.1/linux-x64/linux-x64.zip linux-x64.zip

rm -r netcoreapp3.1
cd ../../