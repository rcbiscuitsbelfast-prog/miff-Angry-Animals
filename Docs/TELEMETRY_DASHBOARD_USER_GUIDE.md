# Telemetry Dashboard User Guide 📊

**Real-time metrics and performance monitoring for Angry Animals development**

---

## 🎯 What You'll Learn

- How to open and use the in-game telemetry dashboard
- Reading real-time performance and analytics metrics
- Exporting session data for analysis
- Understanding what each metric means for game development
- Troubleshooting performance issues using telemetry data

---

## 📋 Prerequisites

- Angry Animals project with telemetry system enabled
- Debug or development build (telemetry dashboard hidden in release builds)
- Basic understanding of game performance metrics

---

## 🚀 Opening the Telemetry Dashboard

### Method 1: Main Menu Access
1. **Start Angry Animals** (debug build)
2. **From Main Menu**, look for **"View Metrics"** button
3. **Click "View Metrics"** to open dashboard
4. Dashboard appears as overlay panel

### Method 2: Keyboard Shortcut
1. **During gameplay**, press **F12** (debug builds only)
2. **Dashboard toggles** on/off
3. Useful for performance monitoring during play

### Method 3: Developer Toggle
1. **In editor**, select **TelemetryDebugPanel** object
2. **Inspector** → **Show Panel** checkbox
3. **Play in editor** to see dashboard

---

## 📱 Understanding the Dashboard Layout

### Dashboard Sections

```
┌─────────────────────────────────────────┐
│         📊 TELEMETRY DASHBOARD          │
├─────────────────────────────────────────┤
│ 📱 SESSION INFO                        │
│ Session Duration: 00:15:32             │
│ Firebase: ✅ Connected                 │
├─────────────────────────────────────────┤
│ ⚡ PERFORMANCE                         │
│ FPS: 60.0         Memory: 245.6 MB     │
├─────────────────────────────────────────┤
│ 📈 ANALYTICS                           │
│ Events This Session: 127               │
│ Queued Events: 5                       │
├─────────────────────────────────────────┤
│ 💥 CRASH REPORTS                       │
│ Total Crashes: 0  Last Crash: None     │
├─────────────────────────────────────────┤
│ 🗂️ RECENT EVENTS                       │
│ [14:32:15] level_completed             │
│ [14:32:10] level_started               │
│ [14:31:58] performance_frame_drop       │
├─────────────────────────────────────────┤
│ [📤 Export] [🔄 Flush] [🗑️ Clear] [❌ Close] │
└─────────────────────────────────────────┘
```

### Color Coding System
- 🟢 **Green**: Excellent performance, no issues
- 🟡 **Yellow**: Warning state, monitor closely  
- 🔴 **Red**: Critical issues, immediate attention needed
- 🔵 **Blue**: Informational, no action needed

---

## 📱 Session Info Section

### Session Duration
- **Format**: HH:MM:SS
- **What it tells you**: How long the current play session has lasted
- **Green**: >10 minutes (engaging gameplay)
- **Yellow**: 2-10 minutes (normal session)
- **Red**: <2 minutes (possible technical issues)

**Development Use Cases:**
- Test session length during playtesting
- Monitor player engagement patterns
- Identify if players are leaving quickly

### Firebase Connection Status
- **✅ Connected**: Firebase is working properly
- **❌ Unavailable**: Firebase not available (editor mode, network issues)
- **🔄 Initializing**: Firebase is starting up

**Development Use Cases:**
- Verify analytics are working during testing
- Troubleshoot analytics integration issues
- Confirm data is being sent to Firebase

---

## ⚡ Performance Section

### FPS (Frames Per Second)
- **What it measures**: How smooth the game runs
- **Target**: 60 FPS for smooth gameplay
- **Green**: ≥50 FPS (excellent)
- **Yellow**: 30-49 FPS (acceptable, monitor)
- **Red**: <30 FPS (poor performance, needs optimization)

**What Affects FPS:**
- Complex particle effects
- Large levels with many objects
- Heavy AI or physics calculations
- Device performance limitations

### Memory Usage
- **What it measures**: RAM usage in megabytes
- **Target**: <500MB for mobile devices
- **Green**: <200MB (excellent efficiency)
- **Yellow**: 200-500MB (acceptable usage)
- **Red**: >500MB (memory warning, potential crashes)

**Memory Optimization Tips:**
- High memory usage can lead to crashes on mobile
- Monitor during intensive gameplay sequences
- Look for memory leaks during long sessions

---

## 📈 Analytics Section

### Events This Session
- **What it measures**: Total analytics events logged during current session
- **What you should see**: 50-200 events for typical play session
- **Green**: Steady increase as you play
- **Yellow**: Sporadic events (possible integration issue)
- **Red**: No events or very few (analytics not working)

**Common Event Counts:**
- **Short session (5 min)**: 20-50 events
- **Medium session (15 min)**: 100-150 events  
- **Long session (30+ min)**: 200+ events

