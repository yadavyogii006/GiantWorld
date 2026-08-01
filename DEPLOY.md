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

### Method B — GitHub Actions activation file (no Unity Hub needed)

This workflow uses Docker directly — it does **not** need `UNITY_EMAIL`/`UNITY_PASSWORD` to generate the `.alf` file.

1. Push latest code:
   ```bash
   git add .
   git commit -m "Fix Unity 6 activation — use Docker ALF workflow"
   git push
   ```
2. GitHub repo → **Actions → Unity License Setup → Run workflow**
3. When green ✓, download artifact **Unity-Activation-File**
4. Go to [https://license.unity3d.com/manual](https://license.unity3d.com/manual) → upload `.alf` → download `.ulf`
5. Open `.ulf` in TextEdit → **Select All → Copy**
6. Add GitHub secret **`UNITY_LICENSE`** → paste full text (NOT base64)
7. Also add **`UNITY_EMAIL`** and **`UNITY_PASSWORD`** (required for the build step)
8. Run **Build WebGL** workflow

---

### If you see: "Invalid version 6000.0.x"

The old `game-ci/unity-activate` action does not support Unity 6. The project now uses **2022.3 LTS** for CI builds (works the same in-game). Pull latest code and re-run **Unity License Setup**.

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

### Build fails: "Failed to login" (HTTP 401) or "Failed to activate ULF license"

Two separate problems — fix **both**:

#### Fix 1 — Wrong email or password (401 error)

```
UnityConnectLoginRequest: Failed to login - please check your username or password
```

1. Go to [https://id.unity.com](https://id.unity.com) and sign in
2. If you use **Google/Apple sign-in**, set a Unity password:
   - **My Account → Security → Password**
3. Test login with email + password (not Google button)
4. Update GitHub secrets — re-type carefully, **no extra spaces**:
   - `UNITY_EMAIL`
   - `UNITY_PASSWORD`

#### Fix 2 — Use serial instead of ULF file (recommended for CI)

The `.ulf` file is tied to the Docker machine that created the `.alf`. GitHub's build runner uses a **different** machine, so ULF activation often fails in CI.

**Switch to serial-based activation:**

1. On your Mac, extract the serial from your downloaded `.ulf`:

```bash
grep DeveloperData ~/Downloads/Unity_v2022.3.50f1.ulf | sed -E 's/.*Value="([^"]+)".*/\1/' | base64 --decode
```

Or use the project script:

```bash
chmod +x scripts/extract-unity-serial.sh
./scripts/extract-unity-serial.sh ~/Downloads/Unity_v2022.3.50f1.ulf
```

2. Output looks like: `XX-XXXX-XXXX-XXXX-XXXX-XXXX`

3. GitHub → **Settings → Secrets**:
   - **Add** `UNITY_SERIAL` = the serial from step 1
   - **Delete** `UNITY_LICENSE` secret (important — ULF causes conflicts)
   - Keep `UNITY_EMAIL` and `UNITY_PASSWORD`

4. Re-run **Build WebGL**

---

### Build fails: "License activation failed" or "strategy could not be determined"

- Ensure secrets exist: `UNITY_EMAIL`, `UNITY_PASSWORD`, and **`UNITY_SERIAL`** (preferred) OR `UNITY_LICENSE`
- For CI, **UNITY_SERIAL works better** than UNITY_LICENSE — see Fix 2 above
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

### Black screen after gzip fix (game loads but canvas stays dark)

Your downloaded build is probably **out of date**. The last pushed GitHub build used **Linear color space** and runtime `Shader.Find()` — both commonly cause a black WebGL canvas.

**Fix:** push the latest code and rebuild:

```bash
cd /Users/yogeshyadav/Interviews/Unity/GiantWorld
git add -A
git commit -m "Fix WebGL black screen: gamma, shaders, debug overlay"
git push
```

Then on GitHub: **Actions → Build WebGL → Run workflow**.

After download, confirm the new build:
- `Build/` has **no `.gz` files** (only `WebGL.data`, `WebGL.framework.js`, `WebGL.wasm`)
- `index.html` shows `productVersion: "1.1"` or newer
- Opening the game shows white status text top-left ("Booting Giant World...") before the kitchen appears

### "Unable to parse WebGL.framework.js.gz" / gzip error

Unity built with **gzip compression** but itch.io and `python3 -m http.server` do **not** send `Content-Encoding: gzip`, so the loader fails.

**Fix your current build locally:**

```bash
./scripts/fix-webgl-build.sh ~/Desktop/Games/WebGL
cd ~/Desktop/Games/WebGL
python3 -m http.server 8080
# open http://localhost:8080
```

This decompresses `Build/*.gz` and patches `index.html` to use `WebGL.data`, `WebGL.framework.js`, `WebGL.wasm`.

**Future CI builds:** the GitHub workflow runs the same fix automatically after each WebGL build.

**Quick test without decompressing** (keeps `.gz` files):

```bash
python3 scripts/serve-webgl.py 8080   # run from folder with index.html
```

### Wrong Unity version error

- Workflow uses `unityVersion: auto` (reads `ProjectSettings/ProjectVersion.txt`)
- Current CI version: **2022.3.50f1** (Unity 6 is not supported by game-ci activate yet)

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
