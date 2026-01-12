using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// UI controller for the Cosmetics Shop screen.
/// Full shop interface with filtering, sorting, and purchasing.
/// </summary>
public partial class CosmeticsShopUI : Control
{
    [Header("References")]
    [Export] public CosmeticsShop? ShopManager;
    [Export] public Control? ShopContainer;
    [Export] public GridContainer? CosmeticsGrid;
    [Export] public Control? PreviewPanel;
    [Export] public Label? PreviewNameLabel;
    [Export] public TextureRect? PreviewTexture;
    [Export] public Label? PreviewDescriptionLabel;
    [Export] public Label? PreviewRarityLabel;
    [Export] public Label? PreviewPriceLabel;
    [Export] public Button? PurchaseButton;
    [Export] public Button? EquipButton;
    [Export] public Button? CloseButton;
    
    [Header("Filter Tabs")]
    [Export] public HBoxContainer? FilterTabs;
    [Export] public Button? FilterAll;
    [Export] public Button? FilterHats;
    [Export] public Button? FilterGlasses;
    [Export] public Button? FilterMustaches;
    [Export] public Button? FilterWigs;
    [Export] public Button? FilterEmotions;
    [Export] public Button? FilterNew;
    [Export] public Button? FilterLimited;
    
    [Header("Sort Controls")]
    [Export] public OptionButton? SortOption;
    [Export] public Button? SortAscendingButton;
    
    [Header("Search")]
    [Export] public LineEdit? SearchInput;
    
    [Header("Loadout")]
    [Export] public Control? LoadoutPanel;
    [Export] public Button? SaveLoadoutButton;
    [Export] public Button? LoadLoadoutButton;
    [Export] public GridContainer? LoadoutGrid;
    
    [Header("Settings")]
    [Export] public PackedScene? CosmeticTileScene;
    [Export] public Vector2 TileSize = new Vector2(120, 150);
    [Export] public int ItemsPerRow = 5;
    
    // State
    private CosmeticCategory _currentCategory = CosmeticCategory.Hat;
    private string _currentSortBy = "rarity";
    private bool _sortAscending = true;
    private CosmeticItem? _selectedCosmetic;
    private Dictionary<string, Control> _cosmeticTiles = new();
    
    public override void _Ready()
    {
        // Connect signals
        CloseButton!.Pressed += OnClosePressed;
        PurchaseButton!.Pressed += OnPurchasePressed;
        EquipButton!.Pressed += OnEquipPressed;
        
        // Filter tab signals
        FilterAll!.Pressed += () => SetCategory(CosmeticCategory.Hat);
        FilterHats!.Pressed += () => SetCategory(CosmeticCategory.Hat);
        FilterGlasses!.Pressed += () => SetCategory(CosmeticCategory.Glasses);
        FilterMustaches!.Pressed += () => SetCategory(CosmeticCategory.Mustache);
        FilterWigs!.Pressed += () => SetCategory(CosmeticCategory.Wig);
        FilterEmotions!.Pressed += () => SetCategory(CosmeticCategory.Emotion);
        FilterNew!.Pressed += OnFilterNewPressed;
        FilterLimited!.Pressed += OnFilterLimitedPressed;
        
        // Sort signals
        SortOption!.ItemSelected += OnSortItemSelected;
        SortAscendingButton!.Pressed += OnSortDirectionPressed;
        
        // Search signal
        SearchInput!.TextChanged += OnSearchTextChanged;
        
        // Initialize shop
        InitializeShop();
        
        // Show shop
        Show();
    }
    
    /// <summary>
    /// Initialize the shop UI.
    /// </summary>
    private void InitializeShop()
    {
        if (ShopManager == null)
        {
            ShopManager = CosmeticsShop.Instance;
        }
        
        // Initialize sort options
        SortOption!.Clear();
        SortOption.AddItem("By Rarity", (int)CosmeticSortType.Rarity);
        SortOption.AddItem("By Price", (int)CosmeticSortType.Price);
        SortOption.AddItem("By Name", (int)CosmeticSortType.Name);
        SortOption.AddItem("Newest First", (int)CosmeticSortType.Newest);
        SortOption.Select(0);
        
        // Populate cosmetics grid
        RefreshCosmeticsGrid();
        
        // Initialize loadout panel
        InitializeLoadoutPanel();
    }
    
