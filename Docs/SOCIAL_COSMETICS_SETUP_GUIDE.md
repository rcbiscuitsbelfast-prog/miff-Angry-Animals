# Social Cosmetics Setup Guide

## Overview

Social cosmetics are exclusive items unlocked through social actions: adding friends, winning challenges, and sharing replays. This creates incentive loops that drive viral growth.

---

## Automatic Unlock System

### How It Works

**Zero Configuration Required:**
- Social cosmetics automatically track progress
- Unlock conditions checked after each social action
- Immediate unlock notification when achieved
- No manual intervention needed

**Built-In Cosmetics:**

1. **Friendship Hat**
   - **Unlock**: Add 5 friends
   - **Type**: Hat
   - **Rarity**: Rare
   - **Visual**: Friend-themed design

2. **Challenge Champion Crown**
   - **Unlock**: Win 10 friend challenges
   - **Type**: Hat
   - **Rarity**: Epic
   - **Visual**: Golden crown with stars

3. **Viral Legend Glasses**
   - **Unlock**: Get 100 replay views
   - **Type**: Glasses
   - **Rarity**: Legendary
   - **Visual**: Shiny star-shaped glasses

4. **Team Player Wig**
   - **Unlock**: Participate in 50 challenges
   - **Type**: Wig
   - **Rarity**: Rare
   - **Visual**: Team-colored hair

5. **Leaderboard Elite Moustache**
   - **Unlock**: Rank in top 100 on any level
   - **Type**: Moustache
   - **Rarity**: Epic
   - **Visual**: Elite golden moustache

---

## Viewing Progress

### In-Game Progress Tracker

**Access Progress:**
1. Main Menu → Social → Achievements
2. View all social cosmetics
3. Progress bars show completion %
4. "Locked" badge until unlocked

**Progress Display:**
```
Friendship Hat
Add 5 friends
Progress: 3/5 (60%)
[▓▓▓░░] 
```

---

## Tracking Friends Toward Achievements

### Achievement Dashboard

**Friend Activity Feed:**
- See which friends are close to unlocks
- Encourage them to complete challenges
- Celebrate when friends unlock cosmetics

**Push Notifications:**
- "You're 2 friends away from Friendship Hat!"
- "Win 3 more challenges for Champion Crown!"
- "Your replay has 95 views! Almost there!"

---

## Creating Social Cosmetic Bundles

### Bundle Configuration

**Competitor Bundle ($2.99):**
- Challenge Champion Crown
- Elite Moustache
- Challenger Glasses
- Victory Trail Effect

**Streamer Bundle ($3.99):**
- Viral Legend Glasses
- Content Creator Hat
- Streaming Wig
- Spotlight Effect

**Friends Bundle ($2.99):**
- Friendship Hat
- Team Player Wig
- Buddy Glasses
- Friendship Trail

### Bundle Creation (No Code)

**Using UnlockablesManager:**
1. Open UnlockablesManager.cs
2. Add bundle definition:
```csharp
new CosmeticBundle
{
    BundleId = "competitor_bundle",
    BundleName = "Competitor Bundle",
    Price = 2.99f,
    Items = new[] {
        "challenge_champion_crown",
        "leaderboard_elite_moustache",
        "challenger_glasses",
        "victory_trail"
    }
}
```
3. Save and restart game
4. Bundle appears in shop

---

## Pricing Social Bundles for Conversion

### Optimal Pricing Strategy

**Psychology:**
- Social cosmetics have perceived higher value (earned status)
- Players willing to pay to "skip the grind"
- Bundle discount = urgency to buy

**Price Points:**
- Individual Social Cosmetic: $1.99 (if purchasable)
- 4-Item Bundle: $2.99-3.99 (25-50% discount)
- Exclusive Bundle: $4.99 (includes limited-time items)

**A/B Testing:**
- Test $2.99 vs $3.99 for same bundle
- Monitor conversion rates
- Track lifetime value (LTV)

### Revenue Projections

**Conservative:**
- 10,000 DAU
- 2% conversion on bundles
- Average bundle price $3.49
- **Revenue**: 200 purchases/day × $3.49 = **$700/day** = **$21k/month**

**Optimistic:**
- 5% conversion (exclusive appeal)
- **Revenue**: 500 purchases/day × $3.49 = **$1,750/day** = **$52.5k/month**

---

## Seasonal Social Cosmetics

### Limited-Time Bundles

