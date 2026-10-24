# KOBNEXA ROYAL — GitHub Setup (Windows PC + Muse sharing)

This is how the same project lives on Wilno's Windows PC and on Muse's computer at the same time.

## One-time setup

1. On GitHub.com, create an empty repository. Suggested name: `kobnexa-royal`.
   Do NOT add a README, .gitignore, or license in GitHub — the project already has its own.
2. On the Windows PC, download the KOBNEXA ROYAL project ZIP from Muse, unzip it,
   and open the folder once in Unity Hub (Unity 6) so Unity creates its Library folder.
   (Library is generated locally and is never committed — see .gitignore.)
3. In a terminal inside the unzipped `kobnexa-royal` folder, run:
   ```
   git init
   git add .
   git commit -m "KOBNEXA ROYAL - initial project"
   git branch -M main
   git remote add origin https://github.com/<your-github-username>/kobnexa-royal.git
   git push -u origin main
   ```
4. In Muse chat, connect GitHub (the connect link Muse provides) and then use the
   "Select repositories" step to let the Muse GitHub App access the `kobnexa-royal` repo.
5. Tell Muse the exact repo name (for example `kobnexa-royal`). Muse will then verify
   it can read the repo before making any change.

## Everyday flow after setup

- Before playing on the PC: `git pull` (gets Muse's latest changes).
- After changing something on the PC in Unity: `git add .`, `git commit -m "..."`,
  `git push` (Muse can then see your changes).
- When Muse finishes a batch of work, it pushes/updates the repo and tells you —
  pull on the PC to see it in Unity.

## What is (and is not) in the repo

- Committed: Assets, Packages, ProjectSettings, Documentation, README, .gitignore.
- Never committed (generated on each computer): Library/, Temp/, Build/, Logs/,
  UserSettings/, *.csproj, *.sln, .vs/.
- Important: Unity `.meta` files ARE committed. Never delete them — they are what
  links scripts and scenes together.
