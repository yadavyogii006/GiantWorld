# Giant World

**Everything is huge. You are insect-sized.**

A coffee mug is a mountain. Books are buildings. The kitchen is an open-world map. Survive four colossal boss fights and escape the Giant Kitchen.

## Requirements

- **Unity 6000.0.x** (Unity 6) or **Unity 2022.3 LTS+**
- Input System package (included in `Packages/manifest.json`)

## Quick Start

1. Open **Unity Hub**
2. Click **Add** → select folder: `Unity/GiantWorld`
3. Open the project (Unity will import packages on first launch)
4. Open scene: `Assets/Scenes/Main.unity`
5. Press **Play**

The entire game bootstraps at runtime — no manual setup needed.

## Controls

| Input | Action |
|-------|--------|
| **WASD / Arrow Keys** | Move |
| **Left Shift** | Sprint |
| **Space** | Jump |
| **Left Mouse Button** | Attack |
| **Right Mouse Button + Drag** | Orbit camera |
| **R** | Restart (after death or victory) |

## World

| Landmark | Description |
|----------|-------------|
| **Coffee Mug Mountain** | Climbable cylinder with hot coffee hazard at the summit |
| **Book City** | Stacked book skyscrapers |
| **Table Legs** | Colossal wooden pillars |
| **The Sink** | Water hazard basin |
| **Stove Volcano** | Burner lava pools |
| **Sugar Crystals** | Collectibles that heal 15 HP |

## Boss Fights

Walk into the marked arena zones to trigger each boss.

### 1. The Cat
- **Location:** Center of the kitchen floor
- **Attacks:** Stalks you, pounce with knockback, paw swipe
- **Tip:** Dodge during pounce windup (cat wiggles before leaping)

### 2. Vacuum Cleaner
- **Location:** Southwest corner
- **Attacks:** Patrol route, suction pull, damage when pulled close
- **Tip:** Break line of sight to the nozzle during overheat phase

### 3. Washing Machine
- **Location:** Southeast corner
- **Attacks:** Spin cycle (AoE damage), door slam, water splash
- **Tip:** Back away during spin-up; don't stand in front of the door

### 4. Human Footsteps
- **Location:** North end of the map
- **Attacks:** Telegraph stomp (red ring), massive impact damage, expanding shockwaves
- **Tip:** Watch the red warning ring and roll out before the foot lands

## Defeat All 4 Bosses → Victory

Collect sugar crystals to heal between fights. Attack boss weak points (head, nozzle, drum, foot) for bonus damage.

## Project Structure

```
Assets/Scripts/
├── Core/           GameManager, GameBootstrap, constants
├── Player/         Movement, combat, health, camera
├── Bosses/         Cat, Vacuum, Washing Machine, Footsteps
├── World/          Kitchen generator, hazards, collectibles
└── UI/             HUD, boss health bars, victory/death screens
```

## Build

1. **File → Build Settings**
2. Ensure `Assets/Scenes/Main.unity` is in Scenes In Build
3. Select platform (PC/Mac/Linux) → **Build**

## Notes

- Uses **procedural primitives** (no external assets required)
- Works with **Built-in Render Pipeline** (Standard shader fallback)
- All logic is in C# — extend bosses in `Assets/Scripts/Bosses/`
