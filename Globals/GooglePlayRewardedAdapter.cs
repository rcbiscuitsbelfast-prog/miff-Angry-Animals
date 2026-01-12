using System;
using Godot;

/// <summary>
/// Android adapter for Google Play rewarded ads.
/// Handles communication with the native AdMob plugin for rewarded videos.
/// </summary>
public partial class GooglePlayRewardedAdapter : Node
{
    private const string AdUnitId = "ca-app-pub-6675121744131727/8406522837";
    private GodotObject? _adMobPlugin;

    public override void _Ready()
    {
        _adMobPlugin = Engine.HasSingleton("AdMob") ? Engine.GetSingleton("AdMob") : null;
    }

    public void LoadRewardedAd()
    {
        if (_adMobPlugin == null) return;
        
        GD.Print($"GooglePlayRewardedAdapter: Loading rewarded ad {AdUnitId}");
        _adMobPlugin.Call("load_rewarded_ad", AdUnitId);
    }

    public void ShowRewardedAd()
    {
        if (_adMobPlugin == null) return;

        GD.Print("GooglePlayRewardedAdapter: Showing rewarded ad");
        _adMobPlugin.Call("show_rewarded_ad", AdUnitId);
    }

    public bool IsAdLoaded()
    {
        if (_adMobPlugin == null) return false;
        return (bool)_adMobPlugin.Call("is_rewarded_ad_loaded");
    }

    // Callbacks from native code would be handled here or via AdsManager
}
