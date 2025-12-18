using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Global file manager for saving and loading level scores.
/// This class handles serialization of LevelScore objects to JSON, and stores/loads them from them disk using Godot's Godot.FileAccess System. 
/// </summary>
public partial class FileManager : Node
{
	public static FileManager Instance { get; private set; } // Singleton for global access as an AutoLoad.
	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() => Instance = this;


	/// <summary>
	/// Saves a list of LevelScore objects to a JSON file.
	/// Note: You can find the file saved on:
	///		Windows: AppData/Roaming/Godot/app_userdata/Angry Animals/...
	///		Linux: ~/.local/share/godot/app_userdata/Angry Animals/...
	/// </summary>
	/// <param name="path">Path to the file.</param>
	/// <param name="content">List of LevelScore objects to save</param>
	public static void SaveLevelScoreToFile(string path, List<LevelScore> content)
	{
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);

		if (file != null)
		{
			// Convert the list to JSON format with indentation for readability.
			string jsonStr = JsonConvert.SerializeObject(content, Formatting.Indented);
			file.StoreString(jsonStr);
		}
	}


	/// <summary>
	/// Loads a list of LevelScore onjects from a JSON file.
	/// </summary>
	/// <param name="path">Path to the file.</param>
	/// <returns>
	/// A list of LevelScore objects loaded from the file.
	/// If the file does not exist or is empty, returns an empty list.
	/// </returns>
    public static List<LevelScore> LoadLevelScoreFromFile(string path)
    {
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);

        if (file != null)
        {
            string jsonStr = file.GetAsText();
            
			if (!string.IsNullOrEmpty(jsonStr)) return JsonConvert.DeserializeObject<List<LevelScore>>(jsonStr);
        }

		// Returns an empty list if no file was found or the file was empty.
		return new List<LevelScore>();
    }
}