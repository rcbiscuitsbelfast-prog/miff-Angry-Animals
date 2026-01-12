using System;
using Godot;

/// <summary>
/// Amazon Appstore adapter for rewarded ads.
/// Handles Amazon-specific rewarded video verification and display.
/// </summary>
public partial class AmazonRewardedAdapter : Node
{
    private const string AdUnitId = "ca-app-pub-6675121744131727/8406522837";
    private GodotObject? _amazonAdsPlugin;

    public override void _Ready()
    {
        _amazonAdsPlugin = Engine.HasSingleton("AmazonAds") ? Engine.GetSingleton("AmazonAds") : null;
    }

    public void LoadRewardedAd()
    {
        if (_amazonAdsPlugin == null) return;
        
        GD.Print($"AmazonRewardedAdapter: Loading rewarded ad {AdUnitId}");
        _amazonAdsPlugin.Call("load_rewarded_video", AdUnitId);
    }

    public void ShowRewardedAd()
    {
        if (_amazonAdsPlugin == null) return;

        GD.Print("AmazonRewardedAdapter: Showing rewarded ad");
        _amazonAdsPlugin.Call("show_rewarded_video");
    }

    public bool IsAdLoaded()
    {
        if (_amazonAdsPlugin == null) return false;
        return (bool)_amazonAdsPlugin.Call("is_rewarded_video_loaded");
    }
}
