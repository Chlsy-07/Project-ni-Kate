# Project ni Kate 🎮

A custom 2D platformer built with Unity 6 (URP 2D). This project features a tight, Celeste-style physics controller, a dynamic level bounds system, fully automated environment prefabs, and more!

## 🚀 Getting Started

### Prerequisites
- **Unity Version:** `6000.6.2f1` (Unity 6 LTS)
- **Git** (for version control)

### How to Clone and Setup
1. Open your terminal or command prompt.
2. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/Chlsy-07/Project-ni-Kate.git
   ```
3. Open **Unity Hub**.
4. Click **Add Project** -> **Add project from disk**.
5. Select the `Project-ni-Kate` folder you just cloned.
6. Open the project in Unity. The first launch may take a few minutes to import assets.

### How to Pull Latest Changes
If someone else (or an AI assistant!) makes changes to the repository, you can pull the latest updates by opening your terminal in the project folder and running:
```bash
git pull origin main
```

---

## 🛠️ Editor Tools
This project comes with custom, 1-click Editor tools to automate setup and fix common issues. You can find them in the top Unity menu under **Tools**.

*   **Tools → Setup → Setup Camera and Background:** Automatically adds smooth tracking to the Main Camera, locks the background sprite so it doesn't scroll, and sets the aspect ratio.
*   **Tools → Setup → Set Background Music:** Instantly creates an AudioManager, assigns the background track, and sets it to loop forever.
*   **Tools → Player → Setup Player Controller:** Rebuilds the player prefab with the correct physics configurations, layers, and rigidbodies for precise 2D platforming.
*   **Tools → Environment → Build Environment Prefabs:** Generates solid ground and platform prefabs out of the raw sprites.
*   **Tools → Environment → Fix Hazard Colliders:** Automatically configures spike traps/hazards so they have a single internal kill trigger, preventing players from bumping into invisible solid walls.
*   **Tools → Environment → Setup Checkpoint:** Configures checkpoint flags to save player progress.
*   **Tools → Environment → Setup Respawn Manager:** Spawns the global manager responsible for resetting the player to their last checkpoint upon death.

## 🕹️ Architecture Highlights
- **PlayerController:** Custom `Rigidbody2D` logic utilizing `LinearVelocity` and manual edge clamping.
- **LevelBoundsManager:** Dynamically scans the scene for colliders and prevents the camera/player from moving off the edges of the map into the void.
- **KillZone & PlayerHitBox:** The player's HitBox is purposefully smaller than their physical body (ignoring feet) so that jumping near spikes is forgiving and fair.

---
*Happy developing!*
