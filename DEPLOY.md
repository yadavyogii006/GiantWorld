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

### 3a. Run the license workflow

1. GitHub repo → **Actions** tab
2. Left sidebar: **Unity License Setup**
3. Click **Run workflow** → **Run workflow**
4. Wait ~2–5 minutes for green checkmark

### 3b. Download the activation file

1. Click the completed workflow run
2. Under **Artifacts**, download **Unity-Activation-File**
3. Unzip — you get a `.alf` file

### 3c. Get your license file

1. Open [https://license.unity3d.com/manual](https://license.unity3d.com/manual)
2. Sign in with the **same Unity email**
3. Upload the `.alf` file
4. Download the `.ulf` license file

### 3d. Add UNITY_LICENSE secret

On your Mac Terminal:

```bash
base64 -i ~/Downloads/Unity_v6000.x.ulf | pbcopy
```

(Press **i** in `base64 -i` — that's the input flag, not a typo.)

This copies the base64 license to your clipboard.

1. GitHub repo → **Settings → Secrets → New repository secret**
2. Name: `UNITY_LICENSE`
3. Value: paste (Cmd+V) the entire base64 string
4. Save

You now have 3 secrets: `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_LICENSE`.

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

### Build fails: "License activation failed"

- Re-run **Unity License Setup** and repeat Step 3
- Unity licenses expire — regenerate `UNITY_LICENSE` every few months if builds suddenly fail

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
