using Godot;

public partial class ScorePopup : Node2D
{
    public void Setup(int value)
    {
        string text = value.ToString();
        float offset = 0;
        
        foreach (char c in text)
        {
            // Attempt to load texture
            // Assumed path based on description: Assets/graphics/kenney2/numbers
            // Assumed filename format: just the digit, e.g. "1.png"
            var texturePath = $"res://Assets/graphics/kenney2/numbers/{c}.png";
            
            if (ResourceLoader.Exists(texturePath))
            {
                var texture = GD.Load<Texture2D>(texturePath);
                var sprite = new Sprite2D();
                sprite.Texture = texture;
                sprite.Position = new Vector2(offset, 0);
                AddChild(sprite);
                offset += texture.GetWidth();
            }
            else
            {
                // Fallback to Label if textures missing
                var label = new Label();
                label.Text = c.ToString();
                label.Position = new Vector2(offset, 0);
                AddChild(label);
                offset += 20; // approximate width
            }
        }
        
        // Center alignment
        float totalWidth = offset;
        foreach (Node2D child in GetChildren())
        {
            child.Position -= new Vector2(totalWidth / 2, 0);
        }

        Animate();
    }

    private void Animate()
    {
        Scale = Vector2.Zero;
        Modulate = new Color(1, 1, 1, 0);

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);
        
        tween.Chain().TweenProperty(this, "position", Position + new Vector2(0, -40), 0.5f);
        tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.5f).SetDelay(0.3f);
        
        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}