### Queued Events
- **What it measures**: Events waiting to be sent to Firebase
- **Normal**: 0-10 events (batched for efficiency)
- **High**: >10 events (possible network issues)
- **Growing**: Events accumulating (analytics may be paused)

**What This Tells You:**
- Network connectivity to Firebase
- Event processing efficiency
- Potential data loss risks

---

## 💥 Crash Reports Section

### Total Crashes
- **What it measures**: Number of crashes during this session
- **Target**: 0 crashes (perfect stability)
- **Green**: 0 crashes
- **Yellow**: 1-2 crashes (minor issues)
- **Red**: 3+ crashes (major stability problems)

### Last Crash
- **Shows**: Most recent crash information
- **Includes**: Crash type, scene name, timestamp
- **Useful for**: Reproducing and fixing issues

**Crash Analysis Tips:**
- Check crash patterns during specific scenes
- Monitor crashes after certain actions
- Note correlation with performance issues

---

## 🗂️ Recent Events Section

### Event Display
- **Format**: `[Timestamp] Event Name`
- **Shows**: Last 10 analytics events
- **Auto-scrolls**: Newest events at bottom
- **Real-time**: Updates as events occur

### Common Events You'll See
```
[14:32:15] level_completed
[14:32:10] level_started  
[14:31:58] performance_frame_drop
[14:31:45] cosmetic_purchased
[14:31:30] crash_detected
[14:31:15] memory_warning
[14:30:58] daily_login_streak_reached
[14:30:42] rewarded_ad_watched
[14:30:25] achievement_unlocked
[14:30:10] session_start
```

### Event Analysis
- **Event frequency**: Are events happening at expected rate?
- **Event order**: Does event sequence make sense?
- **Event types**: Are you seeing the events you expect?

---

## 🔧 Dashboard Controls

### Export Data Button 📤
**Purpose**: Export all telemetry data for external analysis
**Output**: 
- `analytics_export.json` - All analytics events
- `heatmap_export.csv` - Difficulty heatmap data

**When to Use:**
- End of playtesting sessions
- Before making major changes
- Creating reports for team
- Sharing data with other developers

### Flush Events Button 🔄  
**Purpose**: Immediately send all queued events to Firebase
**Effect**: Clears the "Queued Events" counter

**When to Use:**
- Testing analytics integration
- Before closing the game
- After network issues resolved
- When you want immediate data in Firebase console

### Clear Data Button 🗑️
**Purpose**: Reset all telemetry data
**Effect**: 
- Clears local analytics storage
- Resets crash counters
- Removes queued events

**When to Use:**
- Starting fresh test sessions
- Before major gameplay changes
- After fixing data collection issues
- Privacy compliance (removing user data)

### Close Button ❌
**Purpose**: Hide the telemetry dashboard
**Effect**: Panel disappears, continues tracking in background

---

## 🎯 Reading Performance Issues

### Low FPS Troubleshooting
**Symptoms**: FPS counter shows red (<30)
**Check in dashboard**:
1. **Memory usage** - Is it also red?
2. **Recent events** - Any performance warnings?
3. **Event frequency** - Are analytics affecting performance?

**Common Solutions**:
- Reduce particle effects in problematic scenes
- Optimize texture sizes
- Limit number of active objects
- Check for memory leaks

### High Memory Usage
**Symptoms**: Memory shows red (>500MB)
**Check in dashboard**:
1. **Session duration** - Has memory grown over time?
2. **Recent events** - Any memory warnings?
3. **FPS correlation** - Does performance degrade with memory?

**Common Solutions**:
- Find and fix memory leaks
- Reduce asset sizes
- Implement object pooling
- Add garbage collection

### Analytics Issues
**Symptoms**: No events or stuck queued events
**Check in dashboard**:
1. **Firebase status** - Is connection working?
2. **Event generation** - Are events being created?
3. **Network connectivity** - Can game reach Firebase?

**Common Solutions**:
- Check Firebase configuration
- Verify network connectivity
- Test with Flush Events button
- Review analytics integration code

---

## 📊 Export Data Analysis

### Analytics Export Format
```json
{
  "export_timestamp": "2024-01-15T14:30:00Z",
  "session_info": {
    "session_id": "20240115_143000",
    "duration": "00:15:32",
    "platform": "Editor"
  },
  "events": [
    {
      "event_name": "level_completed",
      "timestamp": "2024-01-15T14:32:15Z",
      "parameters": {
        "level_number": 15,
        "completion_time": 45.2,
        "attempts": 2,
        "user_segment": "free"
      }
    }
  ]
}
```

### Heatmap Export Format
```csv
Level Number,Total Attempts,Completions,Failures,Completion Rate %,Average Time (s),Fastest Time (s),Slowest Time (s),Difficulty Score,Rage Quits,First Attempt Success Rate %,Failure Reasons
15,45,32,13,71.1,45.2,32.1,78.9,65.3,3,62.2,"platform_issue(8),timing(5)"
```

