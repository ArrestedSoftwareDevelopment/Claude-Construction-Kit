using Godot;
using System;
using System.Collections.Generic;

namespace Dack;

public partial class PinballOverlay : Control
{
    private readonly List<PinballBall> _balls = [];
    private readonly HashSet<int> _litRollovers = [];
    private float _charge;
    private int _score;
    private int _ballsServed;
    private string _status = "SPACE CHARGES PLUNGER";
    private bool _leftFlipperWasHeld;
    private bool _rightFlipperWasHeld;

    public PlayfieldSurface? Playfield { get; set; }
    public bool Paused { get; set; } = true;

    public event Action<string>? SoundRequested;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetProcess(true);
        ResetGame();
    }

    public override void _Process(double delta)
    {
        if (!Visible || Playfield is null)
            return;

        if (!Paused)
            Step((float)delta);

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Playfield is null)
            return;

        DrawHud();
        foreach (PinballBall ball in _balls)
            DrawBall(ball);

        DrawFlipperPreview();
    }

    public void ResetGame()
    {
        _balls.Clear();
        _litRollovers.Clear();
        _charge = 0;
        _score = 0;
        _ballsServed = 0;
        _leftFlipperWasHeld = false;
        _rightFlipperWasHeld = false;
        _status = "SPACE CHARGES PLUNGER";
        ServeBall();
        QueueRedraw();
    }

    private void Step(float delta)
    {
        if (_balls.Count == 0)
            ServeBall();

        bool leftFlipperHeld = IsLeftFlipperHeld();
        bool rightFlipperHeld = IsRightFlipperHeld();
        if ((leftFlipperHeld && !_leftFlipperWasHeld) || (rightFlipperHeld && !_rightFlipperWasHeld))
            SoundRequested?.Invoke("pinball-flipper");
        _leftFlipperWasHeld = leftFlipperHeld;
        _rightFlipperWasHeld = rightFlipperHeld;

        bool spaceHeld = Input.IsKeyPressed(Key.Space);
        if (_balls.Count > 0 && _balls[0].Captured)
        {
            if (spaceHeld)
            {
                _charge = Mathf.Min(1f, _charge + delta * 0.82f);
                _status = $"PLUNGER {_charge * 100f:0}%";
            }
            else if (_charge > 0.02f)
            {
                LaunchCapturedBall();
            }
        }

        const float fixedStep = 1f / 120f;
        float remaining = Mathf.Min(delta, 1f / 20f);
        while (remaining > 0)
        {
            float step = Mathf.Min(fixedStep, remaining);
            StepBalls(step);
            remaining -= step;
        }
    }

    private void StepBalls(float dt)
    {
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        Vector2 gravity = new(0, 760f);

        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            PinballBall ball = _balls[i];
            if (!ball.Captured)
            {
                ball.PreviousPosition = ball.Position;
                ball.Velocity += gravity * dt;
                ball.Velocity *= Mathf.Pow(0.998f, dt * 120f);
                ClampSpeed(ref ball.Velocity, 960f);
                ball.Position += ball.Velocity * dt;

                ResolveBounds(ref ball, bounds);
                ResolveFlippers(ref ball);
                ResolveBumpers(ref ball);
                ResolveRollovers(ref ball);
                PlowThroughLetters(ball);

                if (ResolveDrain(ball, bounds))
                {
                    _balls.RemoveAt(i);
                    _status = "DRAIN";
                    SoundRequested?.Invoke("pinball-drain");
                    continue;
                }
            }

            _balls[i] = ball;
        }
    }

    private void ServeBall()
    {
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        PlungerGeometry plunger = GetPlunger(bounds);
        _balls.Add(new PinballBall(plunger.CapturePoint, Vector2.Zero, Mathf.Max(7f, (Playfield?.TextUnitPixels ?? 7f) * 1.08f), true, plunger.Direction));
        _ballsServed++;
        _charge = 0;
    }

    private void LaunchCapturedBall()
    {
        if (_balls.Count == 0)
            return;

        PinballBall ball = _balls[0];
        if (!ball.Captured)
            return;

        float force = Mathf.Lerp(360f, 980f, _charge);
        ball.Captured = false;
        ball.Velocity = ball.LaunchDirection * force;
        _balls[0] = ball;
        _status = "LIVE BALL";
        _charge = 0;
        SoundRequested?.Invoke("pinball-launch");
    }

    private void ResolveBounds(ref PinballBall ball, Rect2 bounds)
    {
        float top = bounds.Position.Y + 18f;
        if (ball.Position.X < bounds.Position.X + ball.Radius)
        {
            ball.Position.X = bounds.Position.X + ball.Radius;
            ball.Velocity.X = Mathf.Abs(ball.Velocity.X) * 0.86f;
        }
        else if (ball.Position.X > bounds.End.X - ball.Radius)
        {
            ball.Position.X = bounds.End.X - ball.Radius;
            ball.Velocity.X = -Mathf.Abs(ball.Velocity.X) * 0.86f;
        }

        if (ball.Position.Y < top + ball.Radius)
        {
            ball.Position.Y = top + ball.Radius;
            ball.Velocity.Y = Mathf.Abs(ball.Velocity.Y) * 0.82f;
        }
    }

    private void ResolveFlippers(ref PinballBall ball)
    {
        foreach (FlipperGeometry flipper in GetFlippers())
            ResolveFlipper(ref ball, flipper);
    }

    private void ResolveFlipper(ref PinballBall ball, FlipperGeometry flipper)
    {
        Vector2 closest = ClosestPointOnSegment(ball.Position, flipper.Pivot, flipper.Tip);
        Vector2 delta = ball.Position - closest;
        float distance = delta.Length();
        float combined = ball.Radius + flipper.Thickness;
        if (distance <= 0.001f || distance > combined)
            return;

        Vector2 normal = delta / distance;
        ball.Position += normal * (combined - distance + 0.5f);
        float speedInto = ball.Velocity.Dot(normal);
        if (speedInto < 0)
            ball.Velocity -= normal * speedInto * 1.85f;

        if (flipper.Active)
        {
            float tipBias = Mathf.Clamp(closest.DistanceTo(flipper.Pivot) / Mathf.Max(1f, flipper.Pivot.DistanceTo(flipper.Tip)), 0.15f, 1f);
            ball.Velocity += normal * flipper.Strength * tipBias;
            ball.Velocity += new Vector2(flipper.LeftSide ? 1f : -1f, -0.35f) * flipper.Strength * 0.18f;
            _status = "FLIP!";
            SoundRequested?.Invoke("pinball-flipper-hit");
        }

        ClampSpeed(ref ball.Velocity, 1120f);
    }

    private void ResolveBumpers(ref PinballBall ball)
    {
        foreach (WorldObject bumper in PinballObjects(WorldObjectKind.PinballBumper))
        {
            Vector2 center = bumper.ResolvePoint(bumper.Start, Playfield!.TextUnitPixels, Playfield.ElapsedSeconds);
            float radius = Mathf.Max(Playfield.TextUnitPixels * 3f, center.DistanceTo(bumper.End));
            Vector2 delta = ball.Position - center;
            float distance = delta.Length();
            if (distance <= 0.001f || distance > radius + ball.Radius)
                continue;

            Vector2 normal = delta / distance;
            ball.Position = center + normal * (radius + ball.Radius + 0.5f);
            ball.Velocity = normal * Mathf.Max(ball.Velocity.Length() * 1.08f, Mathf.Max(420f, Mathf.Abs(bumper.SpeedUnits) * 8f));
            _score += 100;
            _status = "POP +100";
            SoundRequested?.Invoke("pinball-bumper");
        }
    }

    private void ResolveRollovers(ref PinballBall ball)
    {
        int index = 0;
        foreach (WorldObject rollover in PinballObjects(WorldObjectKind.PinballRollover))
        {
            Rect2 sensor = rollover.Bounds(Playfield!.TextUnitPixels, Playfield.ElapsedSeconds);
            if (sensor.HasPoint(ball.Position) && _litRollovers.Add(index))
            {
                _score += 250;
                _status = "ROLLOVER +250";
                SoundRequested?.Invoke("pinball-rollover");
            }

            index++;
        }
    }

    private bool ResolveDrain(PinballBall ball, Rect2 bounds)
    {
        foreach (WorldObject drain in PinballObjects(WorldObjectKind.PinballDrain))
        {
            if (drain.Bounds(Playfield!.TextUnitPixels, Playfield.ElapsedSeconds).Grow(ball.Radius).HasPoint(ball.Position))
                return true;
        }

        return ball.Position.Y > bounds.End.Y + ball.Radius * 2f;
    }

    private void PlowThroughLetters(PinballBall ball)
    {
        if (Playfield is null || !Playfield.HasCapturedPage)
            return;

        float radius = Mathf.Max(ball.Radius * 1.35f, Playfield.TextUnitPixels * 1.4f);
        Rect2 plowBounds = new(ball.Position - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));
        int removed = 0;

        foreach (Rect2 letter in Playfield.GetTextObjectRegions(TextObjectGranularity.Letter))
        {
            if (removed >= 4)
                break;

            if (!plowBounds.Intersects(letter, true))
                continue;

            Vector2 offset = letter.GetCenter() - ball.Position;
            if (offset.LengthSquared() > radius * radius)
                continue;

            Playfield.EraseDocumentText(letter.Grow(2.0f));
            removed++;
        }

        if (removed <= 0)
            return;

        _score += removed * 15;
        _status = $"INK PLOW +{removed * 15}";
        Playfield.ThrowRandomLetters(ball.Position, removed);
        SoundRequested?.Invoke("pinball-text-plow");
    }

    private IEnumerable<WorldObject> PinballObjects(WorldObjectKind kind)
    {
        if (Playfield is null)
            yield break;

        foreach (WorldObject worldObject in Playfield.GetPlacedWorldObjects())
        {
            if (worldObject.Kind == kind && !worldObject.IsEditorOnly)
                yield return worldObject;
        }
    }

    private List<FlipperGeometry> GetFlippers()
    {
        List<FlipperGeometry> flippers = [];
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        float unit = Playfield?.TextUnitPixels ?? 7f;
        foreach (WorldObject flipper in PinballObjects(WorldObjectKind.PinballFlipper))
        {
            Vector2 pivot = flipper.ResolvePoint(flipper.Start, unit, Playfield!.ElapsedSeconds);
            Vector2 restTip = flipper.ResolvePoint(flipper.End, unit, Playfield.ElapsedSeconds);
            bool left = pivot.X < bounds.GetCenter().X;
            bool active = left ? IsLeftFlipperHeld() : IsRightFlipperHeld();
            float angle = Mathf.DegToRad((active ? 34f : 0f) * (left ? -1f : 1f));
            Vector2 tip = pivot + (restTip - pivot).Rotated(angle);
            flippers.Add(new FlipperGeometry(pivot, tip, Mathf.Max(9f, unit * flipper.ThicknessUnits * 1.35f), left, active, Mathf.Max(520f, Mathf.Abs(flipper.SpeedUnits) * 34f)));
        }

        if (flippers.Count == 0)
        {
            Vector2 leftPivot = new(bounds.Position.X + bounds.Size.X * 0.38f, bounds.End.Y - unit * 8f);
            Vector2 rightPivot = new(bounds.Position.X + bounds.Size.X * 0.62f, bounds.End.Y - unit * 8f);
            bool leftActive = IsLeftFlipperHeld();
            bool rightActive = IsRightFlipperHeld();
            Vector2 leftRest = leftPivot + new Vector2(unit * 11f, unit * 1.7f);
            Vector2 rightRest = rightPivot + new Vector2(-unit * 11f, unit * 1.7f);
            flippers.Add(new FlipperGeometry(leftPivot, leftPivot + (leftRest - leftPivot).Rotated(Mathf.DegToRad(leftActive ? -34f : 0f)), Mathf.Max(10f, unit * 1.45f), true, leftActive, 780f));
            flippers.Add(new FlipperGeometry(rightPivot, rightPivot + (rightRest - rightPivot).Rotated(Mathf.DegToRad(rightActive ? 34f : 0f)), Mathf.Max(10f, unit * 1.45f), false, rightActive, 780f));
        }

        return flippers;
    }

    private PlungerGeometry GetPlunger(Rect2 bounds)
    {
        foreach (WorldObject plunger in PinballObjects(WorldObjectKind.PinballPlunger))
        {
            Vector2 start = plunger.ResolvePoint(plunger.Start, Playfield!.TextUnitPixels, Playfield.ElapsedSeconds);
            Vector2 end = plunger.ResolvePoint(plunger.End, Playfield.TextUnitPixels, Playfield.ElapsedSeconds);
            Vector2 direction = (end - start).LengthSquared() < 0.01f ? new Vector2(-0.35f, -1f).Normalized() : (end - start).Normalized();
            return new PlungerGeometry(start, direction);
        }

        return new PlungerGeometry(
            new Vector2(bounds.End.X - Mathf.Max(52f, bounds.Size.X * 0.08f), bounds.End.Y - 44f),
            new Vector2(-0.28f, -1f).Normalized()
        );
    }

    private void DrawBall(PinballBall ball)
    {
        DrawCircle(ball.Position + new Vector2(2, 3), ball.Radius + 2f, new Color(0, 0, 0, 0.24f));
        DrawCircle(ball.Position, ball.Radius, new Color("#F7F5EF"));
        DrawCircle(ball.Position + new Vector2(-ball.Radius * 0.28f, -ball.Radius * 0.28f), ball.Radius * 0.36f, new Color("#5CB8FF", 0.72f));
        DrawCircle(ball.Position, ball.Radius, new Color("#202A34"), false, 1.4f);

        if (ball.Captured)
            DrawArc(ball.Position, ball.Radius + 8f + _charge * 18f, 0, Mathf.Tau * _charge, 24, new Color("#FF2BD6"), 3f);
    }

    private void DrawFlipperPreview()
    {
        foreach (FlipperGeometry flipper in GetFlippers())
        {
            Color shadow = new(0, 0, 0, 0.38f);
            Color body = flipper.Active ? new Color("#FF2BD6", 0.92f) : new Color("#5CB8FF", 0.88f);
            Color core = flipper.Active ? new Color("#FFF0A8", 0.96f) : new Color("#F7F5EF", 0.94f);
            Vector2 shadowOffset = new(3f, 5f);
            DrawLine(flipper.Pivot + shadowOffset, flipper.Tip + shadowOffset, shadow, flipper.Thickness * 2.45f);
            DrawLine(flipper.Pivot, flipper.Tip, new Color("#202A34", 0.96f), flipper.Thickness * 2.18f);
            DrawLine(flipper.Pivot, flipper.Tip, body, flipper.Thickness * 1.72f);
            DrawLine(flipper.Pivot, flipper.Tip, core, Mathf.Max(3f, flipper.Thickness * 0.42f));
            DrawCircle(flipper.Pivot + shadowOffset, flipper.Thickness * 1.75f, shadow);
            DrawCircle(flipper.Pivot, flipper.Thickness * 1.62f, new Color("#202A34", 0.98f));
            DrawCircle(flipper.Pivot, flipper.Thickness * 1.05f, body);
            DrawCircle(flipper.Pivot, flipper.Thickness * 0.62f, core);
        }
    }

    private void DrawHud()
    {
        Rect2 hud = new(18, 72, 238, 92);
        DrawRect(hud, new Color("#101820", 0.62f), true);
        DrawRect(hud, new Color("#F7F5EF", 0.55f), false, 1.2f);
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 24), "PINBALL TEST", HorizontalAlignment.Left, hud.Size.X - 24, 15, new Color("#FFF0A8"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 48), $"SCORE  {_score}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#F7F5EF"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 68), $"BALLS  {_balls.Count} live / {_ballsServed} served", HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 86), _status, HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#5CB8FF"));
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.LengthSquared();
        if (lengthSquared <= 0.001f)
            return start;

        float t = Mathf.Clamp((point - start).Dot(segment) / lengthSquared, 0f, 1f);
        return start + segment * t;
    }

    private static void ClampSpeed(ref Vector2 velocity, float maxSpeed)
    {
        if (velocity.LengthSquared() > maxSpeed * maxSpeed)
            velocity = velocity.Normalized() * maxSpeed;
    }

    private static bool IsLeftFlipperHeld() => Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left);
    private static bool IsRightFlipperHeld() => Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right);

    private struct PinballBall(Vector2 position, Vector2 velocity, float radius, bool captured, Vector2 launchDirection)
    {
        public Vector2 Position = position;
        public Vector2 PreviousPosition = position;
        public Vector2 Velocity = velocity;
        public float Radius = radius;
        public bool Captured = captured;
        public Vector2 LaunchDirection = launchDirection;
    }

    private readonly record struct FlipperGeometry(Vector2 Pivot, Vector2 Tip, float Thickness, bool LeftSide, bool Active, float Strength);
    private readonly record struct PlungerGeometry(Vector2 CapturePoint, Vector2 Direction);
}
