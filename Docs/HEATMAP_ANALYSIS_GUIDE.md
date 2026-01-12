# Heatmap Analysis Guide 📊

**Identify problem levels and balance difficulty using data-driven insights**

---

## 🎯 What You'll Learn

- How to read difficulty heatmap data and identify problem areas
- Understanding "rage quit" patterns and what they mean
- Using heatmap data to make informed level rebalancing decisions
- Exporting and analyzing data in Excel or Google Sheets
- Actionable insights from difficulty analysis

---

## 📋 Prerequisites

- Angry Animals with telemetry system enabled
- Some gameplay data collected (at least 20-30 level attempts)
- Access to difficulty heatmap data (via telemetry dashboard)
- Basic understanding of your game's difficulty progression

---

## 🔍 Understanding Difficulty Heatmap Data

### What the Heatmap Tracks

The difficulty heatmap automatically tracks for each level:

- **Total Attempts**: How many times players tried the level
- **Completion Rate**: % of attempts that succeeded
- **Average Completion Time**: How long successful attempts took
- **Difficulty Score**: Calculated difficulty rating (0-100)
- **Rage Quit Count**: Number of players who quit after 3+ failures
- **First Attempt Success Rate**: % who beat it first try

### Reading the Data

```
Level | Attempts | Completions | Success Rate | Avg Time | Difficulty | Rage Quits
------|----------|-------------|--------------|----------|------------|------------
  15  |    45    |     32      |    71.1%     |  45.2s   |   65.3     |     3
  16  |    38    |     15      |    39.5%     |  67.8s   |   87.2     |     8
  17  |    12    |      2      |    16.7%     |  89.1s   |   95.1     |     4
```

**What This Tells You:**
- **Level 15**: Well-balanced (71% success rate, reasonable time)
- **Level 16**: Too difficult (39% success rate, long completion times)
- **Level 17**: Way too hard (17% success rate, players are rage quitting)

---

## 🚨 Identifying Problem Levels

### Red Flags in Heatmap Data

#### 1. Extremely Low Success Rate
**Indicator**: <30% completion rate
**What it means**: Level is too difficult or poorly designed
**Action**: Simplify or redesign level

#### 2. Very Long Completion Times
**Indicator**: 2x longer than expected time
**What it means**: Level is confusing or overly complex
**Action**: Streamline objectives or add hints

#### 3. High Rage Quit Rate
**Indicator**: >30% of attempts result in rage quits
**What it means**: Players getting frustrated and giving up
**Action**: Immediate difficulty adjustment needed

#### 4. Difficulty Spikes
**Indicator**: Sudden jump in difficulty score vs neighbors
**What it means**: Poor difficulty progression
**Action**: Smooth out difficulty curve

### Success Rate Benchmarks

| Success Rate | Assessment | Action Required |
|--------------|------------|----------------|
| >80% | Too Easy | Make slightly harder |
| 60-80% | Well Balanced | Keep as-is |
| 40-60% | Challenging | Monitor closely |
| 20-40% | Too Hard | Reduce difficulty |
| <20% | Broken/Unfair | Immediate fix needed |

---

## 😡 Understanding "Rage Quit" Patterns

### What Counts as a Rage Quit

**Definition**: Player makes 3+ attempts on same level within 5 minutes, then stops playing for at least 30 seconds.

**Why It Matters**: Rage quits are the strongest indicator of player frustration and likely churn.

### Rage Quit Patterns to Watch

#### Pattern 1: Multiple Levels in Sequence
```
Level 12: 2 rage quits
Level 13: 4 rage quits  
Level 14: 6 rage quits
```
**Interpretation**: Difficulty spike affecting multiple levels
**Action**: Review entire difficulty curve, not just individual levels

#### Pattern 2: Single Problem Level
```
Level 18: 12 rage quits
Level 19: 1 rage quit
Level 20: 0 rage quits
```
**Interpretation**: One outlier level causing major frustration
**Action**: Focus on Level 18 specifically

#### Pattern 3: Increasing Rage Quit Rate
```
Week 1: 2 rage quits total
Week 2: 8 rage quits total
Week 3: 15 rage quits total
```
**Interpretation**: Game getting harder or players less skilled
**Action**: Consider difficulty rebalancing or better onboarding

### Rage Quit Prevention

**Signs of healthy difficulty progression**:
- Rage quits spread across multiple levels
- No single level has >20% rage quit rate
- Rage quit rate decreases over time (learning effect)

**Warning signs**:
- One level dominates rage quits
- Rage quit rate increasing over time
- Rage quits clustered in specific difficulty range

---

## 📈 Making Data-Driven Level Rebalancing Decisions

### Decision Framework

