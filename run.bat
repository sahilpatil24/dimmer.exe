@echo off
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -Command "if (Test-Path '.\dimmer.exe') { Unblock-File -Path '.\dimmer.exe' }"
start "" ".\dimmer.exe"
