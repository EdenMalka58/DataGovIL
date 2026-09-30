@echo off
title DataGovIL Web
cd /d "%~dp0web"

if not exist node_modules (
    echo Installing npm packages...
    call npm install
    if errorlevel 1 goto :end
)

echo Starting web app on http://localhost:5173 (API calls are proxied to http://localhost:5080)
call npm run dev

:end
pause
