# 🎨 COSMETIC CUSTOMIZATION GUIDE
## Visual Personalization Without Code!

> **TL;DR:** Add cosmetics via Inspector dropdown, create new ones with simple art + code template.

---

## 🎯 Quick Start (2 minutes)

### Add Glasses to Mom:
1. Open `Scenes/Prefabs/NPCs/MomNPC.tscn`
2. Select **CosmeticOverlay** node
3. Inspector → `cosmetic_overlays` → Add Element
4. Choose **"glasses"** from dropdown
5. Save → Test in level
6. **Result:** Mom now wears glasses!

---

## 🎭 Available Cosmetics Catalog

### Academic Style
- `glasses` - Smart appearance
- `academic_hat` - Graduation cap
- `mortarboard` - Formal academic look

### Authority Style  
- `crown` - Royal/leadership authority
- `military_helmet` - Military authority
- `police_cap` - Law enforcement

### Casual Style
- `beanie` - Relaxed, everyday look
- `bandana` - Rebel/casual attitude
- `cap` - Baseball cap style

### Family Style
- `moustache` - Fatherly appearance
- `apron` - Homemaker look
- `bow_tie` - Formal family events

### Special Style
- `pirate_hat` - Adventurous character
- `alien_antenna` - Sci-fi character
- `scarf` - Weather protection

---

## 🎨 Using Existing Cosmetics

### Method 1: Inspector Dropdown (Recommended)
```
1. Select NPC prefab (MomNPC.tscn)
2. Select CosmeticOverlay node
3. Inspector → cosmetic_overlays → Add Element
4. Select from available options:
   - glasses
   - moustache  
   - crown
   - academic_hat
   - military_helmet
   - [etc.]
5. Save scene
```

### Method 2: Multiple Cosmetics
```
1. Select NPC prefab
2. Select CosmeticOverlay node  
3. Inspector → cosmetic_overlays → Add Element
4. Add multiple items:
   - Element 1: "glasses"
   - Element 2: "academic_hat"
   - Element 3: "bow_tie"
5. Result: NPC has all three cosmetics
```

---

## 🖼️ Creating New Cosmetics

### Step 1: Create Art (5 minutes)
```
1. Open Aseprite, Photoshop, or GIMP
2. Create 64x64 pixel image
3. Draw your cosmetic item (glasses, hat, etc.)
4. Save as PNG with transparency
5. Name: YOUR_COSMETIC_NAME.png
```

### Step 2: Add to Game (3 minutes)
```
1. Save PNG to: Assets/Sprites/Cosmetics/YOUR_COSMETIC_NAME.png
2. Open: Script/CosmeticOverlay.cs
3. Find the enum CosmeticType:
   public enum CosmeticType
   {
       glasses,
       moustache,
       // ADD YOUR COSMETIC HERE:
       YOUR_COSMETIC_NAME,  ← Add this line
   }
4. Find the switch statement:
   case CosmeticType.YOUR_COSMETIC_NAME:
       overlaySprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cosmetics/YOUR_COSMETIC_NAME.png");
       break;
5. Save the C# file
6. In Godot: Project → Build → Compile
7. Test in Inspector dropdown
```

---

## 🎨 Art Guidelines

### Technical Specifications
- **Size**: 64x64 pixels (standard)
- **Format**: PNG with transparency
- **Style**: Simple, clear, readable
- **Position**: Designed to overlay on head area

### Design Tips
- **Simple shapes work best**
- **Bold colors stand out**
- **Avoid tiny details**
- **Test visibility at game scale**
- **Consider animation compatibility**

### Common Mistakes to Avoid
- ❌ Too complex/little details
- ❌ Wrong size (not 64x64)
- ❌ No transparency background
- ❌ Blocking face completely
- ❌ Hard to see at game resolution

---

## 🎭 Cosmetic Categories

### Authority Cosmetics
Use for: Teachers, principals, soldiers, police
- `crown` - Ultimate authority
- `academic_hat` - Educational authority  
- `military_helmet` - Military authority
- `police_cap` - Law enforcement

### Family Cosmetics
Use for: Mom, dad, relatives
- `moustache` - Father figure
- `apron` - Homemaker
- `bow_tie` - Formal occasions
- `glasses` - Smart parent look

### Casual Cosmetics
Use for: Friends, classmates, peers
- `beanie` - Relaxed friend
- `cap` - Casual buddy
- `bandana` - Rebellious friend
- `scarf` - Stylish companion

### Special Cosmetics
Use for: Unique characters, bosses
- `pirate_hat` - Adventure character
- `alien_antenna` - Sci-fi character
- `crown` - King/queen character
- `glasses` - Smart boss character

---

## 🔧 Advanced Techniques

