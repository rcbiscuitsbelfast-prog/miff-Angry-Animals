# Analytics Configuration Guide for Angry Animals 📈

**Customize what your game tracks - no coding required!**

---

## 🎯 What You'll Learn

- How to enable/disable specific event tracking using Inspector checkboxes
- Which events to prioritize for your business goals
- How to interpret Firebase reports and insights
- How to create custom user segments for analysis
- Frequency recommendations for efficient data collection

---

## 📋 Prerequisites

- Completed Firebase Setup Guide
- Access to Angry Animals project in editor
- Basic understanding of your game goals

---

## 🔧 Event Tracking Configuration

### Accessing Analytics Settings

1. **Open Angry Animals project** in editor
2. **Select FirebaseManager** in the scene hierarchy
3. **Inspector panel** will show analytics configuration options

### Event Category Toggles

In the Inspector, you'll see these checkbox options:

#### 🎮 Gameplay Events
- ✅ **Level Started**: Tracks when players begin levels
- ✅ **Level Completed**: Tracks successful level completions
- ✅ **Level Failed**: Tracks failed attempts
- ✅ **Perfect Score**: Tracks perfect gameplay achievements

**Recommendation**: Keep all enabled - essential for difficulty balancing

#### 💰 Monetization Events  
- ✅ **Cosmetic Purchased**: Track cosmetic sales
- ✅ **Remove Ads Purchased**: Track ad removal purchases
- ✅ **Rewarded Ad Watched**: Track engagement with reward ads
- ✅ **Battle Pass**: Track battle pass purchases

**Recommendation**: Keep all enabled - critical for revenue analysis

#### 📱 Engagement Events
- ✅ **Daily Login Streak**: Track player retention
- ✅ **Achievement Unlocked**: Track progression engagement
- ✅ **Seasonal Events**: Track event participation

**Recommendation**: Keep all enabled - vital for retention analysis

#### ⚠️ Quality Events
- ✅ **Crash Detected**: Track game stability
- ✅ **Performance Issues**: Track frame rate problems
- ✅ **Memory Warnings**: Track performance bottlenecks

**Recommendation**: Keep all enabled - essential for game quality

---

## 🎯 Prioritizing Events for Your Goals

### For Revenue Analysis
**Priority Events:**
1. `cosmetic_purchased` - Track what's selling
2. `remove_ads_purchased` - Monitor ad-free conversions
3. `rewarded_ad_watched` - Analyze ad engagement
4. `level_completed` - Track gameplay monetization moments

### For Player Retention
**Priority Events:**
1. `daily_login_streak_reached` - Measure habit formation
2. `level_completed` / `level_failed` - Track progression satisfaction
3. `achievement_unlocked` - Monitor engagement spikes
4. `session_start` / `session_end` - Measure session patterns

### For Game Balance
**Priority Events:**
1. `level_failed` - Identify problematic levels
2. `perfect_score_achieved` - Track skill ceilings
3. `performance_frame_drop` - Monitor device performance
4. `crash_detected` - Track stability issues

### For Marketing Optimization
**Priority Events:**
1. `cosmetic_unlocked` - Track cosmetic appeal
2. `seasonal_event_started` - Measure event engagement
3. `user_segment` - Analyze different player types
4. `device_type` - Understand your audience

---

## 📊 Reading Firebase Console Reports

### Accessing Analytics Dashboard

1. **Go to Firebase Console**: https://console.firebase.google.com
2. **Select your project**
3. **Click "Analytics"** in left sidebar
4. **Click "Dashboard"** for overview

### Key Metrics Explained

#### 👥 User Acquisition
- **New Users**: How many new players per day/week/month
- **Active Users**: Daily Active Users (DAU), Weekly Active Users (WAU)
- **User Retention**: % of users who return after first visit

**What to look for:**
- Retention dropping below 20% day-1 = onboarding issues
- Retention dropping below 5% day-7 = mid-game content problems

#### 🎮 Engagement
- **Session Duration**: How long players stay in each session
- **Session Frequency**: How often players return
- **Levels Completed**: Gameplay progression metrics

**What to look for:**
- Session duration < 5 minutes = potential engagement issues
- Low levels completed = difficulty or motivation problems

#### 💰 Revenue
- **Purchase Events**: When and what players buy
- **Revenue Metrics**: Track income trends
- **Conversion Rates**: % of players who make purchases

**What to look for:**
- Purchase conversion < 2% = monetization optimization needed
- Revenue spikes during specific events = marketing opportunities

---

## 🎨 Creating Custom Event Segments

### Step 1: Define User Segments

#### Segment Examples:
- **New Players**: Users with <7 days since install
- **Veterans**: Users with >30 days since install  
- **High Spenders**: Users who spent >$10
- **Casual Players**: Users with <30 minutes play time
- **Hardcore Players**: Users with >3 hours play time

### Step 2: Create Segments in Firebase

