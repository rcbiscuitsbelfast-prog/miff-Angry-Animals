# A/B Testing Guide for Non-Coders

## What is A/B Testing?

A/B testing lets you test different versions of your game features to see what works best. Think of it like comparing two different headlines for a website - you show version A to some players and version B to others, then see which gets better results.

**Why it matters:**
- **15-30% revenue increase** from optimized pricing
- **Better player retention** through improved user experience  
- **Data-driven decisions** instead of guessing
- **Reduced risk** when making changes

## How A/B Testing Works

### 1. The Players Split
- **Control Group (50% of players):** See the original version
- **Treatment Groups (50% combined):** See the new version(s)
- **Random Assignment:** Each player always sees the same variant

### 2. What Gets Tested
- **Price points** ($3.99 vs $4.99 vs $5.99)
- **Ad frequency** (every 3 levels vs 5 levels vs 8 levels)
- **Messages** (standard vs personalized vs emojis)
- **Features** (button colors, layouts, rewards)

### 3. Success Metrics
- **Conversion Rate:** % of players who make purchases
- **ARPPU:** Average Revenue Per Paying User
- **Engagement:** How long players stay in game
- **Retention:** % of players who return next day/week

## Creating an A/B Test (Inspector Method)

### Step 1: Open Inspector Configuration
1. In Unity Editor, select **ABTestingManager** in Hierarchy
2. In Inspector, expand **Test Configurations**
3. Click **+** to add new test

### Step 2: Configure Test Details
```
Test Name: "Cosmetics Price Test"
Test ID: "cosmetics_price_test" 
Description: "Testing $2.99 vs $3.99 vs $4.99 for premium cosmetics"
Duration (days): 14
Target Metric: "conversion_rate"
```

### Step 3: Set Up Variants
**Control Group:**
```
Variant ID: "control"
Variant Name: "Current Price"
Configuration: {"price": 2.99}
Traffic %: 33.3
```

**Treatment Group 1:**
```
Variant ID: "variant_1" 
Variant Name: "Higher Price"
Configuration: {"price": 3.99}
Traffic %: 33.3
```

**Treatment Group 2:**
```
Variant ID: "variant_2"
Variant Name: "Premium Price" 
Configuration: {"price": 4.99}
Traffic %: 33.4
```

### Step 4: Activate Test
1. Check **Is Active** box
2. Set **Start Date** to current time
3. Set **End Date** (typically 2+ weeks)
4. Click **Apply Configuration**

## Running the Test

### What Happens Automatically
✅ **Player Assignment:** New players randomly assigned to variants  
✅ **Persistent Assignment:** Same player always sees same variant  
✅ **Data Collection:** All player actions tracked and stored  
✅ **Statistical Analysis:** Real-time conversion rate calculations  

### What You Monitor
📊 **Daily Check:** Open A/B Testing Dashboard (Debug Builds Only)  
📊 **Key Metrics:** Conversion rate, ARPPU, user engagement  
📊 **Winner Detection:** Automatic when statistically significant  

## Reading Test Results

### Dashboard Overview
```
A/B Testing Dashboard
Active Tests (3)

Cosmetics Price Test
├─ Control: 33.3% | 1,250 users | 8.2% conversion
├─ Variant 1: 33.3% | 1,248 users | 9.8% conversion  
└─ Variant 2: 33.4% | 1,252 users | 7.1% conversion
Status: Running (5 days remaining)
Statistical Significance: Not yet reached
```

### Interpreting Results

#### Green = Good Performance
- **Conversion Rate > 15%:** Excellent
- **ARPPU Increase > 10%:** Winning variant
- **Player Engagement Up:** Users prefer this variant

#### Yellow = Neutral
- **Conversion Rate 5-15%:** Average performance
- **ARPPU Change < 10%:** No clear winner yet
- **Need More Data:** Test running too short

#### Red = Poor Performance  
- **Conversion Rate < 5%:** Below expectations
- **ARPPU Decrease > 10%:** Variant underperforming
- **Stop Early:** If variant clearly failing

### Statistical Significance Indicators

**When to Declare a Winner:**
- **Minimum Duration:** 7+ days (more data = more reliable)
- **Minimum Users:** 1,000+ per variant
- **Clear Winner:** 95% confidence level
- **Practical Impact:** >5% difference in target metric

**Example Winner Detection:**
```
🎉 WINNER: Variant 1 ($3.99 price)
✅ Statistical Confidence: 97%
✅ Conversion Rate: 9.8% vs 8.2% control (+19.5% improvement)
✅ Sample Size: 3,750 total users
✅ Duration: 14 days
```

## Common A/B Test Examples

### 1. Price Testing
**Scenario:** "Should we charge $3.99 or $4.99 for battle pass?"

**Setup:**
- Control: $3.99 (current price)
- Variant: $4.99 (higher price)
- Duration: 14 days
- Metric: conversion_rate

**Expected Results:**
- If Variant wins: Higher price acceptable
- If Control wins: Keep current price
- If Tie: Test even higher price

### 2. Ad Frequency Testing
**Scenario:** "How many ads are too many?"

**Setup:**
- Control: Interstitial every 5 levels
- Variant 1: Interstitial every 3 levels (aggressive)
- Variant 2: Interstitial every 8 levels (conservative)
- Duration: 21 days
- Metric: arpu

**Expected Results:**
- Aggressive: Higher revenue, lower retention
- Conservative: Lower revenue, better retention  
- Balanced: Best overall performance

### 3. Notification Testing
**Scenario:** "When should we send push notifications?"

**Setup:**
- Control: 9 AM, standard message
- Variant 1: 7 AM, personalized message
- Variant 2: 11 AM, emoji message
- Duration: 28 days
- Metric: retention_d1

