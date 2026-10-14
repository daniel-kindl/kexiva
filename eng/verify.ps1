# Full local and CI verification. Keep in sync with eng/verify.sh.
# Kexiva.Verify checks that both scripts run the same dotnet command sequence.
$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repositoryRoot

$resultsDirectory = Join-Path $repositoryRoot (Join-Path "TestResults" ("verification-" + [guid]::NewGuid().ToString("N")))
New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

function Invoke-DotNet([scriptblock] $Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "A dotnet command failed with exit code $LASTEXITCODE."
    }
}

Write-Host "=== Kexiva Verification ==="

Write-Host "Checking prerequisites..."
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw "git is not on PATH (Nerdbank.GitVersioning needs git history)." }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "dotnet is not on PATH. Install the SDK version in global.json." }
$activeSdk = & dotnet --version
if ($LASTEXITCODE -ne 0) { throw "No installed SDK satisfies global.json." }
Write-Host "Active SDK: $activeSdk (global.json pins the SDK; rollForward latestPatch)"
if ((& git rev-parse --is-shallow-repository) -ne "false") { throw "Shallow clone. Nerdbank.GitVersioning needs full history (fetch-depth: 0)." }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

Write-Host "Restoring locked dependencies (NuGet audit fails restore on any advisory)..."
Invoke-DotNet { dotnet restore --locked-mode }

Write-Host "Building Release with warnings as errors..."
Invoke-DotNet { dotnet build -c Release --no-restore --warnaserror }

Write-Host "Checking formatting and analyzers..."
Invoke-DotNet { dotnet format --verify-no-changes --severity info --no-restore }

Write-Host "Running unit and architecture tests with coverage..."
Invoke-DotNet { dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --settings eng/coverage.runsettings --collect:"XPlat Code Coverage" --results-directory "$resultsDirectory" }

Write-Host "Running public API compatibility tests..."
Invoke-DotNet { dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build }

Write-Host "Checking coverage and repository policies..."
Invoke-DotNet { dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- "$resultsDirectory" }

Write-Host "=== Verification succeeded ==="
