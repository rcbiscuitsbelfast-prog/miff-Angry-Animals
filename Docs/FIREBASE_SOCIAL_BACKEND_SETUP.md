# Firebase Social Backend Setup Guide

## Overview

This guide configures Firebase Realtime Database for social features: leaderboards, friend lists, challenges, and replays.

**Estimated Time**: 20 minutes  
**Difficulty**: Intermediate  
**Prerequisites**: Firebase project created (see FIREBASE_SETUP_GUIDE.md)

---

## Database Structure

### Recommended Structure

```json
{
  "leaderboards": {
    "by_level": {
      "level_01": {
        "top_100_scores": [
          {
            "player_id": "player_001",
            "player_name": "ProGamer",
            "score": 15000,
            "stars": 5,
            "rank": 1,
            "date": "2024-01-20T14:00:00Z",
            "replay_id": "replay_123"
          }
        ]
      }
    },
    "total_score": {
      "top_100_players": [...]
    },
    "perfect_levels": {
      "top_100_players": [...]
    },
    "replay_views": {
      "top_100_replays": [...]
    }
  },
  "players": {
    "player_001": {
      "profile": {
        "player_name": "ProGamer",
        "total_score": 145230,
        "perfect_levels": 8,
        "achievements_count": 42
      },
      "friends": {
        "friend_001": {
          "friend_id": "friend_001",
          "friend_name": "CasualGamer",
          "friendship_date": "2024-01-15T10:00:00Z"
        }
      },
      "challenges": {
        "challenge_001": {
          "status": "completed",
          "winner_id": "friend_001"
        }
      },
      "replays": {
        "replay_001": {
          "level_id": "level_01",
          "score": 15000,
          "view_count": 42
        }
      }
    }
  },
  "challenges": {
    "challenge_001": {
      "challenger_id": "player_001",
      "challengee_id": "player_002",
      "level_id": "level_01",
      "target_score": 15000,
      "status": "pending",
      "created_date": "2024-01-20T10:00:00Z"
    }
  },
  "replays": {
    "replay_001": {
      "player_id": "player_001",
      "level_id": "level_01",
      "score": 15000,
      "encoded_data": "[base64-string]",
      "view_count": 42,
      "share_count": 5
    }
  },
  "social_events": {
    "2024-01-20": {
      "friend_added": 45,
      "challenge_created": 128,
      "replay_shared": 67
    }
  }
}
```

---

## Step 1: Create Database

