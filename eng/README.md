# Engineering Scripts

## Verification

Primary command (Linux/macOS):
```bash
./eng/verify.sh
```

Windows:
```powershell
.\eng\verify.ps1
```

Validates:
0. Prerequisites (git, SDK from global.json, full git history)
1. Locked dependency restore with NuGet audit (advisories fail the restore)
2. Release build
3. Zero warnings and errors
4. Static analysis
5. Formatting and style checks
6. Unit tests
7. Compatibility tests
8. Architecture checks
9. Coverage thresholds
10. Public API checks
11. Repository policy checks

Coverage applies only to production source files in the Kexiva assembly. It requires at least 80% line coverage and 70% branch coverage when those lines or branches exist. Generated files, AssemblyInfo.cs, tests, and verification infrastructure are excluded. The current foundation has no coverable production source lines, so coverage thresholds apply when production code is added.

The Public API Analyzer checks the shipped and unshipped API baselines during the Release build. Add approved API changes to PublicAPI.Unshipped.txt.

The scripts require the .NET SDK pinned in global.json and restore dependencies from committed NuGet lock files. The Windows script also requires PowerShell 7.

Full fuzzing, mutation, and performance regression suites run separately.
