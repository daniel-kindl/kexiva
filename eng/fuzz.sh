#!/usr/bin/env bash
# Nightly seed-corpus fuzz. Not part of eng/verify.sh.
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

dotnet restore --locked-mode
dotnet build tests/Kexiva.Fuzz/Kexiva.Fuzz.csproj -c Release --no-restore --warnaserror
dotnet test tests/Kexiva.Fuzz/Kexiva.Fuzz.csproj -c Release --no-build