1. **Firebase Console** → Your Project
2. **Realtime Database** → **Create Database**
3. Select location: **us-central1** (or closest to users)
4. Start in **Test Mode** (we'll add security rules later)
5. Click **Enable**

---

## Step 2: Configure Security Rules

### Basic Security Rules

**Replace default rules with:**

```json
{
  "rules": {
    "leaderboards": {
      ".read": true,
      ".write": false,
      "by_level": {
        "$level_id": {
          "top_100_scores": {
            ".write": "auth != null"
          }
        }
      }
    },
    "players": {
      "$player_id": {
        ".read": true,
        ".write": "$player_id === auth.uid",
        "profile": {
          ".validate": "newData.hasChildren(['player_name', 'total_score'])"
        },
        "friends": {
          "$friend_id": {
            ".validate": "newData.hasChildren(['friend_id', 'friend_name'])"
          }
        },
        "challenges": {
          "$challenge_id": {
            ".write": "$player_id === auth.uid || data.child('challengee_id').val() === auth.uid"
          }
        },
        "replays": {
          "$replay_id": {
            ".write": "$player_id === auth.uid"
          }
        }
      }
    },
    "challenges": {
      "$challenge_id": {
        ".read": true,
        ".write": "auth != null && (newData.child('challenger_id').val() === auth.uid || newData.child('challengee_id').val() === auth.uid)"
      }
    },
    "replays": {
      "$replay_id": {
        ".read": true,
        ".write": "auth != null && newData.child('player_id').val() === auth.uid"
      }
    },
    "social_events": {
      ".read": "auth != null",
      ".write": false
    }
  }
}
```

**What These Rules Do:**
- ✅ Anyone can read leaderboards (public)
- ✅ Players can only write their own data
- ✅ Challenge participants can update challenges
- ✅ Replay owners can update their replays
- ✅ Social events are read-only (admin analytics)

---

## Step 3: Add Indexes for Performance

**Firebase Console** → **Realtime Database** → **Rules** tab

Add indexing rules:

```json
{
  "rules": {
    "leaderboards": {
      "by_level": {
        "$level_id": {
          "top_100_scores": {
            ".indexOn": ["score", "date"]
          }
        }
      },
      "total_score": {
        ".indexOn": ["score"]
      }
    },
    "players": {
      "$player_id": {
        "friends": {
          ".indexOn": ["friend_name", "last_interaction_date"]
        },
        "challenges": {
          ".indexOn": ["status", "created_date"]
        },
        "replays": {
          ".indexOn": ["level_id", "score", "view_count"]
        }
      }
    },
    "challenges": {
      ".indexOn": ["status", "created_date", "challenger_id", "challengee_id"]
    },
    "replays": {
      ".indexOn": ["player_id", "level_id", "view_count", "score"]
    }
  }
}
```

**Why Indexes Matter:**
- Query performance: 10x-100x faster
- Enable sorting by score, date, views
- Essential for leaderboards

---

## Step 4: Set Up Cloud Functions (Optional)

### Leaderboard Validation Function

**Purpose**: Prevent cheating by validating scores server-side

Create `functions/index.js`:

```javascript
const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp();

// Validate leaderboard submission
exports.validateScore = functions.database
  .ref('/leaderboards/by_level/{levelId}/top_100_scores/{playerId}')
  .onWrite(async (change, context) => {
    const newScore = change.after.val();
    const levelId = context.params.levelId;
    
    // Check if score is suspiciously high
    const maxPossibleScore = getMaxScoreForLevel(levelId);
    
    if (newScore.score > maxPossibleScore) {
      console.log(`Suspicious score detected: ${newScore.score} on ${levelId}`);
      // Flag for manual review
      await admin.database()
        .ref(`/admin/suspicious_scores/${context.params.playerId}`)
        .set({
          level_id: levelId,
          score: newScore.score,
          timestamp: admin.database.ServerValue.TIMESTAMP
        });
      
      // Optionally remove the score
      return change.after.ref.remove();
    }
    
    return null;
  });

function getMaxScoreForLevel(levelId) {
  // Define max possible scores per level
  const maxScores = {
    'level_01': 20000,
    'level_02': 25000,
    // ... etc
  };
  return maxScores[levelId] || 50000; // Default max
}

// Clean up expired challenges
exports.cleanupExpiredChallenges = functions.pubsub
  .schedule('every 24 hours')
  .onRun(async (context) => {
    const now = Date.now();
    const challengesRef = admin.database().ref('/challenges');
    
    const snapshot = await challengesRef
      .orderByChild('status')
      .equalTo('pending')
      .once('value');
    
    const updates = {};
    snapshot.forEach(child => {
      const challenge = child.val();
      const expirationDate = new Date(challenge.expiration_date).getTime();
      
      if (expirationDate < now) {
        updates[`${child.key}/status`] = 'expired';
      }
    });
    
    if (Object.keys(updates).length > 0) {
      await challengesRef.update(updates);
      console.log(`Expired ${Object.keys(updates).length} challenges`);
    }
    
    return null;
  });
```

**Deploy Cloud Functions:**
```bash
cd functions
npm install firebase-functions firebase-admin
firebase deploy --only functions
```

---

## Step 5: Configure Data Retention

### Automatic Data Cleanup

**Firebase Console** → **Realtime Database** → **Usage**

**Retention Policies:**
- **Replays**: Delete after 30 days
- **Expired Challenges**: Delete after 7 days
- **Old Leaderboard Entries**: Keep top 100 only
- **Social Events**: Aggregate daily, keep 90 days

**Cloud Function for Cleanup:**

```javascript
exports.cleanupOldReplays = functions.pubsub
  .schedule('every 24 hours')
  .onRun(async (context) => {
    const thirtyDaysAgo = Date.now() - (30 * 24 * 60 * 60 * 1000);
    const replaysRef = admin.database().ref('/replays');
    
    const snapshot = await replaysRef
      .orderByChild('recorded_date')
      .endAt(thirtyDaysAgo)
      .once('value');
    
    const updates = {};
    snapshot.forEach(child => {
      updates[child.key] = null; // Mark for deletion
    });
    
    if (Object.keys(updates).length > 0) {
      await replaysRef.update(updates);
      console.log(`Deleted ${Object.keys(updates).length} old replays`);
    }
    
    return null;
  });
```

---

## Step 6: Test Database Connection

### C# Code Integration

In your game, test Firebase connection:

```csharp
using Firebase.Database;

public async void TestFirebaseConnection()
{
    try
    {
        var reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        // Test write
        await reference.Child("test").SetValueAsync("Hello Firebase!");
        
        // Test read
        var snapshot = await reference.Child("test").GetValueAsync();
        var value = snapshot.Value.ToString();
        
        GD.Print($"Firebase test successful: {value}");
    }
    catch (Exception ex)
    {
        GD.PrintErr($"Firebase test failed: {ex.Message}");
    }
}
```

---

## Step 7: Monitor Database Usage

### Firebase Console Monitoring

**Realtime Database** → **Usage** tab

**Key Metrics to Track:**
- **Storage**: Should stay under 1GB (free tier)
- **Downloads**: Monitor bandwidth usage
- **Connections**: Concurrent user count
- **Operations**: Read/write frequency

**Optimization Tips:**
- Cache leaderboards locally (5-minute intervals)
- Batch write operations where possible
- Use indexes for all queries
- Compress replay data (base64 encoding)

---

## Step 8: Set Up Backup

### Automated Backups

**Firebase Console** → **Realtime Database** → **Backups**

**Configuration:**
1. Enable daily backups
2. Retention: 7 days
3. Location: Same as database region
4. Restore process: Firebase Console → Backups → Restore

**Manual Export:**
```bash
firebase database:get / --project your-project-id > backup.json
```

---

## Troubleshooting

### Common Issues

**Problem: Permission denied when writing data**
- ✅ Check security rules allow write for authenticated users
- ✅ Verify user is authenticated (`auth != null`)
- ✅ Check `player_id` matches `auth.uid`

**Problem: Slow leaderboard queries**
- ✅ Add indexes for queried fields
- ✅ Limit query results (top 100 only)
- ✅ Cache results locally

**Problem: Database quota exceeded**
- ✅ Enable data compression
- ✅ Delete old replays/challenges
- ✅ Implement data retention policies
- ✅ Upgrade to paid plan if needed

**Problem: Cloud Functions not triggering**
- ✅ Check function logs in Firebase Console
- ✅ Verify deployment: `firebase deploy --only functions`
- ✅ Check trigger conditions are met

---

## Cost Optimization

### Firebase Pricing (Spark Plan - Free)

**Free Tier Limits:**
- **Storage**: 1 GB
- **Downloads**: 10 GB/month
- **Connections**: 100 simultaneous

**Staying Under Limits:**
1. Cache leaderboards locally (reduces downloads)
2. Compress replay data (reduces storage)
3. Limit concurrent connections (queue requests)
4. Delete old data automatically

**When to Upgrade (Blaze Plan):**
- >10k DAU (downloads exceed 10 GB/month)
- Need more than 1 GB storage
- Require Cloud Functions scaling

**Estimated Costs (Blaze Plan):**
- 10k DAU: ~$20-50/month
- 50k DAU: ~$100-200/month
- 100k DAU: ~$300-500/month

---

## Security Best Practices

### Checklist

✅ Enable authentication (Firebase Auth)
✅ Use security rules to restrict write access
✅ Validate all user input server-side
✅ Implement rate limiting (Cloud Functions)
✅ Monitor for suspicious activity
✅ Encrypt sensitive data before storage
✅ Regular security audits
✅ Keep Firebase SDK updated

---

## Success Metrics

After setup, you should see:

✅ Leaderboards sync in <5 seconds
✅ Friend data persists across devices
✅ Challenges delivered in real-time
✅ Replay uploads complete in <2 seconds
✅ Zero security rule violations
✅ Database operations stay under quota

---

**Your social backend is now production-ready!** 🚀🔥
