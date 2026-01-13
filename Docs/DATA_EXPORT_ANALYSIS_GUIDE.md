# Data Export Analysis Guide for Non-Coders

## What is Data Export?

Data export lets you extract all your game analytics from the game and analyze them in Excel, Google Sheets, or other tools. Think of it like downloading your bank statement to see your spending patterns - you get raw data to make informed decisions.

**Why it matters:**
- **15-30% revenue increase** from data-driven optimizations
- **Better game balance** through difficulty analysis
- **Improved player retention** via targeted improvements
- **Competitive advantage** through deep data analysis

## What Data Can You Export?

### A/B Testing Data
**What it shows:** Which game features work best
```
Example: Battle Pass Price Test
Variant A ($3.99): 12.4% conversion rate
Variant B ($4.99): 15.8% conversion rate
Variant C ($5.99): 11.2% conversion rate
Winner: $4.99 with 27% higher conversion
```

### Performance Metrics
**What it shows:** How well your game runs
```
Example: Performance Data
Level 1: 58 FPS average, 234MB memory
Level 8: 32 FPS average, 456MB memory  
Level 15: 18 FPS average, 678MB memory
Issue: Performance degrades in later levels
```

### Difficulty Heatmap
**What it shows:** Which levels are too hard or easy
```
Example: Level Difficulty
Level 3: 15% failure rate (too easy)
Level 8: 78% failure rate (too hard)
Level 12: 89% failure rate (impossible)
Level 15: 23% failure rate (perfect balance)
```

### Sales Data
**What it shows:** What players buy and when
```
Example: Cosmetics Sales
Common Rarity: 45% of sales, $1.99 avg
Rare Rarity: 35% of sales, $3.99 avg
Legendary Rarity: 20% of sales, $7.99 avg
Insight: Rare items have best profit margin
```

## Exporting Data from the Game

### Step 1: Access Data Export
**For Developers:**
1. In Unity Editor, select **DataExporter** in Hierarchy
2. In Inspector, find **Available Exports**
3. Check boxes for data types you want

**For Players (Debug Builds Only):**
1. Open **Developer Menu** (press **D** key)
2. Click **Data Export** tab
3. Select export types
4. Click **Export All** or individual **Export** buttons

### Step 2: Choose Export Format
**CSV Format (Recommended for Analysis):**
- Opens in Excel, Google Sheets
- Easy to sort and filter
- Good for creating charts and graphs
- Compatible with most analysis tools

**JSON Format (For Developers):**
- Structured data format
- Good for custom analysis tools
- Preserves data relationships
- Used by advanced analytics platforms

**Excel Format (Ready-to-Use):**
- Pre-formatted with charts
- Pivot tables already created
- Professional presentation
- Share with stakeholders

### Step 3: Export Location
**Automatic Export Locations:**
```
Desktop: ~/Desktop/GameExports/
Documents: ~/Documents/GameExports/
Game Folder: user://exports/
Cloud Drive: ~/Google Drive/GameExports/ (if configured)
```

**File Naming Convention:**
```
A/B Tests: ab_test_results_20240115_143022.csv
Performance: performance_metrics_20240115_143022.csv
Difficulty: difficulty_heatmap_20240115_143022.csv
Sales: cosmetics_sales_20240115_143022.csv
```

### Step 4: Automated Scheduling
**Daily Exports (Automatic):**
- Performance metrics (every day at 2 AM)
- Crash reports (every day at 3 AM)
- Sales data (every day at 4 AM)

**Weekly Exports (Automatic):**
- A/B test results (every Sunday at 2 AM)
- Difficulty heatmap (every Saturday at 2 AM)
- Retention cohorts (every Friday at 2 AM)

**Monthly Exports (Automatic):**
- Comprehensive analysis report (1st of month)
- Business intelligence dashboard (1st of month)
- Player behavior patterns (1st of month)

## Opening Data in Excel/Google Sheets

### Opening CSV Files
**Excel (Windows/Mac):**
1. Open Excel
2. File → Open → Select CSV file
3. Choose **Delimited** format
4. Select **Comma** as delimiter
5. Choose **General** for data types
6. Click **Finish**

