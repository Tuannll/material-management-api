@echo off
setlocal enabledelayedexpansion

echo ==========================================================
echo     CD MACHINE (MACHINE B) - AUTOMATED SETUP ^& RUNNER
echo ==========================================================

cd /d "%~dp0"

:: 1. Check Python
where python >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Python is not installed or not in PATH.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('python --version') do echo [OK] Found Python: %%v

:: 2. Check Docker
where docker >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker is not installed or not in PATH.
    pause
    exit /b 1
)

docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker Desktop or daemon is not running.
    echo Please start Docker Desktop and run this script again.
    pause
    exit /b 1
)
echo [OK] Docker daemon is running.

:: 3. Detect Docker Compose
docker compose version >nul 2>&1
if %errorlevel% equ 0 (
    set COMPOSE_CMD=docker compose
) else (
    set COMPOSE_CMD=docker-compose
)
echo [OK] Docker Compose detected: %COMPOSE_CMD%

:: 4. Prepare .env file
if not exist ".env" (
    if exist ".env.example" (
        echo [INFO] .env not found. Creating from .env.example...
        copy /y ".env.example" ".env" >nul
        echo [OK] Created .env file.
    ) else if exist "env.example" (
        copy /y "env.example" ".env" >nul
        echo [OK] Created .env file from env.example.
    )
) else (
    echo [OK] Found existing .env file.
)

:: 5. Start SQL Server container
echo [INFO] Starting SQL Server database container...
%COMPOSE_CMD% up -d sqlserver
if %errorlevel% neq 0 (
    echo [WARNING] Failed to start sqlserver container or still initializing.
)

:: 6. Display IP and Webhook instructions
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4 Address" /c:"IPv4" 2^>nul') do (
    set IP_CANDIDATE=%%a
    set IP_CANDIDATE=!IP_CANDIDATE: =!
    if not "!IP_CANDIDATE!"=="" (
        set CD_IP=!IP_CANDIDATE!
    )
)
if "%CD_IP%"=="" set CD_IP=localhost

echo ==========================================================
echo  [CD Machine Ready]
echo  Local Webhook URL  : http://localhost:9000/deploy
echo  Network Webhook URL: http://%CD_IP%:9000/deploy
echo  Healthcheck URL    : http://%CD_IP%:9000/health
echo  Web API Port       : 8080
echo.
echo  Sample trigger command from Jenkins (Machine A):
echo    curl -X POST http://%CD_IP%:9000/deploy ^
echo         -H "Content-Type: application/json" ^
echo         -d "{\"tag\": \"15\"}"
echo ==========================================================

:: 7. Start Webhook Receiver
echo [INFO] Starting Webhook Receiver on port 9000...
python "%~dp0webhook_receiver.py"

pause
