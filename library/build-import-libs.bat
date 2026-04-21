@echo off
setlocal
dotnet run --file build-import-libs.cs -- -BuildDll %*