    /// <summary>
    /// Set the current category filter.
    /// </summary>
    private void SetCategory(CosmeticCategory category)
    {
        _currentCategory = category;
        ShopManager?.SetCategoryFilter(category);
        RefreshCosmeticsGrid();
        UpdateFilterHighlight();
    }
    
    /// <summary>
    /// Update the filter tab highlight.
    /// </summary>
    private void UpdateFilterHighlight()
    {
        // Reset all button styles
        var buttons = new[] { FilterAll, FilterHats, FilterGlasses, FilterMustaches, FilterWigs, FilterEmotions, FilterNew, FilterLimited };
        foreach (var btn in buttons)
        {
            btn?.RemoveThemeStyleboxOverride("normal");
        }
        
        // Highlight active filter
        Button? activeButton = _currentCategory switch
        {
            CosmeticCategory.Hat => FilterHats,
            CosmeticCategory.Glasses => FilterGlasses,
            CosmeticCategory.Mustache => FilterMustaches,
            CosmeticCategory.Wig => FilterWigs,
            CosmeticCategory.Emotion => FilterEmotions,
            _ => FilterAll
        };
        
        if (activeButton != null)
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.3f, 0.3f, 0.3f);
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            activeButton.AddThemeStyleboxOverride("normal", style);
        }
    }
    
    /// <summary>
    /// Refresh the cosmetics grid with current filters.
    /// </summary>
    private void RefreshCosmeticsGrid()
    {
        if (ShopManager == null || CosmeticsGrid == null)
            return;
        
        // Clear existing tiles
        foreach (var tile in _cosmeticTiles.Values)
        {
            tile.QueueFree();
        }
        _cosmeticTiles.Clear();
        
        // Get cosmetics based on current filter
        CosmeticItem[] cosmetics;
        
        if (_currentCategory == (CosmeticCategory)99) // New filter
        {
            cosmetics = ShopManager.GetShopCosmetics().Where(c => c.SortOrder >= 1000).ToArray();
        }
        else if (_currentCategory == (CosmeticCategory)100) // Limited filter
        {
            cosmetics = ShopManager.GetLimitedTimeCosmetics();
        }
        else
        {
            cosmetics = ShopManager.GetCosmeticsByCategory(_currentCategory);
        }
        
        // Apply search filter
        if (!string.IsNullOrEmpty(SearchInput?.Text))
        {
            var searchTerm = SearchInput.Text.ToLower();
            cosmetics = cosmetics.Where(c => 
                c.DisplayName.ToLower().Contains(searchTerm) ||
                c.Description.ToLower().Contains(searchTerm)
            ).ToArray();
        }
        
        // Sort cosmetics
        cosmetics = ShopManager.GetShopCosmetics();
        
        // Create tiles
        foreach (var cosmetic in cosmetics)
        {
            CreateCosmeticTile(cosmetic);
        }
        
        GD.Print($"Refreshed cosmetics grid: {cosmetics.Length} items");
    }
    
    /// <summary>
    /// Create a tile for a cosmetic.
    /// </summary>
    private void CreateCosmeticTile(CosmeticItem cosmetic)
    {
        if (CosmeticsGrid == null || ShopManager == null)
            return;
        
        // Create tile container
        var tile = new Control();
        tile.CustomMinimumSize = TileSize;
        
        // Create background panel
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        
        // Set rarity color
        var style = new StyleBoxFlat();
        style.BgColor = cosmetic.GetRarityColor().WithAlpha(0.3f);
        style.BorderColor = cosmetic.GetRarityColor();
        style.BorderWidthLeft = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthBottom = 2;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        panel.AddThemeStyleboxOverride("panel", style);
        
        // Create vertical container
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MouseFilter = Control.MouseFilterEnum.Stop;
        
        // Cosmetic icon/preview
        var icon = new TextureRect();
        icon.CustomMinimumSize = new Vector2(80, 80);
        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        
        // Load texture if available
        if (!string.IsNullOrEmpty(cosmetic.AssetPath) && ResourceLoader.Exists(cosmetic.AssetPath))
        {
            icon.Texture = ResourceLoader.Load<Texture2D>(cosmetic.AssetPath);
        }
        
        // Name label
        var nameLabel = new Label();
        nameLabel.Text = cosmetic.DisplayName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AutowrapMode = TextServer.AutowrapType.Smart;
        nameLabel.MaxLinesVisible = 2;
        
        // Rarity label
        var rarityLabel = new Label();
        rarityLabel.Text = cosmetic.GetRarityName();
        rarityLabel.HorizontalAlignment = HorizontalAlignment.Center;
        rarityLabel.Modulate = cosmetic.GetRarityColor();
        
        // Price label
        var priceLabel = new Label();
        priceLabel.Text = cosmetic.GetPriceString();
        priceLabel.HorizontalAlignment = HorizontalAlignment.Center;
        
        // Owned badge
        if (ShopManager.OwnsCosmetic(cosmetic.Id))
        {
            var ownedLabel = new Label();
            ownedLabel.Text = "OWNED";
            ownedLabel.HorizontalAlignment = HorizontalAlignment.Center;
            ownedLabel.Modulate = Colors.Green;
            vbox.AddChild(ownedLabel);
        }
        
        vbox.AddChild(icon);
        vbox.AddChild(nameLabel);
        vbox.AddChild(rarityLabel);
        vbox.AddChild(priceLabel);
        panel.AddChild(vbox);
        tile.AddChild(panel);
        
        // Connect click event
        tile.GuiInput += (inputEvent) => OnCosmeticTileClicked(inputEvent, cosmetic);
        
        CosmeticsGrid.AddChild(tile);
        _cosmeticTiles[cosmetic.Id] = tile;
    }
    
    /// <summary>
    /// Handle cosmetic tile click.
    /// </summary>
    private void OnCosmeticTileClicked(InputEvent @event, CosmeticItem cosmetic)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            SelectCosmetic(cosmetic);
        }
    }
    
    /// <summary>
    /// Select a cosmetic and show its preview.
    /// </summary>
    private void SelectCosmetic(CosmeticItem cosmetic)
    {
        _selectedCosmetic = cosmetic;
        
        // Record view
        ShopManager?.RecordCosmeticViewed(cosmetic.Id);
        
        // Update preview panel
        if (PreviewNameLabel != null)
            PreviewNameLabel.Text = cosmetic.DisplayName;
        
        if (PreviewDescriptionLabel != null)
            PreviewDescriptionLabel.Text = cosmetic.Description;
        
        if (PreviewRarityLabel != null)
        {
            PreviewRarityLabel.Text = cosmetic.GetRarityName();
            PreviewRarityLabel.Modulate = cosmetic.GetRarityColor();
        }
        
        if (PreviewPriceLabel != null)
            PreviewPriceLabel.Text = cosmetic.GetPriceString();
        
        // Update buttons based on ownership
        UpdateActionButtons();
        
        // Show preview panel
        PreviewPanel?.Show();
    }
    
    /// <summary>
    /// Update purchase/equip buttons based on ownership.
    /// </summary>
    private void UpdateActionButtons()
    {
        if (_selectedCosmetic == null || ShopManager == null)
            return;
        
        var owned = ShopManager.OwnsCosmetic(_selectedCosmetic.Id);
        
        if (owned)
        {
            PurchaseButton!.Hide();
            EquipButton!.Show();
            
            // Check if already equipped
            var equipped = ShopManager.GetEquippedCosmetic(_selectedCosmetic.Category);
            if (equipped == _selectedCosmetic.Id)
            {
                EquipButton.Text = "Equipped";
                EquipButton.Disabled = true;
            }
            else
            {
                EquipButton.Text = "Equip";
                EquipButton.Disabled = false;
            }
        }
        else
        {
            PurchaseButton!.Show();
            EquipButton!.Hide();
            
            // Update purchase button text
            if (_selectedCosmetic.CanPurchaseWithMoney())
            {
                PurchaseButton.Text = $"Buy for ${_selectedCosmetic.PriceUsd:F2}";
            }
            else if (_selectedCosmetic.CanPurchaseWithCoins())
            {
                PurchaseButton.Text = $"Buy for {_selectedCosmetic.PriceCoins} coins";
            }
            else
            {
                PurchaseButton.Text = "Unlock";
            }
        }
    }
    
    /// <summary>
    /// Handle purchase button click.
    /// </summary>
    private async void OnPurchasePressed()
    {
        if (_selectedCosmetic == null || ShopManager == null)
            return;
        
        var owned = ShopManager.OwnsCosmetic(_selectedCosmetic.Id);
        if (owned)
            return;
        
        // Check if player has enough coins
        if (_selectedCosmetic.CanPurchaseWithCoins())
        {
            var canAfford = ShopManager.CanAffordWithCoins(_selectedCosmetic.Id);
            if (!canAfford)
            {
                ShowMessage("Not enough coins!");
                return;
            }
            
            // Confirm purchase
            var confirmed = await ShowPurchaseConfirmation(_selectedCosmetic);
            if (!confirmed)
                return;
            
            // Purchase with coins
            var success = ShopManager.PurchaseCosmeticWithCoins(_selectedCosmetic.Id);
            if (success)
            {
                ShowMessage("Purchase successful!");
                UpdateActionButtons();
                RefreshCosmeticsGrid();
            }
        }
        else if (_selectedCosmetic.CanPurchaseWithMoney())
        {
            // Purchase with IAP
            var success = await ShopManager.PurchaseCosmeticWithIAP(_selectedCosmetic.Id);
            if (success)
            {
                ShowMessage("Purchase successful!");
                UpdateActionButtons();
                RefreshCosmeticsGrid();
            }
        }
        else
        {
            // Free unlock
            ShopManager.UnlockCosmetic(_selectedCosmetic.Id, "shop");
            ShowMessage("Cosmetic unlocked!");
            UpdateActionButtons();
            RefreshCosmeticsGrid();
        }
    }
    
    /// <summary>
    /// Show purchase confirmation dialog.
    /// </summary>
    private async Task<bool> ShowPurchaseConfirmation(CosmeticItem cosmetic)
    {
        var dialog = new ConfirmationDialog();
        dialog.Title = "Confirm Purchase";
        dialog.DialogText = $"Purchase {cosmetic.DisplayName} for {cosmetic.GetPriceString()}?";
        dialog.Size = new Vector2i(400, 200);
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        var tcs = new TaskCompletionSource<bool>();
        
        dialog.Confirmed += () => tcs.SetResult(true);
        dialog.Canceled += () => tcs.SetResult(false);
        
        var result = await tcs.Task;
        dialog.QueueFree();
        
        return result;
    }
    
    /// <summary>
    /// Handle equip button click.
    /// </summary>
    private void OnEquipPressed()
    {
        if (_selectedCosmetic == null || ShopManager == null)
            return;
        
        ShopManager.EquipCosmetic(_selectedCosmetic.Id);
        UpdateActionButtons();
        RefreshCosmeticsGrid();
        
        ShowMessage("Cosmetic equipped!");
    }
    
    /// <summary>
    /// Handle close button click.
    /// </summary>
    private void OnClosePressed()
    {
        ShopManager?.CloseShop();
        QueueFree();
    }
    
    /// <summary>
    /// Handle new filter pressed.
    /// </summary>
    private void OnFilterNewPressed()
    {
        _currentCategory = (CosmeticCategory)99;
        RefreshCosmeticsGrid();
        UpdateFilterHighlight();
    }
    
    /// <summary>
    /// Handle limited filter pressed.
    /// </summary>
    private void OnFilterLimitedPressed()
    {
        _currentCategory = (CosmeticCategory)100;
        RefreshCosmeticsGrid();
        UpdateFilterHighlight();
    }
    
    /// <summary>
    /// Handle sort option selected.
    /// </summary>
    private void OnSortItemSelected(long index)
    {
        _currentSortBy = index switch
        {
            0 => "rarity",
            1 => "price",
            2 => "name",
            3 => "newest",
            _ => "rarity"
        };
        
        ShopManager?.SetSortOptions(_currentSortBy, _sortAscending);
        RefreshCosmeticsGrid();
    }
    
    /// <summary>
    /// Handle sort direction toggle.
    /// </summary>
    private void OnSortDirectionPressed()
    {
        _sortAscending = !_sortAscending;
        ShopManager?.SetSortOptions(_currentSortBy, _sortAscending);
        RefreshCosmeticsGrid();
    }
    
    /// <summary>
    /// Handle search text changed.
    /// </summary>
    private void OnSearchTextChanged(string text)
    {
        ShopManager?.SetSearchTerm(text);
        RefreshCosmeticsGrid();
    }
    
    /// <summary>
    /// Initialize the loadout panel.
    /// </summary>
    private void InitializeLoadoutPanel()
    {
        SaveLoadoutButton!.Pressed += OnSaveLoadoutPressed;
        LoadLoadoutButton!.Pressed += OnLoadLoadoutPressed;
        
        RefreshLoadoutGrid();
    }
    
    /// <summary>
    /// Refresh the loadout grid.
    /// </summary>
    private void RefreshLoadoutGrid()
    {
        if (ShopManager == null || LoadoutGrid == null)
            return;
        
        // Clear existing
        foreach (var child in LoadoutGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Get loadouts
        var loadouts = ShopManager.GetLoadouts();
        
        foreach (var loadout in loadouts)
        {
            var btn = new Button();
            btn.Text = loadout.Name;
            btn.CustomMinimumSize = new Vector2(100, 40);
            
            btn.Pressed += () => ShopManager.ApplyLoadout(loadouts.IndexOf(loadout));
            
            LoadoutGrid.AddChild(btn);
        }
    }
    
    /// <summary>
    /// Handle save loadout button click.
    /// </summary>
    private async void OnSaveLoadoutPressed()
    {
        if (ShopManager == null)
            return;
        
        var dialog = new AcceptDialog();
        dialog.Title = "Save Loadout";
        dialog.DialogText = "Enter a name for your loadout:";
        dialog.Size = new Vector2i(400, 200);
        
        var input = new LineEdit();
        input.PlaceholderText = "Loadout name";
        input.CustomMinimumSize = new Vector2(200, 30);
        
        var vbox = new VBoxContainer();
        vbox.AddChild(input);
        
        dialog.ContentRect = new Rect2(0, 0, 400, 100);
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        await Task.Delay(100); // Wait for popup
        
        var tcs = new TaskCompletionSource<string>();
        
        dialog.Confirmed += () => tcs.SetResult(input.Text);
        dialog.Canceled += () => tcs.SetResult(string.Empty);
        
        var name = await tcs.Task;
        dialog.QueueFree();
        
        if (!string.IsNullOrEmpty(name))
        {
            ShopManager.CreateLoadout(name);
            RefreshLoadoutGrid();
        }
    }
    
    /// <summary>
    /// Handle load loadout button click.
    /// </summary>
    private void OnLoadLoadoutPressed()
    {
        // Show loadout selection
    }
    
    /// <summary>
    /// Show a message to the player.
    /// </summary>
    private void ShowMessage(string message)
    {
        var notification = new Label();
        notification.Text = message;
        notification.HorizontalAlignment = HorizontalAlignment.Center;
        notification.AddThemeFontSizeOverride("font_size", 24);
        notification.Modulate = Colors.White;
        
        notification.Position = (Size / 2) - (notification.GetMinimumSize() / 2);
        notification.AnchorLeft = 0.5f;
        notification.AnchorRight = 0.5f;
        
        AddChild(notification);
        
        // Animate out
        var tween = CreateTween();
        tween.TweenProperty(notification, "modulate:a", 0f, 1.5f);
        tween.TweenCallback(notification.QueueFree);
    }
    
    /// <summary>
    /// Show the shop.
    /// </summary>
    public void Show()
    {
        ShopManager?.OpenShop();
        Visible = true;
    }
    
    /// <summary>
    /// Hide the shop.
    /// </summary>
    public void Hide()
    {
        Visible = false;
    }
}

/// <summary>
/// Sort types for cosmetics.
/// </summary>
public enum CosmeticSortType
{
    Rarity = 0,
    Price = 1,
    Name = 2,
    Newest = 3
}
