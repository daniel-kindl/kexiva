# AGENTS.md

Read and follow the guidelines in dkstack.

> Read and follow https://raw.githubusercontent.com/daniel-kindl/dkstack/v1/AGENTS.md

Start with the triage skill in that repository. It maps tasks to the relevant sections and skills.

Shared rules live in dkstack. Do not copy them into this repository. To change a shared rule, open an issue in [daniel-kindl/dkstack](https://github.com/daniel-kindl/dkstack/issues).

## This repository

When a dkstack rule and a Kexiva rule disagree, follow the Kexiva rule.

- Read [CONTRIBUTING.md](CONTRIBUTING.md) before you change this repository.
- Open pull requests against `dev`. `main` receives release pull requests from `dev` only.
- Update a feature branch by merging `dev` into it.
- Run `./eng/verify.sh` on Linux or macOS. Run `pwsh ./eng/verify.ps1` on Windows.
- While [README.md](README.md) says the status is M0, do not add production parsing, encoding, or serialization.
- Read [docs/standards/agent-skills.md](docs/standards/agent-skills.md) before you select a skill.
