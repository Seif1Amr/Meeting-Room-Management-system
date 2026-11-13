@echo off
title MeetingHub Test Suite
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "%~dp0test.ps1"
pause