**Summer Social Event:**
- "Beach Party Bundle" ($4.99)
- Unlocks: Beach Hat, Sunglasses, Surfboard Trail
- Available June-August only
- Drives FOMO purchases

**Holiday Bundles:**
- Halloween: Spooky Social Bundle
- Christmas: Festive Friends Bundle
- Valentine's: Love Connection Bundle

**Implementation:**
```csharp
new SeasonalCosmeticBundle
{
    BundleId = "summer_social_bundle",
    AvailableFrom = new DateTime(2024, 6, 1),
    AvailableUntil = new DateTime(2024, 8, 31),
    Price = 4.99f,
    IsExclusive = true
}
```

---

## Social Achievement Notifications

### Unlock Celebrations

**When Player Unlocks:**
1. Full-screen celebration animation
2. "Achievement Unlocked!" banner
3. Show cosmetic preview
4. Auto-equip option
5. Share button ("Show off your achievement!")

**Notification Types:**
- In-game toast: "Friendship Hat unlocked!"
- Push notification (if offline): "Come back to claim your reward!"
- Email (for milestones): "You're a Viral Legend!"

---

## Analytics Integration

### Tracking Social Cosmetic Performance

**Firebase Events:**
```
social_cosmetic_unlocked
  - cosmetic_id
  - unlock_type (earned vs purchased)
  - time_to_unlock (days since install)

social_bundle_purchased
  - bundle_id
  - price
  - payment_method

social_achievement_viewed
  - achievement_id
  - progress_percentage
```

### Dashboard Metrics

**Key Performance Indicators:**
1. **Unlock Rate**: % of players unlocking each cosmetic
2. **Time to Unlock**: Average days to unlock
3. **Bundle Conversion**: % viewing bundle → purchasing
4. **Revenue per DAU**: Social cosmetic revenue / total DAU

**Optimization:**
- If unlock rate < 5%: Conditions too hard
- If bundle conversion < 2%: Price too high or value unclear
- If time to unlock > 30 days: Reduce requirement

---

## Best Practices

### Balancing Unlock Requirements

**Too Easy:**
- Friendship Hat (5 friends) = 40% unlock rate ✓
- Challenge Champion (10 wins) = 15% unlock rate ✓

**Too Hard:**
- Viral Legend (100 views) = 2% unlock rate (elite status) ✓
- Leaderboard Elite (top 100) = 1% unlock rate (prestige) ✓

**Rule of Thumb:**
- Common: 30-50% unlock rate
- Rare: 10-30%
- Epic: 5-15%
- Legendary: <5%

### Communicating Value

**In-App Messaging:**
- "Only 3% of players have this!"
- "Show your friends you're a champion!"
- "Exclusive to social masters!"

**Social Proof:**
- Show friends who've unlocked
- Leaderboard for achievement completion
- Community showcases

---

## Troubleshooting

**Problem: Cosmetic not unlocking despite meeting requirements**
- ✅ Call `SocialCosmetics.Instance.CheckAllUnlocks()`
- ✅ Verify progress in debug logs
- ✅ Check PlayerProfile.UnlockedCosmetics contains ID

**Problem: Bundle not appearing in shop**
- ✅ Restart game after adding bundle
- ✅ Check bundle ID matches cosmetic IDs
- ✅ Verify price is set correctly

**Problem: Progress not tracking**
- ✅ Ensure social systems are initialized
- ✅ Check Firebase Analytics events are firing
- ✅ Verify friend/challenge data is persisting

---

## Revenue Optimization Checklist

✅ Set competitive bundle prices ($2.99-4.99)
✅ Create seasonal limited-time bundles
✅ Use FOMO tactics (countdown timers)
✅ Offer "starter pack" with 1-2 social cosmetics
✅ A/B test bundle contents and pricing
✅ Track conversion funnels (view → purchase)
✅ Highlight exclusivity in marketing
✅ Show cosmetics in friend profiles (social proof)

---

## Expected Results

**Month 1:**
- Social cosmetics: 10-15% of total revenue
- +$3-5k additional monthly revenue
- 2-5% bundle conversion rate

**Month 6:**
- Social cosmetics: 20-25% of revenue
- +$10-15k monthly revenue
- 5-8% conversion (optimized)

**Key Success Factors:**
- Quality cosmetic design (must look cool!)
- Fair unlock requirements (achievable but aspirational)
- Strong social features (friends, challenges, replays)
- Seasonal events and exclusivity

---

**All configured and ready to drive revenue. No code changes needed!** 💰🎮
