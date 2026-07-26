using Godot;

namespace Dack;

public enum WorldObjectKind
{
    Platform,
    Ladder,
    Ramp,
    Conveyor,
    Elevator
}

public sealed record WorldObject(
    WorldObjectKind Kind,
    Vector2 Start,
    Vector2 End,
    float ThicknessUnits = 0.8f,
    float SpeedUnits = 0f,
    float Phase = 0f
)
{
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

        float lift = Mathf.Sin(elapsed * SpeedUnits + Phase) * unit * 5f;
        return point + new Vector2(0, lift);
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
}
