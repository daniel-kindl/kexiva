#!/usr/bin/env bash
set -euo pipefail

echo "=== Kexiva Verification ==="

# 1. Locked dependency restore
echo "Restoring dependencies..."
dotnet restore --locked-mode

# 2. Release build
echo "Building Release..."
dotnet build -c Release --no-restore

# 3-5. Zero warnings, static analysis, formatting (enforced via build props)
echo "Format and style verification..."
dotnet format --verify-no-changes --severity info --no-restore

# 6. Unit tests
echo "Running unit tests..."
dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults

# 7. Compatibility tests
echo "Running compatibility tests..."
dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build

# 8. Architecture checks (placeholder — expand with NetArchTest later)
echo "Architecture checks: placeholder passed"

# 9. Coverage thresholds (placeholder — add thresholds when production code exists)
echo "Coverage: reports generated under TestResults"

# 10. Public API checks (placeholder)
echo "Public API checks: placeholder"

# 11. Repository policy checks (placeholder)
echo "Repository policy checks: placeholder"

echo "=== Verification succeeded ==="
