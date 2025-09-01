@echo off
echo Starting Camera Management API and Webhook Apps...

start "CameraManagementAPI" cmd /k "cd /d C:\test\CameraManagementAPI && dotnet run"
timeout /t 3 /nobreak >nul

start "WebhookApp1" cmd /k "cd /d C:\test\WebhookApp1 && dotnet run"
timeout /t 2 /nobreak >nul

start "WebhookApp2" cmd /k "cd /d C:\test\WebhookApp2 && dotnet run"

echo All applications are starting...
echo - CameraManagementAPI: http://localhost:7080
echo - WebhookApp1: http://localhost:7081
echo - WebhookApp2: http://localhost:7082
pause