1. **Identify the Problem**: Which levels have concerning metrics?
2. **Analyze Patterns**: Is it isolated or systematic?
3. **Check Player Segments**: Are new players affected differently?
4. **Prioritize Fixes**: Start with highest impact changes
5. **Implement & Measure**: Track improvement after changes

### Common Rebalancing Strategies

#### Strategy 1: Difficulty Curve Smoothing
**Problem**: Sudden difficulty spike
**Solution**: Add intermediate difficulty levels
**Example**: Levels 15-16 have 20-point difficulty jump
**Fix**: Create Level 15.5 with intermediate difficulty

#### Strategy 2: Hint System Integration
**Problem**: High completion times but good success rate
**Solution**: Add optional hints for struggling players
**Example**: Level has 85% success rate but 2x expected time
**Fix**: Add subtle hints to guide players faster

#### Strategy 3: Mechanic Introduction
**Problem**: New gameplay mechanic causing confusion
**Solution**: Dedicated tutorial levels for new mechanics
**Example**: New "power-up" mechanic shows high rage quit rate
**Fix**: Add tutorial level specifically for power-ups

#### Strategy 4: Visual/Audio Feedback
**Problem**: Players don't understand level objectives
**Solution**: Improve visual/audio cues
**Example**: High completion time, good success rate
**Fix**: Make objectives clearer, add better feedback

### Priority Matrix

| Issue Severity | Frequency | Fix Priority | Effort | Timeline |
|----------------|-----------|--------------|---------|----------|
| Critical (>50% rage quits) | Any | P0 | Any | Immediate |
| High (>30% rage quits) | Multiple levels | P1 | Low-Medium | 1-2 weeks |
| Medium (20-30% rage quits) | Single level | P2 | Low | Next sprint |
| Low (<20% rage quits) | Any | P3 | When convenient | Backlog |

---

## 📊 Exporting and Analyzing Data

### Using Telemetry Dashboard Export

1. **Open telemetry dashboard** during/after play session
2. **Click "Export Data"** button
3. **Files created**:
   - `analytics_export.json` - Raw analytics events
   - `heatmap_export.csv` - Difficulty heatmap data

### Excel/Google Sheets Analysis

#### Step 1: Import CSV Data
1. **Open Excel/Google Sheets**
2. **File** → **Import** → **Upload** → Select `heatmap_export.csv`
3. **Choose delimiter**: Comma
4. **Data types**: Automatic detection

#### Step 2: Create Difficulty Visualization

**Create a simple chart**:
1. **Select columns**: Level Number, Difficulty Score, Success Rate
2. **Insert** → **Chart** → **Scatter Plot**
3. **X-axis**: Level Number
4. **Y-axis**: Difficulty Score
5. **Size/Color**: Success Rate (for visual heatmap effect)

**Result**: Visual difficulty curve with problem areas highlighted

#### Step 3: Identify Outliers

**Sort by metrics to find issues**:
```
Sort by Success Rate (ascending) - Find too-hard levels
Sort by Difficulty Score (descending) - Find hardest levels  
Sort by Rage Quits (descending) - Find most frustrating levels
Sort by Average Time (descending) - Find slowest levels
```

### Advanced Analysis Formulas

#### Calculate Difficulty Index
```excel
=(100-Success_Rate) * 0.4 + (Avg_Time/Expected_Time) * 100 * 0.3 + (Rage_Quits/Attempts) * 100 * 0.3
```

#### Find Difficulty Spikes
```excel
=ABS(Current_Difficulty - AVERAGE(Previous_Level_Difficulty,Next_Level_Difficulty))
```

#### Player Retention Impact
```excel
=Rage_Quits * 2 + (100-Success_Rate) * 0.5  // Higher = worse for retention
```

---

## 🎯 Interpreting Heatmap Analysis Results

### Healthy Game Indicators

✅ **Good Difficulty Progression**:
- Success rates between 60-80% for most levels
- Difficulty scores increase gradually
- Rage quits spread across multiple levels (not clustered)
- Average completion times reasonable (<2x expected)

✅ **Good Player Learning**:
- First few levels have high success rates
- Difficulty gradually increases
- Players adapt to new mechanics
- Rage quit rate decreases over time

✅ **Sustainable Engagement**:
- No single "wall" level with <20% success
- Players completing progression paths
- Session lengths reasonable (not too short)

### Problem Indicators

🚨 **Critical Issues**:
- Any level with <10% success rate
- Consecutive levels with <30% success rate  
- >50% rage quit rate on any level
- Difficulty score >90 on early levels

🚨 **Retention Risks**:
- High rage quit rate early in game
- Difficulty spikes without warning
- New mechanics introduced without proper ramp-up
- Players giving up before reaching content

### Action Items from Analysis

#### Immediate Actions (This Week)
- [ ] Fix any levels with <20% success rate
- [ ] Address rage quit rates >40% on any level
- [ ] Smooth out difficulty spikes >30 points
- [ ] Add hints or tutorials for confusing mechanics

