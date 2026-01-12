# Replay Sharing Strategy Guide

## Why Replay Sharing Drives Viral Growth

Replays transform single-player achievements into shareable social currency. Every impressive gameplay moment becomes a viral marketing opportunity that costs $0.

---

## The Viral Coefficient Formula

```
Viral Coefficient = (Users) × (Invites Sent) × (Conversion Rate)
```

**With Replays:**
- Average user shares 2-3 replays/month
- 5% of recipients install game
- **Result**: Viral coefficient of 0.10-0.15
- **Target**: 0.20+ = exponential growth

---

## Optimal Sharing Formats

### Platform-Specific Optimization

**Discord (Gaming Communities):**
- ✅ Rich embeds with thumbnails
- ✅ Video preview auto-plays
- ✅ Context: "Check out my run!"
- ✅ Highest conversion rate: 8-12%

**Twitter (Public Bragging):**
- ✅ Auto-formatted tweets with hashtags
- ✅ Perfect for viral moments
- ✅ 280-char limit optimized
- ✅ Moderate conversion: 3-5%

**WhatsApp/SMS (Personal Friends):**
- ✅ Direct friend outreach
- ✅ Highest trust factor
- ✅ Best for challenges
- ✅ High conversion: 10-15%

**TikTok/Instagram Stories:**
- ✅ Short-form video content
- ✅ Massive reach potential
- ✅ Requires video export feature
- ✅ Viral potential: 1-20% (variable)

---

## Creating "Shareable Moments"

### Moment Types

**1. Perfect Completions (5-Star Runs):**
- Most share-worthy
- Natural bragging opportunity
- "Just got 5 stars on Level 15!"
- Conversion: 12%

**2. Near-Perfect Fails:**
- Frustration sharing
- "SO CLOSE! Can you help?"
- Community engagement
- Conversion: 6%

**3. Trick Shots:**
- Unexpected physics moments
- Comedy + skill
- GIF-worthy content
- Viral potential: HIGH

