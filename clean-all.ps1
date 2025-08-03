Write-Host "Cleaning WebUI build..." -ForegroundColor Cyan

# Save current location
$originalLocation = Get-Location

Write-Host "`nCleaning WebUI.Desktop..." -ForegroundColor Yellow
Set-Location src\WebUI.Desktop
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
Set-Location ..\samples\HelloWorld
dotnet clean 2>&1 | Out-Null
Remove-Item -Path bin, obj, panels -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning test-webui-package sample..." -ForegroundColor Yellow
Set-Location ..\..\..\samples\test-webui-package
dotnet clean 2>&1 | Out-Null
Remove-Item -Path bin, obj -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning solution packages..." -ForegroundColor Yellow
Remove-Item -Path $originalLocation\src\packages\* -Recurse -Force -ErrorAction SilentlyContinue

# Return to original location
Set-Location $originalLocation
Write-Host "`n✅ Clean complete!" -ForegroundColor Green