#### Short-term Actions (Next Sprint)  
- [ ] Redesign levels with 20-40% success rate
- [ ] Add visual/audio feedback for unclear objectives
- [ ] Create intermediate difficulty levels for spikes
- [ ] Implement progressive tutorial system

#### Long-term Actions (Next Month)
- [ ] Analyze player segment differences (new vs veteran)
- [ ] A/B test different difficulty curves
- [ ] Track correlation between difficulty and monetization
- [ ] Develop dynamic difficulty adjustment system

---

## 🔄 Tracking Improvement Over Time

### Before/After Comparison

**Export heatmap data**:
1. **Before changes**: Export current difficulty data
2. **Implement fixes**: Make your balance changes
3. **After changes**: Collect new data (1-2 weeks)
4. **Compare**: Use same metrics to measure improvement

### Success Metrics

**Quantitative Improvements**:
- Success rate increases by 10-15%
- Rage quit rate decreases by 50%+
- Average completion times normalize
- Difficulty curve becomes smoother

**Qualitative Improvements**:
- Players report level 15-20 as "challenging but fair"
- Fewer complaints about "impossible" levels
- Better session lengths (players not giving up early)
- Higher completion rates for later levels

### Iteration Cycle

1. **Collect baseline data** (1-2 weeks normal play)
2. **Identify problem areas** using heatmap analysis
3. **Implement targeted fixes** based on data insights
4. **Collect new data** with same metrics
5. **Measure improvement** and iterate as needed

---

## 📋 Heatmap Analysis Checklist

### Data Collection Phase
- [ ] **Sufficient sample size**: At least 20 attempts per level
- [ ] **Diverse player base**: Mix of skill levels represented
- [ ] **Normal gameplay**: No unusual events or promotions during collection
- [ ] **Clean data**: No corrupted or incomplete entries

### Analysis Phase
- [ ] **Success rates calculated** for all levels
- [ ] **Difficulty scores reviewed** for spikes
- [ ] **Rage quit patterns identified** and categorized
- [ ] **Player segments analyzed** separately if needed
- [ ] **Comparative analysis** done (before/after or segment comparisons)

### Action Planning Phase
- [ ] **Priority list created** based on impact vs effort
- [ ] **Specific fixes designed** for each problem area
- [ ] **Success metrics defined** for measuring improvement
- [ ] **Timeline established** for implementing changes
- [ ] **Follow-up plan created** for data collection

---

## 🎯 Success Stories

### Case Study 1: "The Wall"
**Problem**: Level 25 had 12% success rate, 60% rage quit rate
**Analysis**: Introduced new "timing-based" mechanic without tutorial
**Solution**: Added dedicated tutorial level, made timing more forgiving
**Result**: Success rate increased to 45%, rage quits dropped to 15%

### Case Study 2: "Difficulty Cliff"  
**Problem**: Levels 18-22 showed sudden difficulty spike
**Analysis**: New enemy type + environmental hazards introduced simultaneously
**Solution**: Split introduction across 3 levels, added intermediate challenges
**Result**: Success rates normalized (65-75%), rage quits spread out

### Case Study 3: "Confusion Factor"
**Problem**: High completion times but good success rates on Level 12
**Analysis**: Players understood what to do but took circuitous routes
**Solution**: Added visual hints and clearer level layout
**Result**: Average completion time decreased 40%, success rate maintained

---

## 🆘 Common Analysis Mistakes

### Mistake 1: Ignoring Sample Size
**Wrong**: Making decisions based on 5 attempts
**Right**: Wait for 20+ attempts per level before analysis

### Mistake 2: Not Segmenting Players
**Wrong**: Treating all players the same
**Right**: Analyze new players vs veterans separately

### Mistake 3: Fixing Symptoms, Not Causes
**Wrong**: Making level easier without understanding why it's hard
**Right**: Investigate root cause (confusion, skill gap, poor design)

### Mistake 4: Over-correcting
**Wrong**: Making major changes based on small sample
**Right**: Make incremental changes and measure impact

### Mistake 5: Not Tracking Improvement
**Wrong**: Making changes without measuring results
**Right**: Establish baseline, implement changes, measure improvement

---

## 🏁 Conclusion

Difficulty heatmap analysis transforms level balancing from guesswork into data-driven decision making. By regularly analyzing this data and making incremental improvements, you can:

- **Increase player satisfaction** by fixing frustrating levels
- **Improve retention** by smoothing difficulty progression  
- **Optimize engagement** by ensuring appropriate challenge level
- **Make informed decisions** about new content based on data

The key is consistency: collect data regularly, analyze systematically, and implement changes thoughtfully. Your players will thank you with longer sessions, better completion rates, and more positive reviews.

---

**Ready to turn your difficulty data into actionable insights!** 📊🎮