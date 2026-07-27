using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dack;

public partial class Main : Control
{
    private readonly List<ActorView> _actors = [];
    private readonly Vector2[] _actorAnchors =
    [
        new(0.16f, 0.66f),
        new(0.43f, 0.34f),
        new(0.70f, 0.62f)
    ];

    private Control _workspace = null!;
    private Control _bossOverlay = null!;
    private PlayfieldSurface _playfield = null!;
    private SpritePad _spritePad = null!;
    private Label _selectionLabel = null!;
    private Label _bindingLabel = null!;
    private Label _toolLabel = null!;
    private Label _motionLabel = null!;
    private HSlider _scaleSlider = null!;
    private PanelContainer _sidebar = null!;
    private Button _spritePanelButton = null!;
    private PanelContainer _playsetToolbar = null!;
    private HBoxContainer _playsetToolbarRow = null!;
    private Button _playsetToolbarToggle = null!;
    private BrickbatOverlay _brickbatOverlay = null!;
    private ActorView _selectedActor = null!;
    private ActorView _player = null!;
    private EditableSpriteModel _initialModel = null!;
    private bool _bossMode;
    private double _elapsed;
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private bool _playerOnGround;
    private PlatformerMode _platformerMode = PlatformerMode.Horizontal;
    private PlaysetMode _playsetMode = PlaysetMode.Platformer;
    private float _textUnitPixels = 7f;