1. **Firebase Console** → **Analytics** → **Audiences**
2. **Click "New audience"**
3. **Define conditions**:

**New Players Example:**
- First open ≥ 7 days ago (NOT)
- Country = [Your target countries]
- Platform = Android/iOS

**High Spenders Example:**
- Purchase revenue ≥ $10
- Transaction count ≥ 1
- Currency = USD

### Step 3: Apply Segments to Analysis

1. **Go to Analytics** → **Reports**
2. **Click "Add filter"**
3. **Select your custom audience**
4. **Compare metrics** between segments

---

## 🚀 Understanding Custom Events

### Available Events in Angry Animals

| Event Name | Trigger | Key Parameters | Use Case |
|------------|---------|----------------|----------|
| `level_started` | Player begins level | `level_number`, `user_segment` | Track progression |
| `level_completed` | Level successful | `level_number`, `completion_time`, `attempts` | Difficulty analysis |
| `level_failed` | Level failed | `level_number`, `attempts`, `failure_reason` | Problem identification |
| `cosmetic_purchased` | Cosmetic bought | `cosmetic_type`, `cost`, `currency` | Revenue analysis |
| `remove_ads_purchased` | Ad removal bought | `cost`, `user_segment` | Monetization tracking |
| `daily_login_streak` | Login streak reached | `streak_days`, `is_new_record` | Retention measurement |
| `crash_detected` | Game crash occurs | `crash_type`, `scene_name` | Quality monitoring |

### Custom Event Analysis Examples

#### "Which cosmetics sell best?"
```
Event: cosmetic_purchased
Break down by: cosmetic_type
Filter by: cost > 0 (paid purchases only)
```

#### "Where do players get frustrated?"
```
Event: level_failed  
Break down by: level_number
Sort by: event count (descending)
Analyze: failure_reason parameter
```

#### "How effective are reward ads?"
```
Event: rewarded_ad_watched
Break down by: reward_type
Analyze: reward_amount, user_segment
```

---

## 📱 Setting Up Crash Alerts

### Step 1: Configure Crash Notifications

1. **Firebase Console** → **Crashlytics** → **Alerts**
2. **Click "Create alert"**
3. **Set alert conditions**:

**Critical Crash Alert:**
- New crash rate > 5% of sessions
- Immediate email notification
- Slack/Teams integration (optional)

**Performance Alert:**
- Frame drops > 20% of frames
- Memory warnings > 100MB
- Daily summary email

### Step 2: Set Team Notifications

1. **Project Settings** → **Integrations**
2. **Add email addresses** for your team
3. **Set notification frequency**:
   - **Immediate**: Critical crashes
   - **Daily digest**: Performance issues
   - **Weekly summary**: General trends

---

## ⏱️ Event Frequency Recommendations

### High Priority Events (Always Track)
- **Session start/end**: Essential for user metrics
- **Level completed/failed**: Core gameplay tracking
- **Purchases**: Critical for revenue analysis
- **Crashes**: Essential for quality monitoring

**Impact**: Minimal battery/data usage
**Value**: Maximum business insights

### Medium Priority Events (Track Most Games)
- **Achievement unlocked**: Good for engagement analysis
- **Daily login streaks**: Valuable for retention
- **Performance metrics**: Important for quality
- **Cosmetic interactions**: Useful for design insights

**Impact**: Low battery/data usage
**Value**: Good additional insights

### Low Priority Events (Selective Tracking)
- **Tutorial completion**: Useful only if tutorial exists
- **Feature usage**: Track only important features
- **Social interactions**: Track if social features exist
- **In-depth performance**: Track only during development

**Impact**: Moderate battery/data usage
**Value**: Situational business value

### Events to Avoid Tracking
- **Every button click**: Too noisy, low value
- **UI navigation**: Rarely actionable
- **Debug events**: Should be disabled in production
- **High-frequency events**: Can impact performance

---

## 🔍 Interpreting Firebase Reports

### User Retention Analysis

#### Day 1 Retention
- **Excellent**: >40%
- **Good**: 25-40%
- **Needs Improvement**: 10-25%
- **Critical**: <10%

**Actions based on retention:**
- **High retention**: Game is engaging, focus on monetization
- **Low retention**: Improve tutorial, early game experience

#### Day 7 Retention  
- **Excellent**: >20%
- **Good**: 10-20%
- **Needs Improvement**: 5-10%
- **Critical**: <5%

**Actions based on retention:**
- **High retention**: Strong core loop, add more content
- **Low retention**: Mid-game content needs improvement

### Revenue Analysis

#### Conversion Rates
- **Excellent**: >5% of users make purchases
- **Good**: 2-5% conversion rate
- **Needs Improvement**: 1-2% conversion rate
- **Critical**: <1% conversion rate

#### Average Revenue Per User (ARPU)
- **Track monthly**: Monthly revenue ÷ Active users
- **Compare segments**: Paying vs non-paying users
- **Monitor trends**: Are revenue metrics growing?