**Google Sheets:**
1. Open Google Sheets
2. File → Import → Upload
3. Select CSV file from computer
4. Choose **Comma** as separator
5. Click **Import Data**

### Recommended Sheet Setup
**Create Separate Tabs for Each Dataset:**
```
Tab 1: "A/B Test Results"
Tab 2: "Performance Metrics"  
Tab 3: "Difficulty Heatmap"
Tab 4: "Sales Data"
Tab 5: "Dashboard Summary"
```

## Creating Charts and Analysis

### A/B Testing Analysis

#### Step 1: Create Conversion Rate Chart
```
Data Selection: Test Name, Variant, Conversion Rate
Chart Type: Column Chart
Title: "Battle Pass Price Test Results"

Data Example:
Control ($3.99): 8.2% conversion
Variant A ($4.99): 11.4% conversion
Variant B ($5.99): 7.8% conversion

Result: Clear winner is $4.99 price
```

#### Step 2: Statistical Significance Analysis
```
Formula for Confidence:
=IF(B2>0.05,"Not Significant","Significant")

Result Interpretation:
P-value < 0.05 = Statistically significant
P-value > 0.05 = Results may be random
```

#### Step 3: Revenue Impact Calculation
```
Formula for Revenue Impact:
=($B2*$C2)*10000

Where:
B2 = Conversion rate
C2 = Average purchase amount
10000 = Number of users

Example:
11.4% * $4.99 * 10,000 = $5,686 additional monthly revenue
```

### Performance Analysis

#### Step 1: FPS Trend Chart
```
X-Axis: Time (every 5 minutes)
Y-Axis: FPS (frames per second)
Series: Current FPS, Average FPS, Minimum FPS

Interpretation:
- Green line (30+ FPS): Good performance
- Yellow line (20-30 FPS): Needs attention
- Red line (<20 FPS): Critical issues
```

#### Step 2: Memory Usage Over Time
```
X-Axis: Time
Y-Axis: Memory Usage (MB)
Series: Current Memory, Peak Memory, Available Memory

Warning Levels:
- <300MB: Safe
- 300-500MB: Monitor closely
- >500MB: Risk of crashes
```

#### Step 3: Performance by Level
```
Data Layout:
Level ID | Avg FPS | Peak Memory | Load Time | Issues
Level 1  | 58.2   | 234 MB      | 1.2s     | None
Level 8  | 32.1   | 456 MB      | 2.8s     | FPS Drop
Level 15 | 18.4   | 678 MB      | 4.5s     | Critical

Color Coding:
Green: Performance OK
Yellow: Needs attention  
Red: Critical issues
```

### Difficulty Heatmap Analysis

#### Step 1: Failure Rate Chart
```
Chart Type: Bar Chart
X-Axis: Level ID
Y-Axis: Failure Rate (%)

Target Levels:
- <20%: Too easy (may bore players)
- 20-60%: Perfect range (good challenge)
- 60-80%: Too hard (may frustrate)
- >80%: Impossible (fix immediately)
```

#### Step 2: Completion Time Analysis
```
Chart Type: Scatter Plot
X-Axis: Level ID
Y-Axis: Average Completion Time (minutes)
Bubble Size: Number of attempts

Interpretation:
- Small bubbles: Few attempts (easy or avoided)
- Large bubbles: Many attempts (difficult)
- Color: Difficulty score
```

#### Step 3: Rage Quit Analysis
```
Formula for Rage Quit Rate:
=IF(D2>0.1,"High Risk",IF(D2>0.05,"Monitor","Good"))

Where D2 = Rage Quit Rate

Action Items:
High Risk: Redesign level immediately
Monitor: Add hints or checkpoints
Good: Current difficulty appropriate
```

### Sales Analysis

#### Step 1: Revenue by Item Rarity
```
Chart Type: Pie Chart
Data: Rarity Type, Sales Count, Revenue
Colors: Common (Green), Rare (Blue), Legendary (Purple)

Insight: Which rarity generates most revenue
```

