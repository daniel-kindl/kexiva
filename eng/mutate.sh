#!/usr/bin/env bash
# Nightly mutation run. Not part of eng/verify.sh.
# Skips when src/Kexiva has no mutable production source.
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

mutable_source=0
while IFS= read -r -d '' source_file; do
    mutable_source=1
    break
done < <(find src/Kexiva -name '*.cs' \
    ! -name 'AssemblyInfo.cs' \
    ! -path '*/bin/*' \
    ! -path '*/obj/*' \
    -print0)

if [ "$mutable_source" -eq 0 ]; then
    echo "No mutable production source. Mutation run skipped."
    exit 0
fi

dotnet tool restore
dotnet restore --locked-mode
dotnet build src/Kexiva/Kexiva.csproj -c Release --no-restore --warnaserror
dotnet build tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-restore --warnaserror
dotnet tool run dotnet-stryker -- --config-file eng/stryker-config.json
