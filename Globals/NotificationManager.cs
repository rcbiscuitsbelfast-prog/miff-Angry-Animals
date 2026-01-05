using Godot;
using System;

public partial class NotificationManager : Node
{
    public static NotificationManager Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
    }

    public void ScheduleDailyReminder()
    {
        if (OS.GetName() != "Android" && OS.GetName() != "iOS") return;
        
        GD.Print("Scheduling daily reminder notification...");
        // In a real mobile app, use Godot plugins for Local Notifications
    }

    public void SendInstantNotification(string title, string message)
    {
        GD.Print($"[NOTIFICATION] {title}: {message}");
        // In a real mobile app, use Godot plugins for Local Notifications
    }
}