### Using Export Data
**For Excel/Google Sheets**:
1. Import CSV files directly
2. Create charts for difficulty trends
3. Identify problem levels visually

**For Custom Analysis**:
1. Parse JSON events with Python/R scripts
2. Create custom dashboards
3. Generate automated reports

---

## 🎮 Real-World Usage Examples

### Playtesting Session
1. **Open dashboard** before starting
2. **Monitor FPS** during intense sequences
3. **Note memory growth** over long sessions  
4. **Export data** at end of session
5. **Review event patterns** for UX issues

### Performance Optimization
1. **Identify low FPS scenes** using dashboard
2. **Monitor memory usage** during optimization
3. **Test fixes** and watch metrics improve
4. **Export before/after data** for comparison

### Analytics Verification
1. **Check event counts** match expectations
2. **Verify Firebase connection** status
3. **Test event parameters** are meaningful
4. **Flush events** to confirm data arrival

### Bug Investigation
1. **Look for crash patterns** in Recent Events
2. **Correlate crashes** with performance issues
3. **Export crash data** for detailed analysis
4. **Track fix effectiveness** with new sessions

---

## 🔍 Advanced Dashboard Features

### Drag and Drop
- **Click and drag** panel header to move dashboard
- **Repositions** persist during session
- **Useful** for avoiding UI elements

### Auto-Hide on Focus Loss
- **Dashboard hides** when game loses focus
- **Prevents** interference during testing
- **Reappears** when game regains focus

### Console Integration
- **All dashboard actions** logged to Godot console
- **Debug information** for troubleshooting
- **Event details** for deep analysis

---

## 🛠️ Troubleshooting Dashboard Issues

### Dashboard Won't Open
**Possible Causes**:
- Not a debug build (telemetry hidden in release)
- Missing telemetry system files
- Configuration errors

**Solutions**:
- Check build type is Debug
- Verify FirebaseManager and AnalyticsEventTracker are loaded
- Review console for initialization errors

### Dashboard Shows No Data
**Possible Causes**:
- Analytics disabled in configuration
- Event tracking not initialized
- Data filtering too aggressive

**Solutions**:
- Check Firebase status indicator
- Play the game to generate events
- Verify analytics configuration

### Performance Impact
**Possible Causes**:
- Dashboard update frequency too high
- Too many events being displayed
- Memory usage growing unexpectedly

**Solutions**:
- Close dashboard when not needed
- Reduce update frequency in code
- Clear data periodically

---

## 📈 Best Practices

### When to Use Dashboard
**✅ Good Times**:
- During development and testing
- Before major feature releases
- When investigating performance issues
- During playtesting sessions

**❌ Avoid Using**:
- In production builds (hidden anyway)
- During performance testing (may skew results)
- When you need full screen gameplay
- For extended periods (close when not needed)

### Data Collection Ethics
- **Respect user privacy** - only collect necessary data
- **Clear data regularly** - don't hoard indefinitely
- **Inform users** about data collection in privacy policy
- **Provide opt-out options** where required by law

### Performance Monitoring
- **Monitor regularly** during development
- **Set performance baselines** for your target devices
- **Track trends** over time, not just snapshots
- **Act on data** - don't just collect it

---

## ✅ Dashboard Checklist

Before using dashboard for analysis:

- [ ] **Dashboard opens successfully** without errors
- [ ] **Performance metrics** update in real-time
- [ ] **Analytics events** appear as expected
- [ ] **Firebase status** shows correct connection
- [ ] **Export functionality** works properly
- [ ] **Console logs** provide useful debugging info
- [ ] **Memory usage** remains stable during use
- [ ] **Event tracking** doesn't impact gameplay

---

## 🎯 Success Metrics

Your telemetry dashboard is working well when:

- ✅ **Opens without errors** in debug builds
- ✅ **Real-time metrics** update smoothly
- ✅ **Performance data** correlates with gameplay experience
- ✅ **Analytics events** appear within expected timeframes
- ✅ **Export functionality** produces usable data files
- ✅ **Memory usage** remains stable during monitoring
- ✅ **Team uses dashboard** regularly for development insights

---

## 🆘 Common Questions

**Q: Why can't I see the dashboard?**
A: Dashboard only appears in debug builds. Check your build configuration.

**Q: Events aren't appearing in Recent Events**
A: Events appear in real-time. Play the game to generate events, then check Recent Events section.

**Q: Should I keep dashboard open always?**
A: No, close it when not actively monitoring. It uses minimal resources but can distract from gameplay.

**Q: How often should I export data?**
A: Export at end of each playtesting session or before making major changes to preserve baseline data.

**Q: Can multiple team members use dashboard simultaneously?**
A: Yes, but each person's dashboard shows their local session data only.

---

**You're now ready to monitor Angry Animals performance in real-time!** 🎮📊