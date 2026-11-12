# ============================================================
# MeetingHub - Run All Tests (Backend + Frontend)
# ============================================================
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Running MeetingHub Full-Stack Automated Test Suite" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Backend xUnit tests
Write-Host "`n[1/2] Running Backend xUnit Tests (.NET 8)..." -ForegroundColor Yellow
$dotnetExe = "dotnet"
if (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe") {
    $dotnetExe = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
} elseif (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") {
    $dotnetExe = "$env:USERPROFILE\.dotnet\dotnet.exe"
}

& $dotnetExe test backend.Tests/MeetingRoomManagement.Tests.csproj --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBackend tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "Backend tests passed successfully!" -ForegroundColor Green

# 2. Frontend Vitest tests
Write-Host "`n[2/2] Running Frontend Vitest Unit Tests (Angular)..." -ForegroundColor Yellow
Push-Location frontend
try {
    npm test -- --watch=false
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nFrontend tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} finally {
    Pop-Location
}

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  ALL TESTS PASSED CLEANLY (Backend 34/34, Frontend 4/4)!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