    public override void _Ready()
    {
        EnsureInputActions();
        BuildInterface();
        CreateActors();
        _playfield.Resized += OnPlayfieldResized;
        SelectActor(_actors[0]);
        UpdateCursorMode();
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        _playfield.ElapsedSeconds = (float)_elapsed;
        UpdatePlayer(delta);

        for (int i = 1; i < _actors.Count; i++)
        {
            if (!_actors[i].Visible)
                continue;

            Vector2 anchor = _actorAnchors[i];
            Vector2 basePosition = new(
                _playfield.Size.X * anchor.X - 52,
                _playfield.Size.Y * anchor.Y - 56
            );
            float bob = Mathf.Sin((float)_elapsed * 1.4f + i * 1.7f) * 5f;
            _actors[i].Position = basePosition + new Vector2(0, bob);
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey key
            && key.Pressed
            && !key.Echo
            && key.CtrlPressed
            && key.AltPressed
            && key.Keycode == Key.B)
        {
            ToggleBossMode();
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventKey toolbarKey
                 && toolbarKey.Pressed
                 && !toolbarKey.Echo
                 && toolbarKey.Keycode == Key.F1)
        {
            TogglePlaysetToolbar();
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildInterface()
    {
        _workspace = new VBoxContainer();
        _workspace.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_workspace);

        PanelContainer header = new()
        {
            CustomMinimumSize = new Vector2(0, 62)
        };
        header.AddThemeStyleboxOverride("panel", FlatStyle("#202A34"));
        header.Visible = false;
        _workspace.AddChild(header);

        MarginContainer headerMargin = Margins(20, 14, 20, 12);
        header.AddChild(headerMargin);
        HBoxContainer headerRow = new();
        headerMargin.AddChild(headerRow);

        Label title = new()
        {
            Text = "DACK  //  LIVE SPRITE LAB",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeColorOverride("font_color", new Color("#F7F5EF"));
        title.AddThemeFontSizeOverride("font_size", 22);
        headerRow.AddChild(title);

        Label status = new()
        {
            Text = "RAD 01   •   LOCAL PLAYFIELD",
            VerticalAlignment = VerticalAlignment.Center
        };
        status.AddThemeColorOverride("font_color", new Color("#AAB7C4"));
        status.AddThemeFontSizeOverride("font_size", 13);
        headerRow.AddChild(status);

        Button bossButton = Button("BOSS KEY  Ctrl+Alt+B");
        bossButton.Pressed += ToggleBossMode;
        headerRow.AddChild(bossButton);

        _spritePanelButton = Button("SHOW SPRITE PAD");
        _spritePanelButton.Pressed += ToggleSpritePanel;
        headerRow.AddChild(_spritePanelButton);

        HSplitContainer body = new()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SplitOffsets = [1280]
        };
        _workspace.AddChild(body);

        PanelContainer playfieldFrame = new()
        {
            CustomMinimumSize = Vector2.Zero
        };
        playfieldFrame.AddThemeStyleboxOverride("panel", FlatStyle("#111820", 0));
        body.AddChild(playfieldFrame);

        MarginContainer playfieldMargin = Margins(0, 0, 0, 0);
        playfieldFrame.AddChild(playfieldMargin);
        _playfield = new PlayfieldSurface
        {
            ClipContents = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        playfieldMargin.AddChild(_playfield);
        BuildPlaysetToolbar();

        _sidebar = new PanelContainer
        {
            CustomMinimumSize = new Vector2(392, 0)
        };
        _sidebar.AddThemeStyleboxOverride("panel", FlatStyle("#F2EFE8", 0));
        body.AddChild(_sidebar);

        MarginContainer sidebarMargin = Margins(22, 18, 22, 18);
        _sidebar.AddChild(sidebarMargin);
        VBoxContainer side = new();
        side.AddThemeConstantOverride("separation", 10);
        sidebarMargin.AddChild(side);

        Label selectedHeading = Heading("SELECTED ACTOR");
        side.AddChild(selectedHeading);

        _selectionLabel = new Label();
        _selectionLabel.AddThemeFontSizeOverride("font_size", 20);
        _selectionLabel.AddThemeColorOverride("font_color", new Color("#202A34"));
        side.AddChild(_selectionLabel);

        _bindingLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _bindingLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _bindingLabel.AddThemeFontSizeOverride("font_size", 13);
        side.AddChild(_bindingLabel);

        _spritePad = new SpritePad();
        side.AddChild(_spritePad);

        HBoxContainer tools = new();
        side.AddChild(tools);

        Button paint = Button("PAINT");
        paint.Pressed += () =>
        {
            _spritePad.Erasing = false;
            _toolLabel.Text = "Tool: paint  •  right-click erases";
        };
        tools.AddChild(paint);

        Button erase = Button("ERASE");
        erase.Pressed += () =>
        {
            _spritePad.Erasing = true;
            _toolLabel.Text = "Tool: erase  •  choose a color to paint";
        };
        tools.AddChild(erase);

        foreach (Color color in new[]
                 {
                     new Color("#181A1F"),
                     new Color("#FF5C35"),
                     new Color("#F4C95D"),
                     new Color("#5CB8A7"),
                     new Color("#4378B8")
                 })
        {
            Button swatch = new()
            {
                CustomMinimumSize = new Vector2(38, 38),
                TooltipText = $"Paint {color.ToHtml(false)}"
            };
            swatch.AddThemeStyleboxOverride("normal", FlatStyle(color.ToHtml(false), 3));
            swatch.AddThemeStyleboxOverride("hover", FlatStyle(color.Lightened(0.15f).ToHtml(false), 3));
            swatch.Pressed += () =>
            {
                _spritePad.PaintColor = color;
                _spritePad.Erasing = false;
                _toolLabel.Text = $"Tool: paint #{color.ToHtml(false)}  •  right-click erases";
            };
            tools.AddChild(swatch);
        }

        _toolLabel = new Label
        {
            Text = "Tool: paint  •  right-click erases"
        };
        _toolLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _toolLabel.AddThemeFontSizeOverride("font_size", 12);
        side.AddChild(_toolLabel);

        HBoxContainer actions = new();
        side.AddChild(actions);

        Button fork = Button("FORK SELECTED");
        fork.TooltipText = "Give this actor its own independent sprite.";
        fork.Pressed += ForkSelected;
        actions.AddChild(fork);

        Button reset = Button("RESET FIGURE");
        reset.Pressed += () => _selectedActor.Model.ResetToProcedural();
        actions.AddChild(reset);

        Label transparentNote = new()
        {
            Text = "32 × 32 live pad  •  white imports as transparent",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        transparentNote.AddThemeColorOverride("font_color", new Color("#737F8C"));
        transparentNote.AddThemeFontSizeOverride("font_size", 12);
        side.AddChild(transparentNote);

        Label motionHeading = Heading("CHARACTER MOTION");
        side.AddChild(motionHeading);

        HBoxContainer motionButtons = new();
        side.AddChild(motionButtons);

        Button horizontal = Button("PITFALL");
        horizontal.TooltipText = "Horizontal platformer tuning.";
        horizontal.Pressed += () => SetPlatformerMode(PlatformerMode.Horizontal);
        motionButtons.AddChild(horizontal);

        Button vertical = Button("CLIMBER");
        vertical.TooltipText = "Vertical platformer tuning with ladders.";
        vertical.Pressed += () => SetPlatformerMode(PlatformerMode.Vertical);
        motionButtons.AddChild(vertical);

        _scaleSlider = new HSlider
        {
            MinValue = 4,
            MaxValue = 18,
            Step = 1,
            Value = _textUnitPixels,
            TooltipText = "Match actor size to apparent text height."
        };
        _scaleSlider.ValueChanged += value =>
        {
            _textUnitPixels = (float)value;
            _playfield.TextUnitPixels = _textUnitPixels;
            ApplyActorScale();
            SnapPlayerToStart();
            _playfield.QueueRedraw();
            RefreshMotionText();
        };
        side.AddChild(_scaleSlider);

        _motionLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _motionLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _motionLabel.AddThemeFontSizeOverride("font_size", 12);
        side.AddChild(_motionLabel);

        _sidebar.Visible = false;
        BuildBossOverlay();
    }

    private void BuildPlaysetToolbar()
    {
        _brickbatOverlay = new BrickbatOverlay
        {
            Playfield = _playfield,
            Visible = false
        };
        _brickbatOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _playfield.AddChild(_brickbatOverlay);

        _playsetToolbar = new PanelContainer
        {
            Position = new Vector2(16, 42),
            MouseFilter = MouseFilterEnum.Stop
        };
        _playsetToolbar.AddThemeStyleboxOverride("panel", FlatStyle("#F7F5EF", 6));
        _playfield.AddChild(_playsetToolbar);

        MarginContainer margin = Margins(8, 8, 8, 8);
        _playsetToolbar.AddChild(margin);
        _playsetToolbarRow = new HBoxContainer();
        _playsetToolbarRow.AddThemeConstantOverride("separation", 6);
        margin.AddChild(_playsetToolbarRow);

        _playsetToolbarToggle = Button("-");
        _playsetToolbarToggle.CustomMinimumSize = new Vector2(34, 32);
        _playsetToolbarToggle.Pressed += TogglePlaysetToolbar;
        _playsetToolbarRow.AddChild(_playsetToolbarToggle);

        Button platformer = Button("PLATFORMER");
        platformer.Pressed += () => SetPlaysetMode(PlaysetMode.Platformer);
        _playsetToolbarRow.AddChild(platformer);

        Button brickbat = Button("BRICKBAT");
        brickbat.Pressed += () => SetPlaysetMode(PlaysetMode.Brickbat);
        _playsetToolbarRow.AddChild(brickbat);

        Button reset = Button("RESET");
        reset.Pressed += () =>
        {
            if (_playsetMode == PlaysetMode.Brickbat)
                _brickbatOverlay.ResetGame();
            else
                SnapPlayerToStart();
        };
        _playsetToolbarRow.AddChild(reset);

        Button side = Button("SIDE PADDLE");
        side.Pressed += () =>
        {
            _brickbatOverlay.SidePaddle = !_brickbatOverlay.SidePaddle;
            side.Text = _brickbatOverlay.SidePaddle ? "BOTTOM PADDLE" : "SIDE PADDLE";
            _brickbatOverlay.ResetGame();
        };
        _playsetToolbarRow.AddChild(side);

        Button grain = Button("LETTER BRICKS");
        grain.Pressed += () =>
        {
            _brickbatOverlay.BrickGranularity = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter
                ? TextObjectGranularity.Word
                : TextObjectGranularity.Letter;
            grain.Text = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter ? "LETTER BRICKS" : "WORD BRICKS";
            _brickbatOverlay.ResetGame();
        };
        _playsetToolbarRow.AddChild(grain);

        Button spritePad = Button("SPRITE PAD");
        spritePad.Pressed += ToggleSpritePanel;
        _playsetToolbarRow.AddChild(spritePad);

        Button boss = Button("BOSS");
        boss.Pressed += ToggleBossMode;
        _playsetToolbarRow.AddChild(boss);
    }

    private void BuildBossOverlay()
    {
        _bossOverlay = new ColorRect
        {
            Color = new Color("#F7F7F7"),
            Visible = false
        };
        _bossOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_bossOverlay);

        VBoxContainer document = new()
        {
            Position = new Vector2(90, 64),
            Size = new Vector2(900, 520)
        };
        _bossOverlay.AddChild(document);

        Label app = new() { Text = "Quarterly Planning Notes" };
        app.AddThemeFontSizeOverride("font_size", 24);
        app.AddThemeColorOverride("font_color", new Color("#25313C"));
        document.AddChild(app);

        Label copy = new()
        {
            Text = "\nQ3 priorities\n\n• Review staffing plan and delivery milestones\n• Reconcile the operating forecast\n• Prepare notes for Monday's status meeting\n\n"
                 + "Draft workspace — press Ctrl+Alt+B to return.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        copy.AddThemeFontSizeOverride("font_size", 16);
        copy.AddThemeColorOverride("font_color", new Color("#485664"));
        document.AddChild(copy);
    }

    private void CreateActors()
    {
        _initialModel = EditableSpriteModel.CreateInitial(out bool loadedThirdPartyAsset);

        for (int i = 0; i < 3; i++)
        {
            ActorView actor = new()
            {
                ActorName = $"Scout {i + 1}",
                Model = _initialModel
            };
            actor.SelectionRequested += SelectActor;
            _actors.Add(actor);
            _playfield.AddChild(actor);

            if (i > 0)
                actor.Visible = false;
        }

        _player = _actors[0];
        _player.ActorName = "Playable Scout";
        _player.IsPlayable = true;
        _player.AnimationSet = SpriteAnimationSet.TryLoadStickman();
        ApplyActorScale();
        SetPlatformerMode(PlatformerMode.Horizontal);

        _toolLabel.Text = loadedThirdPartyAsset
            ? "OctoPyte CC BY 4.0 figure loaded"
            : "Procedural project figure loaded  •  export-safe";
    }

    private void SelectActor(ActorView actor)
    {
        _selectedActor = actor;
        foreach (ActorView candidate in _actors)
            candidate.Selected = candidate == actor;

        _spritePad.Model = actor.Model;
        RefreshBindingText();
    }

    private void ForkSelected()
    {
        _selectedActor.Model = _selectedActor.Model.Fork();
        _spritePad.Model = _selectedActor.Model;
        RefreshBindingText();
    }

    private void RefreshBindingText()
    {
        int linkedActors = _actors.Count(actor => ReferenceEquals(actor.Model, _selectedActor.Model));
        _selectionLabel.Text = _selectedActor.ActorName;
        _bindingLabel.Text = linkedActors > 1
            ? $"LIVE LINK ACTIVE — edits update {linkedActors} actors instantly. Fork to make this actor independent."
            : "INDEPENDENT SPRITE — edits affect only this actor.";
    }

    private void ToggleBossMode()
    {
        _bossMode = !_bossMode;
        _workspace.Visible = !_bossMode;
        _bossOverlay.Visible = _bossMode;
        UpdateCursorMode();
    }

    private void ToggleSpritePanel()
    {
        _sidebar.Visible = !_sidebar.Visible;
        _spritePanelButton.Text = _sidebar.Visible ? "HIDE SPRITE PAD" : "SHOW SPRITE PAD";
        UpdateCursorMode();
    }

    private void TogglePlaysetToolbar()
    {
        bool collapsed = _playsetToolbarRow.GetChildCount() > 1 && _playsetToolbarRow.GetChild<Button>(1).Visible;

        for (int i = 1; i < _playsetToolbarRow.GetChildCount(); i++)
            _playsetToolbarRow.GetChild<Control>(i).Visible = !collapsed;

        _playsetToolbarToggle.Text = collapsed ? "+" : "-";
        UpdateCursorMode();
    }

    private void SetPlaysetMode(PlaysetMode mode)
    {
        _playsetMode = mode;
        bool brickbat = mode == PlaysetMode.Brickbat;
        _player.Visible = !brickbat;
        _brickbatOverlay.Visible = brickbat;

        if (brickbat)
            _brickbatOverlay.ResetGame();
        else
            SnapPlayerToStart();

        UpdateCursorMode();
    }

    private void SetPlatformerMode(PlatformerMode mode)
    {
        _platformerMode = mode;
        _playfield.Mode = mode;
        _playerVelocity = Vector2.Zero;
        SnapPlayerToStart();
        _playfield.QueueRedraw();
        RefreshMotionText();
    }

    private void UpdatePlayer(double delta)
    {
        if (_player is null || _bossMode || _playsetMode != PlaysetMode.Platformer)
            return;

        float dt = (float)delta;
        float unit = _textUnitPixels;
        float motionUnit = Mathf.Max(unit, 10f);
        float inputX = Input.GetAxis("dack_left", "dack_right");
        bool jumpPressed = Input.IsActionJustPressed("dack_jump");
        bool upHeld = Input.IsActionPressed("dack_up");
        bool downHeld = Input.IsActionPressed("dack_down");
        Rect2 actorBounds = new(_playerPosition, _player.Size);
        bool onLadder = _playfield.IsTouchingLadder(actorBounds);
        bool onSlide = _playfield.IsTouchingRamp(actorBounds);

        float maxSpeed = motionUnit * 16f;
        float acceleration = motionUnit * 70f;
        float friction = motionUnit * 78f;
        float gravity = motionUnit * 58f;
        float jumpSpeed = motionUnit * 24f;
        Vector2 conveyorVelocity = _playfield.GetConveyorVelocity(actorBounds);

        if (!Mathf.IsZeroApprox(inputX))
        {
            _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, inputX * maxSpeed, acceleration * dt);
            _player.FacingRight = inputX > 0;
        }
        else
        {
            _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, 0, friction * dt);
        }

        bool crawlingText = _platformerMode == PlatformerMode.Vertical && _playfield.HasCapturedPage && onLadder && (upHeld || downHeld);
        if (_platformerMode == PlatformerMode.Vertical && onLadder && (upHeld || downHeld))
        {
            float climb = (downHeld ? 1f : 0f) - (upHeld ? 1f : 0f);
            _playerVelocity.Y = climb * motionUnit * 11f;
            if (crawlingText)
                _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, inputX * motionUnit * 6f, acceleration * 0.5f * dt);
            _playerOnGround = false;
        }
        else
        {
            if (jumpPressed && (_playerOnGround || onLadder))
                _playerVelocity.Y = -jumpSpeed;

            _playerVelocity.Y += gravity * dt;
        }

        if (onSlide)
            _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, motionUnit * 9f, motionUnit * 30f * dt);

