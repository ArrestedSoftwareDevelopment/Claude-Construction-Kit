using Godot;
using System.Collections.Generic;

namespace Dack;

public partial class BrickbatOverlay : Control
{
    private readonly List<Rect2> _bricks = [];
    private readonly List<Vector2> _ballPositions = [];
    private readonly List<Vector2> _ballVelocities = [];
    private readonly List<FloatingText> _floatingTexts = [];
    private Rect2 _paddle;
    private bool _initialized;
    private int _score;
    private int _hits;
    private int _bonusEvery = 18;
    private double _laserFlashSeconds;

    public PlayfieldSurface Playfield { get; set; } = null!;
    public bool SidePaddle { get; set; }
    public TextObjectGranularity BrickGranularity { get; set; } = TextObjectGranularity.Letter;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += ResetGame;
    }

    public override void _Process(double delta)
    {
        if (!Visible || Playfield is null)
            return;

        if (!_initialized)
            ResetGame();

        UpdatePaddle();
        UpdateBall((float)delta);
        UpdateEffects((float)delta);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Visible)
            return;

        DrawRect(_paddle, new Color("#202A34"), true);
        DrawRect(_paddle, new Color("#F7F5EF"), false, 2f);

        foreach (Vector2 ballPosition in _ballPositions)
        {
            DrawCircle(ballPosition, 5f, new Color("#F4C95D"));
            DrawCircle(ballPosition, 5f, new Color("#202A34"), false, 1.5f);
        }

        if (_laserFlashSeconds > 0)
            DrawLine(_paddle.GetCenter(), _paddle.GetCenter() - new Vector2(0, 180), new Color("#FF5C35", 0.65f), 2f);

        DrawHud();

        foreach (FloatingText text in _floatingTexts)
            DrawString(ThemeDB.FallbackFont, text.Position, text.Text, HorizontalAlignment.Center, 140, 14, text.Color with { A = text.Alpha });
    }

    public void ResetGame()
    {
        _bricks.Clear();
        _floatingTexts.Clear();
        Playfield?.ResetDocumentImage();

        if (Playfield is not null)
        {
            Rect2 playBounds = Playfield.PlayBounds;
            foreach (Rect2 platform in Playfield.GetTextObjectRegions(BrickGranularity))
            {
                if (platform.Size.X >= 3f && platform.Position.Y > playBounds.Position.Y + 40f && platform.Position.Y < playBounds.End.Y - 80f)
                    _bricks.Add(platform);
            }
        }

        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        _ballPositions.Clear();
        _ballVelocities.Clear();
        if (SidePaddle)
        {
            _ballPositions.Add(new Vector2(bounds.End.X - 86f, bounds.GetCenter().Y));
            _ballVelocities.Add(new Vector2(-260f, -70f));
        }
        else
        {
            _ballPositions.Add(new Vector2(bounds.GetCenter().X, bounds.End.Y - 96f));
            _ballVelocities.Add(new Vector2(220f, -190f));
        }

        _score = 0;
        _hits = 0;
        _laserFlashSeconds = 0;
        UpdatePaddle();
        _initialized = true;
        QueueRedraw();
    }

    private void UpdatePaddle()
    {
        Vector2 mouse = GetLocalMousePosition();
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        if (SidePaddle)
        {
            float height = 96f;
            float y = Mathf.Clamp(mouse.Y - height * 0.5f, bounds.Position.Y + 40f, Mathf.Max(bounds.Position.Y + 40f, bounds.End.Y - height - 12f));
            _paddle = new Rect2(bounds.End.X - 32f, y, 12f, height);
        }
        else
        {
            float width = 120f;
            float x = Mathf.Clamp(mouse.X - width * 0.5f, bounds.Position.X + 12f, Mathf.Max(bounds.Position.X + 12f, bounds.End.X - width - 12f));
            _paddle = new Rect2(x, bounds.End.Y - 36f, width, 12f);
        }
    }

    private void UpdateBall(float delta)
    {
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        for (int ballIndex = _ballPositions.Count - 1; ballIndex >= 0; ballIndex--)
        {
            Vector2 ballPosition = _ballPositions[ballIndex] + _ballVelocities[ballIndex] * delta;
            Vector2 ballVelocity = _ballVelocities[ballIndex];

            if (ballPosition.X <= bounds.Position.X + 5f)
            {
                ballPosition.X = bounds.Position.X + 5f;
                ballVelocity.X = Mathf.Abs(ballVelocity.X);
            }
            else if (!SidePaddle && ballPosition.X >= bounds.End.X - 5f)
            {
                ballPosition.X = bounds.End.X - 5f;
                ballVelocity.X = -Mathf.Abs(ballVelocity.X);
            }

            if (SidePaddle && ballPosition.Y >= bounds.End.Y - 5f)
            {
                ballPosition.Y = bounds.End.Y - 5f;
                ballVelocity.Y = -Mathf.Abs(ballVelocity.Y);
            }

            if (ballPosition.Y <= bounds.Position.Y + 35f)
            {
                ballPosition.Y = bounds.Position.Y + 35f;
                ballVelocity.Y = Mathf.Abs(ballVelocity.Y);
            }

            if ((SidePaddle && ballPosition.X > bounds.End.X + 24f) || (!SidePaddle && ballPosition.Y > bounds.End.Y + 24f))
            {
                RemoveBall(ballIndex);
                continue;
            }

            Rect2 ball = new(ballPosition - new Vector2(5, 5), new Vector2(10, 10));
            if (ball.Intersects(_paddle, true))
                BounceFrom(_paddle, ref ballPosition, ref ballVelocity);

            for (int i = _bricks.Count - 1; i >= 0; i--)
            {
                if (!ball.Intersects(_bricks[i], true))
                    continue;

                BounceFrom(_bricks[i], ref ballPosition, ref ballVelocity);
                HitBrick(i, ballPosition);
                break;
            }

            _ballPositions[ballIndex] = ballPosition;
            _ballVelocities[ballIndex] = ballVelocity;
        }

        if (_ballPositions.Count == 0)
            ResetGame();

        if (_bricks.Count == 0)
            ResetGame();
    }

    private void BounceFrom(Rect2 target, ref Vector2 ballPosition, ref Vector2 ballVelocity)
    {
        Vector2 targetCenter = target.GetCenter();
        Vector2 delta = ballPosition - targetCenter;

        if (Mathf.Abs(delta.X / Mathf.Max(target.Size.X, 1f)) > Mathf.Abs(delta.Y / Mathf.Max(target.Size.Y, 1f)))
            ballVelocity.X = delta.X >= 0 ? Mathf.Abs(ballVelocity.X) : -Mathf.Abs(ballVelocity.X);
        else
            ballVelocity.Y = delta.Y >= 0 ? Mathf.Abs(ballVelocity.Y) : -Mathf.Abs(ballVelocity.Y);

        ballVelocity = ballVelocity.Normalized() * Mathf.Min(ballVelocity.Length() * 1.015f, 420f);
    }

    private void HitBrick(int brickIndex, Vector2 hitPosition)
    {
        Rect2 brick = _bricks[brickIndex];
        Playfield?.EraseDocumentText(brick);
        _bricks.RemoveAt(brickIndex);

        int points = BrickGranularity == TextObjectGranularity.Word ? 50 : 10;
        _score += points;
        _hits++;
        _floatingTexts.Add(new FloatingText($"+{points}", hitPosition, new Color("#5CB8A7"), 1f));

        if (_hits % _bonusEvery == 0)
            TriggerBonus(hitPosition);
    }

    private void TriggerBonus(Vector2 position)
    {
        if ((_hits / _bonusEvery) % 2 == 1)
        {
            SpawnMultiball();
            _floatingTexts.Add(new FloatingText("MULTIBALL", position + new Vector2(0, -18), new Color("#F4C95D"), 1.25f));
        }
        else
        {
            _laserFlashSeconds = 0.35f;
            _floatingTexts.Add(new FloatingText("LASER READY", position + new Vector2(0, -18), new Color("#FF5C35"), 1.25f));
        }
    }

    private void SpawnMultiball()
    {
        if (_ballPositions.Count == 0 || _ballPositions.Count >= 4)
            return;

        int existing = _ballPositions.Count;
        for (int i = 0; i < existing && _ballPositions.Count < 4; i++)
        {
            Vector2 velocity = _ballVelocities[i].Rotated(i % 2 == 0 ? 0.35f : -0.35f);
            _ballPositions.Add(_ballPositions[i] + new Vector2(8f, -8f));
            _ballVelocities.Add(velocity);
        }
    }

    private void RemoveBall(int index)
    {
        _ballPositions.RemoveAt(index);
        _ballVelocities.RemoveAt(index);
    }

    private void UpdateEffects(float delta)
    {
        _laserFlashSeconds = Mathf.Max(0, _laserFlashSeconds - delta);

        for (int i = _floatingTexts.Count - 1; i >= 0; i--)
        {
            FloatingText text = _floatingTexts[i];
            text.Position += new Vector2(0, -22f * delta);
            text.Alpha -= delta * 0.9f;
            if (text.Alpha <= 0)
                _floatingTexts.RemoveAt(i);
            else
                _floatingTexts[i] = text;
        }
    }

    private void DrawHud()
    {
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        Rect2 hud = GetHudRect(bounds);
        DrawRect(hud, new Color("#202A34", 0.88f), true);
        DrawRect(hud, new Color("#D9E3EA", 0.65f), false, 1f);
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 22), $"SCORE  {_score}", HorizontalAlignment.Left, hud.Size.X - 24, 15, new Color("#F7F5EF"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 42), $"HITS   {_hits}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 60), $"LEFT   {_bricks.Count}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 78), $"BALLS  {_ballPositions.Count}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
    }

    private Rect2 GetHudRect(Rect2 bounds)
    {
        if (Size.X - bounds.End.X >= 150f)
            return new Rect2(bounds.End.X + 14f, bounds.Position.Y + 18f, 132f, 92f);

        if (Size.Y - bounds.End.Y >= 104f)
            return new Rect2(bounds.Position.X + 14f, bounds.End.Y + 10f, 150f, 86f);

        return new Rect2(bounds.End.X - 164f, bounds.Position.Y + 18f, 150f, 86f);
    }

    private struct FloatingText
    {
        public string Text;
        public Vector2 Position;
        public Color Color;
        public float Alpha;

        public FloatingText(string text, Vector2 position, Color color, float alpha)
        {
            Text = text;
            Position = position;
            Color = color;
            Alpha = alpha;
        }
    }
}