### Cosmetic Combinations
```
Academic Authority:
- glasses + academic_hat
- Perfect for: Teachers, professors

Military Commander:
- military_helmet + serious dialogue
- Perfect for: Military levels

Formal Family:
- bow_tie + formal dialogue  
- Perfect for: Important family events

Rebel Student:
- bandana + casual dialogue
- Perfect for: Teenage characters
```

### Dynamic Cosmetics
```
1. Create cosmetic variants:
   - glasses_thick.png
   - glasses_thin.png
   - glasses_sunglasses.png

2. Use in different scenarios:
   - Thick glasses: Smart characters
   - Thin glasses: Cool characters  
   - Sunglasses: Secret agents
```

### Seasonal Cosmetics
```
Winter Theme:
- scarf + beanie
- Use for: Holiday levels

Summer Theme:  
- cap + sunglasses
- Use for: Beach/vacation levels

School Theme:
- academic_hat + glasses
- Use for: Educational levels
```

---

## 🎯 Popular Cosmetic Combinations

### Teacher Look
```
Cosmetic Overlays:
1. glasses
2. academic_hat

Dialogue Style:
- "DETENTION!"
- "This is unacceptable!"
- "Your parents will hear about this!"

Perfect for: Classroom levels
```

### Dad Look  
```
Cosmetic Overlays:
1. moustache
2. bow_tie (optional)

Dialogue Style:
- "I pay for this house!"
- "MY rules!"
- "OUTSIDE!"

Perfect for: House/family levels
```

### Cool Friend Look
```
Cosmetic Overlays:
1. cap
2. bandana

Dialogue Style:
- "Hey dude!"
- "That's awesome!"
- "Let's do this!"

Perfect for: Friend/peer levels
```

### Boss Character Look
```
Cosmetic Overlays:
1. crown
2. glasses (optional)

Dialogue Style:
- "KNEEL BEFORE ME!"
- "I am the ultimate authority!"
- "OBEY or face consequences!"

Perfect for: Boss levels
```

---

## 🛠️ Troubleshooting

### Cosmetic Not Appearing?
1. **Check file path**: `Assets/Sprites/Cosmetics/YOUR_COSMETIC.png`
2. **Verify PNG format**: Must have transparency
3. **Check enum entry**: Must match exactly
4. **Rebuild project**: Project → Build → Compile

### Cosmetic Position Wrong?
1. **Art position**: Design cosmetic for head overlay area
2. **Check sprite anchor**: Should center on head
3. **Test scale**: 64x64 pixels should fit properly

### Multiple Cosmetics Overlapping?
1. **Check art overlap**: Design cosmetics to not interfere
2. **Layer order**: Some cosmetics should be behind others
3. **Simplify design**: Avoid overly complex combinations

---

## 📝 Example Workflows

### Workflow 1: Create "Nerd Glasses"
```
1. Design 64x64 PNG: thick black frames
2. Save as: Assets/Sprites/Cosmetics/nerd_glasses.png
3. Add to CosmeticOverlay.cs enum:
   nerd_glasses,
4. Add case:
   case CosmeticType.nerd_glasses:
       overlaySprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cosmetics/nerd_glasses.png");
       break;
5. Use in TeacherNPC for smart character
```

### Workflow 2: Create "Winter Scarf"
```
1. Design 64x64 PNG: colorful winter scarf
2. Save as: Assets/Sprites/Cosmetics/winter_scarf.png
3. Add to enum: winter_scarf,
4. Add case for winter_scarf
5. Combine with beanie for full winter look
```

### Workflow 3: Create "Royal Crown"
```
1. Design 64x64 PNG: golden crown
2. Save as: Assets/Sprites/Cosmetics/royal_crown.png  
3. Add to enum: royal_crown,
4. Add case for royal_crown
5. Use with crown dialogue for ultimate authority
```

---

## 🎉 Success Checklist

- [ ] Apply existing cosmetic to NPC
- [ ] Create new cosmetic PNG (64x64, transparent)
- [ ] Add cosmetic to code enum
- [ ] Add case statement for new cosmetic
- [ ] Test cosmetic appears in Inspector dropdown
- [ ] Apply cosmetic to NPC and test in game
- [ ] Create cosmetic combination (2+ cosmetics)
- [ ] Design themed cosmetic set (academic, military, etc.)

**Once you complete this checklist, you can create unlimited cosmetics! 🎨**

---

## 🔗 Next Steps

- **NPC System**: See `Docs/GUIDES/NPC_PLACEMENT_GUIDE.md`
- **Dialogue Writing**: See `Docs/GUIDES/DIALOGUE_WRITING_GUIDE.md`
- **Content Management**: See `Docs/GUIDES/CONTENT_MANAGEMENT_MASTER.md`
- **Marketing**: See `Docs/MARKETING/SOCIAL_MEDIA_GUIDE.md`

**Remember: Every cosmetic you create adds personality and uniqueness to your game! 🌟**