### Engagement Metrics

#### Session Duration
- **Excellent**: >15 minutes average
- **Good**: 5-15 minutes average
- **Needs Improvement**: 2-5 minutes average
- **Critical**: <2 minutes average

**Actions based on session duration:**
- **Short sessions**: Add more content or improve pacing
- **Long sessions**: Check for engagement or addiction patterns

#### Session Frequency
- **Daily players**: Most valuable users
- **Weekly players**: Good engagement
- **Monthly players**: At risk of churning
- **Dormant players**: Need re-engagement campaigns

---

## 🎛️ Advanced Configuration

### Custom Event Parameters

You can enhance events with additional context:

```csharp
// Example: Enhanced level completion tracking
AnalyticsEventTracker.Instance.TrackLevelCompleted(
    levelNumber: 15,
    completionTime: 45.2f,
    attempts: 2,
    score: 1250,
    perfect: false
);

// This automatically adds context:
// - user_segment: "premium" or "free"
// - device_type: "high_end", "mid_range", "low_end"  
// - session_duration: Time since session start
// - platform: "Android", "iOS", "Editor"
```

### Privacy Compliance

#### GDPR Compliance
1. **User Consent**: Analytics only tracks if user consents
2. **Data Deletion**: Provide options to clear analytics data
3. **Data Export**: Allow users to download their data

#### Implementation Notes
- Analytics respects user consent settings
- Local data storage follows retention policies
- No personally identifiable information tracked

---

## 🛠️ Testing Your Configuration

### Step 1: Enable Debug Mode

1. **Select FirebaseManager** in scene
2. **Inspector** → **Enable Debug Logging** ✅
3. **Play the game** and watch console output

### Step 2: Verify Event Tracking

1. **Open Telemetry Dashboard** (Debug builds only)
2. **Monitor "Events This Session"** counter
3. **Check "Recent Events"** list for real-time events
4. **Verify Firebase status** shows connected

### Step 3: Validate in Console

1. **Firebase Console** → **Analytics** → **Events**
2. **Wait 2-5 minutes** for events to appear
3. **Verify event parameters** are being tracked
4. **Check event frequency** matches expectations

---

## 📈 Making Data-Driven Decisions

### Decision Framework

1. **Identify the Problem**: Low retention? Poor revenue? Balance issues?
2. **Find Relevant Data**: Which events show the problem?
3. **Segment Analysis**: Who is affected? New vs veteran players?
4. **Implement Changes**: Based on data insights
5. **Measure Impact**: Track same events after changes
6. **Iterate**: Continuous improvement cycle

### Common Decision Examples

#### "Level 15 has too many failures"
```
Problem: level_failed events spike at level 15
Analysis: 70% failure rate vs 30% average
Action: Reduce difficulty or add hints
Measurement: level_failed rate should decrease
```

#### "Cosmetic sales are low"
```
Problem: cosmetic_purchased events below target
Analysis: Low conversion rate, high price sensitivity  
Action: Add cheaper cosmetics, improve presentation
Measurement: cosmetic_purchase conversion should increase
```

#### "Players churn after day 3"
```
Problem: day_3 retention below 15%
Analysis: New players struggle with progression
Action: Improve tutorial, add daily rewards
Measurement: day_3 retention should improve
```

---

## ✅ Configuration Checklist

Before going live:

- [ ] **Event tracking configured** based on business goals
- [ ] **User segments defined** for targeted analysis
- [ ] **Crash alerts set up** for critical issues
- [ ] **Retention metrics** being tracked
- [ ] **Revenue events** properly configured
- [ ] **Privacy compliance** implemented
- [ ] **Debug mode tested** in development
- [ ] **Firebase console** accessible to team
- [ ] **Reporting schedule** established (daily/weekly reviews)

---

## 🎯 Success Metrics

Your analytics are working well when:

- ✅ **Events appear in Firebase console** within 5 minutes
- ✅ **Retention reports** show actionable data
- ✅ **Crash alerts** notify you of critical issues
- ✅ **Revenue tracking** provides business insights
- ✅ **Performance data** helps optimize game quality
- ✅ **Team reviews** analytics reports regularly

---

## 🆘 Troubleshooting Common Issues

### Events Not Appearing
- **Check event frequency** - Firebase batches events
- **Verify configuration** - Events may be disabled
- **Test with telemetry dashboard** - See events in real-time
- **Wait for propagation** - Can take 5+ minutes

### Poor Data Quality
- **Review event parameters** - Are they meaningful?
- **Check data sampling** - May need larger sample size
- **Validate user segments** - Are they properly defined?
- **Analyze event flow** - Are events logically connected?

### Privacy Compliance Issues
- **Implement consent flow** - Get user permission first
- **Provide data deletion** - Allow users to clear data
- **Review data retention** - Don't keep data longer than needed
- **Document compliance** - Keep records of consent

---

**You're now ready to make data-driven decisions for Angry Animals!** 📊🎮