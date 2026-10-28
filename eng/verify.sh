#!/usr/bin/env bash
# Full local and CI verification. Keep in sync with eng/verify.ps1.
# Kexiva.Verify checks that both scripts run the same dotnet command sequence.
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

results_directory="TestResults/verification-$(date +%s)-$$"
mkdir -p "$results_directory"

echo "=== Kexiva Verification ==="

echo "Checking prerequisites..."
command -v git >/dev/null 2>&1 || { echo "git is not on PATH (Nerdbank.GitVersioning needs git history)." >&2; false; }
command -v dotnet >/dev/null 2>&1 || { echo "dotnet is not on PATH. Install the SDK version in global.json." >&2; false; }
active_sdk="$(dotnet --version)" || { echo "No installed SDK satisfies global.json." >&2; false; }
echo "Active SDK: ${active_sdk} (global.json pins the SDK; rollForward latestPatch)"
shallow_repository="$(git rev-parse --is-shallow-repository)" || { echo "git rev-parse failed. Run the script inside the Kexiva git work tree." >&2; false; }
[ "$shallow_repository" = "false" ] || { echo "Shallow clone. Nerdbank.GitVersioning needs full history (fetch-depth: 0)." >&2; false; }
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

echo "Restoring locked dependencies (NuGet audit fails restore on any advisory)..."
dotnet restore --locked-mode

echo "Building Release with warnings as errors..."
dotnet build -c Release --no-restore --warnaserror

echo "Checking formatting and analyzers..."
dotnet format --verify-no-changes --severity info --no-restore

echo "Running unit and architecture tests with coverage..."
dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --results-directory "$results_directory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*"

echo "Running public API compatibility tests with coverage..."
dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build --results-directory "$results_directory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*"

echo "Checking coverage and repository policies..."
dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- "$results_directory"

echo "=== Verification succeeded ==="
