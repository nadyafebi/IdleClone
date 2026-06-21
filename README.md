# IdleClone

A 2D idle/platformer game inspired by [IdleOn](https://www.idleon.info/). Built as a demo featuring the core gameplay loop: explore platformer zones, fight enemies and gather resources, complete quests, upgrade your character, and progress toward a final boss — all while earning progress even when you're offline.

---

## Features

### Combat & Gathering
- Click-to-move platformer movement using a runtime-built platform graph with BFS pathfinding
- Click enemies or resource nodes to approach and engage automatically
- Auto-attack targeting — the player re-engages the nearest enemy after a kill
- Mining and woodcutting with level requirements per node
- Loot drops with arc animation; drag-or-hover collection

### Character Progression
- XP and level system with exponential scaling
- Four upgrade tiers: Strength, Resilience, Vitality, Yield
- Two player classes: Beginner and Awakened (unlocked via quest)
- Equipment slots: weapon, shield, consumable potion
- Derived stats (attack, defense, health, speed) computed from level + equipment + upgrades

### Skills
- **Slash** — melee AoE visual effect
- **Barrier** — temporary invincibility sphere
- **Fireball** — projectile; unlocked through class progression

### Quests
- Kill, collect, and upgrade quest types
- Quest chains with dependency locking
- NPC quest indicators (!, ?) with state-driven dialogue
- Quest progress tracked in the HUD

### Economy
- Crafting system with recipe data and ingredient requirements
- Shop NPCs with item purchases using in-game currency
- Inventory grid (16 slots) with drag-and-drop equipping

### Boss Encounter
- Multi-phase boss state machine (melee pulses → orb barrages → defense phases)
- Minion spawning at 66% and 33% HP thresholds
- Dedicated boss arena scene; death returns the player to Field3
- Boss health bar UI

### Save System
- JSON save via PlayerPrefs; auto-save every 60 seconds and on quit
- Saves level, XP, health, inventory, equipment, upgrades, class, quest state, kill counts, and last scene
- Offline progression: calculates AFK XP and loot earned while the game was closed (5 min – 3 hour window), shown on next login

### UI
- Built entirely with UI Toolkit
- HUD: XP bar, HP bar, potion slot, class label, skill cooldown indicators
- Side panel menus: Inventory, Equipment, Crafting, Shop, Upgrades, Player Stats
- Floating damage/heal/requirement popups
- Item tooltips on hover
- Screen fade transitions between scenes

---

## Technical

### Details

| | |
|---|---|
| **Engine** | Unity 6000.3 |
| **Render Pipeline** | Universal Render Pipeline (URP), 2D |
| **UI** | UI Toolkit |
| **Input** | Unity Input System |

### Project Structure

```
Assets/
├── Scenes/          — all 6 scenes
├── Scripts/
│   ├── Boss/        — BossController, BossHUD, BossOrbProjectile
│   ├── Data/        — ScriptableObjects (ItemData, EnemyData, QuestData, etc.)
│   ├── Enemy/       — Enemy, EnemyHealth, EnemyMovement, EnemyAttack, EnemySpawner, …
│   ├── Input/       — ClickRouter, ClickIndicator, LootDragCollector
│   ├── Interactables/ — NPC, Portal, CraftingNpc, ShopNpc, …
│   ├── Items/       — DroppedItem
│   ├── Movement/    — PlatformGraphBuilder, PlatformPathfinder
│   ├── Player/      — PlayerMovement, PlayerCombat, PlayerStats, PlayerSkills, …
│   ├── Quest/       — QuestManager
│   ├── ResourceNode/ — ResourceNode, ResourceNodeHealth, ResourceNodeInteraction
│   ├── Save/        — SaveSystem, SaveData, SaveRegistry, OfflineProgressionCalculator
│   ├── Skills/      — FireballProjectile, SlashEffect, BarrierEffect
│   └── UI/          — HUDController, InventoryMenu, CraftingMenu, ShopMenu, …
└── ...
```

### Scenes

| Scene | Description |
|---|---|
| `Start` | Title screen with new/load/delete save and time cheat |
| `Town` | Hub world — NPCs, shop, crafting, quest givers |
| `Field1` | First combat and gathering zone |
| `Field2` | Second combat and gathering zone |
| `Field3` | Third zone; portal to the boss arena |
| `BossArena` | Final boss encounter |

---

## Third-Party Credits

> *Fill in as needed.*

| Asset | Author / Source | License | Notes |
|---|---|---|---|
| Pixel Platformer | [Kenney](https://kenney.nl/assets/pixel-platformer) | CC0 | |
| Cursor Pixel Pack | [Kenney](https://kenney.nl/assets/cursor-pixel-pack) | CC0 | |
| UI Pack - Pixel Adventure | [Kenney](https://kenney.nl/assets/ui-pack-pixel-adventure) | CC0 | |
| Desert Shooter Pack | [Kenney](https://kenney.nl/assets/desert-shooter-pack) | CC0 | Used for enemies |
| Tiny Dungeon | [Kenney](https://kenney.nl/assets/tiny-dungeon) | CC0 | Used for items |
| Tiny Town | [Kenney](https://kenney.nl/assets/tiny-town) | CC0 | Used for log item |
| Tiny Ski | [Kenney](https://kenney.nl/assets/tiny-ski) | CC0 | Used for rock sprite |

---
