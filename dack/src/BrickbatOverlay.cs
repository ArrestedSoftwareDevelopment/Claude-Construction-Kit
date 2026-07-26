using Godot;
using System.Collections.Generic;

namespace Dack;

public partial class BrickbatOverlay : Control
{
    private readonly List<Rect2> _bricks = [];
    private Vector2 _ballPosition;
    private Vector2 _ballVelocity = new(220, -190);
    private Rect2 _paddle;
    private bool _initialized;

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
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Visible)
            return;

        DrawRect(_paddle, new Color("#202A34"), true);
        DrawRect(_paddle, new Color("#F7F5EF"), false, 2f);
        DrawCircle(_ballPosition, 5f, new Color("#F4C95D"));
        DrawCircle(_ballPosition, 5f, new Color("#202A34"), false, 1.5f);
    }

    public void ResetGame()
    {
        _bricks.Clear();
        Playfield?.ResetDocumentImage();

        if (Playfield is not null)
        {
            foreach (Rect2 platform in Playfield.GetTextObjectRegions(BrickGranularity))
            {
                if (platform.Size.X >= 3f && platform.Position.Y > 40f && platform.Position.Y < Size.Y - 80f)
                    _bricks.Add(platform);
            }
        }

        if (SidePaddle)
        {
            _ballPosition = new Vector2(Size.X - 86f, Size.Y * 0.5f);
            _ballVelocity = new Vector2(-260f, -70f);
        }
        else
        {
            _ballPosition = new Vector2(Size.X * 0.5f, Size.Y - 96f);
            _ballVelocity = new Vector2(220f, -190f);
        }

        UpdatePaddle();
        _initialized = true;
        QueueRedraw();
    }

    private void UpdatePaddle()
    {
        Vector2 mouse = GetLocalMousePosition();
        if (SidePaddle)
        {
            float height = 96f;
            float y = Mathf.Clamp(mouse.Y - height * 0.5f, 40f, Mathf.Max(40f, Size.Y - height - 12f));
            _paddle = new Rect2(Size.X - 32f, y, 12f, height);
        }
        else
        {
            float width = 120f;
            float x = Mathf.Clamp(mouse.X - width * 0.5f, 12f, Mathf.Max(12f, Size.X - width - 12f));
            _paddle = new Rect2(x, Size.Y - 36f, width, 12f);
        }
    }

    private void UpdateBall(float delta)
    {
        _ballPosition += _ballVelocity * delta;

        if (_ballPosition.X <= 5f)
        {
            _ballPosition.X = 5f;
            _ballVelocity.X = Mathf.Abs(_ballVelocity.X);
        }
        else if (!SidePaddle && _ballPosition.X >= Size.X - 5f)
        {
            _ballPosition.X = Size.X - 5f;
            _ballVelocity.X = -Mathf.Abs(_ballVelocity.X);
        }

        if (SidePaddle && _ballPosition.Y >= Size.Y - 5f)
        {
            _ballPosition.Y = Size.Y - 5f;
            _ballVelocity.Y = -Mathf.Abs(_ballVelocity.Y);
        }

        if (_ballPosition.Y <= 35f)
        {
            _ballPosition.Y = 35f;
            _ballVelocity.Y = Mathf.Abs(_ballVelocity.Y);
        }

        if ((SidePaddle && _ballPosition.X > Size.X + 24f) || (!SidePaddle && _ballPosition.Y > Size.Y + 24f))
            ResetGame();

        Rect2 ball = new(_ballPosition - new Vector2(5, 5), new Vector2(10, 10));
        if (ball.Intersects(_paddle, true))
            BounceFrom(_paddle);

        for (int i = _bricks.Count - 1; i >= 0; i--)
        {
            if (!ball.Intersects(_bricks[i], true))
                continue;

            BounceFrom(_bricks[i]);
            Playfield.EraseDocumentText(_bricks[i]);
            _bricks.RemoveAt(i);
            break;
        }

        if (_bricks.Count == 0)
            ResetGame();
    }

    private void BounceFrom(Rect2 target)
    {
        Vector2 targetCenter = target.GetCenter();
        Vector2 delta = _ballPosition - targetCenter;

        if (Mathf.Abs(delta.X / Mathf.Max(target.Size.X, 1f)) > Mathf.Abs(delta.Y / Mathf.Max(target.Size.Y, 1f)))
            _ballVelocity.X = delta.X >= 0 ? Mathf.Abs(_ballVelocity.X) : -Mathf.Abs(_ballVelocity.X);
        else
            _ballVelocity.Y = delta.Y >= 0 ? Mathf.Abs(_ballVelocity.Y) : -Mathf.Abs(_ballVelocity.Y);

        _ballVelocity = _ballVelocity.Normalized() * Mathf.Min(_ballVelocity.Length() * 1.015f, 420f);
    }
}
