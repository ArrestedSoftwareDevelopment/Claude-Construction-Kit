using Godot;
using System;
using System.Collections.Generic;

namespace Dack;

public partial class BrickbatOverlay : Control
{
    private readonly List<BrickbatTarget> _bricks = [];
    private readonly List<Vector2> _ballPositions = [];
    private readonly List<Vector2> _ballVelocities = [];
    private readonly PsychedelicEffects _effects = new();
    private readonly List<string> _recentDestroyedWords = [];
    private Rect2 _paddle;
    private Rect2 _laserBeam;
    private Rect2 _lastHudRect;
    private Vector2? _manualHudPosition;
    private Vector2 _hudDragOffset;
    private bool _initialized;
    private bool _roundEnded;
    private bool _hudDragging;
    private bool _hudEditable;
    private int _score;
    private int _hits;
    private int _bonusEvery = 18;
    private int _reserveBalls;
    private int _laserStrength;
    private double _laserFlashSeconds;
    private double _laserDelaySeconds = -1;
    private double _multiballCooldownSeconds;
    private readonly RandomNumberGenerator _random = new();

    public PlayfieldSurface Playfield { get; set; } = null!;
    public IReadOnlyList<ActorView> Actors { get; set; } = Array.Empty<ActorView>();
    public bool SidePaddle { get; set; }
    public TextObjectGranularity BrickGranularity { get; set; } = TextObjectGranularity.Letter;
    public TextCollisionMode TextCollisionMode { get; set; } = TextCollisionMode.Bounce;
    public event Action<string>? SoundRequested;
    public bool HudEditable
    {
        get => _hudEditable;
        set
        {
            _hudEditable = value;
            MouseFilter = value ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
            if (!value)
                _hudDragging = false;
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _random.Randomize();
        Resized += QueueRedraw;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!HudEditable)
            return;

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed && GetHudRectForDraw().Grow(6f).HasPoint(mouseButton.Position))
            {
                _hudDragging = true;
                _hudDragOffset = mouseButton.Position - GetHudRectForDraw().Position;
                AcceptEvent();
                return;
            }

