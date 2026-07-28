using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dack;

public partial class Main : Control
{
    private readonly List<ActorView> _actors = [];
    private readonly List<PlayerShot> _playerShots = [];
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
    private PanelContainer _cockpit = null!;
    private Control _platformerPanel = null!;
    private Control _brickbatPanel = null!;
    private Control _pinballPanel = null!;
    private Control _overheadPanel = null!;
    private Label _cockpitStatus = null!;
    private Label _inspectorText = null!;
    private Label _attributeText = null!;
    private HSlider _speedSlider = null!;
    private HSlider _thicknessSlider = null!;
    private HSlider _rangeSlider = null!;
    private HSlider _opacitySlider = null!;
    private HSlider _gravitySlider = null!;
    private ColorPickerButton _tintPicker = null!;
    private CheckBox _customTintCheck = null!;
    private BrickbatOverlay _brickbatOverlay = null!;
    private ActorView _selectedActor = null!;
    private ActorView _player = null!;
    private EditableSpriteModel _initialModel = null!;
    private bool _bossMode;
    private double _elapsed;
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private bool _playerOnGround;
    private bool _platformerSafetyFloor = true;
    private bool _textTerrainEnabled = true;
    private bool _textDestructionEnabled = true;
    private bool _updatingAttributeControls;
    private PlatformerMode _platformerMode = PlatformerMode.Horizontal;
    private PlaysetMode _playsetMode = PlaysetMode.Platformer;
    private float _gravityScale = 1f;
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
        if (_cockpit is not null && _cockpit.Visible)
            RefreshCockpitStatus();

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
        else if (@event is InputEventKey escKey
                 && escKey.Pressed
                 && !escKey.Echo
                 && escKey.Keycode == Key.Escape)
        {
            ToggleCockpit();
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
        _playfield.WorldObjectSelectionChanged += text =>
        {
            if (_inspectorText is not null)
                _inspectorText.Text = text;
        };
        _playfield.WorldObjectSelectionObjectChanged += UpdateAttributeControls;
        BuildPlaysetToolbar();
        BuildCockpit();

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
        _cockpit.Visible = false;
        _playfield.ShowEditorOnlyObjects = false;
        BuildBossOverlay();
    }

