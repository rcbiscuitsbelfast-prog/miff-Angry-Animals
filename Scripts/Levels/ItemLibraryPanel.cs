using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Item library panel for selecting items in the level editor
/// </summary>
public partial class ItemLibraryPanel : PanelContainer
{
    [Signal] public delegate void ItemSelectedEventHandler(ItemDefinition item);
    
    private LineEdit _searchBox;
    private OptionButton _categoryFilter;
    private GridContainer _itemGrid;
    private ItemDefinition[] _allItems;
    private ItemDefinition[] _filteredItems;
    private ItemDefinition _selectedItem;
    
    public ItemDefinition SelectedItem => _selectedItem;
    
    public override void _Ready()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        var mainVBox = new VBoxContainer();
        AddChild(mainVBox);
        
        // Search box
        _searchBox = new LineEdit();
        _searchBox.PlaceholderText = "🔍 Search items...";
        _searchBox.TextChanged += OnSearchChanged;
        mainVBox.AddChild(_searchBox);
        
        // Category filter
        _categoryFilter = new OptionButton();
        _categoryFilter.AddItem("All Categories");
        
        var categories = Enum.GetValues(typeof(ItemCategory));
        foreach (ItemCategory category in categories)
        {
            _categoryFilter.AddItem(category.ToString());
        }
        
        _categoryFilter.ItemSelected += OnCategoryChanged;
        mainVBox.AddChild(_categoryFilter);
        
        // Scroll container for grid
        var scroll = new ScrollContainer();
        scroll.CustomMinimumSize = new Vector2(280, 300);
        scroll.HorizontalScrollMode = ScrollContainer.ScrollModeEnum.Disabled;
        mainVBox.AddChild(scroll);
        
        // Item grid
        _itemGrid = new GridContainer();
        _itemGrid.Columns = 3;
        scroll.AddChild(_itemGrid);
    }
    
    public void SetItems(ItemDefinition[] items)
    {
        _allItems = items ?? new ItemDefinition[0];
        _filteredItems = _allItems;
        RefreshItemGrid();
    }
    
    public void SetCategory(ItemCategory category)
    {
        var index = (int)category + 1; // +1 for "All Categories"
        if (index < _categoryFilter.ItemCount)
        {
            _categoryFilter.Selected = index;
            OnCategoryChanged(index);
        }
    }
    
    private void OnSearchChanged(string newText)
    {
        FilterItems();
    }
    
    private void OnCategoryChanged(long index)
    {
        FilterItems();
    }
    
    private void FilterItems()
    {
        string searchText = _searchBox?.Text?.ToLower() ?? "";
        int categoryIndex = (int)(_categoryFilter?.Selected ?? 0);
        
        _filteredItems = _allItems.Where(item =>
        {
            // Apply search filter
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!item.ItemName.ToLower().Contains(searchText) &&
                    !item.ItemId.ToLower().Contains(searchText))
                    return false;
            }
            
            // Apply category filter
            if (categoryIndex > 0)
            {
                var selectedCategory = (ItemCategory)(categoryIndex - 1);
                if (item.Category != selectedCategory)
                    return false;
            }
            
            return true;
        }).ToArray();
        
        RefreshItemGrid();
    }
    
    private void RefreshItemGrid()
    {
        // Clear existing items
        foreach (var child in _itemGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Create item buttons
        foreach (var item in _filteredItems)
        {
            var button = CreateItemButton(item);
            _itemGrid.AddChild(button);
        }
    }
    
    private Button CreateItemButton(ItemDefinition item)
    {
        var container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(80, 80);
        container.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        
        // Icon button
        var button = new Button();
        button.CustomMinimumSize = new Vector2(60, 60);
        button.Text = GetItemIcon(item);
        button.AddThemeFontSizeOverride("font_size", 24);
        
        // Style based on category
        var style = new StyleBoxFlat();
        style.BgColor = GetCategoryColor(item.Category);
        button.AddThemeStyleboxOverride("normal", style);
        
        // Store item reference and connect pressed event
        button.SetMeta("item", item);
        button.Pressed += () => OnItemButtonPressed(item);
        
        container.AddChild(button);
        
        // Item name label
        var label = new Label();
        label.Text = item.ItemName;
        label.AddThemeOverride("font_size", 10);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.CustomMinimumSize = new Vector2(80, 30);
        container.AddChild(label);
        
        return container;
    }
    
    private void OnItemButtonPressed(ItemDefinition item)
    {
        _selectedItem = item;
        EmitSignal(SignalName.ItemSelected, item);
        
        GD.Print($"Item selected: {item.ItemName}");
    }
    
    private string GetItemIcon(ItemDefinition item)
    {
        return item.Category switch
        {
            ItemCategory.Furniture => "🪑",
            ItemCategory.Electronics => "📺",
            ItemCategory.Food => "🍎",
            ItemCategory.Decoration => "🖼️",
            ItemCategory.Structure => "📦",
            ItemCategory.Tool => "🔧",
            ItemCategory.Explosive => "💣",
            _ => "❓"
        };
    }
    
    private Color GetCategoryColor(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Furniture => new Color(0.6f, 0.4f, 0.2f),
            ItemCategory.Electronics => new Color(0.2f, 0.4f, 0.6f),
            ItemCategory.Food => new Color(0.8f, 0.6f, 0.2f),
            ItemCategory.Decoration => new Color(0.7f, 0.3f, 0.7f),
            ItemCategory.Structure => new Color(0.5f, 0.5f, 0.5f),
            ItemCategory.Tool => new Color(0.4f, 0.4f, 0.4f),
            ItemCategory.Explosive => new Color(0.9f, 0.2f, 0.2f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
    }
    
    public void ClearSelection()
    {
        _selectedItem = null;
    }
}

// Compatibility alias for older naming
public partial class ItemLibrary : ItemLibraryPanel
{
}