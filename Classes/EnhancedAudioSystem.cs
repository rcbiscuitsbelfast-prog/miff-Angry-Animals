using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enhanced audio system with variant support, dynamic mixing, and spatial audio
/// Provides 5+ audio variants per sound type with pitch/volume variation
/// </summary>
public class EnhancedAudioSystem : Node
{
    public static EnhancedAudioSystem Instance { get; private set; }

    // Audio configuration
    private AudioConfig _config;
    private Dictionary<string, AudioVariant[]> _audioVariants = new Dictionary<string, AudioVariant[]>();
    private Dictionary<string, AudioPool> _audioPools = new Dictionary<string, AudioPool>();
    
    // Audio mixing
    private AudioMixer _mixer;
    private Dictionary<string, AudioBusGroup> _busGroups = new Dictionary<string, AudioBusGroup>();
    
    // Spatial audio
    private Dictionary<Node2D, SpatialAudioSource> _spatialSources = new Dictionary<Node2D, SpatialAudioSource>();
    
    // Audio pools for better performance
    private Dictionary<string, Queue<AudioStreamPlayer>> _playerPools = new Dictionary<string, Queue<AudioStreamPlayer>>();
    
    [Signal]
    public delegate void AudioVariantPlayedEventHandler(string audioType, AudioVariant variant);
    
    [Signal]
    public delegate void AudioPoolExhaustedEventHandler(string audioType);
    