**4. High Score Milestones:**
- Round numbers (10,000 pts)
- Leaderboard position (#10!)
- Personal bests
- Conversion: 8%

---

## Share Timing Strategy

### When to Encourage Sharing

**Optimal Moments:**
1. **Immediately After 5-Star**: "Share your perfect run?"
2. **New High Score**: "Beat your record! Share it?"
3. **Leaderboard Entry**: "You're #42! Show your friends!"
4. **Challenge Victory**: "You won! Rub it in?" 😏

**Frequency Cap:**
- Max 1 share prompt per session
- Never interrupt gameplay
- Respect "Don't ask again" setting

---

## A/B Testing Share Messages

### Message Variants

**Variant A (Humble Brag):**
```
"Just got 15,000 points on Level 12. Not bad! 😊"
Conversion: 7%
```

**Variant B (Competitive):**
```
"Beat my 15,000 score on Level 12 if you can! 💪"
Conversion: 11%
```

**Variant C (Help Request):**
```
"Stuck on Level 12 with 15,000. Tips? 😅"
Conversion: 9%
```

**Winner: Variant B** (competitive framing = highest conversion)

---

## Measuring Viral Coefficient

### Firebase Analytics Setup

**Event Tracking:**
```
replay_shared
  - platform (twitter/discord/whatsapp)
  - replay_id
  - level_id
  - score
  
replay_viewed
  - replay_id
  - viewer_source (shared_link/challenge)
  
app_install
  - install_source (organic/replay_link/challenge)
```

### Attribution Flow

```
User A shares replay
  ↓
User B clicks link (replay_viewed)
  ↓
User B installs game (app_install, source=replay_link)
  ↓
Count as viral conversion
```

### Dashboard Metrics

**Track Daily:**
1. Replays shared (total count)
2. Replay views (click-through rate)
3. Installs from replays (conversion rate)
4. Viral coefficient trend

**Goal: 0.20+ Viral Coefficient**
- 100 users × 2 shares × 10% conversion = 20 new users
- 20% growth from organic sharing alone

---

## Social Media Optimization

### Hashtag Strategy

**Game-Specific:**
- #AngryAnimals
- #Level12Domination
- #5StarRun

**Gaming Generic:**
- #MobileGaming
- #CasualGamer
- #GameplayHighlight

**Platform-Specific:**
- Twitter: Max 2-3 hashtags
- Instagram: Up to 10 hashtags
- TikTok: Trend-based hashtags

### Rich Previews (OpenGraph)

**Optimal Metadata:**
```html
<meta property="og:title" content="5-Star Run on Level 12!" />
<meta property="og:description" content="15,000 points! Can you beat it?" />
<meta property="og:image" content="[Replay Thumbnail]" />
<meta property="og:type" content="video.other" />
```

**Impact:**
- With rich preview: 8% CTR
- Without: 2% CTR
- **4x improvement**

---

## Community Building

### Replay-Driven Communities

**Discord Server Setup:**
1. #replay-showcase channel
2. Weekly "Best Replay" contests
3. Replay reaction leaderboard
4. Community challenges with replays

**Content Creation:**
- Feature top replays on social media
- "Replay of the Week" blog posts
- Compilation videos (Top 10 Replays)
- Creator program (share rewards)

---

## Incentivizing Shares

### Reward Structure

**Share Rewards:**
- First share of day: +50 coins
- Share earns 10+ views: +100 coins
- Replay goes viral (100+ views): Exclusive cosmetic

**Social Achievements:**
- "Influencer": 10 replays shared
- "Viral Legend": 100 total views
- "Content Creator": 1,000 views

**Unlock Social Cosmetics:**
- Sharing unlocks exclusive items
- Trade-off: Privacy vs cosmetics
- Opt-in system

---

## Privacy Considerations

### User Control

**Privacy Settings:**
- Toggle: "Allow replay sharing" (default ON)
- Toggle: "Show my name in replays" (default ON)
- Toggle: "Public leaderboard" (default ON)

**Content Policy:**
- No personal information in replays
- Usernames can be changed
- Report inappropriate content
- GDPR/CCPA compliant

---

## Advanced: Viral Loops

### Challenge → Share → Invite Loop

```
Player A completes level
  ↓
Shares replay publicly
  ↓
Friend B sees, clicks link
  ↓
Friend B installs game
  ↓
Friend B challenges Player A
  ↓
Player A sees notification, re-engages
  ↓
Both players retained
```

### Leaderboard → Replay → Challenge Loop

```
Player climbs leaderboard
  ↓
Shares leaderboard rank
  ↓
Friends see rank, want to compete
  ↓
Friends challenge Player
  ↓
Player defends rank, stays engaged
```

---

## Success Case Studies

### Hypothetical Scenario

**Month 1 Baseline:**
- 10,000 DAU
- 0 replay shares
- Organic growth: 2% month-over-month

**Month 2 With Replays:**
- 10,000 DAU
- 500 replays shared/day
- 5% conversion rate
- **Result**: +25 installs/day = +750/month
- **New growth rate**: 7.5% MoM

**Month 6 Projection:**
- Viral coefficient compounds
- 10,000 → 15,000 DAU
- 50% growth from replay sharing alone

---

## Actionable Checklist

✅ Implement automatic replay recording
✅ Add 1-tap share buttons to results screen
✅ Optimize share messages for each platform
✅ Set up Firebase Analytics attribution
✅ Create Discord server with replay channel
✅ Run A/B test on share message variants
✅ Track viral coefficient weekly
✅ Incentivize sharing with rewards
✅ Feature top replays on social media
✅ Monitor and optimize conversion funnel

---

## Expected Results

**Conservative Projections:**
- Viral coefficient: 0.15-0.20
- +15-20% MoM growth from sharing
- +25% engagement (challenge participation)
- +$3-5k/month revenue from social cosmetics

**Optimistic Projections:**
- Viral coefficient: 0.25-0.30
- +30-40% MoM growth
- +50% engagement
- +$10k/month revenue

---

**Turn players into evangelists. Every replay is a billboard.** 📣🚀
