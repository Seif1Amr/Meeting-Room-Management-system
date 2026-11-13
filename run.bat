@echo off
title MeetingHub - Space Management System
cd /d "%~dp0"

echo ============================================================
echo   MeetingHub Full-Stack Application Launcher
echo ============================================================
echo.

:: 1. Check if Docker is available and running
where docker >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    docker info >nul 2>&1
    if %ERRORLEVEL% EQU 0 (
        echo [INFO] Docker detected and running. Launching with Docker Compose...
        echo -> Web Application: http://localhost:4200
        echo -> Swagger API Docs: http://localhost:5000/swagger
        echo.
        start http://localhost:4200
        docker compose up --build
        goto :eof
    )
)

:: 2. Fallback to native .NET 8 + Angular startup
echo Starting backend (.NET 8) and frontend (Angular)...
powershell -ExecutionPolicy Bypass -File "%~dp0start.ps1"