        if (conveyorVelocity != Vector2.Zero)
            _playerVelocity.X += conveyorVelocity.X * dt;

        Vector2 next = _playerPosition;
        next.X += _playerVelocity.X * dt;
        Rect2 playBounds = _playfield.PlayBounds;
        next.X = Mathf.Clamp(next.X, playBounds.Position.X, Mathf.Max(playBounds.Position.X, playBounds.End.X - _player.Size.X));

        next.Y += _playerVelocity.Y * dt;
        ResolveVerticalCollisions(ref next);

        if (next.Y > playBounds.End.Y + _player.Size.Y)
        {
            SnapPlayerToStart();
            RefreshMotionText();
            return;
        }

        _playerPosition = next;
        _player.Position = _playerPosition;
        UpdatePlayerAnimation(inputX, crawlingText);
        _player.QueueRedraw();
        RefreshMotionText();
    }

    private void UpdatePlayerAnimation(float inputX, bool crawlingText)
    {
        _player.AnimationClock = _elapsed;

        if (crawlingText)
        {
            _player.MotionState = ActorMotionState.Crawl;
            return;
        }

        if (!_playerOnGround)
        {
            _player.MotionState = _playerVelocity.Y < 0 ? ActorMotionState.JumpUp : ActorMotionState.JumpDown;
            return;
        }

        _player.MotionState = Mathf.Abs(inputX) > 0.05f || Mathf.Abs(_playerVelocity.X) > motionThreshold()
            ? ActorMotionState.Run
            : ActorMotionState.Idle;

        float motionThreshold() => Mathf.Max(_textUnitPixels, 6f);
    }

    private void ResolveVerticalCollisions(ref Vector2 next)
    {
        _playerOnGround = false;
        Rect2 nextBounds = new(next, _player.Size);
        float previousBottom = _playerPosition.Y + _player.Size.Y;

        foreach (Rect2 surface in GetSolidSurfaces())
        {
            if (_playerVelocity.Y >= 0
                && previousBottom <= surface.Position.Y + 2f
                && nextBounds.End.Y >= surface.Position.Y
                && nextBounds.Position.X < surface.End.X
                && nextBounds.End.X > surface.Position.X)
            {
                next.Y = surface.Position.Y - _player.Size.Y;
                _playerVelocity.Y = 0;
                _playerOnGround = true;
                nextBounds = new Rect2(next, _player.Size);
            }
        }

        foreach (WorldObject surface in GetLineSurfaces())
        {
            float centerX = next.X + _player.Size.X * 0.5f;
            float surfaceY = surface.SurfaceYAt(centerX, _textUnitPixels, _playfield.ElapsedSeconds);
            if (_playerVelocity.Y >= 0
                && previousBottom <= surfaceY + _textUnitPixels * 0.4f
                && nextBounds.End.Y >= surfaceY
                && surface.ContainsXRange(nextBounds.Position.X, nextBounds.End.X, _textUnitPixels, _playfield.ElapsedSeconds))
            {
                next.Y = surfaceY - _player.Size.Y;
                _playerVelocity.Y = 0;
                _playerOnGround = true;
                nextBounds = new Rect2(next, _player.Size);
            }
        }

        if (next.Y < 30)
        {
            next.Y = 30;
            _playerVelocity.Y = 0;
        }
    }

    private IEnumerable<Rect2> GetSolidSurfaces()
    {
        if (!_playfield.HasCapturedPage)
        {
            yield return _playfield.GetFloor();

            foreach (Rect2 platform in _playfield.GetPlatforms())
                yield return platform;

            yield break;
        }

        foreach (Rect2 word in _playfield.GetTextObjectRegions(TextObjectGranularity.Word))
            yield return new Rect2(
                word.Position + new Vector2(0, 2f),
                new Vector2(word.Size.X, Mathf.Max(2f, Mathf.Min(word.Size.Y, _textUnitPixels * 0.45f)))
            );
    }

    private IEnumerable<WorldObject> GetLineSurfaces()
    {
        foreach (WorldObject ramp in _playfield.GetRamps())
            yield return ramp;

        foreach (WorldObject conveyor in _playfield.GetConveyors())
            yield return conveyor;

        foreach (WorldObject elevator in _playfield.GetElevators())
            yield return elevator;
    }

    private void ApplyActorScale()
    {
        Vector2 playerSize = new(Mathf.Round(_textUnitPixels * 3.0f), Mathf.Round(_textUnitPixels * 4.6f));
        Vector2 cardSize = new(_textUnitPixels * 6.5f, _textUnitPixels * 7f);

        for (int i = 0; i < _actors.Count; i++)
        {
            _actors[i].Size = i == 0 ? playerSize : cardSize;
            _actors[i].CustomMinimumSize = _actors[i].Size;
        }
    }

    private void UpdateCursorMode()
    {
        Input.MouseMode = _bossMode || _sidebar.Visible || IsPlaysetToolbarExpanded()
            ? Input.MouseModeEnum.Visible
            : Input.MouseModeEnum.Hidden;
    }

    private bool IsPlaysetToolbarExpanded()
    {
        return _playsetToolbarRow.GetChildCount() <= 1 || _playsetToolbarRow.GetChild<Control>(1).Visible;
    }

    private void SnapPlayerToStart()
    {
        if (_player is null)
            return;

        ApplyActorScale();
        if (_platformerMode == PlatformerMode.Vertical)
            _playerPosition = new Vector2(_playfield.Size.X * 0.21f, _playfield.Size.Y * 0.72f - _player.Size.Y);
        else
            _playerPosition = _playfield.GetSpawnPosition(_player.Size);

        _player.Position = _playerPosition;
        _playerVelocity = Vector2.Zero;
        _playerOnGround = true;
    }

    private void OnPlayfieldResized()
    {
        SnapPlayerToStart();
        _playfield.QueueRedraw();
    }

    private void RefreshMotionText()
    {
        if (_motionLabel is null || _player is null)
            return;

        string mode = _platformerMode == PlatformerMode.Horizontal ? "horizontal run" : "vertical climb";
        string ground = _playerOnGround ? "grounded" : "airborne";
        _motionLabel.Text = $"{mode}  |  text unit {_textUnitPixels:0}px  |  actor {_player.Size.Y:0}px tall  |  {ground}";
    }

    private static void EnsureInputActions()
    {
        EnsureAction("dack_left", Key.A, Key.Left);
        EnsureAction("dack_right", Key.D, Key.Right);
        EnsureAction("dack_up", Key.W, Key.Up);
        EnsureAction("dack_down", Key.S, Key.Down);
        EnsureAction("dack_jump", Key.Space, Key.W, Key.Up);
    }

    private static void EnsureAction(string actionName, params Key[] keys)
    {
        StringName action = actionName;
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);

        foreach (Key key in keys)
        {
            InputMap.ActionAddEvent(action, new InputEventKey
            {
                PhysicalKeycode = key
            });
        }
    }

    private static Label Heading(string text)
    {
        Label label = new() { Text = text };
        label.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        label.AddThemeFontSizeOverride("font_size", 12);
        return label;
    }

    private static Button Button(string text)
    {
        Button button = new()
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 38),
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeFontSizeOverride("font_size", 12);
        return button;
    }

    private static MarginContainer Margins(int left, int top, int right, int bottom)
    {
        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", left);
        margin.AddThemeConstantOverride("margin_top", top);
        margin.AddThemeConstantOverride("margin_right", right);
        margin.AddThemeConstantOverride("margin_bottom", bottom);
        return margin;
    }

    private static StyleBoxFlat FlatStyle(string color, int cornerRadius = 6)
    {
        StyleBoxFlat style = new()
        {
            BgColor = new Color(color),
            CornerRadiusTopLeft = cornerRadius,
            CornerRadiusTopRight = cornerRadius,
            CornerRadiusBottomLeft = cornerRadius,
            CornerRadiusBottomRight = cornerRadius
        };
        return style;
    }
}