            if (!mouseButton.Pressed && _hudDragging)
            {
                _hudDragging = false;
                AcceptEvent();
                return;
            }
        }

        if (@event is InputEventMouseMotion motion && _hudDragging)
        {
            Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
            Vector2 hudSize = HudSize;
            Vector2 desired = motion.Position - _hudDragOffset;
            _manualHudPosition = new Vector2(
                Mathf.Clamp(desired.X, bounds.Position.X + 4f, Mathf.Max(bounds.Position.X + 4f, bounds.End.X - hudSize.X - 4f)),
                Mathf.Clamp(desired.Y, bounds.Position.Y + 4f, Mathf.Max(bounds.Position.Y + 4f, bounds.End.Y - hudSize.Y - 4f))
            );
            QueueRedraw();
            AcceptEvent();
        }
    }

    public override void _Process(double delta)
    {
        if (!Visible || Playfield is null)
            return;

        if (!_initialized)
            ResetGame();

        UpdatePaddle();
        QueueNearbyOcrTargets();
        UpdateBall((float)delta);
        UpdateEffects((float)delta);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Visible)
            return;

        DrawDeadZoneCover();
        DrawRect(_paddle, new Color("#202A34"), true);
        DrawRect(_paddle, new Color("#F7F5EF"), false, 2f);

        if (_laserFlashSeconds > 0 && Playfield is not null)
        {
            Color beamColor = new("#FF2BD6", 0.15f + 0.23f * Mathf.Sin((float)_laserFlashSeconds * 42f) * Mathf.Sin((float)_laserFlashSeconds * 42f));
            DrawRect(_laserBeam.Grow(7f), new Color("#5CB8FF", 0.09f), true);
            DrawRect(_laserBeam.Grow(3f), beamColor, true);
            DrawRect(_laserBeam, new Color("#FFF0A8", 0.46f), true);
        }

        _effects.Draw(this);

        foreach (Vector2 ballPosition in _ballPositions)
            DrawBall(ballPosition);

        DrawHud();
    }

    private void DrawBall(Vector2 ballPosition)
    {
        if (_effects.HasActiveEffects)
        {
            DrawCircle(ballPosition, 10f, new Color("#101820", 0.88f));
            DrawCircle(ballPosition, 7f, new Color("#F7F5EF", 0.96f));
            DrawCircle(ballPosition + new Vector2(Mathf.Sin((float)Time.GetTicksMsec() * 0.024f), 0), 3f, new Color("#FF2BD6", 0.82f));
            DrawCircle(ballPosition, 10f, new Color("#F7F5EF"), false, 1.4f);
            DrawCircle(ballPosition, 6f, new Color("#101820"), false, 1.2f);
            return;
        }

        DrawCircle(ballPosition, 6f, new Color("#F4C95D"));
        DrawCircle(ballPosition + new Vector2(Mathf.Sin((float)Time.GetTicksMsec() * 0.018f), 0), 2.5f, new Color("#FF5C35", 0.75f));
        DrawCircle(ballPosition, 6f, new Color("#202A34"), false, 1.5f);
    }

    public void ResetGame()
    {
        _bricks.Clear();
        _effects.Clear();
        Playfield?.ResetDocumentImage();
        _roundEnded = false;

        if (Playfield is not null)
        {
            Rect2 playBounds = Playfield.PlayBounds;
            foreach (Rect2 platform in Playfield.GetTextObjectRegions(BrickGranularity))
            {
                if (platform.Size.X >= 3f && platform.Position.Y > playBounds.Position.Y + 40f && platform.Position.Y < playBounds.End.Y - 80f)
                    _bricks.Add(new BrickbatTarget(platform, BrickGranularity == TextObjectGranularity.Word, null, null));
            }

            foreach (Rect2 anchor in Playfield.GetTextObjectRegions(TextObjectGranularity.BonusAnchor))
            {
                if (anchor.Position.Y > playBounds.Position.Y + 30f && anchor.Position.Y < playBounds.End.Y - 70f)
                    _bricks.Add(new BrickbatTarget(anchor, false, null, null));
            }

            foreach (ActorView actor in Actors)
            {
                if (actor.IsPlayable || !actor.Visible || actor.AnimationSet is null)
                    continue;

                Rect2 actorBounds = new(actor.Position, actor.Size);
                if (actorBounds.Size.X < 4f || actorBounds.Size.Y < 4f || !actorBounds.Intersects(playBounds, true))
                    continue;

                _bricks.Add(new BrickbatTarget(actorBounds.Grow(-Mathf.Min(actorBounds.Size.X, actorBounds.Size.Y) * 0.12f), false, actor, actor.ActorName));
            }
        }

        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        _ballPositions.Clear();
        _ballVelocities.Clear();
        _reserveBalls = 3;
        LaunchReserveBall(bounds);

        _score = 0;
        _hits = 0;
        _recentDestroyedWords.Clear();
        _laserStrength = 0;
        _laserFlashSeconds = 0;
        _laserDelaySeconds = -1;
        _multiballCooldownSeconds = 0;
        UpdatePaddle();
        _initialized = true;
        QueueRedraw();
    }

    public void ResetHudPosition()
    {
        _manualHudPosition = null;
        _hudDragging = false;
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
        if (_roundEnded)
            return;

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
            {
                BounceFromPaddle(ref ballPosition, ref ballVelocity);
                _effects.PaddleSpark(ballPosition);
                RequestSound("brickbat-paddle");
                ApplyBallSpeedTiers();
            }

            for (int i = _bricks.Count - 1; i >= 0; i--)
            {
                if (!ball.Intersects(_bricks[i].Bounds, true))
                    continue;

                if (_bricks[i].Actor is null && Playfield is not null && !Playfield.IsTextRegionStillActive(_bricks[i].Bounds))
                {
                    _bricks.RemoveAt(i);
                    continue;
                }

                if (TextCollisionMode == TextCollisionMode.Bounce)
                    BounceFrom(_bricks[i].Bounds, ref ballPosition, ref ballVelocity);

                HitBrick(i, ballPosition);

                if (TextCollisionMode == TextCollisionMode.Bounce)
                    break;
            }

            _ballPositions[ballIndex] = ballPosition;
            _ballVelocities[ballIndex] = ballVelocity;
        }

        if (!_roundEnded && _ballPositions.Count == 0)
        {
            if (_reserveBalls > 0)
                LaunchReserveBall(bounds);
            else
                EndRound("OUT OF COPY", new Color("#FF5C35"));
        }

        if (!_roundEnded && _bricks.Count == 0)
            EndRound("PAGE CLEARED", new Color("#5CB8A7"));
    }

    private void BounceFromPaddle(ref Vector2 ballPosition, ref Vector2 ballVelocity)
    {
        float speed = Mathf.Max(ballVelocity.Length() * 1.025f, 385f);
        if (SidePaddle)
        {
            float relative = Mathf.Clamp((ballPosition.Y - _paddle.GetCenter().Y) / (_paddle.Size.Y * 0.5f), -1f, 1f);
            ballPosition.X = _paddle.Position.X - 6f;
            ballVelocity = new Vector2(-1f, relative * 1.15f).Normalized() * Mathf.Min(speed, 560f);
        }
        else
        {
            float relative = Mathf.Clamp((ballPosition.X - _paddle.GetCenter().X) / (_paddle.Size.X * 0.5f), -1f, 1f);
            ballPosition.Y = _paddle.Position.Y - 6f;
            ballVelocity = new Vector2(relative * 1.15f, -1f).Normalized() * Mathf.Min(speed, 560f);
        }
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
        BrickbatTarget brick = _bricks[brickIndex];
        string? label = TryGetTargetLabel(brick);
        RemoveBrickCluster(brickIndex, brick.Bounds.Grow(4f));

        bool actorTarget = brick.Actor is not null;
        int points = actorTarget ? 100 : BrickGranularity == TextObjectGranularity.Word ? 50 : 10;
        _score += points;
        _hits++;
        _effects.TextHit(hitPosition, points, actorTarget || BrickGranularity == TextObjectGranularity.Word, label);
        if (actorTarget)
            _effects.ComicImpact(hitPosition, RandomBrickbatImpactWord(), 1.55f);

        RememberDestroyedWord(label);
        RequestSound(actorTarget || BrickGranularity == TextObjectGranularity.Word ? "brickbat-word-break" : "brickbat-text-hit");

        if (_hits % _bonusEvery == 0)
            TriggerBonus(hitPosition);
    }

    private string RandomBrickbatImpactWord()
    {
        string[] words = ["BANG", "POW", "BOOM", "KAPOW", "WHAM", "ZAP"];
        return words[_random.RandiRange(0, words.Length - 1)];
    }

    private void TriggerBonus(Vector2 position)
    {
        bool wantsMultiball = (_hits / _bonusEvery) % 2 == 1;
        if (wantsMultiball && SpawnMultiball())
        {
            _effects.Multiball(position);
            RequestSound("power-up");
            _multiballCooldownSeconds = 30;
        }
        else
        {
            ArmLaser(position);
            RequestSound("power-up");
        }
    }

    private void ArmLaser(Vector2 position)
    {
        _laserDelaySeconds = _random.RandfRange(0.45f, 1.25f);
        _laserStrength = _random.RandiRange(1, 10);
        _effects.LaserArmed(position, _laserStrength);
    }

    private bool SpawnMultiball()
    {
        if (_ballPositions.Count == 0 || _ballPositions.Count >= 3)
            return false;

        if (_multiballCooldownSeconds > 0)
            return false;

        int existing = _ballPositions.Count;
        for (int i = 0; i < existing && _ballPositions.Count < 3; i++)
        {
            Vector2 velocity = _ballVelocities[i].Rotated(i % 2 == 0 ? 0.35f : -0.35f);
            _ballPositions.Add(_ballPositions[i] + new Vector2(8f, -8f));
            _ballVelocities.Add(velocity);
        }

        ApplyBallSpeedTiers();
        return _ballPositions.Count > existing;
    }

    private void RemoveBall(int index)
    {
        _ballPositions.RemoveAt(index);
        _ballVelocities.RemoveAt(index);
        RequestSound("brickbat-ball-lost");
    }

    private void UpdateEffects(float delta)
    {
        _laserFlashSeconds = Mathf.Max(0, _laserFlashSeconds - delta);
        _multiballCooldownSeconds = Mathf.Max(0, _multiballCooldownSeconds - delta);
        if (_laserDelaySeconds > 0)
        {
            _laserDelaySeconds -= delta;
            if (_laserDelaySeconds <= 0)
                FireLaser();
        }

        _effects.Update(delta);
    }

    private void DrawHud()
    {
        Rect2 hud = GetHudRectForDraw();
        _lastHudRect = hud;
        DrawRect(hud, new Color("#202A34", 0.88f), true);
        DrawRect(hud, new Color("#D9E3EA", 0.65f), false, 1f);
        if (HudEditable)
        {
            DrawRect(hud.Grow(3f), new Color("#FF2BD6", 0.44f), false, 2f);
            DrawCircle(hud.Position + new Vector2(hud.Size.X - 12f, 12f), 4f, new Color("#FFF0A8"));
        }
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 22), $"SCORE  {_score}", HorizontalAlignment.Left, hud.Size.X - 24, 15, new Color("#F7F5EF"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 42), $"HITS   {_hits}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 60), $"LEFT   {_bricks.Count}", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 78), $"BALLS  {_reserveBalls} stock / {_ballPositions.Count} live", HorizontalAlignment.Left, hud.Size.X - 24, 13, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 96), $"MB CD  {Mathf.CeilToInt((float)_multiballCooldownSeconds),2}s", HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 114), $"TEXT   {TextCollisionMode.ToString().ToUpperInvariant()}", HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#D9E3EA"));
        DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 132), Playfield?.Ocr.StatusText ?? "OCR OFF", HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#8A97A5"));
        if (_recentDestroyedWords.Count > 0)
            DrawString(ThemeDB.FallbackFont, hud.Position + new Vector2(12, 150), string.Join(" / ", _recentDestroyedWords), HorizontalAlignment.Left, hud.Size.X - 24, 12, new Color("#FFF0A8"));
    }

    private Rect2 GetHudRectForDraw()
    {
        Rect2 bounds = Playfield?.PlayBounds ?? new Rect2(Vector2.Zero, Size);
        Vector2 hudSize = HudSize;
        if (_manualHudPosition is Vector2 manual)
        {
            return new Rect2(
                new Vector2(
                    Mathf.Clamp(manual.X, bounds.Position.X + 4f, Mathf.Max(bounds.Position.X + 4f, bounds.End.X - hudSize.X - 4f)),
                    Mathf.Clamp(manual.Y, bounds.Position.Y + 4f, Mathf.Max(bounds.Position.Y + 4f, bounds.End.Y - hudSize.Y - 4f))
                ),
                hudSize
            );
        }

        return Playfield?.FindWhitespaceRect(hudSize) ?? GetHudRect(bounds);
    }

    private void DrawDeadZoneCover()
    {
        if (Playfield is null || SidePaddle)
            return;

        Rect2 bounds = Playfield.PlayBounds;
        Rect2 cover = new(bounds.Position.X, _paddle.End.Y + 4f, bounds.Size.X, Mathf.Max(0, bounds.End.Y - _paddle.End.Y - 4f));
        if (cover.Size.Y <= 0)
            return;

        DrawRect(cover, new Color("#202A34", 0.96f), true);
        for (float x = cover.Position.X; x < cover.End.X; x += 18f)
            DrawLine(new Vector2(x, cover.Position.Y), new Vector2(x + cover.Size.Y, cover.End.Y), new Color("#52606D", 0.35f), 1f);
    }

    private void RemoveBrickCluster(int brickIndex, Rect2 blastRegion)
    {
        if (brickIndex >= 0 && brickIndex < _bricks.Count)
        {
            RemoveTargetFromPlayfield(_bricks[brickIndex]);
            _bricks.RemoveAt(brickIndex);
        }

        for (int i = _bricks.Count - 1; i >= 0; i--)
        {
            if (!_bricks[i].Bounds.Intersects(blastRegion, true))
                continue;

            RemoveTargetFromPlayfield(_bricks[i]);
            _bricks.RemoveAt(i);
        }
    }

    private void RemoveTargetFromPlayfield(BrickbatTarget target)
    {
        if (target.Actor is not null)
        {
            target.Actor.Visible = false;
            return;
        }

        Playfield?.EraseDocumentText(target.Bounds);
    }

    private void LaunchReserveBall(Rect2 bounds)
    {
        if (_reserveBalls <= 0)
            return;

        float spread = _random.RandfRange(-0.2f, 0.2f);
        if (SidePaddle)
        {
            _ballPositions.Add(new Vector2(bounds.End.X - _random.RandfRange(72f, 142f), _random.RandfRange(bounds.Position.Y + 90f, bounds.End.Y - 130f)));
            _ballVelocities.Add(new Vector2(-1f, spread).Normalized() * 385f);
        }
        else
        {
            _ballPositions.Add(new Vector2(_random.RandfRange(bounds.Position.X + 90f, bounds.End.X - 90f), bounds.End.Y - _random.RandfRange(86f, 148f)));
            _ballVelocities.Add(new Vector2(spread, -1f).Normalized() * 385f);
        }

        _reserveBalls--;
        ApplyBallSpeedTiers();
    }

    private void ApplyBallSpeedTiers()
    {
        float targetSpeed = _ballPositions.Count switch
        {
            >= 3 => 520f,
            2 => 455f,
            _ => 385f
        };

        for (int i = 0; i < _ballVelocities.Count; i++)
        {
            if (_ballVelocities[i].LengthSquared() <= 0.01f)
                continue;

            _ballVelocities[i] = _ballVelocities[i].Normalized() * targetSpeed;
        }
    }

    private void QueueNearbyOcrTargets()
    {
        if (Playfield is null || _bricks.Count == 0)
            return;

        for (int queued = 0; queued < 2; queued++)
        {
            Rect2? bestWord = null;
            float bestScore = float.PositiveInfinity;
            foreach (Rect2 word in Playfield.GetTextObjectRegions(TextObjectGranularity.Word))
            {
                if (Playfield.Ocr.TryGetLabel(word, out _))
                    continue;

                float score = DistanceToPlayHotspots(word);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestWord = word;
                }
            }

            if (bestWord is not Rect2 targetToQueue)
                return;

            if (Playfield.TryCreateOcrSample(targetToQueue, out Image? sample) && sample is not null)
                Playfield.Ocr.QueueRegion(targetToQueue, sample);
        }
    }

    private float DistanceToPlayHotspots(Rect2 region)
    {
        Vector2 center = region.GetCenter();
        float best = center.DistanceSquaredTo(_paddle.GetCenter()) * 0.55f;
        foreach (Vector2 ball in _ballPositions)
            best = Mathf.Min(best, center.DistanceSquaredTo(ball));

        return best;
    }

    private string? TryGetTargetLabel(BrickbatTarget target)
    {
        if (!string.IsNullOrWhiteSpace(target.Label))
            return target.Label;

        if (target.CanOcr && Playfield.Ocr.TryGetLabel(target.Bounds, out string label))
            return label;

        if (Playfield is null)
            return null;

        Rect2 probe = target.Bounds.Grow(4f);
        foreach (Rect2 word in Playfield.GetTextObjectRegions(TextObjectGranularity.Word))
        {
            if (!word.Intersects(probe, true))
                continue;

            if (Playfield.Ocr.TryGetLabel(word, out label))
                return label;

            if (Playfield.TryCreateOcrSample(word, out Image? sample) && sample is not null)
                Playfield.Ocr.QueueRegion(word, sample);
        }

        return null;
    }

    private void RememberDestroyedWord(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return;

        _recentDestroyedWords.Insert(0, label);
        while (_recentDestroyedWords.Count > 5)
            _recentDestroyedWords.RemoveAt(_recentDestroyedWords.Count - 1);
    }

    private void FireLaser()
    {
        if (Playfield is null)
            return;

        RequestSound("brickbat-laser");
        _laserFlashSeconds = 0.45f;
        int strength = _laserStrength <= 0 ? _random.RandiRange(1, 10) : _laserStrength;
        Rect2 playBounds = Playfield.PlayBounds;
        Rect2 beam = CreateLaserBeam(playBounds, strength);
        _laserBeam = beam;

        int destroyed = 0;
        Rect2 hitBeam = beam.Grow(1.5f + strength * 1.5f);
        for (int i = _bricks.Count - 1; i >= 0; i--)
        {
            if (!_bricks[i].Bounds.Intersects(hitBeam, true))
                continue;

            BrickbatTarget target = _bricks[i];
            RememberDestroyedWord(TryGetTargetLabel(target));
            RemoveTargetFromPlayfield(target);
            _bricks.RemoveAt(i);
            destroyed++;
        }

        if (destroyed > 0)
        {
            int points = destroyed * (BrickGranularity == TextObjectGranularity.Word ? 50 : 10);
            _score += points;
            _hits += destroyed;
            _effects.LaserColumn(beam, _paddle.GetCenter() + new Vector2(0, -32), strength, destroyed, points);
        }
        else
        {
            _effects.LaserColumn(beam, _paddle.GetCenter() + new Vector2(0, -32), strength, destroyed, 0);
        }

        _laserStrength = 0;
        _laserDelaySeconds = -1;
    }

    private Rect2 CreateLaserBeam(Rect2 playBounds, int strength)
    {
        float percentage = Mathf.Clamp(strength / 10f, 0.1f, 1f);
        float thickness = 8f + strength * 3.5f;
        Vector2 paddleCenter = _paddle.GetCenter();

        if (SidePaddle)
        {
            float length = playBounds.Size.X * percentage;
            float y = SnapLaserYToTarget(paddleCenter.Y, _paddle.Position.X - length, _paddle.Position.X, strength);
            return new Rect2(_paddle.Position.X - length, y - thickness * 0.5f, length, thickness);
        }

        float height = playBounds.Size.Y * percentage;
        float x = SnapLaserXToTarget(paddleCenter.X, _paddle.Position.Y - height, _paddle.Position.Y, strength);
        return new Rect2(x - thickness * 0.5f, _paddle.Position.Y - height, thickness, height);
    }

    private float SnapLaserXToTarget(float fallbackX, float minY, float maxY, int strength)
    {
        float lockRange = 18f + strength * 10f;
        float bestScore = float.PositiveInfinity;
        float bestX = fallbackX;

        foreach (BrickbatTarget target in _bricks)
        {
            Vector2 center = target.Bounds.GetCenter();
            if (center.Y < minY || center.Y > maxY)
                continue;

            float lateral = Mathf.Abs(center.X - fallbackX);
            if (lateral > lockRange)
                continue;

            float distanceFromPaddle = Mathf.Abs(_paddle.Position.Y - center.Y);
            float score = lateral * 3f + distanceFromPaddle * 0.05f;
            if (score >= bestScore)
                continue;

            bestScore = score;
            bestX = center.X;
        }

        return bestX;
    }

    private float SnapLaserYToTarget(float fallbackY, float minX, float maxX, int strength)
    {
        float lockRange = 18f + strength * 10f;
        float bestScore = float.PositiveInfinity;
        float bestY = fallbackY;

        foreach (BrickbatTarget target in _bricks)
        {
            Vector2 center = target.Bounds.GetCenter();
            if (center.X < minX || center.X > maxX)
                continue;

            float lateral = Mathf.Abs(center.Y - fallbackY);
            if (lateral > lockRange)
                continue;

            float distanceFromPaddle = Mathf.Abs(_paddle.Position.X - center.X);
            float score = lateral * 3f + distanceFromPaddle * 0.05f;
            if (score >= bestScore)
                continue;

            bestScore = score;
            bestY = center.Y;
        }

        return bestY;
    }

    private void EndRound(string message, Color color)
    {
        _roundEnded = true;
        _effects.RoundBanner(Playfield.PlayBounds.GetCenter(), message, color);
    }

    private void RequestSound(string key)
    {
        SoundRequested?.Invoke(key);
    }

    private Rect2 GetHudRect(Rect2 bounds)
    {
        if (Size.X - bounds.End.X >= 150f)
            return new Rect2(bounds.End.X + 14f, bounds.Position.Y + 18f, 132f, 92f);

        if (Size.Y - bounds.End.Y >= 104f)
            return new Rect2(bounds.Position.X + 14f, bounds.End.Y + 10f, 150f, 86f);

        return new Rect2(bounds.End.X - 164f, bounds.Position.Y + 18f, 150f, 86f);
    }

    private static Vector2 HudSize => new(190, 164);

    private readonly record struct BrickbatTarget(Rect2 Bounds, bool CanOcr, ActorView? Actor, string? Label);
}