#### Step 2: Price Point Analysis
```
Chart Type: Line Chart
X-Axis: Price Point ($)
Y-Axis: Sales Count
Multiple Lines: Different item types

Optimization: Find optimal price points
```

#### Step 3: Sales Over Time
```
Chart Type: Line Chart with Trendline
X-Axis: Date
Y-Axis: Daily Revenue
Trendline: Shows growth/decline patterns

Business Intelligence:
- Seasonal patterns
- Marketing campaign impact
- Long-term trends
```

## Pivot Table Analysis

### A/B Testing Pivot Table
**Setup:**
1. Select A/B test data
2. Insert → PivotTable
3. Rows: "Test Name" + "Variant"
4. Values: "Conversion Rate" (Average)
5. Filters: "Date Range"

**Analysis Questions Answered:**
- Which variant performs best across all tests?
- What's the average conversion rate per test?
- How do results vary by test duration?

### Performance Pivot Table
**Setup:**
1. Select performance data
2. Insert → PivotTable  
3. Rows: "Level ID"
4. Values: "Average FPS" + "Peak Memory"
5. Filters: "Device Type"

**Analysis Questions Answered:**
- Which levels have worst performance?
- How does performance vary by device?
- What's the performance trend over time?

### Sales Pivot Table
**Setup:**
1. Select sales data
2. Insert → PivotTable
3. Rows: "Item Rarity" + "Item Type"
4. Values: "Revenue" (Sum) + "Quantity Sold" (Count)
5. Filters: "Date Range"

**Analysis Questions Answered:**
- Which items generate most revenue?
- What's the best performing rarity?
- How do sales vary by time period?

## Advanced Analysis Techniques

### 1. Cohort Analysis
**Player Retention Cohorts:**
```
Create cohorts by signup date:
Week 1 Cohort: Players who joined Week 1
Week 2 Cohort: Players who joined Week 2
Track retention for each cohort over time

Formula for D7 Retention:
=COUNTIF(Day7Column,"Active")/COUNT(Day7Column)

Results:
Week 1 Cohort: 45% D7 retention
Week 2 Cohort: 52% D7 retention  
Week 3 Cohort: 48% D7 retention
Insight: Week 2 had best onboarding
```

### 2. Funnel Analysis
**Purchase Funnel:**
```
Step 1: Store Visit (10,000 players)
Step 2: Browse Items (6,500 players) = 65%
Step 3: Add to Cart (2,800 players) = 28%
Step 4: Complete Purchase (1,400 players) = 14%

Formula for Step Conversion:
=Step2Count/Step1Count*100

Optimization Opportunities:
- 35% drop from visit to browse (improve store visibility)
- 50% drop from browse to cart (improve item appeal)
- 50% drop from cart to purchase (optimize checkout)
```

### 3. Correlation Analysis
**Performance vs Retention:**
```
Data: Average FPS vs D1 Retention by Level
Chart Type: Scatter Plot

Interpretation:
- Strong positive correlation = Better performance = Better retention
- No correlation = Performance doesn't affect retention
- Negative correlation = Other factors more important

Example Result:
Correlation = 0.78 (strong)
Insight: Improving FPS could improve retention
```

## Simple Analysis Questions & Answers

### "Which level has highest failure rate?"
**Answer Steps:**
1. Open difficulty_heatmap CSV
2. Sort by "Failure Rate" column (highest to lowest)
3. Look at top entries

**Expected Result:**
```
Level 12: 89% failure rate
Level 8: 78% failure rate  
Level 15: 71% failure rate
Action: Redesign Level 12 immediately
```

### "Which cosmetic rarity sells best?"
**Answer Steps:**
1. Open cosmetics_sales CSV
2. Create pivot table with "Rarity" and "Revenue"
3. Sort by total revenue

**Expected Result:**
```
Legendary: $45,200 (35% of revenue)
Rare: $38,900 (30% of revenue)
Common: $25,400 (20% of revenue)
Insight: Legendary items most profitable
```

