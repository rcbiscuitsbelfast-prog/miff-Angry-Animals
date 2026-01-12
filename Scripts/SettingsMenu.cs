using Godot;

/// <summary>
/// Settings menu for adjusting game options like volume, graphics, and game feel.
/// Designed to be beginner-friendly with clear controls.
/// </summary>
public partial class SettingsMenu : Control
{
    [Signal] public delegate void SettingsClosedEventHandler();
    [Signal] public delegate void VolumeChangedEventHandler(float master, float music, float sfx);

    [Export] private NodePath _panelPath;
    [Export] private NodePath _backButtonPath;
    [Export] private NodePath _masterVolumeSliderPath;
    [Export] private NodePath _musicVolumeSliderPath;
    [Export] private NodePath _sfxVolumeSliderPath;
    [Export] private NodePath _screenShakeTogglePath;
    [Export] private NodePath _particlesTogglePath;
    [Export] private NodePath _hapticsTogglePath;
    [Export] private NodePath _easyModeButtonPath;
    [Export] private NodePath _normalModeButtonPath;
    [Export] private NodePath _hardModeButtonPath;
    [Export] private NodePath _removeAdsButtonPath;

    private Control? _panel;
    private Button? _backButton;
    private HSlider? _masterVolumeSlider;
    private HSlider? _musicVolumeSlider;
    private HSlider? _sfxVolumeSlider;
    private CheckBox? _screenShakeToggle;
    private CheckBox? _particlesToggle;
    private CheckBox? _hapticsToggle;
    private Button? _easyModeButton;
    private Button? _normalModeButton;
    private Button? _hardModeButton;
    private Button? _removeAdsButton;

    public override void _Ready()
    {
        InitializeSettingsMenu();
        ConnectSignals();
        LoadCurrentSettings();
    }

    private void InitializeSettingsMenu()
    {
        _panel = GetNodeOrNull<Control>(_panelPath);
        _backButton = GetNodeOrNull<Button>(_backButtonPath);
        _masterVolumeSlider = GetNodeOrNull<HSlider>(_masterVolumeSliderPath);
        _musicVolumeSlider = GetNodeOrNull<HSlider>(_musicVolumeSliderPath);
        _sfxVolumeSlider = GetNodeOrNull<HSlider>(_sfxVolumeSliderPath);
        _screenShakeToggle = GetNodeOrNull<CheckBox>(_screenShakeTogglePath);
        _particlesToggle = GetNodeOrNull<CheckBox>(_particlesTogglePath);
        _hapticsToggle = GetNodeOrNull<CheckBox>(_hapticsTogglePath);
        _easyModeButton = GetNodeOrNull<Button>(_easyModeButtonPath);
        _normalModeButton = GetNodeOrNull<Button>(_normalModeButtonPath);
        _hardModeButton = GetNodeOrNull<Button>(_hardModeButtonPath);
        _removeAdsButton = GetNodeOrNull<Button>(_removeAdsButtonPath);

        // Initially hide the panel
        if (_panel != null)
        {
            _panel.Visible = false;
        }
    }

