param(
    [string]$OutputPath = "$env:USERPROFILE\Desktop\TransportExpenditureTracker"
)

# Kill running instance if any
$exeName = "TransportExpenditureTracker.Desktop.exe"
Get-Process -Name ($exeName -replace '\.exe$', '') -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean output directory to avoid stale files
if (Test-Path -LiteralPath $OutputPath) {
    Remove-Item -Path "$OutputPath\*" -Recurse -Force -ErrorAction SilentlyContinue
}

dotnet publish "TransportExpenditureTracker.Desktop\TransportExpenditureTracker.Desktop.csproj" `
    -c Release `
    -o $OutputPath `
    --nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host "Published to: $OutputPath" -ForegroundColor Green
} else {
    Write-Host "Publish failed" -ForegroundColor Red
}