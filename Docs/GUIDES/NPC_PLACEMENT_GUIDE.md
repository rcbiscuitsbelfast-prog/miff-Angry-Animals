# 🎭 NPC PLACEMENT GUIDE
## Drag & Drop NPCs - Zero Code Required!

> **TL;DR:** Drag any NPC prefab into any level, position with mouse, customize via Inspector. Done!

---

## 🎯 Quick Start (30 seconds)

### Add Mom to Level 15:
1. Open `Scenes/Levels/Room015.tscn`
2. Drag `Scenes/Prefabs/NPCs/MomNPC.tscn` into scene
3. Position with mouse (drag to move)
4. **Optional:** Select NPC → Inspector → dialogue → Add custom line
5. Save (Ctrl+S) → Test (F5)
6. **Result:** Mom appears in Level 15!

---

## 🎨 Available NPC Prefabs

### Family NPCs
- **👩 MomNPC.tscn** - Mother figure, static position
- **👨 DadNPC.tscn** - Father figure, patrols back/forth

### Authority NPCs  
- **👩‍🏫 TeacherNPC.tscn** - Academic, stands at desk
- **👮 SoldierNPC.tscn** - Military, patrol patterns

### Peer NPCs
- **👨‍🎓 SchoolmateNPC.tscn** - Classmate, caged or moving

### Special NPCs
- **👑 PrincipalNPC.tscn** - School authority, crown cosmetic
- **🏴‍☠️ PirateNPC.tscn** - Special character, pirate hat

---

## 🔧 Inspector Customization

Every NPC has these settings you can change in the Inspector:

### Basic Settings
- **npc_type**: FAMILY, SCHOOLMATE, AUTHORITY, SOLDIER
- **face_source**: PLAYER_FACE or NPC_UNIQUE
- **behavior_type**: STATIC, MOVING_PATROL, CAGED
- **health**: 50-150 (destructible points)

### Dialogue Settings
- **dialogue**: Array of speech bubble text
- **dialogue_interval**: Time between speech bubbles
- **speak_on_hit**: Whether to speak when damaged

### Cosmetic Settings
- **cosmetic_overlays**: Array of cosmetics (moustache, glasses, etc.)

---

## 🎭 NPC Types Explained

### FAMILY NPCs
- **Mom**: Says protective/commanding things
- **Dad**: Says disciplinary/bossy things
- **Use for:** Personal, emotional encounters

### AUTHORITY NPCs  
- **Teacher**: Academic authority
- **Soldier**: Military/physical authority
- **Principal**: School authority
- **Use for:** Institutional challenges

### SCHOOLMATE NPCs
- **Classmate**: Peer relationships
- **Use for:** Social dynamics, support/opposition

---

## 🎨 Cosmetic System

### Available Cosmetics:
- `moustache` - For Dad characters
- `glasses` - For academic characters
- `academic_hat` - For teachers
- `military_helmet` - For soldiers
- `crown` - For authority figures
- `pirate_hat` - For special characters
- `bandana` - For rebels
- `beanie` - For casual characters
- `scarf` - For winter themes

### Adding Cosmetics:
1. Select NPC node
2. Inspector → cosmetic_overlays → Add Element
3. Choose cosmetic from dropdown
4. Save and test

---

## 🚀 Advanced Customization

### Creating Custom Dialogue:
```
Current: ["WATCH IT!", "Not MY rules!", "Clean your room!"]
Custom: ["You're grounded!", "No games today!", "Study first!"]
```

### Setting Behavior:
- **STATIC**: NPC stays in one place
- **MOVING_PATROL**: NPC walks back and forth
- **CAGED**: NPC is trapped (bars visible)

### Health Settings:
- **50 HP**: Easy to defeat
- **100 HP**: Standard difficulty  
- **150 HP**: Very challenging

---

## 🎯 Level Design Tips

### Placement Strategy:
- **Mom**: Near valuable objects to protect
- **Dad**: Near exits to block escape
- **Teacher**: At "front of class" positions
- **Soldier**: Near strategic chokepoints
- **Schoolmate**: Near other NPCs for social dynamics

### Dialogue Flow:
- Start with 3-5 dialogue lines
- Make them memorable/catchy
- Use character-specific speech patterns
- Consider voice acting implications

### Cosmetic Combinations:
- **Authoritative look**: glasses + academic_hat
- **Casual look**: beanie + scarf
- **Military look**: military_helmet + serious dialogue
- **Special characters**: crown, pirate_hat for unique roles

---

## 🛠️ Troubleshooting

### NPC Not Appearing?
- Check if prefab was saved correctly
- Verify collision shapes are present
- Ensure node is visible in scene tree

### Cosmetics Not Showing?
- Verify cosmetic files exist in Assets/Sprites/Cosmetics/
- Check cosmetic name spelling matches exactly
- Ensure overlay node is properly configured

### Dialogue Not Playing?
- Check dialogue array is not empty
- Verify speech bubble manager is working
- Test with simple single-line dialogue first

---

## 📝 Example Workflows

### Workflow 1: Add Dad to Level 25 with Crown
```
1. Open Room025.tscn
2. Drag DadNPC.tscn into scene
3. Position Dad near exit (drag with mouse)
4. Select Dad node
5. Inspector → cosmetic_overlays → Add "crown"
6. Inspector → dialogue → Replace with:
   ["I'm the KING of this house!", "My rules!", "Obey me!"]
7. Save → F5 test
```

### Workflow 2: Create Classroom Scene
```
1. Open new level (RoomXXX.tscn)
2. Add TeacherNPC at front (static, academic_hat + glasses)
3. Add 3 SchoolmateNPCs in desks (caged, random cosmetics)
4. Add PrincipalNPC (crown, commanding dialogue)
5. Position strategically for optimal gameplay
```

### Workflow 3: Military Base Level
```
1. Open RoomXXX.tscn  
2. Add 2 SoldierNPCs (military_helmet, patrol behavior)
3. Add 1 authority figure (crown, high health)
4. Set dialogue to military/boss commands
5. Position for tactical gameplay
```

---

## 🎉 Success Checklist

- [ ] Add at least one NPC to a level
- [ ] Customize dialogue for your NPC
- [ ] Apply at least one cosmetic
- [ ] Test the level to ensure NPC works
- [ ] Try different behavior types
- [ ] Create a themed level with multiple NPCs

**Once you complete this checklist, you can create unlimited NPCs! 🎮**

---

## 🔗 Next Steps

- **Cosmetics**: See `Docs/GUIDES/COSMETIC_CUSTOMIZATION.md`
- **Dialogue**: See `Docs/GUIDES/DIALOGUE_WRITING_GUIDE.md`
- **Level Design**: See `Docs/GUIDES/LEVEL_DESIGN_GUIDE.md`
- **Content Management**: See `Docs/GUIDES/CONTENT_MANAGEMENT_MASTER.md`

**Remember: Every NPC you create makes your game more unique and engaging! 🌟**