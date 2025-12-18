using Godot;

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
    private const string UI_BUS = "UI";

    // Audio streams
    private AudioStreamPlayer? _backgroundMusicPlayer;
    private AudioStreamPlayer? _slingshotSfxPlayer;
    private AudioStreamPlayer? _destructionSfxPlayer;
    private AudioStreamPlayer? _uiClickPlayer;
    private AudioStreamPlayer? _comboPlayer;
    private AudioStreamPlayer? _ragePlayer;

    // Audio resources (to be loaded from res://Assets/Audio/)
    private AudioStream? _backgroundMusic;
    private AudioStream? _slingshotSound;
    private AudioStream? _destructionSound;
    private AudioStream? _uiClickSound;
    private AudioStream? _comboSound;
    private AudioStream? _rageSound;

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
        _backgroundMusicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume);
        AddChild(_backgroundMusicPlayer);

        // Initialize SFX players
        _slingshotSfxPlayer = new AudioStreamPlayer();
        _slingshotSfxPlayer.Name = "SlingshotSfxPlayer";
        _slingshotSfxPlayer.Bus = SFX_BUS;
        _slingshotSfxPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        AddChild(_slingshotSfxPlayer);

        _destructionSfxPlayer = new AudioStreamPlayer();
        _destructionSfxPlayer.Name = "DestructionSfxPlayer";
        _destructionSfxPlayer.Bus = SFX_BUS;
        _destructionSfxPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        AddChild(_destructionSfxPlayer);

        _uiClickPlayer = new AudioStreamPlayer();
        _uiClickPlayer.Name = "UiClickPlayer";
        _uiClickPlayer.Bus = UI_BUS;
        _uiClickPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        AddChild(_uiClickPlayer);

        _comboPlayer = new AudioStreamPlayer();
        _comboPlayer.Name = "ComboPlayer";
        _comboPlayer.Bus = SFX_BUS;
        _comboPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        AddChild(_comboPlayer);

        _ragePlayer = new AudioStreamPlayer();
        _ragePlayer.Name = "RagePlayer";
        _ragePlayer.Bus = SFX_BUS;
        _ragePlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        AddChild(_ragePlayer);
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
            _backgroundMusicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume);
        }
        EmitSignal(SignalName.MusicVolumeChanged, MusicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp(volume, 0f, 1f);
        
        if (_slingshotSfxPlayer != null)
            _slingshotSfxPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        if (_destructionSfxPlayer != null)
            _destructionSfxPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        if (_uiClickPlayer != null)
            _uiClickPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        if (_comboPlayer != null)
            _comboPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);
        if (_ragePlayer != null)
            _ragePlayer.VolumeDb = Mathf.LinearToDb(SfxVolume);

        EmitSignal(SignalName.SfxVolumeChanged, SfxVolume);
    }

    public void SetMusicMute(bool muted)
    {
        MuteMusic = muted;
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.StreamPaused = muted;
        }
    }

    public void SetSfxMute(bool muted)
    {
        MuteSfx = muted;
        
        if (_slingshotSfxPlayer != null)
            _slingshotSfxPlayer.StreamPaused = muted;
        if (_destructionSfxPlayer != null)
            _destructionSfxPlayer.StreamPaused = muted;
        if (_uiClickPlayer != null)
            _uiClickPlayer.StreamPaused = muted;
        if (_comboPlayer != null)
            _comboPlayer.StreamPaused = muted;
        if (_ragePlayer != null)
            _ragePlayer.StreamPaused = muted;
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
}