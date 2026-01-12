# Replay System - User Guide

## Overview

The Replay System automatically records your gameplay and allows you to share epic moments with friends and social media. Every level completion is saved as a replay that can be viewed, shared, and used to challenge friends.

---

## Automatic Replay Recording

### How It Works

**Automatic Recording:**
- Every level you complete is automatically recorded
- Recording starts when you launch the projectile
- Ends when the level completes (success or failure)
- No manual recording button needed!

**What Gets Recorded:**
- All slingshot inputs (drag positions, angles, timing)
- Physics snapshots (projectile trajectory, collisions)
- Your score, stars earned, and completion time
- Your equipped cosmetics during the play
- Level conditions (weather, difficulty modifiers)

**Storage:**
- Up to 20 replays stored per device
- Oldest replays auto-deleted when limit reached
- Each replay is <500KB (lightweight!)
- Stored locally on your device

---

## Viewing Your Replays

### Accessing Replay Library

1. **From Main Menu:**
   - Tap "Replays" button
   - Browse your replay library
   - Sorted by date (newest first)

2. **From Level Results:**
   - After completing a level
   - Tap "Watch Replay" to instantly view
   - Tap "Share Replay" to share immediately

### Replay Library Features

**Replay Metadata Display:**
- Thumbnail with level preview
- Score and stars achieved
- Date and time recorded
- File size (KB)
- View count (if shared)
- "Perfect Run" badge for 5-star completions

**Filtering Options:**
- By Level
- By Score (highest first)
- By Stars (5-star only)
- By Date (newest/oldest)
- By Views (most watched)

---

## Replay Playback Controls

### Watching Replays

**Basic Controls:**
- ▶️ **Play/Pause**: Start or pause replay
- ⏪ **Rewind**: Jump back 5 seconds
- ⏩ **Fast Forward**: Skip ahead 5 seconds
- 🔄 **Restart**: Replay from beginning

**Speed Controls:**
- **1x**: Normal speed
- **1.5x**: Faster playback
- **2x**: Double speed (for long replays)
- **0.5x**: Slow motion (see every detail)

**Timeline Scrubbing:**
- Drag the timeline slider to any point
- Tap timeline to jump to specific moment
- Timeline shows key events (impacts, destructions)

### Replay Viewer UI

**On-Screen Display:**
- **Top-Left**: Running score counter
- **Top-Right**: Time elapsed
- **Top-Center**: Level name + difficulty
- **Bottom-Left**: Player name + cosmetics
- **Bottom-Right**: Playback controls

**Additional Info:**
- Stars earned indicator
- Physics accuracy indicator
- "Live" badge if still recording

---

## Sharing Replays

### Share Destinations

**Supported Platforms:**
- 🎮 **Discord**: Rich embed with score/level/cosmetics preview
- 🐦 **Twitter**: Auto-formatted tweet with replay link
- 💬 **WhatsApp/SMS**: Clickable link opens game + replay
- 📧 **Email**: Replay attachment with metadata
- 🎯 **In-Game**: Share directly with friends

### How to Share

**Method 1: From Replay Viewer**
1. Open any replay from library
2. Tap "Share" button (top-right)
3. Select platform (Discord, Twitter, etc.)
4. Message auto-generated, edit if desired
5. Tap "Send" to share

**Method 2: From Level Results**
1. Complete a level
2. Tap "Share Replay" button immediately
3. Same flow as Method 1

**Method 3: Challenge Friend with Replay**
1. Open replay
2. Tap "Challenge Friend"
3. Select friend
4. Challenge includes replay link automatically

### Share Message Formats

**Twitter:**
```
I got ⭐⭐⭐⭐⭐ on Level 12 with 15,430 points! Beat my score!
game://replay/[encoded-replay-link]
```

**Discord:**
```
Check out my ⭐⭐⭐⭐⭐ run on Level 12!
Score: 15,430
Can you do better?
game://replay/[encoded-replay-link]
```

**WhatsApp:**
```
Hey! I just got 15,430 points on Level 12. Think you can beat it?
game://replay/[encoded-replay-link]
```

---

## Importing Friend Replays

### Receiving Shared Replays

**From Share Link:**
1. Friend sends you a replay link
2. Click link on your device
3. Game opens automatically
4. "Import Replay" dialog appears
5. Tap "Import" to add to your library

**From Challenge:**
1. Friend challenges you with their replay
2. Accept challenge
3. Tap "Watch Their Replay" before playing
4. Learn their strategy before competing

### Deep Linking

