Write-Host "Cleaning WebUI experimental build..." -ForegroundColor Cyan

Write-Host "`nCleaning WebUI.Desktop..." -ForegroundColor Yellow
Set-Location experiments\src\WebUI.Desktop
dotnet clean 2>&1 | Out-Null
Remove-Item -Path bin, obj -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning WebUI.Api..." -ForegroundColor Yellow
Set-Location Api
Remove-Item -Path node_modules, dist -Recurse -Force -ErrorAction SilentlyContinue
Set-Location ..

Write-Host "Cleaning build tools..." -ForegroundColor Yellow
Set-Location tools\build
Remove-Item -Path node_modules -Recurse -Force -ErrorAction SilentlyContinue
Set-Location ..\..

Write-Host "Cleaning HelloWorld sample..." -ForegroundColor Yellow
Set-Location ..\..\samples\HelloWorld
dotnet clean 2>&1 | Out-Null
Remove-Item -Path bin, obj -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning test package..." -ForegroundColor Yellow
Set-Location ..\..\..\test-webui-package
dotnet clean 2>&1 | Out-Null
Remove-Item -Path bin, obj -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning packages..." -ForegroundColor Yellow
Set-Location ..\experiments
Remove-Item -Path packages\* -Recurse -Force -ErrorAction SilentlyContinue

Set-Location ..
Write-Host "`n✅ Clean complete!" -ForegroundColor Green