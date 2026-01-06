using Godot;
using Script;

/// <summary>
/// Audio manager that handles all game audio including background music and sound effects.
/// Integrates with SignalManager for gameplay events and uses the audio bus layout.
/// </summary>
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; } = null!;

    [Signal] public delegate void MusicVolumeChangedEventHandler(float volume);
    [Signal] public delegate void SfxVolumeChangedEventHandler(float volume);

    // Audio buses
    private const string MUSIC_BUS = "Music";
    private const string SFX_BUS = "SFX";
    private const string UI_BUS = "SFX"; // UI sounds use SFX bus

    // Audio streams
    private AudioStreamPlayer? _backgroundMusicPlayer;
    private AudioStreamPlayer? _slingshotSfxPlayer;
    private AudioStreamPlayer? _destructionSfxPlayer;
    private AudioStreamPlayer? _uiClickPlayer;
    private AudioStreamPlayer? _comboPlayer;
    private AudioStreamPlayer? _ragePlayer;

    // Vocal audio streams (with pitch/volume randomization support)
    private AudioStreamPlayer? _vocalLaunchPlayer;
    private AudioStreamPlayer? _vocalImpactPlayer;
    private AudioStreamPlayer? _vocalExpressionPlayer;

    // Audio resources (to be loaded from res://Assets/Audio/)
    private AudioStream? _backgroundMusic;
    private AudioStream? _slingshotSound;
    private AudioStream? _destructionSound;
    private AudioStream? _uiClickSound;
    private AudioStream? _comboSound;
    private AudioStream? _rageSound;

    // Launch vocal resources
    private AudioStream? _launchGrunt1;
    private AudioStream? _launchGrunt2;
    private AudioStream? _launchWhoosh1;
    private AudioStream? _launchWhoosh2;

    // Impact vocal resources
    private AudioStream? _impactOof1;
    private AudioStream? _impactOof2;
    private AudioStream? _impactThud1;
    private AudioStream? _impactCrash1;

    // Expression vocal resources
    private AudioStream? _vocalLaugh;
    private AudioStream? _vocalScream;
    private AudioStream? _vocalAngryRoar;
    private AudioStream? _vocalDizzyGroan;

    // Volume settings
    [Export] public float MusicVolume { get; set; } = 0.7f;
    [Export] public float SfxVolume { get; set; } = 0.8f;
    [Export] public bool MuteMusic { get; set; } = false;
    [Export] public bool MuteSfx { get; set; } = false;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        InitializeAudioPlayers();
        LoadAudioResources();
        ConnectSignals();
        StartBackgroundMusic();
    }

    private void InitializeAudioPlayers()
    {
        // Initialize background music player
        _backgroundMusicPlayer = new AudioStreamPlayer();
        _backgroundMusicPlayer.Name = "BackgroundMusicPlayer";
        _backgroundMusicPlayer.Bus = MUSIC_BUS;
        _backgroundMusicPlayer.VolumeDb = LinearToDb(MusicVolume);
        AddChild(_backgroundMusicPlayer);

        // Initialize SFX players
        _slingshotSfxPlayer = new AudioStreamPlayer();
        _slingshotSfxPlayer.Name = "SlingshotSfxPlayer";
        _slingshotSfxPlayer.Bus = SFX_BUS;
        _slingshotSfxPlayer.VolumeDb = LinearToDb(SfxVolume);
        AddChild(_slingshotSfxPlayer);

        _destructionSfxPlayer = new AudioStreamPlayer();
        _destructionSfxPlayer.Name = "DestructionSfxPlayer";
        _destructionSfxPlayer.Bus = SFX_BUS;
        _destructionSfxPlayer.VolumeDb = LinearToDb(SfxVolume);
        AddChild(_destructionSfxPlayer);

        _uiClickPlayer = new AudioStreamPlayer();
        _uiClickPlayer.Name = "UiClickPlayer";
        _uiClickPlayer.Bus = UI_BUS;
        _uiClickPlayer.VolumeDb = LinearToDb(SfxVolume);
        AddChild(_uiClickPlayer);

        _comboPlayer = new AudioStreamPlayer();
        _comboPlayer.Name = "ComboPlayer";
        _comboPlayer.Bus = SFX_BUS;
        _comboPlayer.VolumeDb = LinearToDb(SfxVolume);
        AddChild(_comboPlayer);

        _ragePlayer = new AudioStreamPlayer();
        _ragePlayer.Name = "RagePlayer";
        _ragePlayer.Bus = SFX_BUS;
        _ragePlayer.VolumeDb = LinearToDb(SfxVolume);
        AddChild(_ragePlayer);

        // Initialize vocal players with pitch randomization
        _vocalLaunchPlayer = new AudioStreamPlayer();
        _vocalLaunchPlayer.Name = "VocalLaunchPlayer";
        _vocalLaunchPlayer.Bus = SFX_BUS;
        _vocalLaunchPlayer.VolumeDb = LinearToDb(SfxVolume);
        _vocalLaunchPlayer.PitchScale = 1.0f;
        AddChild(_vocalLaunchPlayer);

        _vocalImpactPlayer = new AudioStreamPlayer();
        _vocalImpactPlayer.Name = "VocalImpactPlayer";
        _vocalImpactPlayer.Bus = SFX_BUS;
        _vocalImpactPlayer.VolumeDb = LinearToDb(SfxVolume);
        _vocalImpactPlayer.PitchScale = 1.0f;
        AddChild(_vocalImpactPlayer);

        _vocalExpressionPlayer = new AudioStreamPlayer();
        _vocalExpressionPlayer.Name = "VocalExpressionPlayer";
        _vocalExpressionPlayer.Bus = SFX_BUS;
        _vocalExpressionPlayer.VolumeDb = LinearToDb(SfxVolume);
        _vocalExpressionPlayer.PitchScale = 1.0f;
        AddChild(_vocalExpressionPlayer);
    }

    private void LoadAudioResources()
    {
        // TODO: Load audio files from res://Assets/Audio/ directory
        // For now, we'll set up the paths and handle loading in a more dynamic way

        // Background music
        _backgroundMusic = LoadAudioResource("res://Assets/Audio/Music/BackgroundMusic.ogg");

        // Sound effects
        _slingshotSound = LoadAudioResource("res://Assets/Audio/SFX/SlingshotSound.ogg");
        _destructionSound = LoadAudioResource("res://Assets/Audio/SFX/DestructionSound.ogg");
        _uiClickSound = LoadAudioResource("res://Assets/Audio/SFX/UiClickSound.ogg");
        _comboSound = LoadAudioResource("res://Assets/Audio/SFX/ComboSound.ogg");
        _rageSound = LoadAudioResource("res://Assets/Audio/SFX/RageSound.ogg");

        // Launch vocal sounds
        _launchGrunt1 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/LaunchGrunt1.wav");
        _launchGrunt2 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/LaunchGrunt2.wav");
        _launchWhoosh1 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/LaunchWhoosh1.wav");
        _launchWhoosh2 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/LaunchWhoosh2.wav");

        // Impact vocal sounds
        _impactOof1 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/ImpactOof1.wav");
        _impactOof2 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/ImpactOof2.wav");
        _impactThud1 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/ImpactThud1.wav");
        _impactCrash1 = LoadAudioResource("res://Assets/Audio/SFX/Vocals/ImpactCrash1.wav");

        // Expression vocal sounds
        _vocalLaugh = LoadAudioResource("res://Assets/Audio/SFX/Vocals/VocalLaugh.wav");
        _vocalScream = LoadAudioResource("res://Assets/Audio/SFX/Vocals/VocalScream.wav");
        _vocalAngryRoar = LoadAudioResource("res://Assets/Audio/SFX/Vocals/VocalAngryRoar.wav");
        _vocalDizzyGroan = LoadAudioResource("res://Assets/Audio/SFX/Vocals/VocalDizzyGroan.wav");

        // Assign streams to players
        if (_backgroundMusicPlayer != null && _backgroundMusic != null)
            _backgroundMusicPlayer.Stream = _backgroundMusic;

        if (_slingshotSfxPlayer != null && _slingshotSound != null)
            _slingshotSfxPlayer.Stream = _slingshotSound;

        if (_destructionSfxPlayer != null && _destructionSound != null)
            _destructionSfxPlayer.Stream = _destructionSound;

        if (_uiClickPlayer != null && _uiClickSound != null)
            _uiClickPlayer.Stream = _uiClickSound;

        if (_comboPlayer != null && _comboSound != null)
            _comboPlayer.Stream = _comboSound;

        if (_ragePlayer != null && _rageSound != null)
            _ragePlayer.Stream = _rageSound;
    }

    private AudioStream? LoadAudioResource(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"Audio resource not found: {path}");
            return null;
        }

        return ResourceLoader.Load<AudioStream>(path);
    }

    private void ConnectSignals()
    {
        // Connect to SignalManager for gameplay events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAttemptMade += OnAttemptMade;
            SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed += OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied += OnAnimalDied;
        }

        // Connect to RageSystem for rage events
        var rageSystem = GetNodeOrNull<RageSystem>("/root/RageSystem");
        if (rageSystem != null)
        {
            rageSystem.RageThresholdReached += OnRageThresholdReached;
            rageSystem.ComboChanged += OnComboChanged;
        }

        // Connect to GameManager for state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
        }
    }

    public override void _ExitTree()
    {
        // Disconnect all signals to prevent memory leaks
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAttemptMade -= OnAttemptMade;
            SignalManager.Instance.OnCupDestroyed -= OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed -= OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied -= OnAnimalDied;
        }

        var rageSystem = GetNodeOrNull<RageSystem>("/root/RageSystem");
        if (rageSystem != null)
        {
            rageSystem.RageThresholdReached -= OnRageThresholdReached;
            rageSystem.ComboChanged -= OnComboChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;
        }
    }

    private void StartBackgroundMusic()
    {
        if (_backgroundMusicPlayer != null && _backgroundMusic != null && !MuteMusic)
        {
            _backgroundMusicPlayer.Play();
        }
    }

    private void StopBackgroundMusic()
    {
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.Stop();
        }
    }

    private void OnAttemptMade()
    {
        PlaySlingshotSound();
    }

    private void OnCupDestroyed()
    {
        PlayDestructionSound();
    }

    private void OnPropDestroyed(Node prop, int scoreValue)
    {
        PlayDestructionSound();
    }

    private void OnAnimalDied()
    {
        // Play animal death sound if needed
    }

    private void OnRageThresholdReached(int thresholdIndex)
    {
        PlayRageSound();
    }

    private void OnComboChanged(int combo)
    {
        if (combo > 1)
        {
            PlayComboSound();
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.MainMenu:
                StopBackgroundMusic();
                break;
            case GameManager.GameState.InRoom:
                StartBackgroundMusic();
                break;
            case GameManager.GameState.Paused:
                // Keep music playing but at lower volume or pause it
                break;
        }
    }

    public void PlaySlingshotSound()
    {
        if (_slingshotSfxPlayer != null && !MuteSfx)
        {
            _slingshotSfxPlayer.Play();
        }
    }

    public void PlayDestructionSound()
    {
        if (_destructionSfxPlayer != null && !MuteSfx)
        {
            _destructionSfxPlayer.Play();
        }
    }

    public void PlayUiClickSound()
    {
        if (_uiClickPlayer != null && !MuteSfx)
        {
            _uiClickPlayer.Play();
        }
    }

    public void PlayComboSound()
    {
        if (_comboPlayer != null && !MuteSfx)
        {
            _comboPlayer.Play();
        }
    }

    public void PlayRageSound()
    {
        if (_ragePlayer != null && !MuteSfx)
        {
            _ragePlayer.Play();
        }
    }

    // Volume control methods
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp(volume, 0f, 1f);
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.VolumeDb = LinearToDb(MusicVolume);
        }
        EmitSignal(SignalName.MusicVolumeChanged, MusicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp(volume, 0f, 1f);

        if (_slingshotSfxPlayer != null)
            _slingshotSfxPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_destructionSfxPlayer != null)
            _destructionSfxPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_uiClickPlayer != null)
            _uiClickPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_comboPlayer != null)
            _comboPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_ragePlayer != null)
            _ragePlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_vocalLaunchPlayer != null)
            _vocalLaunchPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_vocalImpactPlayer != null)
            _vocalImpactPlayer.VolumeDb = LinearToDb(SfxVolume);
        if (_vocalExpressionPlayer != null)
            _vocalExpressionPlayer.VolumeDb = LinearToDb(SfxVolume);

        EmitSignal(SignalName.SfxVolumeChanged, SfxVolume);
    }

    public void SetMusicMute(bool muted)
    {
        MuteMusic = muted;
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.Muted = muted;
        }
    }

    public void SetSfxMute(bool muted)
    {
        MuteSfx = muted;

        if (_slingshotSfxPlayer != null)
            _slingshotSfxPlayer.Muted = muted;
        if (_destructionSfxPlayer != null)
            _destructionSfxPlayer.Muted = muted;
        if (_uiClickPlayer != null)
            _uiClickPlayer.Muted = muted;
        if (_comboPlayer != null)
            _comboPlayer.Muted = muted;
        if (_ragePlayer != null)
            _ragePlayer.Muted = muted;
        if (_vocalLaunchPlayer != null)
            _vocalLaunchPlayer.Muted = muted;
        if (_vocalImpactPlayer != null)
            _vocalImpactPlayer.Muted = muted;
        if (_vocalExpressionPlayer != null)
            _vocalExpressionPlayer.Muted = muted;
    }

    // Public static API for other scripts
    public static void PlaySlingshotSfx()
    {
        if (Instance != null)
            Instance.PlaySlingshotSound();
    }
    
    public static void PlayDestructionSfx()
    {
        if (Instance != null)
            Instance.PlayDestructionSound();
    }
    
    public static void PlayUiClickSfx()
    {
        if (Instance != null)
            Instance.PlayUiClickSound();
    }
    
    public static void PlayComboSfx()
    {
        if (Instance != null)
            Instance.PlayComboSound();
    }
    
    public static void PlayRageSfx()
    {
        if (Instance != null)
            Instance.PlayRageSound();
    }

    #region Vocal Sound Effects

    /// <summary>
    /// Plays a random launch vocalization (grunt or whoosh).
    /// Includes slight pitch/volume randomization for variety.
    /// </summary>
    public void PlayLaunchVocal()
    {
        if (_vocalLaunchPlayer == null || MuteSfx) return;

        // Select random launch vocal
        var launchVocals = new AudioStream?[] { _launchGrunt1, _launchGrunt2, _launchWhoosh1, _launchWhoosh2 };
        var selectedVocal = launchVocals[GD.RandRange(0, launchVocals.Length - 1)];

        if (selectedVocal != null)
        {
            _vocalLaunchPlayer.Stream = selectedVocal;

            // Apply randomization: ±20% pitch, ±15% volume
            _vocalLaunchPlayer.PitchScale = 1.0f + (float)GD.RandRange(-0.2f, 0.2f);
            _vocalLaunchPlayer.VolumeDb = LinearToDb(SfxVolume) + (float)GD.RandRange(-2f, 2f);

            _vocalLaunchPlayer.Play();
        }
    }

    /// <summary>
    /// Plays a random impact vocalization (oof, thud, or crash).
    /// Includes slight pitch/volume randomization for variety.
    /// </summary>
    public void PlayImpactVocal()
    {
        if (_vocalImpactPlayer == null || MuteSfx) return;

        // Select random impact vocal
        var impactVocals = new AudioStream?[] { _impactOof1, _impactOof2, _impactThud1, _impactCrash1 };
        var selectedVocal = impactVocals[GD.RandRange(0, impactVocals.Length - 1)];

        if (selectedVocal != null)
        {
            _vocalImpactPlayer.Stream = selectedVocal;

            // Apply randomization: ±20% pitch, ±15% volume
            _vocalImpactPlayer.PitchScale = 1.0f + (float)GD.RandRange(-0.2f, 0.2f);
            _vocalImpactPlayer.VolumeDb = LinearToDb(SfxVolume) + (float)GD.RandRange(-2f, 2f);

            _vocalImpactPlayer.Play();
        }
    }

    /// <summary>
    /// Plays a vocal sound appropriate to the current expression.
    /// Includes slight pitch/volume randomization for variety.
    /// </summary>
    /// <param name="expression">The current expression type</param>
    public void PlayExpressionVocal(ExpressionType expression)
    {
        if (_vocalExpressionPlayer == null || MuteSfx) return;

        AudioStream? selectedVocal = null;

        // Select vocal based on expression
        switch (expression)
        {
            case ExpressionType.Happy:
            case ExpressionType.Excited:
                selectedVocal = _vocalLaugh;
                break;
            case ExpressionType.Scared:
            case ExpressionType.Frightened:
                selectedVocal = _vocalScream;
                break;
            case ExpressionType.Angry:
            case ExpressionType.Determined:
                selectedVocal = _vocalAngryRoar;
                break;
            case ExpressionType.Dizzy:
            case ExpressionType.Nauseous:
                selectedVocal = _vocalDizzyGroan;
                break;
        }

        if (selectedVocal != null)
        {
            _vocalExpressionPlayer.Stream = selectedVocal;

            // Apply randomization: ±20% pitch, ±15% volume
            _vocalExpressionPlayer.PitchScale = 1.0f + (float)GD.RandRange(-0.2f, 0.2f);
            _vocalExpressionPlayer.VolumeDb = LinearToDb(SfxVolume) + (float)GD.RandRange(-2f, 2f);

            _vocalExpressionPlayer.Play();
        }
    }

    #endregion

    #region Static Vocal API

    /// <summary>
    /// Static API for playing launch vocal from any script
    /// </summary>
    public static void PlayLaunchVocalSfx()
    {
        Instance?.PlayLaunchVocal();
    }

    /// <summary>
    /// Static API for playing impact vocal from any script
    /// </summary>
    public static void PlayImpactVocalSfx()
    {
        Instance?.PlayImpactVocal();
    }

    /// <summary>
    /// Static API for playing expression vocal from any script
    /// </summary>
    public static void PlayExpressionVocalSfx(ExpressionType expression)
    {
        Instance?.PlayExpressionVocal(expression);
    }

    #endregion
}