**Expected Results:**
- Personalized: Better engagement
- Time-based: Find optimal send time
- Emoji: Test if modern messaging works

## A/B Testing Best Practices

### ✅ DO These Things
- **Test One Variable:** Don't change price AND ad frequency at once
- **Run for 2+ Weeks:** Get enough data for statistical significance
- **Have Clear Hypothesis:** "I think $4.99 will increase revenue by 20%"
- **Plan Action:** Know what you'll do if each variant wins
- **Track Secondary Metrics:** Watch for unintended consequences

### ❌ DON'T Do These Things
- **Stop Too Early:** Need minimum sample size for reliable results
- **Test Too Many Variants:** 2-3 variants maximum
- **Change Tests Mid-Way:** Let them run to completion
- **Ignore Statistical Significance:** 95% confidence minimum
- **Test Obvious Winners:** Only test when you're genuinely uncertain

### Statistical Terms Made Simple

**P-Value:** Probability the results are due to chance
- **< 0.05 = Good:** Results likely real, not random
- **> 0.05 = Bad:** Results might be random chance

**Confidence Interval:** Range where true value likely falls
- **95% Confidence:** "I'm 95% sure the true conversion rate is between X and Y"

**Sample Size:** Number of players in test
- **More = Better:** Larger samples = more reliable results
- **Minimum:** 1,000 users per variant

## Making Decisions from Test Results

### When a Variant Clearly Wins
```
Example: Price Test Results
Control ($2.99): 8.2% conversion
Variant ($3.99): 11.4% conversion  
Winner: Variant with 39% higher conversion
Action: Roll out $3.99 price to all players
Revenue Impact: +$2,500/month from 10k DAU
```

### When Results Are Unclear
```
Example: Ad Frequency Test Results  
Control (5 levels): $6.20 ARPU
Variant A (3 levels): $6.80 ARPU (+9.7%)
Variant B (8 levels): $5.90 ARPU (-4.8%)
Action: Test closer frequencies (4 vs 6 levels)
Reason: Current test inconclusive
```

### When Results Show Problems
```
Example: Notification Test Results
Control: 75% D1 retention
Variant: 68% D1 retention (-7%)
Action: Keep original notification strategy
Reason: New strategy hurts retention
```

## Exporting and Sharing Results

### Export to CSV
1. In A/B Testing Dashboard, click **Export CSV**
2. File saved to `user://exports/ab_test_results_YYYYMMDD.csv`
3. Open in Excel/Google Sheets for analysis

### CSV Contains:
- Test names and variant details
- User assignments and conversions  
- Conversion rates and statistical confidence
- Export timestamp and duration

### Share with Team
```
Email Subject: "A/B Test Results - Battle Pass Pricing"

Results Summary:
✅ Test Duration: 14 days
✅ Total Users: 7,485
✅ Winner: $3.99 price variant
✅ Improvement: +19.5% conversion rate
✅ Revenue Impact: +$2,500/month
✅ Statistical Confidence: 97%

Recommendation: Implement $3.99 price globally

Next Steps:
1. Update store listings
2. Monitor for negative feedback
3. Consider testing bundle pricing
```

## Pre-Launch Validation Checklist

Before launching your game, run these A/B tests:

### Critical Tests (Must Run)
- [ ] **Battle Pass Price:** $3.99 vs $4.99 vs $5.99
- [ ] **Ad Frequency:** Conservative vs Balanced vs Aggressive  
- [ ] **First Purchase:** $0.99 vs $1.99 vs $2.99
- [ ] **Tutorial Length:** 3 levels vs 5 levels vs 7 levels

### Important Tests (Should Run)
- [ ] **Daily Login Reward:** 5 coins vs 10 coins vs 20 coins
- [ ] **Push Notification Time:** 7am vs 9am vs 11am
- [ ] **Challenge Rewards:** 50 coins vs 100 coins vs 150 coins
- [ ] **Level Difficulty:** Easy vs Normal vs Hard progression

### Optional Tests (Nice to Have)
- [ ] **Cosmetic Bundle Sizes:** 3 items vs 5 items vs 7 items
- [ ] **Event Duration:** 2 weeks vs 4 weeks vs 6 weeks
- [ ] **Social Features:** Hidden vs Visible vs Prominent
- [ ] **Leaderboard Updates:** Real-time vs 5 min vs 1 hour

### Success Criteria
Each test should show:
- **Clear Winner:** Statistically significant difference
- **Practical Impact:** >5% improvement in target metric
- **Revenue Protection:** No negative impact on monetization
- **Player Satisfaction:** No increase in negative feedback

## Measuring Success

### Short-term (During Test)
- **Daily Monitoring:** Check dashboard for anomalies
- **Player Feedback:** Monitor app store reviews and social media
- **Performance Impact:** Watch for crashes or bugs

### Long-term (After Implementation)  
- **Revenue Tracking:** Measure actual revenue increase
- **Retention Monitoring:** Ensure improvements persist
- **Player Satisfaction:** Track app store ratings
- **Competitive Analysis:** Monitor competitor responses

### Success Metrics
```
Target Improvements:
Revenue: +15-30% from optimized pricing
Retention: +5-10% from better UX
Engagement: +10-20% from optimized features
Conversion: +20-40% from better messaging

Example Results:
Before A/B Testing: $5.00 ARPU, 45% D7 retention
After A/B Testing: $6.20 ARPU (+24%), 52% D7 retention (+16%)
Annual Impact: +$150,000 revenue from 10k DAU
```

This comprehensive A/B testing framework gives you the power to make data-driven decisions that significantly improve your game's performance and revenue. Start with critical tests before launch, and continuously optimize based on real player behavior!