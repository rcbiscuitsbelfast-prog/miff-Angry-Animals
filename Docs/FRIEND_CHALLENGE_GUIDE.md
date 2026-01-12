# Friend Challenge System - User Guide

## Table of Contents
1. [Overview](#overview)
2. [Creating Friend Challenges](#creating-friend-challenges)
3. [Accepting Challenges](#accepting-challenges)
4. [Challenge Rewards](#challenge-rewards)
5. [Challenge History](#challenge-history)
6. [Challenge Analytics](#challenge-analytics)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The Friend Challenge System allows you to compete directly with your friends by challenging them to beat your score on any level. This creates friendly competition and drives engagement.

**Key Features:**
- Challenge friends to beat your score on specific levels
- Receive notifications when friends challenge you
- Earn rewards for winning (and losing!) challenges
- Track your challenge history and statistics
- View friend profiles with cosmetics and achievements

---

## Creating Friend Challenges

### Step-by-Step: How to Create a Challenge

1. **Complete a Level**
   - Play any level and achieve a score you're proud of
   - After completing the level, you'll see your final score screen

2. **Initiate Challenge**
   - Look for the "Challenge Friend" button on the results screen
   - Alternatively, navigate to your Friends List from the main menu
   - Select "Create Challenge" next to any friend

3. **Select Details**
   - **Friend**: Choose which friend to challenge
   - **Level**: Select the level (defaults to your current level)
   - **Target Score**: Your score they need to beat
   - **Message** (optional): Add a personal taunt or encouragement

4. **Send Challenge**
   - Review the challenge details
   - Tap "Send Challenge"
   - Your friend will receive a notification immediately

### Challenge Options

**Challenge Message Examples:**
- "Think you can do better? 😏"
- "Beat this if you dare!"
- "I've mastered this level. Your turn!"
- "Let's see who's the real champion!"

**Challenge Duration:**
- All challenges expire after 7 days
- Friends can accept anytime before expiration
- You'll be notified if a challenge expires

---

## Accepting Challenges

### Step-by-Step: How to Accept a Challenge

1. **Receive Notification**
   - You'll get a push notification: "{Friend Name} challenged you!"
   - A badge appears on the Challenges tab in the main menu

2. **View Challenge Details**
   - Open the Challenges menu
   - Tap on the pending challenge
   - Review: Level, Target Score, Friend's Message

3. **Accept or Decline**
   - Tap "Accept Challenge" to compete
   - Tap "Decline" if you're not interested
   - Accepted challenges take you directly to the level

4. **Complete the Challenge**
   - Play the level normally
   - Try to beat your friend's target score
   - After completion, results are automatically submitted

### Challenge Status Icons

- 🟡 **Pending**: Awaiting your response
- 🟢 **Accepted**: You've accepted, now complete it
- ✅ **Completed**: Challenge finished (win or loss)
- ⏰ **Expired**: Challenge expired before acceptance
- ❌ **Declined**: You declined the challenge

---

## Challenge Rewards

### Reward Breakdown

**Loser Reward:**
- 50 Coins (consolation prize)
- Experience points for trying

**Winner Reward:**
- 200 Coins
- Cosmetic unlock progress
- Bragging rights!

**Both Complete Bonus:**
- +100 Coins for both players
- Encourages friendly competition, not avoidance

### Special Achievements

Complete challenges to unlock exclusive social cosmetics:

- **Challenge Champion**: Win 10 friend challenges → Unlock "Champion Crown"
- **Team Player**: Participate in 50 challenges → Unlock "Team Player Wig"
- **Undefeated**: Win 20 challenges in a row → Unlock "Legend Badge"

---

## Challenge History

### Viewing Past Challenges

1. Navigate to **Main Menu** → **Challenges** → **History**
2. See all completed challenges with outcomes:
   - Date completed
   - Friend name
   - Level name
   - Your score vs their score
   - Winner icon (👑)

### Statistics Dashboard

**Your Challenge Stats:**
- Total challenges sent
- Total challenges received
- Challenges won
- Challenges lost
- Win rate percentage
- Current win streak

**Per-Friend Stats:**
- Challenges with [Friend Name]
- Your record vs this friend
- Average score difference
- Last challenge date

---

## Challenge Analytics

### Tracking Challenge Engagement

**Key Metrics:**
- **Acceptance Rate**: % of challenges accepted by friends
- **Completion Rate**: % of accepted challenges completed
- **Average Response Time**: How long friends take to accept
- **Best Performing Levels**: Which levels get most challenges

### Firebase Analytics Events

The system automatically tracks:
- `challenge_created`: When you send a challenge
- `challenge_accepted`: When a friend accepts
- `challenge_won`: When you win
- `challenge_lost`: When you lose
- `challenge_expired`: When challenges expire

**Analyzing Performance:**
1. Go to Firebase Console → Analytics → Events
2. Filter by `challenge_*` events
3. View funnel: Created → Accepted → Completed
4. Identify drop-off points to improve engagement

---

## Best Practices

### Maximizing Challenge Success

**Timing:**
- Send challenges after major updates or events
- Challenge friends who are currently online
- Don't send too many challenges at once (max 5 pending per friend)

**Level Selection:**
- Choose levels with high replay value
- Avoid extremely difficult levels (low completion rate)
- Early levels = higher acceptance rates
- Perfect scores = more impressive challenges

**Message Strategy:**
- Friendly taunts increase engagement
- Emoji use = +15% acceptance rate
- Personal messages > generic challenges

### Social Strategy

**Building Friend Networks:**
- Add friends who play regularly (higher acceptance)
- Challenge new friends first (welcome gesture)
- Rotate challenges across all friends (don't spam one person)

**Competitive Ladders:**
- Create informal leagues with friend groups
- Track monthly challenge records
- Organize tournaments (most wins in 30 days)

---

## Troubleshooting

### Common Issues

**Problem: Friend didn't receive my challenge**
- ✅ Check they have push notifications enabled
- ✅ Verify friend is still in your friends list
- ✅ Check challenge wasn't auto-declined due to full pending list

**Problem: Can't accept challenge**
- ✅ Challenge may have expired (check timestamp)
- ✅ Friend may have deleted the challenge
- ✅ You may have hit your active challenge limit (20 max)

**Problem: Challenge rewards not received**
- ✅ Restart the game to sync data
- ✅ Check your coin balance increased (may not show notification)
- ✅ Contact support if coins still missing after 24 hours

**Problem: Challenge not appearing in history**
- ✅ Refresh the history tab (pull down)
- ✅ Check if challenge is still "In Progress"
- ✅ Data may be syncing with server (wait 1-2 minutes)

### Support

If you encounter persistent issues:
1. Screenshot the problem
2. Note the challenge ID (found in challenge details)
3. Contact support with: Friend Name, Level Name, Date/Time
4. Expected behavior vs actual behavior

---

## Advanced: Challenge Strategy Guide

### Psychological Edge

**Choosing Target Scores:**
- Set score slightly higher than achievable (motivates retries)
- Round numbers (10,000 instead of 9,847) = cleaner challenges
- Beat by exactly 1 point = maximum frustration 😈

**Message Tone:**
- Friendly competition > aggressive taunting
- Emoji usage increases engagement
- Inside jokes with close friends work best

### Viral Challenge Loops

**Creating Challenge Chains:**
1. You challenge Friend A
2. Friend A challenges Friend B to beat your score
3. Friend B challenges you back
4. Creates engagement loop

**Leaderboard + Challenges:**
- Challenge friends who are close to you on leaderboards
- Creates back-and-forth competition
- Drives both challenge engagement AND leaderboard activity

---

## Success Metrics

After implementing friend challenges, you should see:

✅ +10% DAU from challenge participants
✅ +20% session length (challenge attempts)
✅ +30% friend invites (to challenge new people)
✅ +15% retention (ongoing competitions)

**Measure Success:**
1. Track challenge acceptance rate (target: >60%)
2. Track completion rate (target: >70%)
3. Track friend invites from challenge CTAs
4. Monitor day 7/day 30 retention of challenge users

---

**All without touching a single line of code!** 🎮🚀