**Supported Link Formats:**
- `game://replay/[base64-encoded-replay]`
- `https://yourgame.com/replay/[replay-id]`
- Email attachments (.replay files)

**Import Process:**
- Link detected → Game launches
- Replay decodes and validates
- Imports to library
- Ready to watch immediately

---

## Challenge Friends Using Replays

### Replay-Based Challenges

**Creating Challenge from Replay:**
1. Open any of your replays
2. Tap "Beat This Score" button
3. Select which friend to challenge
4. Challenge includes:
   - Your replay video
   - Target score to beat
   - Your completion time
   - Cosmetics you wore

**Friend Experience:**
1. Friend receives challenge notification
2. Opens challenge → sees your replay
3. Watches your run to study strategy
4. Taps "Accept" to compete
5. Plays same level trying to beat your score

---

## Replay Storage Management

### Storage Limits

**Per-Device Limits:**
- Maximum 20 replays stored
- Oldest replays deleted automatically
- Total storage: ~5-10 MB

**Managing Storage:**
1. Go to Replay Library
2. Tap "Manage Storage"
3. See total replays + storage used
4. Delete individual replays manually
5. "Delete Old Replays" bulk action

### Which Replays to Keep

**Auto-Prioritization:**
- 5-star (perfect) runs kept longer
- High-score replays protected
- Shared replays (with views) protected
- Recent replays (<7 days) protected

**Manual Management:**
- Star/favorite replays to protect them
- Manually delete low-score replays
- Export replays to cloud storage (optional)

---

## Troubleshooting

### Common Issues

**Problem: Replay won't play**
- ✅ Restart the game
- ✅ Check replay file isn't corrupted (re-import if shared)
- ✅ Ensure game version matches replay version
- ✅ Delete and re-import from source

**Problem: Import failed**
- ✅ Check link is complete (not truncated)
- ✅ Verify link format (game://replay/...)
- ✅ Ensure storage not full (delete old replays)
- ✅ Try copying link manually if auto-open failed

**Problem: Playback stutters**
- ✅ Close background apps (free RAM)
- ✅ Reduce playback speed to 1x
- ✅ Replay may be corrupted (try re-import)
- ✅ Update game to latest version

**Problem: Share button not working**
- ✅ Check internet connection
- ✅ Grant share permissions in system settings
- ✅ Try different platform (Discord vs Twitter)
- ✅ Copy link manually and paste in app

**Problem: Replay looks different from actual gameplay**
- ✅ Physics determinism issues (rare)
- ✅ Report as bug with replay ID
- ✅ Replay may be from different game version
- ✅ Re-record the run if needed

---

## Advanced Features

### Replay Analytics

**View Count Tracking:**
- See how many times your replay was viewed
- Most-viewed replays highlighted
- "Viral Legend" cosmetic unlocks at 100 views

**Engagement Metrics:**
- Replays viewed → New player installs (attribution)
- Share → Accept rate (conversion funnel)
- Replay views → Challenge acceptance

### Replay Competitions

**Community Events:**
- "Replay of the Week" contests
- Most creative replay wins prizes
- Highest score on featured level
- Submit replays for judging

**Viral Moments:**
- Share incredible trick shots
- Physics glitches (funny moments)
- Perfect 5-star runs
- Speedrun records

---

## Privacy & Safety

**Replay Data:**
- Replays contain no personal information
- Only gameplay data + cosmetics
- Player name visible (can be changed in settings)
- Share links expire after 30 days

**Content Moderation:**
- Report inappropriate replay messages
- Block players who spam replays
- Privacy mode: disable replay viewing

---

## Best Practices for Sharing

### Optimal Sharing Strategy

**When to Share:**
- ✅ 5-star perfect completions
- ✅ New high scores
- ✅ Funny/unexpected moments
- ✅ After major game updates

**Where to Share:**
- Discord: Best for gaming communities
- Twitter: Public bragging rights
- WhatsApp: Personal friend groups
- In-Game: Direct friend challenges

**Maximizing Views:**
- Share in prime hours (evening)
- Use engaging message text
- Include emoji and excitement
- Challenge popular friends

---

## Success Metrics

**Expected Results:**
- +15% viral coefficient (5% of shares → installs)
- +20% friend engagement (replay views)
- +10% challenge acceptance (watching replays first)
- +25% social media mentions

**Tracking:**
1. Firebase Analytics → `replay_shared` event
2. Monitor `replay_viewed` count
3. Track share → install attribution
4. Measure viral coefficient growth

---

**All features work out-of-the-box. No coding required!** 🎮📹
