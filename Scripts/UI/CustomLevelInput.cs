using Godot;
using System;

/// <summary>
/// Dialog for entering custom level share codes.
/// </summary>
public partial class CustomLevelInputDialog : ConfirmationDialog
{
    private TextEdit _shareCodeInput;
    private Label _statusLabel;

    public override void _Ready()
    {
        Title = "Load Custom Level";
        DialogText = "Enter the share code from a friend:";
        
        // Create input field
        _shareCodeInput = new TextEdit();
        _shareCodeInput.CustomMinimumSize = new Vector2(400, 100);
        _shareCodeInput.PlaceholderText = "Paste share code here (e.g., AA1_...)";
        
        // Create status label
        _statusLabel = new Label();
        _statusLabel.Text = "";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;

        // Add to dialog
        var vbox = new VBoxContainer();
        vbox.AddChild(_shareCodeInput);
        vbox.AddChild(_statusLabel);
        AddChild(vbox);

        // Connect signals
        Confirmed += OnConfirmed;
        Canceled += OnCanceled;
    }

    private void OnConfirmed()
    {
        string code = _shareCodeInput.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(code))
        {
            ShowError("Please enter a share code");
            return;
        }

        // Try to decode the level
        if (!CustomLevelCode.TryDecodeLevel(code, out var level))
        {
            ShowError("Invalid share code! Please check and try again.");
            return;
        }

        // Validate the level
        var validation = CustomLevelValidator.ValidateLevel(level);
        if (!validation.IsValid)
        {
            ShowError($"Level cannot be played: {validation.Message}");
            return;
        }

        // Load and play the level
        LoadCustomLevel(level);
    }

    private void OnCanceled()
    {
        QueueFree();
    }

    private void LoadCustomLevel(CustomLevelData level)
    {
        GD.Print($"Loading custom level: {level.LevelName}");

        // Load the CustomPlayRoom scene
        var scene = GD.Load<PackedScene>("res://Scenes/CustomPlay/CustomPlayRoom.tscn");
        if (scene == null)
        {
            ShowError("Custom play room scene not found!");
            return;
        }

        var room = scene.Instantiate<CustomPlayRoom>();
        room.LoadCustomLevel(level);

        // Switch to the custom level
        GetTree().Root.AddChild(room);
        GetTree().CurrentScene.QueueFree();
        GetTree().CurrentScene = room;
    }

    private void ShowError(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
        
        // Also show as popup
        var errorDialog = new AcceptDialog();
        errorDialog.DialogText = message;
        errorDialog.Title = "Error";
        GetTree().Root.AddChild(errorDialog);
        errorDialog.PopupCentered();
    }

    /// <summary>
    /// Static helper to show the input dialog
    /// </summary>
    public static void ShowDialog(Node parent)
    {
        var dialog = new CustomLevelInputDialog();
        parent.AddChild(dialog);
        dialog.PopupCentered();
    }
}
