using Godot;

/// <summary>
/// Global manager for audio playback including background music and sound effects.
/// Handles music transitions and volume control.
/// </summary>
public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	[Export] AudioStreamPlayer _musicPlayer;
	[Export] float _musicVolume = 0.5f;
	[Export] float _sfxVolume = 0.8f;

	private bool _isMusicPlaying = false;

	public override void _Ready()
	{
		Instance = this;

		if (_musicPlayer == null)
		{
			_musicPlayer = new AudioStreamPlayer();
			AddChild(_musicPlayer);
			_musicPlayer.Bus = "Master";
		}

		_musicPlayer.VolumeDb = Mathf.Linear2Db(_musicVolume);
	}

	public static void PlayBackgroundMusic(string musicPath, bool loop = true)
	{
		if (Instance == null || Instance._musicPlayer == null) return;

		var audioStream = GD.Load<AudioStream>(musicPath);
		if (audioStream != null)
		{
			Instance._musicPlayer.Stream = audioStream;
			Instance._musicPlayer.Bus = "Music";
			Instance._musicPlayer.Play();
			Instance._isMusicPlaying = true;
		}
		else
		{
			GD.PrintErr($"Failed to load audio stream: {musicPath}");
		}
	}

	public static void StopBackgroundMusic()
	{
		if (Instance == null || Instance._musicPlayer == null) return;

		Instance._musicPlayer.Stop();
		Instance._isMusicPlaying = false;
	}

	public static void PauseBackgroundMusic()
	{
		if (Instance == null || Instance._musicPlayer == null) return;
		Instance._musicPlayer.StreamPaused = true;
	}

	public static void ResumeBackgroundMusic()
	{
		if (Instance == null || Instance._musicPlayer == null) return;
		Instance._musicPlayer.StreamPaused = false;
	}

	public static void SetMusicVolume(float volume)
	{
		if (Instance == null || Instance._musicPlayer == null) return;

		Instance._musicVolume = Mathf.Clamp(volume, 0f, 1f);
		Instance._musicPlayer.VolumeDb = Mathf.Linear2Db(Instance._musicVolume);
	}

	public static void PlaySFX(string sfxPath, float volumeDb = 0f)
	{
		if (Instance == null) return;

		var audioStream = GD.Load<AudioStream>(sfxPath);
		if (audioStream == null)
		{
			GD.PrintErr($"Failed to load SFX: {sfxPath}");
			return;
		}

		var sfxPlayer = new AudioStreamPlayer
		{
			Stream = audioStream,
			Bus = "SFX",
			VolumeDb = volumeDb
		};

		Instance.AddChild(sfxPlayer);
		sfxPlayer.Play();

		sfxPlayer.Finished += () => sfxPlayer.QueueFree();
	}
}
