using System;
using Godot;

/// <summary>
/// iOS adapter for App Store rewarded ads.
/// Handles SKAdNetwork rewarded video integration.
/// </summary>
public partial class AppStoreRewardedAdapter : Node
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
        
        GD.Print($"AppStoreRewardedAdapter: Loading rewarded ad {AdUnitId}");
        _adMobPlugin.Call("load_rewarded_ad", AdUnitId);
    }

    public void ShowRewardedAd()
    {
        if (_adMobPlugin == null) return;

        GD.Print("AppStoreRewardedAdapter: Showing rewarded ad");
        _adMobPlugin.Call("show_rewarded_ad", AdUnitId);
    }

    public bool IsAdLoaded()
    {
        if (_adMobPlugin == null) return false;
        return (bool)_adMobPlugin.Call("is_rewarded_ad_loaded");
    }
}