### "Is battle pass worth $4.99?"
**Answer Steps:**
1. Open ab_test_results CSV
2. Filter for "battle_pass_price_test"
3. Compare conversion rates

**Expected Result:**
```
$3.99: 8.2% conversion, $3.27 ARPU
$4.99: 12.4% conversion, $4.96 ARPU  
$5.99: 7.1% conversion, $4.26 ARPU
Answer: Yes, $4.99 wins with 51% higher ARPU
```

### "Are performance issues affecting retention?"
**Answer Steps:**
1. Merge performance and retention data
2. Create scatter plot: FPS vs D1 Retention
3. Calculate correlation

**Expected Result:**
```
Correlation: 0.73 (strong positive)
Low FPS levels: 65% D1 retention
High FPS levels: 82% D1 retention
Insight: Fixing FPS could improve retention by 17%
```

## Sharing Data with Team

### Email Report Template
```
Subject: Weekly Game Analytics - Performance Issues Detected

Key Findings:
🔴 CRITICAL: Level 12 has 89% failure rate
🟡 WARNING: FPS drops below 20 in 3 levels
🟢 POSITIVE: A/B test shows $4.99 optimal price

Data Exports:
📊 Full analysis: performance_dashboard_20240115.xlsx
📊 A/B results: ab_test_results_20240115.csv
📊 Sales data: cosmetics_sales_20240115.csv

Action Items:
1. Redesign Level 12 (high priority)
2. Optimize particle effects in Levels 8, 12, 15
3. Implement $4.99 battle pass price
4. Monitor Level 12 redesign impact

Next Review: January 22, 2024
```

### Dashboard Creation
**Executive Summary Dashboard:**
```
Metrics Cards:
- Total Revenue: $127,450 (+15% vs last month)
- D1 Retention: 68% (+3% vs last month)  
- Average FPS: 34.2 (-2.1 vs last month)
- A/B Tests Active: 3

Charts:
- Revenue trend (last 30 days)
- Retention cohorts (weekly)
- Performance by level (heatmap)
- Top performing A/B tests
```

### Team Collaboration
**Shared Google Sheets:**
1. Create team folder: "GameAnalytics"
2. Share with relevant team members
3. Set appropriate permissions (View/Edit)
4. Include analysis notes and insights

**Weekly Team Review:**
1. Review exported data together
2. Discuss insights and patterns
3. Plan optimization experiments
4. Assign action items

## Excel Formulas for Common Analysis

### Percentage Calculations
```excel
Conversion Rate:
=B2/COUNT(B:B)*100

Growth Rate:
=(B2-B1)/B1*100

Percentage of Total:
=B2/SUM(B:B)*100
```

### Performance Metrics
```excel
Average FPS:
=AVERAGE(C:C)

Performance Rating:
=IF(C2>=30,"Good",IF(C2>=20,"Fair","Poor"))

Trend Direction:
=IF(D2>D1,"Improving","Declining")
```

### Business Intelligence
```excel
Revenue per User:
=B2/C2

ROI Calculation:
=(B2-A2)/A2*100

Statistical Significance:
=IF(E2<0.05,"Significant","Not Significant")
```

## Success Metrics

### Data Analysis KPIs
```
Monthly Analysis Goals:
✅ Export all data types weekly
✅ Create 5+ meaningful insights monthly
✅ Identify 3+ optimization opportunities
✅ Share findings with team regularly

Analysis Impact:
• 20% more optimization opportunities found
• 40% faster problem identification
• 60% better decision making
• 80% more data-driven changes
```

### Business Impact Tracking
```
Data-Driven Improvements:
• Revenue Optimization: +$15,000/month from A/B testing
• Performance Fixes: +8% retention from FPS improvements
• Difficulty Balancing: +12% completion rate
• User Experience: +0.4 app store rating

Annual Business Impact: +$200,000+ from data analysis
```

This comprehensive data export and analysis system transforms raw game data into actionable business intelligence, enabling continuous optimization and data-driven decision making for maximum game success!