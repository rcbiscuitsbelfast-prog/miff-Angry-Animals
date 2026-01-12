using Godot;
using System;

/// <summary>
/// Data structure for serializing and deserializing facial landmark data.
/// Used to save and load landmark information for face animation.
/// </summary>
public class LandmarkData
{
    public string DetectionTime { get; set; }
    public float AverageConfidence { get; set; }
    public FaceBoundsData FaceBounds { get; set; }
    public KeyFeaturesData KeyFeatures { get; set; }
    public LandmarkPoint[] AllLandmarks { get; set; }
    public float[] ConfidenceScores { get; set; }
}

public class FaceBoundsData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class KeyFeaturesData
{
    public LandmarkPoint LeftEye { get; set; }
    public LandmarkPoint RightEye { get; set; }
    public LandmarkPoint LeftEyebrow { get; set; }
    public LandmarkPoint RightEyebrow { get; set; }
    public LandmarkPoint LeftMouthCorner { get; set; }
    public LandmarkPoint RightMouthCorner { get; set; }
    public LandmarkPoint MouthCenter { get; set; }
    public LandmarkPoint NoseTip { get; set; }
    public LandmarkPoint JawLeft { get; set; }
    public LandmarkPoint JawRight { get; set; }
    public LandmarkPoint JawBottom { get; set; }
    public LandmarkPoint FaceCenter { get; set; }
}

public class LandmarkPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    
    public LandmarkPoint() { }
    
    public LandmarkPoint(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
    
    public static LandmarkPoint FromVector2(Vector2 vec)
    {
        return new LandmarkPoint(vec.X, vec.Y);
    }
}