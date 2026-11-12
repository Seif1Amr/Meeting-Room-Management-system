# ============================================================
# MeetingHub - One-Click Local Launcher (Windows)
# ============================================================
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Launching MeetingHub Full-Stack Application" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# Find dotnet SDK executable
$dotnetExe = "dotnet"
if (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe") {
    $dotnetExe = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
} elseif (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") {
    $dotnetExe = "$env:USERPROFILE\.dotnet\dotnet.exe"
}

Write-Host "`n1. Starting .NET 8 Backend API on http://localhost:5000..." -ForegroundColor Yellow
$backendJob = Start-Process -FilePath $dotnetExe -ArgumentList "run --project backend/MeetingRoomManagement.Api.csproj --urls http://localhost:5000" -PassThru

Write-Host "   -> Swagger API documentation: http://localhost:5000/swagger" -ForegroundColor Green

Write-Host "`n2. Starting Angular Frontend on http://localhost:4200..." -ForegroundColor Yellow
Push-Location frontend
try {
    if (-not (Test-Path "node_modules")) {
        Write-Host "   Installing frontend dependencies (first-time run)..." -ForegroundColor Yellow
        npm install --legacy-peer-deps
    }
    Write-Host "   -> Frontend web application: http://localhost:4200" -ForegroundColor Green
    Write-Host "`nPress Ctrl+C to stop both frontend and backend.`n" -ForegroundColor Cyan
    npm start
} finally {
    Pop-Location
    if ($backendJob -and -not $backendJob.HasExited) {
        Stop-Process -Id $backendJob.Id -Force -ErrorAction SilentlyContinue
    }
}