    private void BuildCockpit()
    {
        _cockpit = new PanelContainer
        {
            Position = new Vector2(24, 24),
            CustomMinimumSize = new Vector2(1160, 520),
            MouseFilter = MouseFilterEnum.Stop
        };
        _cockpit.AddThemeStyleboxOverride("panel", FlatStyle("#202A34", 10));
        _playfield.AddChild(_cockpit);

        MarginContainer margin = Margins(16, 14, 16, 14);
        _cockpit.AddChild(margin);
        VBoxContainer root = new();
        root.AddThemeConstantOverride("separation", 12);
        margin.AddChild(root);

        HBoxContainer top = new();
        root.AddChild(top);

        Label title = new()
        {
            Text = "DACK COCKPIT  //  PLAY  BUILD  UNDERSTAND",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeColorOverride("font_color", new Color("#F7F5EF"));
        title.AddThemeFontSizeOverride("font_size", 18);
        top.AddChild(title);

        _cockpitStatus = new Label
        {
            Text = "Esc hides cockpit  •  Ctrl+Alt+B Boss Key",
            VerticalAlignment = VerticalAlignment.Center
        };
        _cockpitStatus.AddThemeColorOverride("font_color", new Color("#AAB7C4"));
        _cockpitStatus.AddThemeFontSizeOverride("font_size", 12);
        top.AddChild(_cockpitStatus);

        Button close = Button("×");
        close.TooltipText = "Close Cockpit (Esc)";
        close.CustomMinimumSize = new Vector2(36, 34);
        close.Pressed += ToggleCockpit;
        top.AddChild(close);

        HBoxContainer columns = new()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        columns.AddThemeConstantOverride("separation", 12);
        root.AddChild(columns);

        _platformerPanel = BuildShelfPanel();
        _brickbatPanel = BuildBrickbatPanel();
        _pinballPanel = BuildPinballPanel();
        _overheadPanel = BuildOverheadPanel();

        columns.AddChild(_platformerPanel);
        columns.AddChild(_brickbatPanel);
        columns.AddChild(_pinballPanel);
        columns.AddChild(_overheadPanel);
        columns.AddChild(BuildInspectorPanel());
        columns.AddChild(BuildUnderstandingPanel());
        UpdateCockpitToolkitPanels();
    }

    private Control BuildShelfPanel()
    {
        PanelContainer panel = CockpitPanel(260);
        VBoxContainer shelf = PanelVBox(panel);
        shelf.AddChild(CockpitHeading("PLATFORMER SHELF"));
        shelf.AddChild(ShelfButton("Add Ladder", WorldObjectKind.Ladder, "Climbable vertical tool; later: draggable endpoints."));
        shelf.AddChild(ShelfButton("Add Ramp", WorldObjectKind.Ramp, "Diagonal standable line for paragraph slants / Donkey Kong feel."));
        shelf.AddChild(ShelfButton("Add Slide", WorldObjectKind.Slide, "Sloped acceleration surface; currently uses ramp physics with slide push."));
        shelf.AddChild(ShelfButton("Add Conveyor", WorldObjectKind.Conveyor, "Moving belt surface; useful for office machinery and factory text."));
        shelf.AddChild(ShelfButton("Add Elevator", WorldObjectKind.Elevator, "Moving platform proof; later gets visible endpoints and timing."));
        shelf.AddChild(ShelfButton("Add Checkpoint", WorldObjectKind.Checkpoint, "Visible marker now; spawn binding comes next."));
        shelf.AddChild(ShelfButton("Add Start Point", WorldObjectKind.StartPoint, "Editor-only spawn marker. Visible while building, hidden during play."));
        shelf.AddChild(ShelfButton("Add Hidden Switch", WorldObjectKind.HiddenSwitch, "Invisible gameplay logic: visible in editor, hidden from the player."));

        Button floor = Button(_platformerSafetyFloor ? "Safety Floor: On" : "Safety Floor: Off");
        floor.Pressed += () =>
        {
            _platformerSafetyFloor = !_platformerSafetyFloor;
            floor.Text = _platformerSafetyFloor ? "Safety Floor: On" : "Safety Floor: Off";
            SetPlaysetMode(PlaysetMode.Platformer);
            SnapPlayerToStart();
            _inspectorText.Text = _platformerSafetyFloor
                ? "Platformer safety floor enabled. Falling below the document catches the player."
                : "Platformer safety floor disabled. Gutter/plunge/death-pit levels can now work.";
        };
        shelf.AddChild(floor);

        Button textTerrain = Button(_textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off");
        textTerrain.Pressed += () =>
        {
            _textTerrainEnabled = !_textTerrainEnabled;
            textTerrain.Text = _textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off";
            _inspectorText.Text = _textTerrainEnabled
                ? "Player text terrain enabled. Captured letters/words can support the scout."
                : "Player text terrain disabled. Only explicit platforms/ramps/elevators/conveyors/floor support the scout.";
        };
        shelf.AddChild(textTerrain);

        Button textCrawl = Button(_playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off");
        textCrawl.Pressed += () =>
        {
            _playfield.TextCrawlEnabled = !_playfield.TextCrawlEnabled;
            textCrawl.Text = _playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off";
            _inspectorText.Text = _playfield.TextCrawlEnabled
                ? "Player text crawl enabled. Dense single-spaced text can act like climb/crawl surface in Climber mode."
                : "Player text crawl disabled. Use explicit ladders or other tools for climbing.";
        };
        shelf.AddChild(textCrawl);

        Button textDestruction = Button(_textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off");
        textDestruction.Pressed += () =>
        {
            _textDestructionEnabled = !_textDestructionEnabled;
            textDestruction.Text = _textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off";
            _inspectorText.Text = _textDestructionEnabled
                ? "Platformer shots can erase captured text in the working clone."
                : "Platformer shots no longer damage text; useful for no-destruction traversal tests.";
        };
        shelf.AddChild(textDestruction);

        Button clear = Button("CLEAR PLACED PARTS");
        clear.Pressed += () =>
        {
            _playfield.ClearPlacedObjects();
            _inspectorText.Text = "Placed toolkit parts cleared. Captured document pixels and Brickbat mutations remain separate.";
        };
        shelf.AddChild(clear);

        shelf.AddChild(CockpitNote("These are DACK overlay objects. They do not edit the source document or screenshot clone."));
        return panel;
    }

    private Control BuildBrickbatPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer brickbat = PanelVBox(panel);
        brickbat.AddChild(CockpitHeading("BRICKBAT PAGE"));
        brickbat.AddChild(CockpitNote("Brickbat-specific controls live here now instead of crowding the always-on strip."));

        Button enter = Button("ENTER BRICKBAT");
        enter.Pressed += () => SetPlaysetMode(PlaysetMode.Brickbat);
        brickbat.AddChild(enter);

        Button paddle = Button(_brickbatOverlay.SidePaddle ? "Paddle: Side" : "Paddle: Bottom");
        paddle.Pressed += () =>
        {
            _brickbatOverlay.SidePaddle = !_brickbatOverlay.SidePaddle;
            paddle.Text = _brickbatOverlay.SidePaddle ? "Paddle: Side" : "Paddle: Bottom";
            SetPlaysetMode(PlaysetMode.Brickbat);
            _brickbatOverlay.ResetGame();
            _inspectorText.Text = _brickbatOverlay.SidePaddle
                ? "Brickbat side-paddle mode. Useful for vertical/side-wall target clearing."
                : "Brickbat bottom-paddle mode. Standard document brick-clearing layout.";
        };
        brickbat.AddChild(paddle);

        Button grain = Button(_brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter ? "Targets: Letters" : "Targets: Words");
        grain.Pressed += () =>
        {
            _brickbatOverlay.BrickGranularity = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter
                ? TextObjectGranularity.Word
                : TextObjectGranularity.Letter;
            grain.Text = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter ? "Targets: Letters" : "Targets: Words";
            SetPlaysetMode(PlaysetMode.Brickbat);
            _brickbatOverlay.ResetGame();
            _inspectorText.Text = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter
                ? "Brickbat target grain set to letters. Fine-grained page destruction; OCR labels can still bleed in from nearby word regions."
                : "Brickbat target grain set to words. Larger targets, +50 scoring, stronger Word Sense / found-poem behavior.";
        };
        brickbat.AddChild(grain);

        Button textCollision = Button(_brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce ? "Text Physics: Bounce" : "Text Physics: Pierce");
        textCollision.Pressed += () =>
        {
            _brickbatOverlay.TextCollisionMode = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce
                ? TextCollisionMode.PassThrough
                : TextCollisionMode.Bounce;
            textCollision.Text = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce ? "Text Physics: Bounce" : "Text Physics: Pierce";
            SetPlaysetMode(PlaysetMode.Brickbat);
            _inspectorText.Text = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce
                ? "Brickbat text collision set to Bounce. Letters/words act like solid targets and deflect the ball."
                : "Brickbat text collision set to Pierce. The ball erases and scores text but keeps traveling through it. This is the seed of ghost-ball/piercing powerups and conditional zones.";
        };
        brickbat.AddChild(textCollision);

        Button reset = Button("RESET BRICKBAT");
        reset.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Brickbat);
            _brickbatOverlay.ResetGame();
            _inspectorText.Text = "Brickbat reset. Clone damage returns to the captured/source image for this run.";
        };
        brickbat.AddChild(reset);

        Button resetHud = Button("AUTO-PLACE SCORE");
        resetHud.Pressed += () =>
        {
            _brickbatOverlay.ResetHudPosition();
            _inspectorText.Text = "Brickbat score panel returned to auto whitespace placement. Open the Cockpit in Brickbat and drag the panel to pin it somewhere else.";
        };
        brickbat.AddChild(resetHud);

        brickbat.AddChild(CockpitHeading("LATER"));
        brickbat.AddChild(CockpitNote("Target recipes, bonus deck, laser settings, persistence policy, HUD style, and word-goal filters belong on this page."));
        return panel;
    }

    private Control BuildPinballPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer pinball = PanelVBox(panel);
        pinball.AddChild(CockpitHeading("PINBALL PAGE"));
        pinball.AddChild(CockpitNote("Starter goal: prove one ball, two flippers, bumpers, drains, and score inserts on the same cloned playfield."));

        Button enter = Button("ENTER PINBALL");
        enter.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Pinball);
            _inspectorText.Text =
                "Pinball module placeholder.\n\n"
                + "Next coding pass should add PinballOverlay: ball physics, flipper arcs, bumpers, drains, plunger lane, tilt/nudge, and score events.";
        };
        pinball.AddChild(enter);

        pinball.AddChild(CockpitHeading("FIRST PARTS"));
        pinball.AddChild(CockpitNote(
            "Flippers, plunger lane, ball spawn, bumper, rollover, drain, gate, ramp rail, lit insert, jackpot target. "
            + "All should use direct handles: flipper sweep arc, bumper radius, drain width, gate direction, and plunger force."
        ));
        pinball.AddChild(CockpitHeading("SOURCE FIT"));
        pinball.AddChild(CockpitNote(
            "Best early sources: Photoshop/GIMP/Krita/Paint canvases, PowerPoint/Draw/diagram slides, desktop/icon layouts, and BBS/textmode table art. "
            + "Word works as a themed table, but canvas apps are the natural home."
        ));
        return panel;
    }

    private Control BuildOverheadPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer overhead = PanelVBox(panel);
        overhead.AddChild(CockpitHeading("OVERHEAD PAGE"));
        overhead.AddChild(CockpitNote("Overhead is a family: Combat tanks, driving, planes/spaceships, RPG actors, animals, insects, and office creatures. Combat is the first preset."));

        Button enter = Button("ENTER OVERHEAD");
        enter.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Overhead);
            _inspectorText.Text =
                "Overhead toolkit placeholder.\n\n"
                + "Next coding pass should add OverheadActorController with movement presets: tank/combat, driving, plane/space, RPG walk, and creature/insect crawl/swarm.";
        };
        overhead.AddChild(enter);

        overhead.AddChild(CockpitHeading("FIRST PARTS"));
        overhead.AddChild(CockpitNote(
            "Player spawn, enemy spawn, patrol/guard route, cover region, ricochet wall, destructible text/object rule, pickup, door/gate, objective, and safe zone."
        ));
        overhead.AddChild(CockpitHeading("MOVEMENT SEEDS"));
        overhead.AddChild(CockpitNote(
            "Tank rotate/drive, car steer/drift, plane thrust/coast, RPG walk/interact, insect wander/forage/swarm. Same top-down world, different movement grammar."
        ));
        return panel;
    }

    private Control BuildInspectorPanel()
    {
        PanelContainer panel = CockpitPanel(330);
        VBoxContainer inspector = PanelVBox(panel);
        inspector.AddChild(CockpitHeading("INSPECTOR"));
        _inspectorText = CockpitNote(
            "Select or place a toolkit object.\n\n"
            + "First pass supports placement from the shelf. Next pass should add direct handles: drag endpoints, set origin, bind to words, toggle text/graphic/hybrid, and fork shared assets."
        );
        inspector.AddChild(_inspectorText);

        inspector.AddChild(CockpitHeading("ATTRIBUTES"));
        _attributeText = CockpitNote("Select a placed object to edit speed, direction, thickness, and slope behavior.");
        inspector.AddChild(_attributeText);

        _speedSlider = AttributeSlider(-12, 12, 0, 0.5);
        _speedSlider.ValueChanged += value =>
        {
            if (_updatingAttributeControls)
                return;

            _playfield.SetSelectedSpeed((float)value);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Speed / force"));
        inspector.AddChild(_speedSlider);

        _thicknessSlider = AttributeSlider(0.3, 3.0, 0.8, 0.1);
        _thicknessSlider.ValueChanged += value =>
        {
            if (_updatingAttributeControls)
                return;

            _playfield.SetSelectedThickness((float)value);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Thickness / collision pad"));
        inspector.AddChild(_thicknessSlider);

        _rangeSlider = AttributeSlider(0, 16, 5, 0.5);
        _rangeSlider.ValueChanged += value =>
        {
            if (_updatingAttributeControls)
                return;

            _playfield.SetSelectedRange((float)value);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Range of motion"));
        inspector.AddChild(_rangeSlider);

        _tintPicker = new ColorPickerButton
        {
            Color = new Color("#5CB8A7"),
            CustomMinimumSize = new Vector2(0, 34),
            FocusMode = FocusModeEnum.None,
            Text = "Object Color"
        };
        _tintPicker.ColorChanged += color =>
        {
            if (_updatingAttributeControls)
                return;

            _playfield.SetSelectedTint(color);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Color / tint"));
        inspector.AddChild(_tintPicker);

        _customTintCheck = new CheckBox
        {
            Text = "Use custom color",
            FocusMode = FocusModeEnum.None
        };
        _customTintCheck.Toggled += enabled =>
        {
            if (_updatingAttributeControls)
                return;

            if (enabled)
                _playfield.SetSelectedTint(_tintPicker.Color);
            else
                _playfield.ClearSelectedTint();

            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(_customTintCheck);

        _opacitySlider = AttributeSlider(0, 1, 1, 0.05);
        _opacitySlider.ValueChanged += value =>
        {
            if (_updatingAttributeControls)
                return;

            _playfield.SetSelectedOpacity((float)value);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Opacity"));
        inspector.AddChild(_opacitySlider);

        Button clearTint = Button("DEFAULT COLOR");
        clearTint.Pressed += () =>
        {
            _playfield.ClearSelectedTint();
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(clearTint);

        Button reverse = Button("REVERSE DIRECTION");
        reverse.Pressed += () =>
        {
            _playfield.ReverseSelectedDirection();
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(reverse);

        Button normalize = Button("RAMP UP / SLIDE DOWN");
        normalize.Pressed += () =>
        {
            _playfield.NormalizeSelectedSlope();
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(normalize);

        inspector.AddChild(CockpitHeading("WORLD RULES"));
        _gravitySlider = AttributeSlider(0.25, 2.0, _gravityScale, 0.05);
        _gravitySlider.ValueChanged += value =>
        {
            if (_updatingAttributeControls)
                return;

            _gravityScale = (float)value;
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(CockpitNote("Player gravity scale"));
        inspector.AddChild(_gravitySlider);

        Button spritePad = Button("TOGGLE SPRITE PAD");
        spritePad.Pressed += ToggleSpritePanel;
        inspector.AddChild(spritePad);

        Button platformer = Button("PLATFORMER MODE");
        platformer.Pressed += () => SetPlaysetMode(PlaysetMode.Platformer);
        inspector.AddChild(platformer);

        Button resetScout = Button("RESET SCOUT START");
        resetScout.Pressed += SnapPlayerToStart;
        inspector.AddChild(resetScout);

        inspector.AddChild(CockpitHeading("PINBALL ASSET NOTE"));
        inspector.AddChild(CockpitNote(
            "VerzatileDev pinball source PNGs are huge (~3937×3937 / ~118 MB each). "
            + "Do not shelf-import them raw. Next step is a curated pinball-parts sheet: flipper, ball, bumper, plunger, insert, gate, drain, ramp segment."
        ));
        return panel;
    }

    private Control BuildUnderstandingPanel()
    {
        PanelContainer panel = CockpitPanel(280);
        VBoxContainer understand = PanelVBox(panel);
        understand.AddChild(CockpitHeading("UNDERSTAND"));
        understand.AddChild(CockpitNote(
            "Layer toggles will live here: source clone, text boxes, word labels, collision, placed objects, invisible logic, mutations, routes, and HUD avoidance."
        ));
        understand.AddChild(CockpitHeading("WORD SENSE"));
        understand.AddChild(CockpitNote(_playfield.Ocr.StatusText));
        understand.AddChild(CockpitHeading("NEXT HANDLES"));
        understand.AddChild(CockpitNote(
            "Ladder endpoints, elevator rails, ramp splines, checkpoint spawn binding, and pinball flipper arcs are the first handle family to build."
        ));
        return panel;
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

        Button pinball = Button("PINBALL");
        pinball.Pressed += () => SetPlaysetMode(PlaysetMode.Pinball);
        _playsetToolbarRow.AddChild(pinball);

        Button overhead = Button("OVERHEAD");
        overhead.Pressed += () => SetPlaysetMode(PlaysetMode.Overhead);
        _playsetToolbarRow.AddChild(overhead);

        Button reset = Button("RESET");
        reset.Pressed += () =>
        {
            if (_playsetMode == PlaysetMode.Brickbat)
                _brickbatOverlay.ResetGame();
            else
                SnapPlayerToStart();
        };
        _playsetToolbarRow.AddChild(reset);

        Button cockpit = Button("COCKPIT");
        cockpit.Pressed += ToggleCockpit;
        _playsetToolbarRow.AddChild(cockpit);

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

    private void ToggleCockpit()
    {
        _cockpit.Visible = !_cockpit.Visible;
        _playfield.ShowEditorOnlyObjects = _cockpit.Visible;
        _brickbatOverlay.HudEditable = _cockpit.Visible;
        _playfield.QueueRedraw();
        RefreshCockpitStatus();
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

    private void RefreshCockpitStatus()
    {
        if (_cockpitStatus is null)
            return;

        string mode = PlaysetModeLabel(_playsetMode);
        _cockpitStatus.Text = $"{mode}  •  {_playfield.Ocr.StatusText}  •  contextual shelves  •  Esc hides cockpit";
    }

    private void UpdateCockpitToolkitPanels()
    {
        if (_platformerPanel is null || _brickbatPanel is null || _pinballPanel is null || _overheadPanel is null)
            return;

        _platformerPanel.Visible = _playsetMode == PlaysetMode.Platformer;
        _brickbatPanel.Visible = _playsetMode == PlaysetMode.Brickbat;
        _pinballPanel.Visible = _playsetMode == PlaysetMode.Pinball;
        _overheadPanel.Visible = _playsetMode == PlaysetMode.Overhead;
    }

    private void SetPlaysetMode(PlaysetMode mode)
    {
        _playsetMode = mode;
        bool brickbat = mode == PlaysetMode.Brickbat;
        bool showScout = mode is PlaysetMode.Platformer or PlaysetMode.Overhead;
        _player.Visible = showScout;
        _brickbatOverlay.Visible = brickbat;
        ClearPlayerShots();

        if (showScout)
            SnapPlayerToStart();

        RefreshCockpitStatus();
        UpdateCockpitToolkitPanels();
        UpdateCursorMode();
    }

    private static string PlaysetModeLabel(PlaysetMode mode)
    {
        return mode switch
        {
            PlaysetMode.Brickbat => "Brickbat",
            PlaysetMode.Pinball => "Pinball",
            PlaysetMode.Overhead => "Overhead",
            _ => "Platformer"
        };
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
        bool shootPressed = Input.IsActionJustPressed("dack_shoot");
        bool upHeld = Input.IsActionPressed("dack_up");
        bool downHeld = Input.IsActionPressed("dack_down");
        Rect2 actorBounds = new(_playerPosition, _player.Size);
        bool onLadder = _playfield.IsTouchingLadder(actorBounds);
        Vector2 slideVelocity = _playfield.GetSlideVelocity(actorBounds);

        float maxSpeed = motionUnit * 16f;
        float acceleration = motionUnit * 70f;
        float friction = motionUnit * 78f;
        float gravity = motionUnit * 58f * _gravityScale;
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

        if (slideVelocity != Vector2.Zero)
        {
            _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, slideVelocity.X, Mathf.Abs(slideVelocity.X) * 4f * dt + motionUnit * 12f * dt);
            if (slideVelocity.Y > 0)
                _playerVelocity.Y += slideVelocity.Y * 0.18f * dt;
        }

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
            ClearPlayerShots();
            RefreshMotionText();
            return;
        }

        _playerPosition = next;
        _player.Position = _playerPosition;
        if (shootPressed)
            FirePlayerShot();

        UpdatePlayerShots(dt);
        UpdatePlayerAnimation(inputX, crawlingText);
        _player.QueueRedraw();
        RefreshMotionText();
    }

    private void FirePlayerShot()
    {
        const int maxPlayerShots = 4;
        if (_playerShots.Count >= maxPlayerShots)
            return;

        Vector2 direction = _player.FacingRight ? Vector2.Right : Vector2.Left;
        Vector2 origin = _playerPosition + new Vector2(
            _player.FacingRight ? _player.Size.X + 2f : -2f,
            _player.Size.Y * 0.42f
        );
        float shotSpeed = Mathf.Max(_textUnitPixels * 44f, 300f);

        _playerShots.Add(new PlayerShot(origin, direction * shotSpeed, 1.35f));
        PushShotPositionsToPlayfield();
    }

    private void UpdatePlayerShots(float dt)
    {
        if (_playerShots.Count == 0)
            return;

        Rect2 playBounds = _playfield.PlayBounds.Grow(12f);
        for (int i = _playerShots.Count - 1; i >= 0; i--)
        {
            PlayerShot shot = _playerShots[i];
            shot.Position += shot.Velocity * dt;
            shot.Life -= dt;
            QueueProjectileOcrTarget(shot);

            Rect2 shotBounds = new(shot.Position - new Vector2(4f, 4f), new Vector2(8f, 8f));
            if (shot.Life <= 0f || !playBounds.HasPoint(shot.Position) || TryHitTextObject(shotBounds))
                _playerShots.RemoveAt(i);
            else
                _playerShots[i] = shot;
        }

        PushShotPositionsToPlayfield();
    }

    private void QueueProjectileOcrTarget(PlayerShot shot)
    {
        if (!_playfield.HasCapturedPage || shot.Velocity.LengthSquared() <= 0.01f)
            return;

        Vector2 direction = shot.Velocity.Normalized();
        Rect2? bestWord = null;
        float bestScore = float.PositiveInfinity;

        foreach (Rect2 word in _playfield.GetTextObjectRegions(TextObjectGranularity.Word))
        {
            if (_playfield.Ocr.TryGetLabel(word, out _))
                continue;

            Vector2 offset = word.GetCenter() - shot.Position;
            float ahead = offset.Dot(direction);
            if (ahead < 0f || ahead > _textUnitPixels * 42f)
                continue;

            float lateral = Mathf.Abs(direction.Cross(offset));
            float allowedLateral = Mathf.Max(word.Size.Y * 1.8f, _textUnitPixels * 3f);
            if (lateral > allowedLateral)
                continue;

            float score = ahead + lateral * 4f;
            if (score < bestScore)
            {
                bestScore = score;
                bestWord = word;
            }
        }

        if (bestWord is Rect2 target && _playfield.TryCreateOcrSample(target, out Image? sample) && sample is not null)
            _playfield.Ocr.QueueRegion(target, sample);
    }

    private bool TryHitTextObject(Rect2 shotBounds)
    {
        if (!_textDestructionEnabled || !_playfield.HasCapturedPage)
            return false;

        foreach (Rect2 letter in _playfield.GetTextObjectRegions(TextObjectGranularity.Letter))
        {
            if (!shotBounds.Intersects(letter))
                continue;

            _playfield.EraseDocumentText(letter.Grow(1.5f));
            return true;
        }

        return false;
    }

    private void ClearPlayerShots()
    {
        if (_playerShots.Count == 0)
            return;

        _playerShots.Clear();
        PushShotPositionsToPlayfield();
    }

    private void PushShotPositionsToPlayfield()
    {
        Vector2[] positions = new Vector2[_playerShots.Count];
        for (int i = 0; i < _playerShots.Count; i++)
            positions[i] = _playerShots[i].Position;

        _playfield.SetPlayerShotPositions(positions);
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

        if (_textTerrainEnabled)
        {
            foreach (Rect2 word in _playfield.GetTextObjectRegions(TextObjectGranularity.Letter))
                yield return new Rect2(
                    word.Position + new Vector2(0, 2f),
                    new Vector2(word.Size.X, Mathf.Max(2f, Mathf.Min(word.Size.Y, _textUnitPixels * 0.45f)))
                );
        }

        if (_platformerSafetyFloor)
            yield return new Rect2(_playfield.PlayBounds.Position.X, _playfield.PlayBounds.End.Y - 4f, _playfield.PlayBounds.Size.X, 4f);
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
        Input.MouseMode = _bossMode || _sidebar.Visible || (_cockpit is not null && _cockpit.Visible) || IsPlaysetToolbarExpanded()
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
        Vector2? editorStart = _playfield.GetEditorStartPosition(_player.Size);
        if (editorStart is not null)
            _playerPosition = editorStart.Value;
        else if (_platformerMode == PlatformerMode.Vertical)
            _playerPosition = new Vector2(_playfield.Size.X * 0.21f, _playfield.Size.Y * 0.72f - _player.Size.Y);
        else
            _playerPosition = _playfield.GetSpawnPosition(_player.Size);

        _player.Position = _playerPosition;
        _playerVelocity = Vector2.Zero;
        _playerOnGround = true;
        ClearPlayerShots();
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
        _motionLabel.Text = $"{mode}  |  text unit {_textUnitPixels:0}px  |  actor {_player.Size.Y:0}px tall  |  gravity {_gravityScale:0.00}x  |  {ground}";
    }

    private void UpdateAttributeControls(WorldObject? selected)
    {
        if (_attributeText is null || _speedSlider is null || _thicknessSlider is null || _rangeSlider is null || _opacitySlider is null || _gravitySlider is null || _tintPicker is null || _customTintCheck is null)
            return;

        _updatingAttributeControls = true;
        _gravitySlider.Value = _gravityScale;
        if (selected is null)
        {
            _speedSlider.Editable = false;
            _thicknessSlider.Editable = false;
            _rangeSlider.Editable = false;
            _opacitySlider.Editable = false;
            _tintPicker.Disabled = true;
            _customTintCheck.Disabled = true;
            _customTintCheck.ButtonPressed = false;
            _speedSlider.Value = 0;
            _thicknessSlider.Value = 0.8;
            _rangeSlider.Value = 5;
            _opacitySlider.Value = 1;
            _tintPicker.Color = new Color("#5CB8A7");
            _attributeText.Text =
                "No placed object selected.\n\n"
                + $"Gravity: {_gravityScale:0.00}x\n"
                + $"Text terrain: {(_textTerrainEnabled ? "on" : "off")}\n"
                + $"Text crawl: {(_playfield.TextCrawlEnabled ? "on" : "off")}\n"
                + $"Shot text damage: {(_textDestructionEnabled ? "on" : "off")}";
            _updatingAttributeControls = false;
            return;
        }

        _speedSlider.Editable = true;
        _thicknessSlider.Editable = true;
        _rangeSlider.Editable = selected.Kind == WorldObjectKind.Elevator;
        _opacitySlider.Editable = true;
        _tintPicker.Disabled = false;
        _customTintCheck.Disabled = false;
        _customTintCheck.ButtonPressed = selected.UseCustomTint;
        _speedSlider.Value = selected.SpeedUnits;
        _thicknessSlider.Value = selected.ThicknessUnits;
        _rangeSlider.Value = selected.RangeUnits;
        _opacitySlider.Value = selected.Opacity;
        _tintPicker.Color = selected.UseCustomTint ? selected.Tint : DefaultWorldObjectColor(selected.Kind);
        _attributeText.Text =
            $"{selected.Kind}\n"
            + $"Speed/force: {selected.SpeedUnits:0.0}\n"
            + $"Thickness: {selected.ThicknessUnits:0.0}\n"
            + $"Range: {selected.RangeUnits:0.0} text units\n"
            + $"Opacity: {selected.Opacity * 100f:0}%\n"
            + $"Color: {(selected.UseCustomTint ? "custom" : "default")}\n"
            + $"Gravity: {_gravityScale:0.00}x\n"
            + (selected.IsEditorOnly ? "Editor-only: visible while building, hidden during play.\n" : "")
            + "Reverse flips conveyors by speed; other line tools swap A/B endpoints.";
        _updatingAttributeControls = false;
    }

    private static void EnsureInputActions()
    {
        EnsureAction("dack_left", Key.A, Key.Left);
        EnsureAction("dack_right", Key.D, Key.Right);
        EnsureAction("dack_up", Key.W, Key.Up);
        EnsureAction("dack_down", Key.S, Key.Down);
        EnsureAction("dack_jump", Key.Space, Key.W, Key.Up);
        EnsureAction("dack_shoot", Key.J, Key.X);
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

    private Button ShelfButton(string text, WorldObjectKind kind, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Platformer);
            _playfield.AddPlacedObject(kind);
            _inspectorText.Text = $"{text} placed.\n\n{description}\n\nDrag either A/B endpoint handle on the playfield to move, scale, or angle it.";
            RefreshCockpitStatus();
        };
        return button;
    }

    private static HSlider AttributeSlider(double min, double max, double value, double step)
    {
        HSlider slider = new()
        {
            MinValue = min,
            MaxValue = max,
            Value = value,
            Step = step,
            CustomMinimumSize = new Vector2(0, 26),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        slider.FocusMode = FocusModeEnum.None;
        return slider;
    }

    private static Color DefaultWorldObjectColor(WorldObjectKind kind)
    {
        return kind switch
        {
            WorldObjectKind.Ladder => new Color("#8A5A37"),
            WorldObjectKind.Ramp => new Color("#5CB8A7"),
            WorldObjectKind.Slide => new Color("#FF5C35"),
            WorldObjectKind.Conveyor => new Color("#4378B8"),
            WorldObjectKind.Elevator => new Color("#F4C95D"),
            WorldObjectKind.StartPoint => new Color("#B56CFF"),
            WorldObjectKind.HiddenSwitch => new Color("#FF2BD6"),
            WorldObjectKind.Checkpoint => new Color("#5CB8A7"),
            _ => new Color("#5CB8A7")
        };
    }

    private static PanelContainer CockpitPanel(float width)
    {
        PanelContainer panel = new()
        {
            CustomMinimumSize = new Vector2(width, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride("panel", FlatStyle("#F7F5EF", 8));
        return panel;
    }

    private static VBoxContainer PanelVBox(PanelContainer panel)
    {
        MarginContainer margin = Margins(12, 12, 12, 12);
        panel.AddChild(margin);
        VBoxContainer box = new();
        box.AddThemeConstantOverride("separation", 8);
        margin.AddChild(box);
        return box;
    }

    private static Label CockpitHeading(string text)
    {
        Label label = Heading(text);
        label.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        return label;
    }

    private static Label CockpitNote(string text)
    {
        Label label = new()
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeColorOverride("font_color", new Color("#52606D"));
        label.AddThemeFontSizeOverride("font_size", 12);
        return label;
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

    private struct PlayerShot(Vector2 position, Vector2 velocity, float life)
    {
        public Vector2 Position = position;
        public Vector2 Velocity = velocity;
        public float Life = life;
    }
}
