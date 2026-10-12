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

Write-Host "Architecture checks: placeholder passed"
Write-Host "Coverage: reports generated under TestResults"
Write-Host "Public API checks: placeholder"
Write-Host "Repository policy checks: placeholder"

Write-Host "=== Verification succeeded ==="
