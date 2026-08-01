# Deploy Giant World with GitHub Actions (no Unity on your Mac)

Build WebGL in the cloud → download zip → upload to itch.io → get a live URL.

**You need:** GitHub account (free), Unity account (free Personal license), itch.io account (free).

---

## Overview

```
Push code to GitHub
    → Set Unity license secret (one-time, ~5 min)
    → GitHub Actions builds WebGL in the cloud (~20–40 min first time)
    → Download GiantWorld-WebGL-zip artifact
    → Upload to itch.io
    → Live URL: https://YOUR_USERNAME.itch.io/giant-world
```

---

## Step 1: Create a GitHub repository

### Option A — Giant World as its own repo (recommended)

```bash
cd /Users/yogeshyadav/Interviews/Unity/GiantWorld

git init
git add .
git commit -m "Giant World Unity game"
```

1. Go to [https://github.com/new](https://github.com/new)
2. Name: `giant-world` (or any name)
3. **Public** or Private — both work
4. Do **not** add README (you already have one)
5. Create repository

```bash
git remote add origin https://github.com/YOUR_USERNAME/giant-world.git
git branch -M main
git push -u origin main
```

### Option B — Inside a larger repo (e.g. Interviews)

If the project lives under `Unity/GiantWorld/` in another repo, edit `.github/workflows/build-webgl.yml` and add:

```yaml
with:
  projectPath: Unity/GiantWorld
  unityVersion: auto
  targetPlatform: WebGL
```

Move the `.github` folder to the **root** of that repo.

---

## Step 2: Add GitHub Secrets

Go to your repo on GitHub:

**Settings → Secrets and variables → Actions → New repository secret**

Add these:

| Secret name | Value |
|-------------|-------|
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password |

Optional (only if you have Unity Pro):

| Secret name | Value |
|-------------|-------|
| `UNITY_SERIAL` | Your Unity serial number |

> Use a Unity **Personal (free)** account at [https://id.unity.com](https://id.unity.com) if you don't have one.

---

## Step 3: One-time Unity license setup

GitHub's cloud runner needs a Unity license. Do this **once**.

You need **3 secrets** before building:
- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `UNITY_LICENSE` (contents of a `.ulf` file)

---

### Method A — Unity Hub only (recommended, ~5 min)

You do **not** need the full Unity Editor or WebGL module. **Unity Hub alone** is enough (~200 MB, works on low-memory Macs).

1. Download **Unity Hub** only: [https://unity.com/download](https://unity.com/download)
2. Sign in with your Unity account
3. **Unity Hub → Settings (gear) → Licenses → Add → Get a free personal license**
4. Find the license file on Mac:
   ```
   /Library/Application Support/Unity/Unity_lic.ulf
   ```
   (In Finder: **Go → Go to Folder** and paste that path)
5. Open `Unity_lic.ulf` in **TextEdit**
6. **Select All → Copy** (copy the entire text, including `<?xml` at the top)
7. GitHub repo → **Settings → Secrets → New repository secret**
   - Name: `UNITY_LICENSE`
   - Value: **paste the full text** (NOT base64)

You now have all 3 secrets. Skip to **Step 4**.

---

### Method B — GitHub Actions activation file (if Hub won't install)

Use this only if you cannot install Unity Hub on any computer.

**Important:** Add `UNITY_EMAIL` and `UNITY_PASSWORD` secrets **first** (Step 2), then push the latest workflow files, then run this.

1. Push latest code (includes fixed `unity-activate.yml`):
   ```bash
   git add .
   git commit -m "Fix Unity license workflow"
   git push
   ```
2. GitHub repo → **Actions → Unity License Setup → Run workflow**
3. First step must show ✅ for both email and password secrets
4. When green ✓, download artifact **Unity-Activation-File** (`.alf`)
5. Go to [https://license.unity3d.com/manual](https://license.unity3d.com/manual)
6. Upload `.alf` → download `.ulf`
7. Open `.ulf` in TextEdit → **Select All → Copy**
8. Add GitHub secret **`UNITY_LICENSE`** → paste full text (NOT base64)

---

### If you see: "License activation strategy could not be determined"

This means GitHub secrets are **missing or empty**:

| Check | Fix |
|-------|-----|
| `UNITY_EMAIL` not set | Add in Settings → Secrets |
| `UNITY_PASSWORD` not set | Add in Settings → Secrets |
| Typo in secret name | Must be exact: `UNITY_EMAIL`, `UNITY_PASSWORD` |
| Ran workflow before adding secrets | Add secrets, then re-run |
| Using a forked repo | Secrets must be on **your** fork, not the original |

The Node 20 warning in the log is harmless — ignore it.

---

## Step 4: Build WebGL

1. GitHub repo → **Actions**
2. Left sidebar: **Build WebGL**
3. **Run workflow** → **Run workflow**
4. Wait **20–40 minutes** (first build is slow; later builds ~10–15 min)

When it succeeds (green ✓):

1. Open the workflow run
2. **Artifacts** section at the bottom:
   - **GiantWorld-WebGL-zip** ← use this for itch.io
   - **GiantWorld-WebGL** ← raw folder (optional)

Download **GiantWorld-WebGL-zip**.

---

## Step 5: Upload to itch.io

1. [https://itch.io/register](https://itch.io/register) — create account
2. **Dashboard → Create new project**
3. Fill in:
   - **Title:** Giant World
   - **URL:** `giant-world`
   - **Kind of project:** **HTML**
4. **Uploads → Upload files** → select `GiantWorld-WebGL.zip`
5. After upload, check:
   - ✅ **This file will be played in the browser**
   - **Viewport:** `1280 x 720` (or `1920 x 1080`)
6. **Pricing → Free**
7. **Save → Public**

Your game URL:

```
https://YOUR_USERNAME.itch.io/giant-world
```

Share that link — anyone can play in the browser.

---

## Step 6: Rebuild after code changes

Whenever you update the game:

```bash
cd /Users/yogeshyadav/Interviews/Unity/GiantWorld
git add .
git commit -m "Update game"
git push
```

GitHub Actions auto-builds on push to `main`. Or trigger manually from **Actions → Build WebGL → Run workflow**.

Download the new zip → re-upload on itch.io (replace old file).

---

## Troubleshooting

### Build fails: "License activation failed" or "strategy could not be determined"

- Ensure all 3 secrets exist: `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_LICENSE`
- `UNITY_LICENSE` = **raw text** from `.ulf` file (TextEdit → Select All → Copy). NOT base64
- Add email/password secrets **before** running Unity License Setup workflow
- Easiest fix: install **Unity Hub only** (no Editor) → get `.ulf` → paste into `UNITY_LICENSE` secret
- Re-get license via Unity Hub if builds fail after months (licenses can expire)

### Build fails: "Insufficient disk space"

- Re-run the workflow — usually works on retry

### Black screen on itch.io

1. In Unity (or ask someone with Unity), set **Player Settings → WebGL → Publishing Settings → Compression Format → Disabled**
2. Push and rebuild

Or edit `ProjectSettings/ProjectSettings.asset` — rebuild in Actions.

### "No index.html in zip"

- Download **GiantWorld-WebGL-zip** (not the folder artifact)
- Open zip — `index.html` should be at the top level

### Workflow not visible in Actions tab

- Push `.github/workflows/` to GitHub:
  ```bash
  git add .github/
  git commit -m "Add GitHub Actions workflows"
  git push
  ```

### Wrong Unity version error

- Workflow uses `unityVersion: auto` (reads `ProjectSettings/ProjectVersion.txt`)
- Current version: **6000.0.28f1**

---

## What runs on your Mac vs cloud

| Task | Where |
|------|-------|
| Write/edit code | Your Mac (Cursor) |
| WebGL build | GitHub cloud (free) |
| Download zip | Your Mac (browser) |
| Upload to itch.io | Your Mac (browser) |
| Players play game | itch.io URL (browser) |

**No Unity install needed on your Mac.**

---

## Quick checklist

- [ ] GitHub repo created and code pushed
- [ ] Secrets: `UNITY_EMAIL`, `UNITY_PASSWORD`
- [ ] Ran **Unity License Setup** → got `.ulf` → added `UNITY_LICENSE` secret
- [ ] Ran **Build WebGL** → success
- [ ] Downloaded **GiantWorld-WebGL-zip**
- [ ] Uploaded to itch.io as **HTML** game
- [ ] Set **Played in browser** + **Public**
- [ ] Tested your `yourname.itch.io/giant-world` URL