    private void ConnectSignals()
    {
        if (_backButton != null)
        {
            _backButton.Pressed += OnBackPressed;
        }

        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
        }

        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
        }

        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;
        }

        if (_screenShakeToggle != null)
        {
            _screenShakeToggle.Toggled += OnScreenShakeToggled;
        }

        if (_particlesToggle != null)
        {
            _particlesToggle.Toggled += OnParticlesToggled;
        }

        if (_hapticsToggle != null)
        {
            _hapticsToggle.Toggled += OnHapticsToggled;
        }

        if (_easyModeButton != null)
        {
            _easyModeButton.Pressed += () => ApplyDifficultyPreset("easy");
        }

        if (_normalModeButton != null)
        {
            _normalModeButton.Pressed += () => ApplyDifficultyPreset("normal");
        }

        if (_hardModeButton != null)
        {
            _hardModeButton.Pressed += () => ApplyDifficultyPreset("hard");
        }

        if (_removeAdsButton != null)
        {
            _removeAdsButton.Pressed += OnRemoveAdsPressed;
        }
    }

    public override void _ExitTree()
    {
        if (_backButton != null)
        {
            _backButton.Pressed -= OnBackPressed;
        }

        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.ValueChanged -= OnMasterVolumeChanged;
        }

        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
        }

        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.ValueChanged -= OnSfxVolumeChanged;
        }

        if (_screenShakeToggle != null)
        {
            _screenShakeToggle.Toggled -= OnScreenShakeToggled;
        }

        if (_particlesToggle != null)
        {
            _particlesToggle.Toggled -= OnParticlesToggled;
        }

        if (_hapticsToggle != null)
        {
            _hapticsToggle.Toggled -= OnHapticsToggled;
        }

        if (_removeAdsButton != null)
        {
            _removeAdsButton.Pressed -= OnRemoveAdsPressed;
        }
    }

    private void LoadCurrentSettings()
    {
        // Get settings manager
        var settings = GameSettingsManager.Instance;
        
        if (settings != null)
        {
            // Load from GameSettingsManager
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.Value = settings.MasterVolume;

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.Value = settings.MusicVolume;

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.Value = settings.SfxVolume;

            // Load game feel settings
            if (_screenShakeToggle != null)
                _screenShakeToggle.ButtonPressed = true; // TODO: Connect to actual settings

            if (_particlesToggle != null)
                _particlesToggle.ButtonPressed = true; // TODO: Connect to actual settings
                
            // Load difficulty presets
            LoadDifficultyPreset(settings.DifficultyPreset);
        }
        else
        {
            // Fallback to existing systems
            if (AudioManager.Instance != null)
            {
                if (_masterVolumeSlider != null)
                    _masterVolumeSlider.Value = AudioManager.Instance.MasterVolume;

                if (_musicVolumeSlider != null)
                    _musicVolumeSlider.Value = AudioManager.Instance.MusicVolume;

                if (_sfxVolumeSlider != null)
                    _sfxVolumeSlider.Value = AudioManager.Instance.SfxVolume;
            }
        }

        // Load from PlayerProfile
        if (_hapticsToggle != null && PlayerProfile.Instance != null)
            _hapticsToggle.ButtonPressed = PlayerProfile.Instance.HighContrastMode; // Placeholder

        // Load Remove Ads status
        UpdateRemoveAdsButton();
    }

    #region Volume Controls

    private void OnMasterVolumeChanged(double value)
    {
        // Update GameSettingsManager if available
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.MasterVolume = (float)value;
        }
        // Fallback to AudioManager
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MasterVolume = (float)value;
        }

        UpdateVolumeDisplay();
    }

    private void OnMusicVolumeChanged(double value)
    {
        // Update GameSettingsManager if available
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.MusicVolume = (float)value;
        }
        // Fallback to AudioManager
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MusicVolume = (float)value;
        }

        UpdateVolumeDisplay();
    }

    private void OnSfxVolumeChanged(double value)
    {
        // Update GameSettingsManager if available
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SfxVolume = (float)value;
        }
        // Fallback to AudioManager
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxVolume = (float)value;
        }

        UpdateVolumeDisplay();
    }

    private void UpdateVolumeDisplay()
    {
        if (_masterVolumeSlider == null || _musicVolumeSlider == null || _sfxVolumeSlider == null)
            return;

        float master = (float)_masterVolumeSlider.Value;
        float music = (float)_musicVolumeSlider.Value;
        float sfx = (float)_sfxVolumeSlider.Value;

        EmitSignal(SignalName.VolumeChanged, master, music, sfx);
    }

    #endregion

    #region Game Feel Toggles

    private void OnScreenShakeToggled(bool pressed)
    {
        // Would update GameFeelManager if properties were exposed
        GameFeelManager.Instance?.OnButtonPress(this);
    }

    private void OnParticlesToggled(bool pressed)
    {
        // Would update GameFeelManager if properties were exposed
        GameFeelManager.Instance?.OnButtonPress(this);
    }

    private void OnHapticsToggled(bool pressed)
    {
        // Would update HapticFeedbackManager if properties were exposed
        GameFeelManager.Instance?.OnButtonPress(this);
    }

    #endregion

    #region Difficulty Presets

    private void ApplyDifficultyPreset(string mode)
    {
        // Provide haptic feedback
        GameFeelManager.Instance?.OnButtonPress(this);

        GD.Print($"Applying {mode} difficulty preset");

        switch (mode.ToLower())
        {
            case "easy":
                ApplyEasyMode();
                break;
            case "normal":
                ApplyNormalMode();
                break;
            case "hard":
                ApplyHardMode();
                break;
        }
    }

    private void ApplyEasyMode()
    {
        // More powerful slingshot
        // Fewer cups
        // Lower difficulty scale
        // Note: These would need to be exposed as properties
        GD.Print("Easy mode applied: More power, fewer cups");
    }

    private void ApplyNormalMode()
    {
        // Default settings
        GD.Print("Normal mode applied: Default settings");
    }

    private void ApplyHardMode()
    {
        // Less powerful slingshot
        // More cups
        // Higher difficulty scale
        // Note: These would need to be exposed as properties
        GD.Print("Hard mode applied: Less power, more cups");
    }

    #endregion

    #region Remove Ads

    private async void OnRemoveAdsPressed()
    {
        GameFeelManager.Instance?.OnButtonPress(this);

        if (_removeAdsButton == null)
            return;

        // Check if already purchased
        if (PremiumManager.Instance?.IsAdFreeVersion == true)
        {
            GD.Print("Remove Ads: Already purchased");
            return;
        }

        // Show loading state
        var originalText = _removeAdsButton.Text;
        _removeAdsButton.Text = "Loading...";
        _removeAdsButton.Disabled = true;

        try
        {
            // Attempt to purchase
            await PremiumManager.Instance?.PurchaseRemoveAds();
        }
        catch (Exception ex)
        {
            GD.PushError($"Remove Ads purchase failed: {ex.Message}");
            _removeAdsButton.Text = originalText;
            _removeAdsButton.Disabled = false;
        }
    }

    private void UpdateRemoveAdsButton()
    {
        if (_removeAdsButton == null)
            return;

        if (PremiumManager.Instance?.IsAdFreeVersion == true)
        {
            // Already purchased - show checkmark
            _removeAdsButton.Text = "✓ Ad-Free";
            _removeAdsButton.Disabled = true;
        }
        else
        {
            // Not purchased - show price
            var price = PremiumManager.Instance?.RemoveAdsPrice ?? "$0.99";
            _removeAdsButton.Text = $"Remove Ads - {price}";
            _removeAdsButton.Disabled = false;
        }
    }

    #endregion

    #region UI Controls

    private void OnBackPressed()
    {
        GameFeelManager.Instance?.OnButtonPress(this);
        HideSettings();
        EmitSignal(SignalName.SettingsClosed);
    }

    public void ShowSettings()
    {
        if (_panel != null)
        {
            _panel.Visible = true;

            // Animate in
            var tween = CreateTween();
            if (tween != null)
            {
                _panel.Modulate = new Color(1, 1, 1, 0);
                tween.TweenProperty(_panel, "modulate:a", 1.0, 0.3).SetTrans(Tween.TransitionType.EaseIn);
            }
        }

        LoadCurrentSettings();

        // Connect to PremiumManager signals to update button state
        ConnectPremiumManagerSignals();
    }

    private void ConnectPremiumManagerSignals()
    {
        if (PremiumManager.Instance == null)
            return;

        // Connect signals for Remove Ads purchase handling
        PremiumManager.Instance.RemoveAdsPurchaseSucceeded += OnRemoveAdsPurchaseSucceeded;
        PremiumManager.Instance.RemoveAdsPurchaseFailed += OnRemoveAdsPurchaseFailed;
    }

    private void OnRemoveAdsPurchaseSucceeded()
    {
        GD.Print("SettingsMenu: Remove Ads purchase succeeded");
        UpdateRemoveAdsButton();
        
        if (_removeAdsButton != null)
        {
            _removeAdsButton.Disabled = false;
        }
    }

    private void OnRemoveAdsPurchaseFailed(string reason)
    {
        GD.Print($"SettingsMenu: Remove Ads purchase failed: {reason}");
        
        if (_removeAdsButton != null)
        {
            var price = PremiumManager.Instance?.RemoveAdsPrice ?? "$0.99";
            _removeAdsButton.Text = $"Remove Ads - {price}";
            _removeAdsButton.Disabled = false;
        }
    }

    public void HideSettings()
    {
        if (_panel != null)
        {
            // Animate out
            var tween = CreateTween();
            if (tween != null)
            {
                tween.TweenProperty(_panel, "modulate:a", 0.0, 0.2);
                tween.TweenCallback(Callable.From(() => _panel.Visible = false));
            }
        }
    }

    public bool IsVisible()
    {
        return _panel != null && _panel.Visible;
    }

    #endregion
}
