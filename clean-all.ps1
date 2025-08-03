Write-Host "Cleaning WebUI build..." -ForegroundColor Cyan

# Clean all bin/obj folders in src directories
Write-Host "`nCleaning all bin/obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path "src" -Include bin,obj -Recurse -Directory | ForEach-Object {
    Write-Host "  Removing $($_.FullName)" -ForegroundColor Gray
    Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

# Clean NuGet packages
Write-Host "`nCleaning NuGet packages..." -ForegroundColor Yellow
Remove-Item -Path "packages\*" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n✅ Clean complete!" -ForegroundColor Green