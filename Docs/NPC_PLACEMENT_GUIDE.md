# NPC Placement Guide (Non‑Coder)

NPCs are optional story actors you can place directly inside level scenes (`Scenes/Levels/RoomXXX.tscn`).

## Placing an NPC
1. Open a level scene (example: `Scenes/Levels/Room010.tscn`).
2. Add a new node (recommended: **Node2D**).
3. Attach the script: `res://Scripts/NPC.cs`
4. Add a face sprite under the NPC:
   - Add a **Sprite2D** named anything you like
   - In the NPC inspector, set **FaceSpritePath** to that sprite

## Key Inspector settings
- **Type**: Family / Schoolmate / Authority / Soldier
- **Face**:
  - PlayerFace = uses the player’s photo
  - NpcUnique = uses `UniqueFaceTexturePath`
- **CosmeticOverlays**: list of overlay ids (ex: `moustache_normal`, `girly_hair`, `glasses_round`)
- **Behaviour**:
  - Static
  - MovingPatrol (configure PatrolPointA/PatrolPointB)
  - Destructible (configure Health)

## Dialogue
To add reactions:
1. Select the NPC
2. Inspector → **Dialogue** → add lines

Example lines:
- “WATCH IT!”
- “NOT MY PAPERS!!”
- “HEY!”

When the NPC is damaged/destroyed (via `TakeDamage()`), it will pick a random line and show it using the DialogueManager.
