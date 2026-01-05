using Godot;
using System;
using System.Collections.Generic;

public enum ExpressionType
{
    Neutral,
    Determined,
    Excited,
    Scared,
    Dizzy,
    Curious,
    Frightened,
    Happy,
    Bored,
    Angry,
    Nauseous,
    Melting,
    Cold,
    Disgusted
}

public partial class ExpressionManager : Node2D
{
    private ExpressionType _currentExpression = ExpressionType.Neutral;
    private float _expressionIntensity = 0f;
    private float _blinkTimer = 0f;
    private bool _isBlinking = false;
    private float _expressionTimer = 0f;

    private Color _featureColor = Colors.Black;
    private float _thickness = 4.0f;

    public ExpressionType CurrentExpression => _currentExpression;

    public override void _Ready()
    {
        _blinkTimer = (float)GD.RandRange(2.0, 5.0);
    }

    public override void _Process(double delta)
    {
        UpdateBlink((float)delta);
        UpdateExpressionTimer((float)delta);
        QueueRedraw();
    }

    public void SetExpression(ExpressionType expression, float duration = 0f)
    {
        if (_currentExpression == expression) return;
        
        _currentExpression = expression;
        _expressionTimer = duration;
        _expressionIntensity = 0f;
        
        // Tween intensity for smooth transition
        var tween = CreateTween();
        tween.TweenProperty(this, "_expressionIntensity", 1.0f, 0.3f);
    }

    private void UpdateBlink(float delta)
    {
        _blinkTimer -= delta;
        if (_blinkTimer <= 0)
        {
            if (!_isBlinking)
            {
                _isBlinking = true;
                _blinkTimer = 0.15f;
            }
            else
            {
                _isBlinking = false;
                _blinkTimer = (float)GD.RandRange(2.0, 5.0);
            }
        }
    }

    private void UpdateExpressionTimer(float delta)
    {
        if (_expressionTimer > 0)
        {
            _expressionTimer -= delta;
            if (_expressionTimer <= 0)
            {
                SetExpression(ExpressionType.Neutral);
            }
        }
    }

    public override void _Draw()
    {
        DrawExpression();
    }

    private void DrawExpression()
    {
        Vector2 center = Vector2.Zero;
        float size = 64f; // Default size, can be adjusted

        // Draw Eyebrows
        DrawEyebrows(center, size);

        // Draw Eyes
        DrawEyes(center, size);

        // Draw Mouth
        DrawMouth(center, size);
    }

    private void DrawEyebrows(Vector2 center, float size)
    {
        Vector2 leftStart = center + new Vector2(-size * 0.4f, -size * 0.4f);
        Vector2 leftEnd = center + new Vector2(-size * 0.1f, -size * 0.4f);
        Vector2 rightStart = center + new Vector2(size * 0.1f, -size * 0.4f);
        Vector2 rightEnd = center + new Vector2(size * 0.4f, -size * 0.4f);

        float offset = 0f;
        float angle = 0f;

        switch (_currentExpression)
        {
            case ExpressionType.Determined:
            case ExpressionType.Angry:
                offset = size * 0.1f;
                angle = Mathf.Pi / 8f;
                break;
            case ExpressionType.Excited:
            case ExpressionType.Happy:
            case ExpressionType.Scared:
            case ExpressionType.Frightened:
                offset = -size * 0.15f;
                break;
            case ExpressionType.Curious:
                // One eyebrow up
                rightStart.Y -= size * 0.2f;
                rightEnd.Y -= size * 0.2f;
                break;
            case ExpressionType.Dizzy:
                angle = (float)GD.RandRange(-0.2, 0.2);
                break;
        }

        DrawEyebrow(leftStart + new Vector2(0, offset), leftEnd + new Vector2(0, offset), angle);
        DrawEyebrow(rightStart + new Vector2(0, offset), rightEnd + new Vector2(0, offset), -angle);
    }

    private void DrawEyebrow(Vector2 start, Vector2 end, float angle)
    {
        Vector2 mid = (start + end) / 2f;
        Vector2 dir = end - start;
        Vector2 rotatedDir = dir.Rotated(angle * _expressionIntensity);
        
        DrawLine(mid - rotatedDir/2f, mid + rotatedDir/2f, _featureColor, _thickness);
    }

