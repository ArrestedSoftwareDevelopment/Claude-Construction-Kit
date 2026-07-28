using Godot;

namespace Dack;

public enum WorldObjectKind
{
    Platform,
    Ladder,
    Ramp,
    Slide,
    Conveyor,
    Elevator,
    Checkpoint,
    StartPoint,
    HiddenSwitch,
    PinballFlipper,
    PinballBumper,
    PinballPlunger,
    PinballDrain,
    PinballRollover,
    PinballGate
}

public enum MarkerRole
{
    None,
    Start,
    Midpoint,
    End,
    Secret,
    Switch
}

public sealed record WorldObject(
    WorldObjectKind Kind,
    Vector2 Start,
    Vector2 End,
    float ThicknessUnits = 0.8f,
    float SpeedUnits = 0f,
    float Phase = 0f,
    float RangeUnits = 5f,
    MarkerRole MarkerRole = MarkerRole.None,
    bool VisibleInPlay = true,
    bool UseCustomTint = false,
    Color Tint = default,
    float Opacity = 1f
)
{
    public Vector2 Center => (Start + End) * 0.5f;
    public bool IsMarker => MarkerRole != MarkerRole.None || Kind is WorldObjectKind.Checkpoint or WorldObjectKind.StartPoint or WorldObjectKind.HiddenSwitch;
    public bool IsEditorOnly => !VisibleInPlay || Kind is WorldObjectKind.StartPoint or WorldObjectKind.HiddenSwitch;
    public Color Styled(Color fallback)
    {
        Color color = UseCustomTint ? Tint : fallback;
        color.A *= Mathf.Clamp(Opacity, 0f, 1f);
        return color;
    }

    public WorldObject Translated(Vector2 delta)
    {
        return this with
        {
            Start = Start + delta,
            End = End + delta
        };
    }

    public Rect2 Bounds(float unit, float elapsed = 0f)
    {
        Vector2 start = ResolvePoint(Start, unit, elapsed);
        Vector2 end = ResolvePoint(End, unit, elapsed);
        float pad = Mathf.Max(unit * ThicknessUnits, 4f);
        Vector2 min = new(Mathf.Min(start.X, end.X), Mathf.Min(start.Y, end.Y));
        Vector2 max = new(Mathf.Max(start.X, end.X), Mathf.Max(start.Y, end.Y));
        return new Rect2(min - new Vector2(pad, pad), max - min + new Vector2(pad * 2f, pad * 2f));
    }

    public Vector2 ResolvePoint(Vector2 point, float unit, float elapsed = 0f)
    {
        if (Kind != WorldObjectKind.Elevator)
            return point;

        float lift = Mathf.Sin(elapsed * SpeedUnits + Phase) * unit * RangeUnits;
        return point + new Vector2(0, lift);
    }

    public Vector2 ElevatorRangeVector(float unit)
    {
        return Kind == WorldObjectKind.Elevator
            ? new Vector2(0, unit * RangeUnits)
            : Vector2.Zero;
    }

    public float SurfaceYAt(float x, float unit, float elapsed = 0f)
    {
        Vector2 start = ResolvePoint(Start, unit, elapsed);
        Vector2 end = ResolvePoint(End, unit, elapsed);

        if (Mathf.Abs(end.X - start.X) < 0.001f)
            return Mathf.Min(start.Y, end.Y);

        float t = Mathf.Clamp((x - start.X) / (end.X - start.X), 0f, 1f);
        return start.Y + (end.Y - start.Y) * t;
    }

    public bool ContainsXRange(float left, float right, float unit, float elapsed = 0f)
    {
        Vector2 start = ResolvePoint(Start, unit, elapsed);
        Vector2 end = ResolvePoint(End, unit, elapsed);
        float min = Mathf.Min(start.X, end.X) - unit * ThicknessUnits;
        float max = Mathf.Max(start.X, end.X) + unit * ThicknessUnits;
        return left < max && right > min;
    }

    public Vector2 Direction(float unit, float elapsed = 0f)
    {
        Vector2 start = ResolvePoint(Start, unit, elapsed);
        Vector2 end = ResolvePoint(End, unit, elapsed);
        Vector2 direction = end - start;
        return direction.LengthSquared() < 0.001f ? Vector2.Right : direction.Normalized();
    }

    public Vector2 MotionDirection(float unit, float elapsed = 0f)
    {
        Vector2 direction = Direction(unit, elapsed);
        return SpeedUnits < 0 ? -direction : direction;
    }

    public Vector2 DownhillDirection(float unit, float elapsed = 0f)
    {
        Vector2 start = ResolvePoint(Start, unit, elapsed);
        Vector2 end = ResolvePoint(End, unit, elapsed);
        Vector2 downhill = start.Y > end.Y ? start - end : end - start;
        return downhill.LengthSquared() < 0.001f ? MotionDirection(unit, elapsed) : downhill.Normalized();
    }
}