    [Signal]
    public delegate void DuckingActivatedEventHandler(string duckingGroup);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeAudioSystem();
    }

    /// <summary>
    /// Initialize enhanced audio system
    /// </summary>
    private void InitializeAudioSystem()
    {
        LoadConfiguration();
        CreateAudioVariants();
        InitializeAudioMixer();
        CreateAudioBuses();
        SetupAudioPools();
        
        GD.Print("Enhanced audio system initialized");
    }

    /// <summary>
    /// Load audio configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new AudioConfig
        {
            MasterVolume = 1.0f,
            MusicVolume = 0.8f,
            SfxVolume = 1.0f,
            VoiceVolume = 0.9f,
            UiVolume = 0.7f,
            EnableSpatialAudio = true,
            EnableDucking = true,
            MaxSimultaneousSounds = 16,
            PoolSize = 8,
            PitchVariationRange = 0.1f, // ±10%
            VolumeVariationRange = 0.05f // ±5%
        };
    }

    /// <summary>
    /// Create audio variants for different sound types
    /// </summary>
    private void CreateAudioVariants()
    {
        // Launch vocalizations (5 variants)
        _audioVariants["launch_vocal"] = new AudioVariant[]
        {
            new AudioVariant { Name = "launch_vocal_1", ResourcePath = "res://Audio/Vocals/launch_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "launch_vocal_2", ResourcePath = "res://Audio/Vocals/launch_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "launch_vocal_3", ResourcePath = "res://Audio/Vocals/launch_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "launch_vocal_4", ResourcePath = "res://Audio/Vocals/launch_4.ogg", Weight = 1.0f },
            new AudioVariant { Name = "launch_vocal_5", ResourcePath = "res://Audio/Vocals/launch_5.ogg", Weight = 1.0f }
        };

        // Impact vocalizations (5 variants)
        _audioVariants["impact_vocal"] = new AudioVariant[]
        {
            new AudioVariant { Name = "impact_vocal_1", ResourcePath = "res://Audio/Vocals/impact_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "impact_vocal_2", ResourcePath = "res://Audio/Vocals/impact_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "impact_vocal_3", ResourcePath = "res://Audio/Vocals/impact_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "impact_vocal_4", ResourcePath = "res://Audio/Vocals/impact_4.ogg", Weight = 1.0f },
            new AudioVariant { Name = "impact_vocal_5", ResourcePath = "res://Audio/Vocals/impact_5.ogg", Weight = 1.0f }
        };

        // Expression sounds (4 variants per expression)
        _audioVariants["expression_happy"] = new AudioVariant[]
        {
            new AudioVariant { Name = "happy_1", ResourcePath = "res://Audio/Expressions/happy_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "happy_2", ResourcePath = "res://Audio/Expressions/happy_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "happy_3", ResourcePath = "res://Audio/Expressions/happy_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "happy_4", ResourcePath = "res://Audio/Expressions/happy_4.ogg", Weight = 1.0f }
        };

        _audioVariants["expression_angry"] = new AudioVariant[]
        {
            new AudioVariant { Name = "angry_1", ResourcePath = "res://Audio/Expressions/angry_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "angry_2", ResourcePath = "res://Audio/Expressions/angry_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "angry_3", ResourcePath = "res://Audio/Expressions/angry_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "angry_4", ResourcePath = "res://Audio/Expressions/angry_4.ogg", Weight = 1.0f }
        };

        _audioVariants["expression_sad"] = new AudioVariant[]
        {
            new AudioVariant { Name = "sad_1", ResourcePath = "res://Audio/Expressions/sad_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "sad_2", ResourcePath = "res://Audio/Expressions/sad_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "sad_3", ResourcePath = "res://Audio/Expressions/sad_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "sad_4", ResourcePath = "res://Audio/Expressions/sad_4.ogg", Weight = 1.0f }
        };

        _audioVariants["expression_surprised"] = new AudioVariant[]
        {
            new AudioVariant { Name = "surprised_1", ResourcePath = "res://Audio/Expressions/surprised_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "surprised_2", ResourcePath = "res://Audio/Expressions/surprised_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "surprised_3", ResourcePath = "res://Audio/Expressions/surprised_3.ogg", Weight = 1.0f },
            new AudioVariant { Name = "surprised_4", ResourcePath = "res://Audio/Expressions/surprised_4.ogg", Weight = 1.0f }
        };

        // UI sounds (3 variants)
        _audioVariants["ui_select"] = new AudioVariant[]
        {
            new AudioVariant { Name = "select_1", ResourcePath = "res://Audio/UI/select_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "select_2", ResourcePath = "res://Audio/UI/select_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "select_3", ResourcePath = "res://Audio/UI/select_3.ogg", Weight = 1.0f }
        };

        _audioVariants["ui_confirm"] = new AudioVariant[]
        {
            new AudioVariant { Name = "confirm_1", ResourcePath = "res://Audio/UI/confirm_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "confirm_2", ResourcePath = "res://Audio/UI/confirm_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "confirm_3", ResourcePath = "res://Audio/UI/confirm_3.ogg", Weight = 1.0f }
        };

        _audioVariants["ui_error"] = new AudioVariant[]
        {
            new AudioVariant { Name = "error_1", ResourcePath = "res://Audio/UI/error_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "error_2", ResourcePath = "res://Audio/UI/error_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "error_3", ResourcePath = "res://Audio/UI/error_3.ogg", Weight = 1.0f }
        };

        // SFX sounds (3 variants)
        _audioVariants["slingshot_pull"] = new AudioVariant[]
        {
            new AudioVariant { Name = "pull_1", ResourcePath = "res://Audio/SFX/slingshot_pull_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "pull_2", ResourcePath = "res://Audio/SFX/slingshot_pull_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "pull_3", ResourcePath = "res://Audio/SFX/slingshot_pull_3.ogg", Weight = 1.0f }
        };

        _audioVariants["collision"] = new AudioVariant[]
        {
            new AudioVariant { Name = "collision_1", ResourcePath = "res://Audio/SFX/collision_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "collision_2", ResourcePath = "res://Audio/SFX/collision_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "collision_3", ResourcePath = "res://Audio/SFX/collision_3.ogg", Weight = 1.0f }
        };

        _audioVariants["victory"] = new AudioVariant[]
        {
            new AudioVariant { Name = "victory_1", ResourcePath = "res://Audio/SFX/victory_1.ogg", Weight = 1.0f },
            new AudioVariant { Name = "victory_2", ResourcePath = "res://Audio/SFX/victory_2.ogg", Weight = 1.0f },
            new AudioVariant { Name = "victory_3", ResourcePath = "res://Audio/SFX/victory_3.ogg", Weight = 1.0f }
        };
    }

    /// <summary>
    /// Initialize audio mixer
    /// </summary>
    private void InitializeAudioMixer()
    {
        _mixer = new AudioMixer();
    }

    /// <summary>
    /// Create audio bus groups for mixing
    /// </summary>
    private void CreateAudioBuses()
    {
        _busGroups["Master"] = new AudioBusGroup { Name = "Master", Volume = _config.MasterVolume, Muted = false };
        _busGroups["Music"] = new AudioBusGroup { Name = "Music", Volume = _config.MusicVolume, Muted = false };
        _busGroups["SFX"] = new AudioBusGroup { Name = "SFX", Volume = _config.SfxVolume, Muted = false };
        _busGroups["Vocals"] = new AudioBusGroup { Name = "Vocals", Volume = _config.VoiceVolume, Muted = false };
        _busGroups["UI"] = new AudioBusGroup { Name = "UI", Volume = _config.UiVolume, Muted = false };
    }

    /// <summary>
    /// Setup audio pools for performance
    /// </summary>
    private void SetupAudioPools()
    {
        foreach (var audioType in _audioVariants.Keys)
        {
            _playerPools[audioType] = new Queue<AudioStreamPlayer>();
            CreateAudioPool(audioType, _config.PoolSize);
        }
    }

    /// <summary>
    /// Create audio pool for a specific type
    /// </summary>
    private void CreateAudioPool(string audioType, int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            var player = new AudioStreamPlayer();
            player.Name = $"{audioType}_Player_{i}";
            AddChild(player);
            _playerPools[audioType].Enqueue(player);
        }
        
        _audioPools[audioType] = new AudioPool
        {
            Type = audioType,
            PoolSize = poolSize,
            ActivePlayers = 0,
            MaxActivePlayers = poolSize
        };
    }

    /// <summary>
    /// Play audio variant with random selection and variation
    /// </summary>
    public void PlayAudioVariant(string audioType, Vector2? position = null, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!_audioVariants.ContainsKey(audioType))
        {
            GD.PrintErr($"Audio type not found: {audioType}");
            return;
        }

        var variants = _audioVariants[audioType];
        var selectedVariant = SelectWeightedRandom(variants);
        
        if (selectedVariant == null)
        {
            GD.PrintErr($"No audio variants available for: {audioType}");
            return;
        }

        // Get available player from pool
        var player = GetAvailablePlayer(audioType);
        if (player == null)
        {
            EmitSignal("AudioPoolExhausted", audioType);
            return;
        }

        // Load and configure audio
        if (ResourceLoader.Exists(selectedVariant.ResourcePath))
        {
            var audioStream = ResourceLoader.Load<AudioStreamOggVorbis>(selectedVariant.ResourcePath);
            player.Stream = audioStream;
            
            // Apply variations
            player.VolumeDb = LinearToDb(volume * GetBusVolume(audioType) * GetRandomVolumeVariation());
            player.PitchScale = pitch * GetRandomPitchVariation();
            
            // Apply spatial audio if position provided
            if (position.HasValue && _config.EnableSpatialAudio)
            {
                ApplySpatialAudio(player, position.Value);
            }
            
            // Play the audio
            player.Play();
            
            EmitSignal("AudioVariantPlayed", audioType, selectedVariant);
        }
        else
        {
            GD.PrintErr($"Audio file not found: {selectedVariant.ResourcePath}");
        }
    }

    /// <summary>
    /// Play expression audio with dynamic selection
    /// </summary>
    public void PlayExpressionAudio(string expressionType, Vector2? position = null)
    {
        string audioType = $"expression_{expressionType.ToLower()}";
        PlayAudioVariant(audioType, position);
        
        // Trigger ducking if enabled
        if (_config.EnableDucking)
        {
            TriggerDucking("Music", 0.3f, 0.5f); // Reduce music to 30% over 0.5s
        }
    }

    /// <summary>
    /// Play collision audio with positional audio
    /// </summary>
    public void PlayCollisionAudio(Vector2 position, float intensity = 1.0f)
    {
        PlayAudioVariant("collision", position, intensity);
    }

    /// <summary>
    /// Play UI audio with volume control
    /// </summary>
    public void PlayUiAudio(string uiType, float volume = 1.0f)
    {
        PlayAudioVariant($"ui_{uiType}", null, volume);
    }

    /// <summary>
    /// Play slingshot audio with tension-based variation
    /// </summary>
    public void PlaySlingshotAudio(float tension, Vector2? position = null)
    {
        var volume = Mathf.Lerp(0.5f, 1.0f, tension);
        var pitch = Mathf.Lerp(0.8f, 1.2f, tension);
        
        PlayAudioVariant("slingshot_pull", position, volume, pitch);
    }

    /// <summary>
    /// Play victory audio with celebration ducking
    /// </summary>
    public void PlayVictoryAudio(Vector2? position = null)
    {
        PlayAudioVariant("victory", position, 1.0f);
        
        // Duck other audio groups during victory
        if (_config.EnableDucking)
        {
            TriggerDucking("SFX", 0.5f, 0.3f);
            TriggerDucking("Vocals", 0.7f, 0.3f);
        }
    }

    /// <summary>
    /// Select weighted random variant
    /// </summary>
    private AudioVariant SelectWeightedRandom(AudioVariant[] variants)
    {
        if (variants.Length == 0) return null;
        if (variants.Length == 1) return variants[0];
        
        float totalWeight = variants.Sum(v => v.Weight);
        float random = new Random().NextSingle() * totalWeight;
        
        foreach (var variant in variants)
        {
            if (random < variant.Weight)
            {
                return variant;
            }
            random -= variant.Weight;
        }
        
        return variants[0]; // Fallback
    }

    /// <summary>
    /// Get available audio player from pool
    /// </summary>
    private AudioStreamPlayer GetAvailablePlayer(string audioType)
    {
        if (!_playerPools.ContainsKey(audioType)) return null;
        
        var pool = _playerPools[audioType];
        
        // Try to find an available player
        while (pool.Count > 0)
        {
            var player = pool.Dequeue();
            
            if (!player.Playing)
            {
                return player;
            }
            else
            {
                // Player is still playing, re-queue and check next
                pool.Enqueue(player);
            }
        }
        
        return null;
    }

    /// <summary>
    /// Get bus volume for audio type
    /// </summary>
    private float GetBusVolume(string audioType)
    {
        return audioType switch
        {
            var x when x.StartsWith("ui_") => _busGroups["UI"].Volume,
            var x when x.StartsWith("expression_") => _busGroups["Vocals"].Volume,
            var x when x.StartsWith("launch_vocal") || x.StartsWith("impact_vocal") => _busGroups["Vocals"].Volume,
            var x when x.StartsWith("slingshot_") || x.StartsWith("collision") || x.StartsWith("victory") => _busGroups["SFX"].Volume,
            _ => _busGroups["SFX"].Volume
        };
    }

    /// <summary>
    /// Get random pitch variation
    /// </summary>
    private float GetRandomPitchVariation()
    {
        var random = new Random();
        return 1.0f + (float)(random.NextDouble() * 2 - 1) * _config.PitchVariationRange;
    }

    /// <summary>
    /// Get random volume variation
    /// </summary>
    private float GetRandomVolumeVariation()
    {
        var random = new Random();
        return 1.0f + (float)(random.NextDouble() * 2 - 1) * _config.VolumeVariationRange;
    }

    /// <summary>
    /// Apply spatial audio effects
    /// </summary>
    private void ApplySpatialAudio(AudioStreamPlayer player, Vector2 position)
    {
        // Convert world position to relative position
        var camera = GetTree().CurrentScene?.GetNodeOrNull<Camera2D>("Camera2D");
        if (camera != null)
        {
            var relativePosition = position - camera.GlobalPosition;
            
            // Apply stereo panning based on relative X position
            var pan = Mathf.Clamp(relativePosition.x / 1000f, -1f, 1f); // Pan range
            player.StereoPan = pan;
            
            // Apply volume attenuation based on distance
            var distance = relativePosition.Length();
            var attenuation = Mathf.Clamp(1.0f - (distance / 500f), 0.1f, 1.0f);
            player.VolumeDb = LinearToDb(attenuation);
        }
    }

    /// <summary>
    /// Trigger audio ducking
    /// </summary>
    public void TriggerDucking(string targetGroup, float targetVolume, float fadeTime)
    {
        if (!_config.EnableDucking || !_busGroups.ContainsKey(targetGroup)) return;
        
        var bus = _busGroups[targetGroup];
        bus.TargetVolume = targetVolume;
        bus.FadeTime = fadeTime;
        bus.IsDucking = true;
        
        EmitSignal("DuckingActivated", targetGroup);
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _config.MasterVolume = Mathf.Clamp(volume, 0f, 1f);
        _busGroups["Master"].Volume = _config.MasterVolume;
    }

    /// <summary>
    /// Set bus group volume
    /// </summary>
    public void SetBusVolume(string busName, float volume)
    {
        if (_busGroups.ContainsKey(busName))
        {
            _busGroups[busName].Volume = Mathf.Clamp(volume, 0f, 1f);
        }
    }

    /// <summary>
    /// Mute/unmute bus group
    /// </summary>
    public void SetBusMuted(string busName, bool muted)
    {
        if (_busGroups.ContainsKey(busName))
        {
            _busGroups[busName].Muted = muted;
        }
    }

    /// <summary>
    /// Get current configuration
    /// </summary>
    public AudioConfig GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// Get bus groups
    /// </summary>
    public Dictionary<string, AudioBusGroup> GetBusGroups()
    {
        return _busGroups;
    }

    /// <summary>
    /// Update audio pools based on usage
    /// </summary>
    public void OptimizeAudioPools()
    {
        foreach (var pool in _audioPools.Values)
        {
            int activeCount = 0;
            if (_playerPools.TryGetValue(pool.Type, out var playerQueue))
            {
                foreach (var player in playerQueue)
                {
                    if (player.Playing) activeCount++;
                }
            }
            
            pool.ActivePlayers = activeCount;
            
            // Resize pool if needed
            if (activeCount >= pool.MaxActivePlayers * 0.8f) // 80% utilization
            {
                CreateAudioPool(pool.Type, pool.PoolSize + 4);
                pool.PoolSize += 4;
            }
        }
    }

    /// <summary>
    /// Get audio statistics
    /// </summary>
    public AudioStatistics GetStatistics()
    {
        var stats = new AudioStatistics
        {
            TotalPools = _audioPools.Count,
            TotalActivePlayers = _audioPools.Values.Sum(p => p.ActivePlayers),
            PoolUtilization = _audioPools.Values.Count > 0 ? 
                _audioPools.Values.Average(p => p.ActivePlayers / (float)p.PoolSize) : 0f
        };
        
        foreach (var pool in _audioPools.Values)
        {
            stats.PoolStats[pool.Type] = new PoolStats
            {
                PoolSize = pool.PoolSize,
                ActivePlayers = pool.ActivePlayers,
                Utilization = pool.ActivePlayers / (float)pool.PoolSize
            };
        }
        
        return stats;
    }

    /// <summary>
    /// Convert linear volume to decibels
    /// </summary>
    private float LinearToDb(float linear)
    {
        return linear > 0f ? 20f * Mathf.Log(linear, 10f) : -80f;
    }

    public override void _Process(float delta)
    {
        // Update ducking
        UpdateDucking(delta);
        
        // Update spatial audio positions
        UpdateSpatialAudio();
        
        // Optimize pools periodically
        _audioPoolOptimizationTimer += delta;
        if (_audioPoolOptimizationTimer >= 5.0f) // Every 5 seconds
        {
            OptimizeAudioPools();
            _audioPoolOptimizationTimer = 0f;
        }
    }

    private float _audioPoolOptimizationTimer = 0f;

    /// <summary>
    /// Update ducking animations
    /// </summary>
    private void UpdateDucking(float delta)
    {
        foreach (var bus in _busGroups.Values)
        {
            if (bus.IsDucking)
            {
                var currentVolume = bus.Volume;
                var targetVolume = bus.TargetVolume;
                var fadeSpeed = bus.FadeTime > 0f ? delta / bus.FadeTime : 1f;
                
                if (Mathf.Abs(currentVolume - targetVolume) > 0.01f)
                {
                    bus.Volume = Mathf.Lerp(currentVolume, targetVolume, fadeSpeed);
                    
                    if (Mathf.Abs(bus.Volume - targetVolume) <= 0.01f)
                    {
                        bus.Volume = targetVolume;
                        bus.IsDucking = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update spatial audio positioning
    /// </summary>
    private void UpdateSpatialAudio()
    {
        var camera = GetTree().CurrentScene?.GetNodeOrNull<Camera2D>("Camera2D");
        if (camera == null) return;
        
        foreach (var kvp in _spatialSources)
        {
            var source = kvp.Value;
            if (source.Player?.Playing == true)
            {
                var relativePosition = source.Position - camera.GlobalPosition;
                var pan = Mathf.Clamp(relativePosition.x / 1000f, -1f, 1f);
                source.Player.StereoPan = pan;
                
                var distance = relativePosition.Length();
                var attenuation = Mathf.Clamp(1.0f - (distance / source.MaxDistance), 0.1f, 1.0f);
                source.Player.VolumeDb = LinearToDb(attenuation * source.BaseVolume);
            }
        }
    }
}

/// <summary>
/// Audio variant with weighted selection
/// </summary>
public class AudioVariant
{
    public string Name { get; set; }
    public string ResourcePath { get; set; }
    public float Weight { get; set; } = 1.0f;
}

/// <summary>
/// Audio pool for performance management
/// </summary>
public class AudioPool
{
    public string Type { get; set; }
    public int PoolSize { get; set; }
    public int ActivePlayers { get; set; }
    public int MaxActivePlayers { get; set; }
}

/// <summary>
/// Audio configuration
/// </summary>
public class AudioConfig
{
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }
    public float VoiceVolume { get; set; }
    public float UiVolume { get; set; }
    public bool EnableSpatialAudio { get; set; }
    public bool EnableDucking { get; set; }
    public int MaxSimultaneousSounds { get; set; }
    public int PoolSize { get; set; }
    public float PitchVariationRange { get; set; }
    public float VolumeVariationRange { get; set; }
}

/// <summary>
/// Audio bus group for mixing
/// </summary>
public class AudioBusGroup
{
    public string Name { get; set; }
    public float Volume { get; set; } = 1.0f;
    public float TargetVolume { get; set; } = 1.0f;
    public float FadeTime { get; set; } = 0f;
    public bool Muted { get; set; } = false;
    public bool IsDucking { get; set; } = false;
}

/// <summary>
/// Audio mixer for dynamic control
/// </summary>
public class AudioMixer
{
    public Dictionary<string, float> BusVolumes { get; set; } = new Dictionary<string, float>();
    public bool EnableDucking { get; set; } = true;
}

/// <summary>
/// Spatial audio source
/// </summary>
public class SpatialAudioSource
{
    public Vector2 Position { get; set; }
    public AudioStreamPlayer Player { get; set; }
    public float MaxDistance { get; set; } = 500f;
    public float BaseVolume { get; set; } = 1.0f;
}

/// <summary>
/// Audio statistics
/// </summary>
public class AudioStatistics
{
    public int TotalPools { get; set; }
    public int TotalActivePlayers { get; set; }
    public float PoolUtilization { get; set; }
    public Dictionary<string, PoolStats> PoolStats { get; set; } = new Dictionary<string, PoolStats>();
}

/// <summary>
/// Pool statistics
/// </summary>
public class PoolStats
{
    public int PoolSize { get; set; }
    public int ActivePlayers { get; set; }
    public float Utilization { get; set; }
}