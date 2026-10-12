#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

results_directory="TestResults/verification-$(date +%s)-$$"
mkdir -p "$results_directory"

echo "=== Kexiva Verification ==="

echo "Restoring locked dependencies..."
dotnet restore --locked-mode

echo "Building Release with warnings as errors..."
dotnet build -c Release --no-restore --warnaserror

echo "Checking formatting and analyzers..."
dotnet format --verify-no-changes --severity info --no-restore

echo "Running unit and architecture tests with coverage..."
dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --settings eng/coverage.runsettings --collect:"XPlat Code Coverage" --results-directory "$results_directory"

echo "Running public API compatibility tests..."
dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build

echo "Checking coverage and repository policies..."
dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- "$results_directory"

echo "=== Verification succeeded ==="
