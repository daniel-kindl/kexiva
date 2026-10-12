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
1. Locked dependency restore
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

Full fuzzing, mutation, and performance regression suites run separately.
