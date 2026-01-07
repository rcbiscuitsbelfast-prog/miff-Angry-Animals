using Godot;

/// <summary>
/// Defines the visual style of the slingshot used to launch projectiles.
/// Each type has unique particle effects and launch animations while maintaining identical physics.
/// </summary>
public enum SlingshotType
{
    /// <summary>
    /// Classic slingshot with confetti launch effect
    /// </summary>
    Catapult,

    /// <summary>
    /// Giant hand launch with sparkle explosion effect
    /// </summary>
    GiantHand,

    /// <summary>
    /// Medieval trebuchet with dust cloud effect
    /// </summary>
    Trebuchet,

    /// <summary>
    /// Spring-loaded launch with bounce effect
    /// </summary>
    Spring
}
