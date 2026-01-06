# Meme Mini‑Game Customization

The legendary meme interlude is:
- Scene: `Scenes/MemeGames/SixToSevenMinigame.tscn`
- Script: `Script/SixToSevenMinigame.cs`
- Trigger logic: `Script/MemeGateway.cs`

## When it triggers
After finishing a level, when you press **Next**, MemeGateway checks:
- `levelNumber % 10 == 6` (6, 16, 26, 36, ... 96)
- “Perfect” score (>= 90% of that level’s OptimalScore)

If true, the mini‑game plays **before** loading the next level.

## Adding more meme variants
Open `Script/SixToSevenMinigame.cs` and:
1. Add a new enum value in `MemeVariant`
2. Add a case in `PlayVariantAsync(...)`
3. Implement a simple tween / label animation

## Rewards
Rewards are cosmetic ids stored in `PlayerProfile.UnlockedCosmetics`.
Edit the reward list in `MemeGateway.AwardMemeReward()` to add more.

## Making a brand new minigame scene
1. Duplicate `Scenes/MemeGames/SixToSevenMinigame.tscn`
2. Attach your own script
3. Point `MemeGateway.MinigameScenePath` to the new scene
