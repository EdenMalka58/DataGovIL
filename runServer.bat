@echo off
title DataGovIL API
cd /d "%~dp0"

echo Starting DataGovIL API on http://localhost:5080 (Swagger: http://localhost:5080/swagger)
dotnet run --project src\DataGovIL.Api\DataGovIL.Api.csproj --launch-profile http

pause
