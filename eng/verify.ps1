# Windows equivalent of eng/verify.sh
# Requires PowerShell and .NET SDK

$ErrorActionPreference = "Stop"

Write-Host "=== Kexiva Verification ==="

Write-Host "Restoring dependencies..."
dotnet restore --locked-mode

Write-Host "Building Release..."
dotnet build -c Release --no-restore

Write-Host "Format and style verification..."
dotnet format --verify-no-changes --severity info --no-restore

Write-Host "Running unit tests..."
dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults

Write-Host "Running compatibility tests..."
dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build

Write-Host "Architecture checks: NOT CONFIGURED — expand with NetArchTest before declaring M0 complete"
exit 1

# Remaining checks intentionally not reached until implemented
# Coverage thresholds: NOT CONFIGURED
# Public API checks: NOT CONFIGURED
# Repository policy checks: NOT CONFIGURED

Write-Host "=== Verification succeeded ==="