    private void DrawEyes(Vector2 center, float size)
    {
        Vector2 leftEye = center + new Vector2(-size * 0.25f, -size * 0.1f);
        Vector2 rightEye = center + new Vector2(size * 0.25f, -size * 0.1f);

        if (_isBlinking)
        {
            DrawLine(leftEye - new Vector2(size * 0.1f, 0), leftEye + new Vector2(size * 0.1f, 0), _featureColor, _thickness);
            DrawLine(rightEye - new Vector2(size * 0.1f, 0), rightEye + new Vector2(size * 0.1f, 0), _featureColor, _thickness);
            return;
        }

        switch (_currentExpression)
        {
            case ExpressionType.Scared:
            case ExpressionType.Frightened:
                DrawCircle(leftEye, size * 0.15f, _featureColor);
                DrawCircle(rightEye, size * 0.15f, _featureColor);
                DrawCircle(leftEye, size * 0.05f, Colors.White);
                DrawCircle(rightEye, size * 0.05f, Colors.White);
                break;
            case ExpressionType.Happy:
            case ExpressionType.Excited:
                // Crescent shape
                DrawArc(leftEye + new Vector2(0, size * 0.05f), size * 0.1f, Mathf.Pi, 2 * Mathf.Pi, 8, _featureColor, _thickness);
                DrawArc(rightEye + new Vector2(0, size * 0.05f), size * 0.1f, Mathf.Pi, 2 * Mathf.Pi, 8, _featureColor, _thickness);
                break;
            case ExpressionType.Dizzy:
                // Spirals or X
                DrawLine(leftEye + new Vector2(-size * 0.1f, -size * 0.1f), leftEye + new Vector2(size * 0.1f, size * 0.1f), _featureColor, _thickness);
                DrawLine(leftEye + new Vector2(size * 0.1f, -size * 0.1f), leftEye + new Vector2(-size * 0.1f, size * 0.1f), _featureColor, _thickness);
                DrawLine(rightEye + new Vector2(-size * 0.1f, -size * 0.1f), rightEye + new Vector2(size * 0.1f, size * 0.1f), _featureColor, _thickness);
                DrawLine(rightEye + new Vector2(size * 0.1f, -size * 0.1f), rightEye + new Vector2(-size * 0.1f, size * 0.1f), _featureColor, _thickness);
                break;
            case ExpressionType.Melting:
                DrawCircle(leftEye + new Vector2(0, size * 0.1f), size * 0.1f, _featureColor);
                DrawCircle(rightEye + new Vector2(0, size * 0.15f), size * 0.08f, _featureColor);
                break;
            default:
                DrawCircle(leftEye, size * 0.1f, _featureColor);
                DrawCircle(rightEye, size * 0.1f, _featureColor);
                break;
        }
    }

    private void DrawMouth(Vector2 center, float size)
    {
        Vector2 mouthPos = center + new Vector2(0, size * 0.3f);

        switch (_currentExpression)
        {
            case ExpressionType.Neutral:
            case ExpressionType.Bored:
                DrawLine(mouthPos - new Vector2(size * 0.15f, 0), mouthPos + new Vector2(size * 0.15f, 0), _featureColor, _thickness);
                break;
            case ExpressionType.Happy:
            case ExpressionType.Excited:
                DrawArc(mouthPos, size * 0.2f, 0, Mathf.Pi, 16, _featureColor, _thickness);
                break;
            case ExpressionType.Scared:
            case ExpressionType.Frightened:
            case ExpressionType.Nauseous:
                // O shape
                DrawArc(mouthPos, size * 0.15f, 0, 2 * Mathf.Pi, 16, _featureColor, _thickness);
                break;
            case ExpressionType.Angry:
            case ExpressionType.Determined:
                // Gritted teeth
                DrawRect(new Rect2(mouthPos - new Vector2(size * 0.2f, size * 0.05f), new Vector2(size * 0.4f, size * 0.1f)), _featureColor, false, _thickness);
                DrawLine(mouthPos - new Vector2(size * 0.2f, 0), mouthPos + new Vector2(size * 0.2f, 0), _featureColor, _thickness);
                break;
            case ExpressionType.Dizzy:
                // Wavy line
                for (int i = 0; i < 10; i++)
                {
                    float x1 = -size * 0.2f + (size * 0.4f * i / 10f);
                    float x2 = -size * 0.2f + (size * 0.4f * (i + 1) / 10f);
                    float y1 = Mathf.Sin(i * 1.5f) * size * 0.05f;
                    float y2 = Mathf.Sin((i+1) * 1.5f) * size * 0.05f;
                    DrawLine(mouthPos + new Vector2(x1, y1), mouthPos + new Vector2(x2, y2), _featureColor, _thickness);
                }
                break;
            case ExpressionType.Cold:
                // Zigzag teeth
                for (int i = 0; i < 8; i++)
                {
                    float x1 = -size * 0.2f + (size * 0.4f * i / 8f);
                    float x2 = -size * 0.2f + (size * 0.4f * (i + 0.5f) / 8f);
                    float x3 = -size * 0.2f + (size * 0.4f * (i + 1) / 8f);
                    DrawLine(mouthPos + new Vector2(x1, 0), mouthPos + new Vector2(x2, size * 0.05f), _featureColor, _thickness);
                    DrawLine(mouthPos + new Vector2(x2, size * 0.05f), mouthPos + new Vector2(x3, 0), _featureColor, _thickness);
                }
                break;
            case ExpressionType.Disgusted:
                DrawArc(mouthPos + new Vector2(0, size * 0.1f), size * 0.2f, Mathf.Pi, 2 * Mathf.Pi, 16, _featureColor, _thickness);
                break;
        }
    }
}
