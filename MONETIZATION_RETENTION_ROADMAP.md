# MONETIZATION + RETENTION ROADMAP (8–10 Weeks)

**Project:** Angry Animals (Godot 4.x, C#)  
**Deliverable:** Implementation roadmap (no code)  
**Audience:** Designers / producers / QA (assumes **no coding knowledge**) + devs  
**Last updated:** 2026-01-12

> **Current baseline already in the repo:**
> - Ads: AdMob banner + interstitial (+ rewarded manager exists)
> - IAP: `full_game_unlock` + `remove_ads`
> - Local telemetry: `AnalyticsManager` (writes `user://analytics_data.json`) + `PerformanceMonitor`
> - Build readiness: `BuildValidator` and store/export docs in `/Docs`

---

## Table of Contents

1. [Executive Overview](#executive-overview)
2. [Phase 1: Analytics & Data Foundation (Weeks 1–2)](#phase-1-analytics--data-foundation-weeks-12)
3. [Phase 2: Monetization Enhancement (Weeks 2–4)](#phase-2-monetization-enhancement-weeks-24)
4. [Phase 3: Retention Mechanics (Weeks 4–6)](#phase-3-retention-mechanics-weeks-46)
5. [Phase 4: Social & Engagement (Weeks 6–8)](#phase-4-social--engagement-weeks-68)
6. [Phase 5: Quality Assurance (Weeks 8–9)](#phase-5-quality-assurance-weeks-89)
7. [Phase 6: Asset Pipeline & Polish (Week 10)](#phase-6-asset-pipeline--polish-week-10)
8. [Reusable Templates (Copy/Paste)](#reusable-templates-copypaste)
9. [FAQ (Non‑Coders)](#faq-noncoders)

---

## How to use this document (non‑coders)

### “Inspector screenshot” legend
When you see blocks like this, they describe what to click in the Godot editor:

```
Scene Tree: ShopScreen (Control)
Inspector (right panel)
  Script Variables
    CatalogPath: res://Config/cosmetics_catalog.json
    UseSeasonalFiltering: [x]
```

### What you can safely change without code
- **JSON/CSV in `res://Config/`**: content, prices, tiers, rewards, events, A/B tests
- **Godot Project Settings keys**: toggles (enable analytics, debug), product IDs
- **Inspector exported fields**: file paths, booleans, numbers, UI references

### Golden rule
Change **one thing at a time**, test immediately, and keep a backup copy of config files.

---

## Executive Overview

### Timeline (8–10 weeks total)

| Phase | Weeks | Main deliverable |
|------:|:-----:|------------------|
| 1 | 1–2 | Firebase Analytics + Crashlytics + event tracking framework |
| 2 | 2–4 | Cosmetics Shop + Battle Pass (20–30 tiers) + Seasonal cosmetics |
| 3 | 4–6 | Daily login streak + Push notifications + Seasonal events (4/year) |
| 4 | 6–8 | Friend leaderboard challenges + Replay sharing + Level sharing |
| 5 | 8–9 | A/B testing + Telemetry dashboards + Difficulty heatmap + Ad tuning |
| 6 | 10 | Asset/pipeline polish + Build optimization verification + Pre-launch QA |

### Total effort (120–150 dev hours)

| Phase | Hours |
|------:|------:|
| 1 | 22–28 |
| 2 | 30–38 |
| 3 | 22–30 |
| 4 | 18–24 |
| 5 | 16–20 |
| 6 | 10–12 |
| **Total** | **118–152** (target: **120–150**) |

### Revenue projection ("$250k+/year potential")

A simple target model (this is what you’re building towards):
- **DAU:** ~10,000 average
- **ARPDAU:** ~$0.07 (ads + IAP combined)
- **Annual:** 10,000 × 0.07 × 365 ≈ **$255,500/year**

Where ARPDAU comes from:
- Ads: better retention → more impressions per user
- IAP: more purchase surfaces (cosmetics + battle pass) → more payers + repeat purchases

### Why each phase matters

- **Phase 1** makes every future decision measurable (and prevents revenue loss from crashes).
- **Phase 2** adds repeatable purchase surfaces beyond “unlock all/remove ads”.
- **Phase 3** creates daily habits (retention drives both ad impressions and purchase likelihood).
- **Phase 4** adds social “reasons to return” and organic sharing.
- **Phase 5** replaces guessing with A/B tests + dashboards + difficulty tuning from real data.
- **Phase 6** reduces build risk, improves perceived quality, and makes updates faster.

### Definition of “done” for the roadmap

At the end of week 10, you should be able to:
- See clean analytics dashboards for retention + monetization funnels.
- Run a season with cosmetics + battle pass, and adjust it via JSON.
- Run daily rewards + 4 seasonal event templates per year.
- Share a level/replay and compare with friends.
- Run an A/B test and read the result.
- Ship builds with a repeatable QA/pipeline checklist.

---

## Phase 1: Analytics & Data Foundation (Weeks 1–2)

### What gets built (technical overview)

- **Firebase Analytics** integration (Android + iOS)
- **Firebase Crashlytics** integration (crash reporting)
- **Event tracking framework**: one consistent API used across the game
- **Consent / privacy switch** (enable/disable analytics cleanly)
- **Validation**: a simple “analytics smoke test” checklist for every release

### Why it matters (revenue/retention impact)

- Crashes and freezes reduce ratings, store conversion, and retention.
- Without event tracking, you cannot confidently optimize ads, difficulty, or pricing.
- Crashlytics + “breadcrumbs” (recent events) drastically speed up bug triage.

### What events to track (minimum viable list)

You can add more later, but do not ship without these.

| Category | Event | Required parameters |
|---|---|---|
| Lifecycle | `first_open` | none |
| Lifecycle | `session_start` | `session_id` |
| Lifecycle | `session_end` | `duration_seconds` |
| Gameplay | `level_start` | `level_id`, `mode`, `seed` (if procedural) |
| Gameplay | `level_complete` | `level_id`, `stars`, `time_seconds`, `attempts` |
| Gameplay | `level_fail` | `level_id`, `fail_reason` |
| Monetization | `ad_impression` | `ad_type`, `placement` |
| Monetization | `iap_purchase_start` | `product_id` |
| Monetization | `iap_purchase_success` | `product_id`, `price`, `currency` |
| Retention | `daily_login` | `streak_day` |
| Social | `share_level` | `level_id`, `seed` |

### Non-coder setup instructions (Firebase)

1. **Create Firebase project** (Firebase Console → Add project).
2. **Add Android app**
   - Enter package name (must match Godot export).
   - Download `google-services.json`.
3. **Add iOS app**
   - Enter bundle identifier (must match iOS export).
   - Download `GoogleService-Info.plist`.
4. **Enable Crashlytics** (Firebase → Crashlytics → Enable).
5. **Enable DebugView** (so you can watch events live while testing).

### Inspector / Project Settings checklist

**A) Project Settings → keys**
- `analytics/enabled` (bool)
- `analytics/provider` (string: `firebase` or `local`)
- `analytics/consent_required` (bool)
- `analytics/debug` (bool)

**B) Autoloads**
- Ensure analytics singleton is enabled (Project Settings → Autoload).

**C) Quick verification steps (non-coders)**
1. Build a device version.
2. Launch → complete one level → quit.
3. Confirm events appear in Firebase DebugView.
4. Trigger a controlled crash on a test build (dev-only) and confirm Crashlytics receives it.

### Configuration format (editable without code)

**Event mapping JSON**: `res://Config/analytics_event_map.json`

```json
{
  "events": {
    "level_start": {"firebase": "level_start"},
    "level_complete": {"firebase": "level_complete"},
    "iap_purchase_success": {"firebase": "purchase"}
  }
}
```

### Example data

A “test session” should generate at least:
- `session_start`
- `level_start`
- `level_complete` OR `level_fail`
- `session_end`

### Troubleshooting

- **No events in Firebase**: bundle/package ID mismatch is the #1 cause.
- **Crashlytics empty**: test on a real device build; allow time for data to appear.
- **Privacy concerns**: never send personal data (names/photos); keep analytics behind consent if required.

### Metrics to watch after Phase 1

- Crash‑free users % (target **99.5%+**)
- D1/D7 retention baseline
- Session length (median + p95)
- Level completion rate per level
- Purchase funnel (view → start → success)

---

## Phase 2: Monetization Enhancement (Weeks 2–4)

### What gets built (technical overview)

- **Cosmetics Shop UI** (data-driven catalog; categories + preview + owned state)
- **Pricing tiers** + bundles + “featured” shelf
- **Battle Pass framework**
  - 20–30 tiers
  - 4‑week seasons
  - free track + premium track
  - XP sources (play + missions)
- **Seasonal cosmetics** (items can be date‑gated to seasons/events)

### Why it matters

- Shop/battle pass turns monetization into an ongoing system instead of one-time purchases.
- A “season” creates urgency (“I should play this month”).
- Cosmetics are safe: they monetize without harming fairness.

### Pricing tiers (starter plan)

Start simple, then optimize in Phase 5:
- Individual skins: **$0.99**
- Effects (trail/hit): **$1.99**
- Packs/bundles: **$2.99–$4.99**
- Premium battle pass: **$4.99**

### Price optimization strategy (non-coder readable)

1. **Launch with fixed tiers** for 2–4 weeks.
2. Measure:
   - shop open rate
   - purchase conversion
   - first purchase rate (critical)
3. Make **one change at a time**:
   - change price OR change layout OR change messaging
4. Prefer testing “value” via bundles before raising prices.

### Non-coder workflow: add a cosmetic item (no code)

1. Add icon + asset files under `Assets/`.
2. Add an item entry in `cosmetics_catalog.json`.
3. Run the game → open shop → confirm item shows and previews correctly.

**Inspector-style checklist (Shop scene)**

```
Scene Tree: ShopScreen (Control)
Inspector
  Script Variables
    CatalogPath: res://Config/cosmetics_catalog.json
    FeaturedCategory: slingshot_skin
    UseSeasonalFiltering: [x]
    ShowOwnedFirst: [x]
```

### Non-coder workflow: create a new battle pass season

1. Copy `battle_pass_season_template.json` → rename to the new season.
2. Set `start_utc`, `end_utc`, `tiers`, `xp_per_tier`.
3. Fill rewards (free + premium).
4. Point the manager to the file.

```
Scene Tree: BattlePassManager (Node)
Inspector
  Script Variables
    ActiveSeasonConfigPath: res://Config/battle_pass_season_2026_02.json
    ShowSeasonCountdown: [x]
```

### Configuration formats (Phase 2)

**A) Cosmetics catalog (JSON)**: `res://Config/cosmetics_catalog.json`

```json
{
  "items": [
    {
      "cosmetic_id": "trail_sparkle",
      "category": "trail",
      "display_name": "Sparkle Trail",
      "price_usd": 1.99,
      "iap_product_id": "trail_sparkle_199",
      "icon_path": "res://Assets/Sprites/UI/Shop/trail_sparkle_icon.png",
      "asset_path": "res://Assets/Effects/Trails/trail_sparkle.tscn",
      "season": "S1"
    }
  ]
}
```

**B) Battle pass season (JSON)**: `res://Config/battle_pass_season_YYYY_MM.json`

```json
{
  "season_id": "S1",
  "display_name": "Season 1: Launch Party",
  "start_utc": "2026-02-01T00:00:00Z",
  "end_utc": "2026-03-01T00:00:00Z",
  "tiers": 25,
  "xp_per_tier": 100,
  "xp_sources": {"level_complete": 10, "three_star_bonus": 5},
  "rewards": [
    {"tier": 1, "free": {"type": "soft_currency", "amount": 100}, "premium": {"type": "cosmetic", "cosmetic_id": "trail_sparkle"}}
  ],
  "premium_product_id": "battle_pass_s1"
}
```

### Example data (starter lineup)

Convert the creative list in `Docs/COSMETICS_CATALOG.md` into catalog entries:
- Slingshot skins: Gold, Crystal, Fire, Ice
- Projectile skins: Angry, Happy, Cool, Dark
- Trails: Fire, Sparkle, Smoke
- Hit effects: Confetti, Plasma, Big Boom
- Victory pack: Crown + Fireworks

### Troubleshooting

- Item missing: wrong category string or broken `icon_path`.
- Owned state not saving: ensure `cosmetic_id` is stable and persisted in the profile.
- Season not active: UTC date format/timezone mismatch.

### Metrics to watch after Phase 2

- Shop open rate
- Item view → purchase conversion
- ARPPU and % payers
- Battle pass attach rate
- Tier completion rate (how many reach tier 25)

---

## Phase 3: Retention Mechanics (Weeks 4–6)

### What gets built (technical overview)

- **Daily login streak** system + claim UI
- **Login bonus progression table** (day 1–30, then loop)
- **Push notifications** (opt-in + scheduling + deep links)
- **Seasonal events framework** (4 events/year; missions + rewards + limited cosmetics)

### Why it matters

Retention is the multiplier:
- More sessions → more ad impressions
- More time → higher chance to convert into shop/battle pass
- Events make the game feel “alive” even between content drops

### Daily login streak rules (recommended)

- Streak increments once per calendar day (use UTC internally).
- Allow **1 grace day** (optional) to reduce frustration.
- Always show the claim popup at a natural break (main menu first open).

### Login bonus progression table (example)

| Day | Reward | Amount |
|---:|--------|-------:|
| 1 | coins | 100 |
| 3 | cosmetic | 1 |
| 7 | rare cosmetic | 1 |
| 14 | big coins | 1000 |
| 30 | premium cosmetic | 1 |

### Push notification scheduling system (non-coder friendly)

Use a push provider dashboard (or Firebase messaging campaigns) so scheduling does not require code.

Recommended recurring pushes:
- **Streak reminder**: if not opened in 24h (send 6pm–8pm local)
- **Season ending**: 48h before season ends
- **Event start**: at event start time

### Non-coder workflow: edit daily rewards (CSV)

1. Edit in Google Sheets.
2. Export CSV.
3. Replace `res://Config/daily_rewards.csv`.

**Inspector-style checklist (DailyRewardsManager)**

```
Scene Tree: DailyRewardsManager (Node)
Inspector
  Script Variables
    RewardsTablePath: res://Config/daily_rewards.csv
    StreakGraceDays: 1
    PopupScene: res://Scenes/UI/DailyRewardPopup.tscn
```

### Non-coder workflow: create a seasonal event

1. Copy an event JSON template.
2. Set `start_utc` and `end_utc`.
3. Choose featured cosmetics.
4. Add 3–5 missions and rewards.

```
Scene Tree: SeasonalEventsManager (Node)
Inspector
  Script Variables
    EventsFolderPath: res://Config/events/
    ActiveEventOverrideId: (leave empty for auto)
```

### Configuration formats (Phase 3)

**A) Daily rewards CSV**: `res://Config/daily_rewards.csv`

```csv
day,reward_type,reward_id,amount
1,soft_currency,coins,100
3,cosmetic,projectile_happy,1
7,cosmetic,trail_sparkle,1
14,soft_currency,coins,1000
30,cosmetic,slingshot_gold,1
```

**B) Seasonal event JSON**: `res://Config/events/halloween_2026.json`

```json
{
  "event_id": "halloween_2026",
  "display_name": "Halloween Havoc",
  "start_utc": "2026-10-20T00:00:00Z",
  "end_utc": "2026-11-03T00:00:00Z",
  "featured_cosmetics": ["trail_smoke"],
  "missions": [
    {"mission_id": "complete_10_levels", "target": 10, "reward": {"type": "soft_currency", "amount": 500}}
  ],
  "notification_templates": {
    "start": {"title": "Event live!", "body": "Limited rewards available now."},
    "ending": {"title": "Ending soon", "body": "Last chance to claim rewards."}
  }
}
```

### Example data (4 events/year)

- `spring_2026.json`
- `summer_2026.json`
- `halloween_2026.json`
- `winter_2026.json`

Each should include at least 1 limited cosmetic and 3 missions.

### Troubleshooting

- Streak resets: device time changes; keep internal logic on UTC.
- Notifications missing: permission not granted or token not registered.
- Event timing wrong: UTC formatting issue.

### Metrics to watch after Phase 3

- D1/D7/D30 retention uplift vs baseline
- Daily reward claim rate
- Notification opt-in rate + open rate
- Event participation rate

---

## Phase 4: Social & Engagement (Weeks 6–8)

### What gets built (technical overview)

- **Friend system (lightweight)**: friend code, add friend, friends list
- **Friend leaderboard challenges**: compare scores and send challenges
- **Replay system**: record + save + share top runs
- **Level sharing**: procedural seed + difficulty + version; share as code/link
- **Social analytics**: `share_level`, `share_replay`, `leaderboard_view`, `challenge_sent`

### Why it matters

- Friends/challenges increase return frequency (“I want to beat them”).
- Shares create organic growth and marketing content.
- Replays also become a QA tool (reproduce edge cases).

### Non-coder workflow: manage challenge types

Design challenge types in a JSON file (no code).

```
Scene Tree: SocialChallengesManager (Node)
Inspector
  Script Variables
    ChallengeCatalogPath: res://Config/challenges.json
    DefaultChallengeDurationHours: 72
    AllowSameLevelChallenges: [x]
```

### Configuration formats (Phase 4)

**Challenge catalog JSON**: `res://Config/challenges.json`

```json
{
  "challenges": [
    {"challenge_type": "beat_score", "display_name": "Beat my score", "default_duration_hours": 72},
    {"challenge_type": "speed_run", "display_name": "Speed Run", "default_duration_hours": 48},
    {"challenge_type": "three_star", "display_name": "Get 3 stars", "default_duration_hours": 72}
  ]
}
```

**Replay share code format (string)**

```
AA|v=1.0.0|lvl=12|seed=839102|replay=BASE64
```

### Example data

A first shipping version can support:
- “Beat my score”
- “Speed run”
- “Get 3 stars”

Add more challenge types later once analytics proves they’re used.

### Troubleshooting

- Friend code fails: whitespace/case; always trim and case-normalize.
- Replay desync: physics changes between versions; include version in replay.
- Suspicious leaderboard scores: add basic validation and outlier flags.

### Metrics to watch after Phase 4

- Share rate per DAU
- Friend add rate
- D7 retention for players with ≥1 friend vs none
- Replay creation + share rate

---

## Phase 5: Quality Assurance (Weeks 8–9)

### What gets built (technical overview)

- **A/B testing framework** (variant assignment + logging)
- **Telemetry dashboards** (crashes, FPS buckets, load times, monetization funnel)
- **Difficulty heatmap** from `level_start/fail/complete` events
- **Ad frequency optimization loop** (test cooldowns/placements safely)
- **Non-coder testing guide** (repeatable release checklists)

### Why it matters

- A/B testing prevents “guess-based” design.
- Heatmaps find difficulty spikes that cause churn.
- Controlled ad tuning protects retention while increasing revenue.

### Dashboard setup (non-coder steps)

1. Enable analytics export (Firebase → BigQuery export OR CSV export pipeline).
2. Create a Looker Studio dashboard with:
   - retention chart
   - monetization funnel
   - top failing levels
   - crash-free users
3. Share dashboard link with the team.

### Ad frequency optimization guide (practical)

- Keep interstitials at natural breaks only (post-level screens).
- Use cooldown A/B tests (e.g., 45s vs 60s).
- Watch session length and D1 retention when increasing ad frequency.

### Non-coder testing guide (printable)

**15-minute smoke test**
- Start game → complete a level → fail a level → retry.
- Open shop → preview an item.
- Confirm ads show for free users.
- Confirm ads do not show for premium.

**60–90 minute release candidate test**
- Fresh install + upgrade install.
- Offline mode.
- Device time moved ±1 day (streak/events).

### Configuration format (Phase 5)

**A/B tests JSON**: `res://Config/ab_tests.json`

```json
{
  "tests": [
    {
      "test_id": "interstitial_cooldown",
      "enabled": true,
      "variants": [
        {"id": "A", "weight": 0.5, "cooldown_seconds": 45},
        {"id": "B", "weight": 0.5, "cooldown_seconds": 60}
      ]
    }
  ]
}
```

### Difficulty heatmap workflow (no code)

- Export events to a spreadsheet.
- For each level, compute:
  - completion rate = completes / starts
  - fail rate = fails / starts
  - avg attempts
- Use conditional formatting (red = low completion).

### Troubleshooting

- Noisy A/B results: increase runtime/sample size; test one variable at a time.
- Performance complaints but metrics look fine: segment by device tier; look at p95.

### Metrics to watch after Phase 5

- Crash-free sessions
- p95 load time (per scene)
- FPS bucket distribution
- Level completion rate and attempts per level
- Session length vs ad impressions
- Variant deltas (retention/conversion)

---

## Phase 6: Asset Pipeline & Polish (Week 10)

### What gets built (technical overview)

- **Asset validation checklist** (CI/CD-ready, but can be manual)
- **Sprite atlas organization** guide
- **Audio standardization** checklist
- **Build optimization verification** checklist
- **Pre-launch QA checklist** (monetization + retention)

### Why it matters

Polish protects ratings; a stable pipeline makes it possible to ship updates frequently.

### Asset validation CI/CD checklist (printable)

- [ ] No missing textures/audio during a full playthrough
- [ ] No placeholder art in menus/shop
- [ ] Icons present for store exports
- [ ] Audio peaks do not clip
- [ ] Mobile build size within target
- [ ] No “error spam” logs on scene loads

### Sprite atlas organization guide

Folder suggestions:
- `Assets/Sprites/UI/`
- `Assets/Sprites/Characters/`
- `Assets/Effects/`

Naming: `category_subject_variant.png` (example: `ui_shop_button_buy.png`).

Atlas guidance (simple):
- UI icons: 1024×1024 atlases
- Character/effects: 2048×2048 atlases (mobile-friendly)

### Audio standardization checklist

- [ ] SFX normalized to consistent loudness
- [ ] Music loops cleanly (no click/pop)
- [ ] Very loud SFX (explosions) capped to protect ears
- [ ] Mobile compression tested on a low-end device

### Build optimization verification checklist

- [ ] Export presets use the correct texture compression per platform (ASTC/ETC2 as appropriate)
- [ ] Release builds disable debug prints/log spam
- [ ] Symbols/debug files are stripped where store policy allows (keep a symbol archive separately)
- [ ] FPS/performance baseline checked on a low-end target device
- [ ] Startup time checked after a clean install (cold start)

### Pre-launch QA checklist (monetization + retention)

- [ ] `remove_ads` hides ads immediately + after restart
- [ ] `full_game_unlock` persists + restore works
- [ ] Shop previews work and purchases grant ownership
- [ ] Battle pass season dates correct
- [ ] Daily reward triggers once per day
- [ ] Push opt-in is compliant + skippable

### Metrics to watch after Phase 6

- Store rating (target **4.5+**)
- Crash-free users
- Build size and load-time regressions

---

## Reusable Templates (Copy/Paste)

### ID naming rules
- Events: `snake_case` (`level_complete`)
- Cosmetics: `category_name` (`trail_sparkle`)
- Seasons: `S1`, `S2`… or `YYYY_MM`

### Reward object (JSON)

```json
{ "type": "cosmetic", "cosmetic_id": "trail_sparkle" }
```

Other common rewards:

```json
{ "type": "soft_currency", "amount": 250 }
{ "type": "token", "token_id": "skip_interstitial", "amount": 1 }
```

### Notification copy templates
- Streak: “Log in to claim today’s reward.”
- Ending soon: “Last chance to claim rewards.”
- New event: “Limited rewards available now.”

---

## FAQ (Non‑Coders)

**Do I need to code to change rewards, cosmetics, seasons, or events?**  
No—this plan is built around JSON/CSV + Inspector fields.

**What if my JSON breaks the game?**  
Change one thing at a time, validate commas/quotes, and keep backups.

**How do I know if a change worked?**  
Use Phase 1 analytics: verify the event fired (shop opened, reward claimed, etc.).

**Is this pay-to-win?**  
No: monetization is cosmetic-first (+ remove ads / unlock full game).
