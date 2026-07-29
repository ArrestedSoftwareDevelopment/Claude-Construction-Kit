using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace Dack;

public partial class Main : Control
{
    private readonly List<ActorView> _actors = [];
    private readonly List<PlayerShot> _playerShots = [];
    private readonly List<EnemyShot> _enemyShots = [];
    private readonly List<ImpactEffect> _impactEffects = [];
    private readonly Dictionary<ActorView, float> _enemyShotTimers = [];
    private readonly Dictionary<ActorView, Vector2> _enemyVelocities = [];
    private readonly Dictionary<ActorView, float> _enemyPatrolDirections = [];
    private readonly Dictionary<ActorView, int> _enemyHealth = [];
    private readonly HashSet<ActorView> _defeatedEnemies = [];
    private readonly Dictionary<string, AudioStreamPlayer> _soundPlayers = [];
    private readonly Vector2[] _actorAnchors =
    [
        new(0.16f, 0.66f),
        new(0.43f, 0.34f),
        new(0.70f, 0.62f)
    ];

    private Control _workspace = null!;
    private Control _bossOverlay = null!;
    private PlayfieldSurface _playfield = null!;
    private CombatFxOverlay _combatFxOverlay = null!;
    private SpritePad _spritePad = null!;
    private Label _selectionLabel = null!;
    private LineEdit _characterNameEdit = null!;
    private Label _bindingLabel = null!;
    private Label _toolLabel = null!;
    private Label _motionLabel = null!;
    private PanelContainer _platformerHud = null!;
    private Label _platformerHudText = null!;
    private bool _platformerHudDragging;
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
    private readonly List<Button> _editorPlayButtons = [];
    private Label _cockpitStatus = null!;
    private Label _inspectorText = null!;
    private Label _attributeText = null!;
    private HSlider _speedSlider = null!;
    private HSlider _thicknessSlider = null!;
    private HSlider _rangeSlider = null!;
    private HSlider _opacitySlider = null!;
    private HSlider _gravitySlider = null!;
    private AnimationStripPreview _tgcStripPreview = null!;
    private SpinBox _tgcNumberBase = null!;
    private VBoxContainer _tgcClipRows = null!;
    private readonly List<TgcClipRow> _tgcClipRowModels = [];
    private string _animationEditorName = "TGC Player";
    private string _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Player_DarkOutline.png";
    private string _animationEditorSourceKind = "raw-local-evaluation";
    private string _animationEditorSourceId = "tgc-player";
    private string _animationEditorFolder = "game-creators-pack-graphics-prep";
    private string _animationEditorFileName = "tgc-player.dackanim.json";
    private int _animationEditorFrameCount;
    private bool _syncingClipUnavailable;
    private bool _syncingCharacterName;
    private readonly string[] _tgcPresetLabels =
    [
        "Idle",
        "Run Shoot",
        "Jump Shoot",
        "Fall",
        "Climb Up",
        "Climb Down",
        "Dig Up",
        "Dig Down",
        "Shoot Up",
        "Shoot Down",
        "Bounce",
        "Turn",
        "Land",
        "Climb",
        "Dig",
        "Shoot",
        "Hurt",
        "Death",
        "Special"
    ];
    private ColorPickerButton _tintPicker = null!;
    private CheckBox _customTintCheck = null!;
    private BrickbatOverlay _brickbatOverlay = null!;
    private PinballOverlay _pinballOverlay = null!;
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
    private bool _gunEnabled = true;
    private bool _editorMode = true;
    private bool _enemyAiEnabled = true;
    private bool _enemyTracksPlayer = true;
    private bool _enemyProjectilesEnabled = true;
    private bool _explosionsDamageText = true;
    private bool _partialDamageEnabled = true;
    private bool _soundEnabled = true;
    private bool _updatingAttributeControls;
    private bool _goalReached;
    private bool _resumePlayWhenCockpitCloses;
    private double _deathTestSeconds;
    private double _shootAnimSeconds;
    private double _contactInvulnerabilitySeconds;
    private double _hazardArmDelaySeconds;
    private int _platformerScore;
    private int _platformerLives = 3;
    private int _playerHealth = 3;
    private int _playerMaxHealth = 3;
    private int _enemyShotDamage = 1;
    private int _playerShotPower = 1;
    private int _platformerDeaths;
    private string _platformerStatus = "READY";
    private float _actorSizeMultiplier = 2f;
    private float _minLandingSupportRatio = 0.45f;
    private float _fallDeathHeightUnits = 18f;
    private float _enemyShotRangeUnits = 34f;
    private float _lastGroundY;
    private PlatformerMode _platformerMode = PlatformerMode.Horizontal;
    private PlaysetMode _playsetMode = PlaysetMode.Platformer;
    private float _gravityScale = 1f;
    private float _textUnitPixels = 7f;

    public override void _Ready()
    {
        EnsureInputActions();
        BuildInterface();
        CreateActors();
        SyncEditorModeToScene();
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
        {
            FitCockpitToViewport();
            RefreshCockpitStatus();
        }

        UpdatePlayer(delta);
        UpdateOverheadPlayer(delta);
        UpdateActorPresentation((float)delta);

        if (!_editorMode)
        {
            UpdateEnemies((float)delta);
            UpdateEnemyShots((float)delta);
        }

        UpdateImpactEffects((float)delta);
        RefreshPlatformerHud();
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
        BuildAudioDeck();
        BuildPlaysetToolbar();
        BuildCockpit();
        BuildPlatformerHud();

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

        _characterNameEdit = new LineEdit
        {
            PlaceholderText = "Character name",
            CustomMinimumSize = new Vector2(0, 32),
            SelectAllOnFocus = true
        };
        _characterNameEdit.TextChanged += RenameSelectedCharacter;
        side.AddChild(_characterNameEdit);

        _bindingLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _bindingLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _bindingLabel.AddThemeFontSizeOverride("font_size", 13);
        side.AddChild(_bindingLabel);

        side.AddChild(Heading("CHARACTER PICKER"));

        HBoxContainer characterPicker = new();
        side.AddChild(characterPicker);

        Button stickman = Button("STICKMAN");
        stickman.TooltipText = "Use the current OctoPyte stick figure animation set.";
        stickman.Pressed += () =>
        {
            LoadStickmanEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(stickman);

        Button gameCreatorPlayer = Button("TGC PLAYER");
        gameCreatorPlayer.TooltipText = "Use The Game Creator's Pack player strip via local blob-detected frames.";
        gameCreatorPlayer.Pressed += () =>
        {
            LoadTgcEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(gameCreatorPlayer);

        Button sunnyDragon = Button("SUNNY DRAGON");
        sunnyDragon.TooltipText = "Add the Legacy Collection Sunny Dragon fly strip as the first animated enemy.";
        sunnyDragon.Pressed += () =>
        {
            LoadSunnyDragonEditorDefaults();
            SetEnemyCharacter("Sunny Dragon", SpriteAnimationSet.TryLoadSunnyDragon(), "Sunny Dragon added as the first animated enemy. Its fly strip is a 9-frame grid source that can be renamed, labeled, saved, and loaded.");
        };
        characterPicker.AddChild(sunnyDragon);

        Button tgcOrange = Button("ORANGE WORKER");
        tgcOrange.TooltipText = "Add the TGC Orange Worker as an enemy.";
        tgcOrange.Pressed += () => AddTgcEnemy(
            "Orange Worker",
            SpriteAnimationSet.TryLoadTgcOrangeWorker(),
            "TGC Orange Worker added as an enemy. Good for simple ground patrol/blocker roles."
        );
        characterPicker.AddChild(tgcOrange);

        Button tgcRed = Button("RED RUNNER");
        tgcRed.TooltipText = "Add the TGC Red Runner as an enemy.";
        tgcRed.Pressed += () => AddTgcEnemy(
            "Red Runner",
            SpriteAnimationSet.TryLoadTgcRedRunner(),
            "TGC Red Runner added as an enemy. Good for faster patrol/chase roles."
        );
        characterPicker.AddChild(tgcRed);

        Button tgcBlue = Button("BLUE GUARD");
        tgcBlue.TooltipText = "Add the TGC Blue Guard as an enemy.";
        tgcBlue.Pressed += () => AddTgcEnemy(
            "Blue Guard",
            SpriteAnimationSet.TryLoadTgcBlueGuard(),
            "TGC Blue Guard added as an enemy. Good for patrol/guard roles."
        );
        characterPicker.AddChild(tgcBlue);

        Button tgcGreen = Button("GREEN CRAWLER");
        tgcGreen.TooltipText = "Add the TGC Green Crawler as an enemy.";
        tgcGreen.Pressed += () => AddTgcEnemy(
            "Green Crawler",
            SpriteAnimationSet.TryLoadTgcGreenCrawler(),
            "TGC Green Crawler added as an enemy. Good for crawling/slime/insect-style hazards."
        );
        characterPicker.AddChild(tgcGreen);

        Button tgcBoss = Button("SHOOTER BOSS");
        tgcBoss.TooltipText = "Add the TGC Shooter Boss as a static/large enemy.";
        tgcBoss.Pressed += () => AddTgcEnemy(
            "Shooter Boss",
            SpriteAnimationSet.TryLoadTgcShooterBoss(),
            "TGC Shooter Boss added as an enemy/boss/pinball toy candidate."
        );
        characterPicker.AddChild(tgcBoss);

        Button tgcFleet = Button("SHOOTER FLEET");
        tgcFleet.TooltipText = "Add the TGC Shooter Fleet sheet as an enemy.";
        tgcFleet.Pressed += () => AddTgcEnemy(
            "Shooter Fleet",
            SpriteAnimationSet.TryLoadTgcShooterFleet(),
            "TGC Shooter Fleet added as an enemy. This is a rough first atlas import; exact per-ship slicing comes later."
        );
        characterPicker.AddChild(tgcFleet);

        Button battleShip = Button("BATTLE SHIP");
        battleShip.TooltipText = "Use the Legacy top-down shooter ship as the Overhead/space player.";
        battleShip.Pressed += () => SetPlayerCharacter(
            "Battle Ship 01",
            SpriteAnimationSet.TryLoadBattleFleetRedShip01(),
            "Battle Ship 01 loaded as the Overhead player. Its five frames are heading bins, not walk frames: movement chooses the visible ship direction.",
            "battle-fleet-red-ship-01"
        );
        characterPicker.AddChild(battleShip);

        side.AddChild(Heading("ANIMATION FRAME EDITOR"));
        side.AddChild(CockpitNote("Name actions, edit start/end frame numbers, and add labels. The preview highlights every labeled range."));
        _tgcStripPreview = new AnimationStripPreview
        {
            Columns = 8,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        side.AddChild(_tgcStripPreview);

        HBoxContainer numbering = new();
        numbering.AddThemeConstantOverride("separation", 6);
        Label numberLabel = new()
        {
            Text = "Number from",
            CustomMinimumSize = new Vector2(92, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        numberLabel.AddThemeColorOverride("font_color", new Color("#202A34"));
        numberLabel.AddThemeFontSizeOverride("font_size", 12);
        numbering.AddChild(numberLabel);
        _tgcNumberBase = ClipEndpointSpin(0, 999);
        _tgcNumberBase.ValueChanged += _ => UpdateTgcStripPreview();
        numbering.AddChild(_tgcNumberBase);
        side.AddChild(numbering);

        _tgcClipRows = new VBoxContainer();
        _tgcClipRows.AddThemeConstantOverride("separation", 4);
        side.AddChild(_tgcClipRows);
        LoadTgcEditorDefaults();

        Button addPreset = Button("ADD PRESET LABEL");
        addPreset.Pressed += () =>
        {
            string label = NextMissingPresetLabel();
            int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
            int frame = Mathf.Clamp(_tgcClipRowModels.Count * 2, 0, maxFrame);
            AddTgcClipRow(label, frame, frame, maxFrame);
            UpdateTgcStripPreview();
            _inspectorText.Text = $"{label} animation label added from the preset vocabulary. Edit its frame numbers to match the strip.";
        };
        side.AddChild(addPreset);

        Button addLabel = Button("ADD LABEL");
        addLabel.Pressed += () =>
        {
            int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
            int frame = Mathf.Clamp(_tgcClipRowModels.Count * 2, 0, maxFrame);
            AddTgcClipRow($"Action {_tgcClipRowModels.Count + 1}", frame, frame, maxFrame);
            UpdateTgcStripPreview();
            _inspectorText.Text = "New animation label added. Rename it, then edit its start/end frame numbers.";
        };
        side.AddChild(addLabel);

        Button applyTgcClips = Button("APPLY ANIM LABELS");
        applyTgcClips.Pressed += ApplyTgcClipRanges;
        side.AddChild(applyTgcClips);

        Button reloadDefaultAnim = Button("RELOAD DEFAULT ANIM");
        reloadDefaultAnim.Pressed += ReloadSelectedAnimationDefaults;
        side.AddChild(reloadDefaultAnim);

        Button testDeath = Button("TEST DEATH");
        testDeath.Pressed += TriggerDeathAnimation;
        side.AddChild(testDeath);

        Button loadTgcClips = Button("LOAD ANIM LABELS");
        loadTgcClips.Pressed += LoadAnimationClipLabels;
        side.AddChild(loadTgcClips);

        Button saveTgcClips = Button("SAVE ANIM LABELS");
        saveTgcClips.Pressed += SaveTgcClipLabels;
        side.AddChild(saveTgcClips);

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

        HBoxContainer actorSizeRow = new();
        actorSizeRow.AddThemeConstantOverride("separation", 6);
        Button halfSize = Button("ACTOR 1/2x");
        halfSize.Pressed += () => SetActorSizeMultiplier(0.5f);
        actorSizeRow.AddChild(halfSize);
        Button normalSize = Button("ACTOR 1x");
        normalSize.Pressed += () => SetActorSizeMultiplier(1f);
        actorSizeRow.AddChild(normalSize);
        Button doubleSize = Button("ACTOR 2x");
        doubleSize.Pressed += () => SetActorSizeMultiplier(2f);
        actorSizeRow.AddChild(doubleSize);
        side.AddChild(actorSizeRow);

        side.AddChild(CockpitNote("Gap/fall tuning: minimum support decides how much of the actor must overlap a surface. Higher values make narrow holes easier to fall through."));
        HSlider supportSlider = AttributeSlider(0.15, 0.95, _minLandingSupportRatio, 0.05);
        supportSlider.ValueChanged += value =>
        {
            _minLandingSupportRatio = (float)value;
            RefreshMotionText();
        };
        side.AddChild(supportSlider);

        HSlider fallDeath = AttributeSlider(4, 40, _fallDeathHeightUnits, 1);
        fallDeath.ValueChanged += value =>
        {
            _fallDeathHeightUnits = (float)value;
            RefreshMotionText();
        };
        side.AddChild(fallDeath);

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

    private void BuildAudioDeck()
    {
        AddSound("player-shot", "res://assets/project/sounds/player-shot.ogg", -11f);
        AddSound("enemy-hit", "res://assets/project/sounds/enemy-hit.ogg", -8f);
        AddSound("enemy-defeat", "res://assets/project/sounds/enemy-defeat.ogg", -7f);
        AddSound("player-hurt", "res://assets/project/sounds/player-hurt.ogg", -7f);
        AddSound("power-up", "res://assets/project/sounds/power-up.ogg", -8f);
        AddSound("brickbat-paddle", "res://assets/project/sounds/brickbat-paddle.ogg", -13f);
        AddSound("brickbat-text-hit", "res://assets/project/sounds/brickbat-text-hit.ogg", -11f);
        AddSound("brickbat-word-break", "res://assets/project/sounds/brickbat-word-break.ogg", -10f);
        AddSound("brickbat-laser", "res://assets/project/sounds/brickbat-laser.ogg", -9f);
        AddSound("brickbat-ball-lost", "res://assets/project/sounds/brickbat-ball-lost.ogg", -9f);
    }

    private void AddSound(string key, string resourcePath, float volumeDb)
    {
        AudioStream? stream = GD.Load<AudioStream>(resourcePath);
        if (stream is null)
            return;

        AudioStreamPlayer player = new()
        {
            Stream = stream,
            VolumeDb = volumeDb,
            Bus = "Master"
        };
        _soundPlayers[key] = player;
        AddChild(player);
    }

    private void PlaySound(string key)
    {
        if (!_soundEnabled || !_soundPlayers.TryGetValue(key, out AudioStreamPlayer? player))
            return;

        if (player.Playing)
            player.Stop();
        player.Play();
    }

    private void BuildCockpit()
    {
        _cockpit = new PanelContainer
        {
            Position = new Vector2(24, 24),
            Size = new Vector2(1160, 520),
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

        ScrollContainer columnScroll = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        root.AddChild(columnScroll);

        HBoxContainer columns = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        columns.AddThemeConstantOverride("separation", 12);
        columnScroll.AddChild(columns);

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
        FitCockpitToViewport();
        UpdateCockpitToolkitPanels();
    }

    private void BuildPlatformerHud()
    {
        _platformerHud = new PanelContainer
        {
            Position = new Vector2(18, 78),
            MouseFilter = MouseFilterEnum.Stop
        };
        _platformerHud.AddThemeStyleboxOverride("panel", FlatStyle("#202A34", 8));
        _platformerHud.GuiInput += OnPlatformerHudInput;
        _playfield.AddChild(_platformerHud);

        MarginContainer margin = Margins(10, 8, 10, 8);
        _platformerHud.AddChild(margin);
        _platformerHudText = new Label
        {
            Text = "",
            CustomMinimumSize = new Vector2(210, 46)
        };
        _platformerHudText.AddThemeColorOverride("font_color", new Color("#F7F5EF"));
        _platformerHudText.AddThemeFontSizeOverride("font_size", 13);
        margin.AddChild(_platformerHudText);
        RefreshPlatformerHud("READY");
    }

    private void OnPlatformerHudInput(InputEvent inputEvent)
    {
        if (!_editorMode)
            return;

        if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                _platformerHudDragging = true;
            }
            else
            {
                _platformerHudDragging = false;
            }
        }
        else if (inputEvent is InputEventMouseMotion motion && _platformerHudDragging)
        {
            Vector2 desired = _platformerHud.Position + motion.Relative;
            Vector2 max = new(
                Mathf.Max(0, _playfield.Size.X - _platformerHud.Size.X),
                Mathf.Max(0, _playfield.Size.Y - _platformerHud.Size.Y)
            );
            _platformerHud.Position = new Vector2(
                Mathf.Clamp(desired.X, 0, max.X),
                Mathf.Clamp(desired.Y, 0, max.Y)
            );
        }
    }

    private Control BuildShelfPanel()
    {
        PanelContainer panel = CockpitPanel(260);
        VBoxContainer shelf = PanelVBox(panel);
        shelf.AddChild(CockpitHeading("PLATFORMER"));
        AddGameTypeSessionBlock(shelf, PlaysetMode.Platformer, "ENTER PLATFORMER");

        shelf.AddChild(CockpitHeading("BUILD TOOLS"));
        shelf.AddChild(ButtonRow(
            ShelfButton("Ladder", WorldObjectKind.Ladder, "Vertical climb volume. Drag A/B to set height; thickness roughly matches player width."),
            ShelfButton("Ramp", WorldObjectKind.Ramp, "Static angled standable line for paragraph slants / Donkey Kong feel.")));
        shelf.AddChild(ButtonRow(
            ShelfButton("Slide", WorldObjectKind.Slide, "Downhill acceleration surface. Slides always push toward the lower endpoint."),
            ShelfButton("Conveyor", WorldObjectKind.Conveyor, "Powered belt/line surface with intentionally strong force; rotate it for angled belts.")));
        shelf.AddChild(ShelfButton("Elevator", WorldObjectKind.Elevator, "Moving platform proof; later gets visible endpoints and timing."));

        shelf.AddChild(CockpitHeading("ROUTE / LOGIC"));
        shelf.AddChild(ButtonRow(
            ShelfButton("Start", WorldObjectKind.StartPoint, "Editor-only spawn marker. Visible while building, hidden during play."),
            ShelfButton("Checkpoint", WorldObjectKind.Checkpoint, "Visible marker now; spawn binding comes next.")));
        shelf.AddChild(ButtonRow(
            ShelfButton("Goal", WorldObjectKind.GoalPoint, "Visible level objective marker: the first complete test level spine is Start -> Midpoint -> Goal."),
            ShelfButton("Hidden Switch", WorldObjectKind.HiddenSwitch, "Invisible gameplay logic: visible in editor, hidden from the player.")));

        shelf.AddChild(CockpitHeading("PLAYER RULES"));
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

        Button gun = Button(_gunEnabled ? "Gun: On" : "Gun: Off");
        gun.Pressed += () =>
        {
            _gunEnabled = !_gunEnabled;
            gun.Text = _gunEnabled ? "Gun: On" : "Gun: Off";
            ClearPlayerShots();
            _inspectorText.Text = _gunEnabled
                ? "Gun enabled. Platformer can use Run Shoot / Jump Shoot labels and fire projectiles."
                : "Gun disabled. Platformer becomes a jump/climb/dig style game; shoot input is ignored.";
        };
        shelf.AddChild(ButtonRow(floor, gun));

        shelf.AddChild(CockpitHeading("ENEMY RULES"));
        Button enemyAi = Button(_enemyAiEnabled ? "Enemy AI: On" : "Enemy AI: Off");
        enemyAi.Pressed += () =>
        {
            _enemyAiEnabled = !_enemyAiEnabled;
            enemyAi.Text = _enemyAiEnabled ? "Enemy AI: On" : "Enemy AI: Off";
            _inspectorText.Text = _enemyAiEnabled
                ? "Enemy AI enabled. Enemies patrol/hover, collide, and can block the route."
                : "Enemy AI disabled. Enemies stay placed for editing.";
        };

        Button enemyTrack = Button(_enemyTracksPlayer ? "Enemy Track: On" : "Enemy Track: Off");
        enemyTrack.Pressed += () =>
        {
            _enemyTracksPlayer = !_enemyTracksPlayer;
            enemyTrack.Text = _enemyTracksPlayer ? "Enemy Track: On" : "Enemy Track: Off";
            _inspectorText.Text = _enemyTracksPlayer
                ? "Enemy tracking enabled. Enemies bias their patrol/facing toward the player and face the player when firing."
                : "Enemy tracking disabled. Enemies keep patrol/guard motion and fire from their current facing.";
        };

        Button enemyShots = Button(_enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off");
        enemyShots.Pressed += () =>
        {
            _enemyProjectilesEnabled = !_enemyProjectilesEnabled;
            enemyShots.Text = _enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off";
            ClearEnemyShots();
            _inspectorText.Text = _enemyProjectilesEnabled
                ? "Enemy projectile ability enabled. Sunny Dragon fires simple aimed shots on a cooldown."
                : "Enemy projectile ability disabled. Contact danger remains if Enemy AI is on.";
        };

        Button enemyRange = Button(EnemyRangeButtonText());
        enemyRange.Pressed += () =>
        {
            _enemyShotRangeUnits = _enemyShotRangeUnits < 28f ? 34f : _enemyShotRangeUnits < 45f ? 55f : 18f;
            enemyRange.Text = EnemyRangeButtonText();
            ClearEnemyShots();
            _inspectorText.Text = $"Enemy shot range set to {_enemyShotRangeUnits:0} text units. Enemies will not fire until the player is inside that threat radius.";
        };

        Button damageModel = Button(_partialDamageEnabled ? "Damage: Hearts" : "Damage: Instant");
        damageModel.Pressed += () =>
        {
            _partialDamageEnabled = !_partialDamageEnabled;
            damageModel.Text = _partialDamageEnabled ? "Damage: Hearts" : "Damage: Instant";
            _playerHealth = _playerMaxHealth;
            RefreshPlatformerHud();
            _inspectorText.Text = _partialDamageEnabled
                ? "Partial damage enabled. Enemy shots remove health before costing a life."
                : "Instant damage enabled. Enemy shots kill like old-school hazards.";
        };
        shelf.AddChild(ButtonRow(enemyAi, enemyTrack));
        shelf.AddChild(ButtonRow(enemyShots, enemyRange));
        shelf.AddChild(damageModel);

        shelf.AddChild(CockpitHeading("TEXT RULES"));
        Button textTerrain = Button(_textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off");
        textTerrain.Pressed += () =>
        {
            _textTerrainEnabled = !_textTerrainEnabled;
            textTerrain.Text = _textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off";
            _inspectorText.Text = _textTerrainEnabled
                ? "Player text terrain enabled. Captured letters/words can support the scout."
                : "Player text terrain disabled. Only explicit platforms/ramps/elevators/conveyors/floor support the scout.";
        };

        Button textCrawl = Button(_playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off");
        textCrawl.Pressed += () =>
        {
            _playfield.TextCrawlEnabled = !_playfield.TextCrawlEnabled;
            textCrawl.Text = _playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off";
            _inspectorText.Text = _playfield.TextCrawlEnabled
                ? "Player text crawl enabled. Dense single-spaced text can act like climb/crawl surface in Climber mode."
                : "Player text crawl disabled. Use explicit ladders or other tools for climbing.";
        };

        Button textDestruction = Button(_textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off");
        textDestruction.Pressed += () =>
        {
            _textDestructionEnabled = !_textDestructionEnabled;
            textDestruction.Text = _textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off";
            _inspectorText.Text = _textDestructionEnabled
                ? "Platformer shots can erase captured text in the working clone."
                : "Platformer shots no longer damage text; useful for no-destruction traversal tests.";
        };
        shelf.AddChild(ButtonRow(textTerrain, textCrawl));
        shelf.AddChild(textDestruction);

        shelf.AddChild(CockpitHeading("RESET"));
        Button clear = Button("CLEAR PLACED PARTS");
        clear.Pressed += () =>
        {
            _playfield.ClearPlacedObjects();
            SyncEditorModeToScene();
            _inspectorText.Text = "Placed toolkit parts cleared. Captured document pixels and Brickbat mutations remain separate.";
        };
        shelf.AddChild(clear);

        return panel;
    }

    private Control BuildBrickbatPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer brickbat = PanelVBox(panel);
        brickbat.AddChild(CockpitHeading("BRICKBAT PAGE"));
        AddGameTypeSessionBlock(brickbat, PlaysetMode.Brickbat, "ENTER BRICKBAT");

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
                : "Brickbat text collision set to Pierce. The ball erases and scores text but keeps traveling through it.";
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

        return panel;
    }

    private Control BuildPinballPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer pinball = PanelVBox(panel);
        pinball.AddChild(CockpitHeading("PINBALL PAGE"));
        AddGameTypeSessionBlock(pinball, PlaysetMode.Pinball, "ENTER PINBALL");

        pinball.AddChild(CockpitHeading("FIRST PARTS"));
        pinball.AddChild(PinballShelfButton("Add Flipper", WorldObjectKind.PinballFlipper, "Pivot-to-tip flipper placeholder. A/B handles define pivot, length, and resting angle."));
        pinball.AddChild(PinballShelfButton("Add Bumper", WorldObjectKind.PinballBumper, "Circular pop bumper placeholder. Drag A/B to place and scale radius."));
        pinball.AddChild(PinballShelfButton("Add Plunger", WorldObjectKind.PinballPlunger, "Launch lane/plunger placeholder. A/B handles define lane and launch direction."));
        pinball.AddChild(PinballShelfButton("Add Drain", WorldObjectKind.PinballDrain, "Drain/outlane placeholder. A/B handles set drain width."));
        pinball.AddChild(PinballShelfButton("Add Rollover", WorldObjectKind.PinballRollover, "Small scoring/lit insert strip. A/B handles set position and width."));
        pinball.AddChild(PinballShelfButton("Add Gate", WorldObjectKind.PinballGate, "One-way gate placeholder. A/B direction points toward allowed travel."));
        return panel;
    }

    private Control BuildOverheadPanel()
    {
        PanelContainer panel = CockpitPanel(250);
        VBoxContainer overhead = PanelVBox(panel);
        overhead.AddChild(CockpitHeading("OVERHEAD PAGE"));
        AddGameTypeSessionBlock(overhead, PlaysetMode.Overhead, "ENTER OVERHEAD");
        return panel;
    }

    private void AddGameTypeSessionBlock(VBoxContainer page, PlaysetMode mode, string enterText)
    {
        page.AddChild(CockpitHeading("SESSION"));

        Button enter = Button(enterText);
        enter.Pressed += () =>
        {
            SetPlaysetMode(mode);
            _inspectorText.Text = $"{PlaysetModeLabel(mode)} toolkit selected.";
        };

        Button play = Button(_editorMode ? "Enter Play Mode" : "Return to Editor");
        play.Pressed += () => SetEditorMode(!_editorMode);
        _editorPlayButtons.Add(play);

        page.AddChild(ButtonRow(enter, play));

        Button reset = Button("RESET THIS GAME");
        reset.Pressed += () => ResetCurrentPlayset(mode);
        page.AddChild(reset);

        Button saveLevel = Button("Save Level");
        saveLevel.Pressed += SaveLevel;
        Button loadLevel = Button("Load Level");
        loadLevel.Pressed += LoadLevel;
        page.AddChild(ButtonRow(saveLevel, loadLevel));
    }

    private void ResetCurrentPlayset(PlaysetMode requestedMode)
    {
        SetPlaysetMode(requestedMode);
        if (requestedMode == PlaysetMode.Brickbat)
        {
            _brickbatOverlay.ResetGame();
            _inspectorText.Text = "Brickbat reset. Clone damage returns to the captured/source image for this run.";
        }
        else if (requestedMode == PlaysetMode.Pinball)
        {
            _pinballOverlay.ResetGame();
            _inspectorText.Text = "Pinball reset. Fresh ball served for the current table.";
        }
        else
        {
            SnapPlayerToStart();
            _inspectorText.Text = $"{PlaysetModeLabel(requestedMode)} reset. Start marker/manual spawn is honored.";
        }
    }

    private Control BuildInspectorPanel()
    {
        PanelContainer panel = CockpitPanel(292);
        ScrollContainer scroll = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        panel.AddChild(scroll);
        MarginContainer margin = Margins(12, 12, 12, 12);
        scroll.AddChild(margin);
        VBoxContainer inspector = new();
        inspector.AddThemeConstantOverride("separation", 8);
        inspector.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        margin.AddChild(inspector);
        inspector.AddChild(CockpitHeading("INSPECTOR"));
        _inspectorText = CockpitNote(
            "Select or place a toolkit object.\n\n"
            + "First pass supports placement from the shelf. Next pass should add direct handles: drag endpoints, set origin, bind to words, toggle text/graphic/hybrid, and fork shared assets."
        );
        inspector.AddChild(_inspectorText);

        inspector.AddChild(CockpitHeading("ATTRIBUTES"));
        _attributeText = CockpitNote("Select a placed object to edit speed, direction, thickness, and slope behavior.");
        inspector.AddChild(_attributeText);

        inspector.AddChild(CockpitHeading("ACTOR COMBAT"));
        inspector.AddChild(CockpitNote("Enemy toughness is regular shots to defeat. Gun power subtracts that many shot-points per hit."));
        Button tougher = Button("TOUGHNESS +");
        tougher.Pressed += () => AdjustSelectedEnemyToughness(1);
        Button weaker = Button("TOUGHNESS -");
        weaker.Pressed += () => AdjustSelectedEnemyToughness(-1);
        inspector.AddChild(ButtonRow(weaker, tougher));

        Button shotPower = Button(PlayerShotPowerButtonText());
        shotPower.Pressed += () =>
        {
            _playerShotPower = _playerShotPower >= 4 ? 1 : _playerShotPower + 1;
            shotPower.Text = PlayerShotPowerButtonText();
            _inspectorText.Text = $"Player gun power set to {_playerShotPower}x.\n\nA {_playerShotPower}x hit removes {_playerShotPower} point(s) of enemy shot toughness.";
        };
        inspector.AddChild(shotPower);

        _speedSlider = AttributeSlider(-180, 180, 0, 1);
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

        Button rotateLeft = Button("ROTATE -15°");
        rotateLeft.Pressed += () =>
        {
            _playfield.RotateSelected(-15f);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        Button rotateRight = Button("ROTATE +15°");
        rotateRight.Pressed += () =>
        {
            _playfield.RotateSelected(15f);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(ButtonRow(rotateLeft, rotateRight));

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
        understand.AddChild(CockpitHeading("WORD SENSE"));
        understand.AddChild(CockpitNote(_playfield.Ocr.StatusText));
        understand.AddChild(CockpitHeading("DISPLAY LAYOUT"));
        Button probeDisplays = Button("Probe Monitors");
        probeDisplays.Pressed += ProbeDisplayLayout;
        Button moveDisplay = Button("Move DACK To Next Monitor");
        moveDisplay.Pressed += MoveDackToNextMonitor;
        understand.AddChild(ButtonRow(probeDisplays, moveDisplay));
        return panel;
    }

    private void BuildPlaysetToolbar()
    {
        _brickbatOverlay = new BrickbatOverlay
        {
            Playfield = _playfield,
            Visible = false
        };
        _brickbatOverlay.SoundRequested += PlaySound;
        _brickbatOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _playfield.AddChild(_brickbatOverlay);

        _pinballOverlay = new PinballOverlay
        {
            Playfield = _playfield,
            Visible = false
        };
        _pinballOverlay.SoundRequested += PlaySound;
        _pinballOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _playfield.AddChild(_pinballOverlay);

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
            else if (_playsetMode == PlaysetMode.Pinball)
                _pinballOverlay.ResetGame();
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

        for (int i = 0; i < 8; i++)
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

        _brickbatOverlay.Actors = _actors;

        _combatFxOverlay = new CombatFxOverlay
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        _combatFxOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _playfield.AddChild(_combatFxOverlay);

        _player = _actors[0];
        _player.ActorName = "Playable Scout";
        _player.AnimationSourceId = "stickman-v0.1";
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
        LoadAnimationEditorForActor(actor);
        RefreshBindingText();
    }

    private void RenameSelectedCharacter(string name)
    {
        if (_syncingCharacterName || _selectedActor is null)
            return;

        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return;

        _selectedActor.ActorName = trimmed;
        if (ReferenceEquals(_selectedActor, _player))
            _animationEditorName = trimmed;

        RefreshBindingText();
        _selectedActor.TooltipText = $"Select {trimmed}";
    }

    private void LoadAnimationEditorForActor(ActorView actor)
    {
        string sourceId = string.IsNullOrWhiteSpace(actor.AnimationSourceId)
            ? GuessAnimationSourceId(actor.ActorName, actor.IsPlayable)
            : actor.AnimationSourceId;

        switch (sourceId)
        {
            case "stickman-v0.1":
                LoadStickmanEditorDefaults();
                break;
            case "tgc-player":
                LoadTgcEditorDefaults();
                break;
            case "sunny-dragon-fly":
                LoadSunnyDragonEditorDefaults();
                break;
            case "tgc-orange-worker":
                LoadTgcOrangeWorkerEditorDefaults(actor.ActorName, sourceId);
                break;
            case "tgc-red-runner":
                LoadTgcRedRunnerEditorDefaults(actor.ActorName, sourceId);
                break;
            case "tgc-blue-guard":
                LoadTgcBlueGuardEditorDefaults(actor.ActorName, sourceId);
                break;
            case "tgc-green-crawler":
                LoadTgcGreenCrawlerEditorDefaults(actor.ActorName, sourceId);
                break;
            case "tgc-shooter-boss":
                LoadShooterBossEditorDefaults();
                break;
            case "tgc-shooter-fleet":
                LoadShooterFleetEditorDefaults();
                break;
        }
    }

    private static string GuessAnimationSourceId(string actorName, bool playable)
    {
        string name = actorName.ToLowerInvariant();
        if (name.Contains("sunny") || name.Contains("dragon"))
            return "sunny-dragon-fly";
        if (name.Contains("orange"))
            return "tgc-orange-worker";
        if (name.Contains("red"))
            return "tgc-red-runner";
        if (name.Contains("blue"))
            return "tgc-blue-guard";
        if (name.Contains("green") || name.Contains("crawler"))
            return "tgc-green-crawler";
        if (name.Contains("boss"))
            return "tgc-shooter-boss";
        if (name.Contains("fleet") || name.Contains("ship"))
            return "tgc-shooter-fleet";
        if (name.Contains("battle"))
            return "battle-fleet-red-ship-01";
        if (name.Contains("tgc"))
            return "tgc-player";
        return playable ? "stickman-v0.1" : "";
    }

    private void SetPlayerCharacter(string actorName, SpriteAnimationSet? animationSet, string note, string animationSourceId = "")
    {
        if (animationSet is null)
        {
            _inspectorText.Text = $"{actorName} could not be loaded. The raw/local source may be missing; keeping the current character.";
            return;
        }

        _player.ActorName = actorName;
        _player.AnimationSet = animationSet;
        _player.AnimationSourceId = string.IsNullOrWhiteSpace(animationSourceId) ? _animationEditorSourceId : animationSourceId;
        SelectActor(_player);
        _inspectorText.Text = note + "\n\nThis is the seed of the Character Picker: choose a source, preview action labels, then edit/fine-tune clips.";
        RefreshMotionText();
    }

    private void SetEnemyCharacter(string actorName, SpriteAnimationSet? animationSet, string note)
    {
        AddEnemyCharacter(actorName, animationSet, note, "This is the first enemy/import test: one source strip, many possible game roles.", canFireProjectiles: true, animationSourceId: GuessAnimationSourceId(actorName, playable: false));
    }

    private void AddTgcEnemy(string actorName, SpriteAnimationSet? animationSet, string note)
    {
        bool canFire = actorName.Contains("Boss", StringComparison.OrdinalIgnoreCase)
            || actorName.Contains("Fleet", StringComparison.OrdinalIgnoreCase);
        AddEnemyCharacter(actorName, animationSet, note, "TGC shelf import: treated as an enemy for now. Drag, scale, save, and later assign behavior/projectiles.", canFire, GuessAnimationSourceId(actorName, playable: false));
    }

    private void AddEnemyCharacter(string actorName, SpriteAnimationSet? animationSet, string note, string footer, bool canFireProjectiles, string animationSourceId = "")
    {
        if (animationSet is null)
        {
            _inspectorText.Text = $"{actorName} could not be loaded. The raw/local source may be missing.";
            return;
        }

        ActorView enemy = NextEnemySlot();
        enemy.ActorName = actorName;
        enemy.AnimationSourceId = string.IsNullOrWhiteSpace(animationSourceId) ? GuessAnimationSourceId(actorName, playable: false) : animationSourceId;
        enemy.AnimationSet = animationSet;
        enemy.MotionState = ActorMotionState.Idle;
        enemy.AnimationClock = 0;
        enemy.IsPlayable = false;
        enemy.Visible = true;
        enemy.FacingRight = false;
        enemy.CanFireProjectiles = canFireProjectiles;
        enemy.ShotToughness = DefaultEnemyShotToughness(actorName);
        enemy.ManualPlacement = true;
        int slotIndex = _actors.IndexOf(enemy);
        enemy.Size = EnemyDefaultSize();
        enemy.CustomMinimumSize = enemy.Size;
        enemy.Position = EnemySpawnPosition(slotIndex, enemy.Size);
        enemy.HomePosition = enemy.Position;
        enemy.TooltipText = $"Select {actorName}";
        SelectActor(enemy);
        _enemyHealth.Remove(enemy);
        _defeatedEnemies.Remove(enemy);
        _inspectorText.Text = note + "\n\n" + footer + $"\n\nProjectile capable: {(canFireProjectiles ? "yes" : "no")}.\nShot toughness: {enemy.ShotToughness}.";
    }

    private ActorView NextEnemySlot()
    {
        for (int i = 1; i < _actors.Count; i++)
        {
            if (!_actors[i].Visible)
                return _actors[i];
        }

        return _actors.Count > 1 ? _actors[1] : _player;
    }

    private Vector2 EnemyDefaultSize()
    {
        float height = Mathf.Max(_textUnitPixels * 7f, 52f);
        return new Vector2(height, height);
    }

    private Vector2 EnemySpawnPosition(int slotIndex, Vector2 size)
    {
        Rect2 bounds = _playfield.PlayBounds;
        int index = Mathf.Max(1, slotIndex);
        int column = (index - 1) % 4;
        int row = (index - 1) / 4;
        Vector2 position = new(
            bounds.Position.X + bounds.Size.X * (0.55f + column * 0.09f),
            bounds.Position.Y + bounds.Size.Y * (0.30f + row * 0.14f)
        );
        return new Vector2(
            Mathf.Clamp(position.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - size.X)),
            Mathf.Clamp(position.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - size.Y))
        );
    }

    private void LoadTgcEditorDefaults()
    {
        _animationEditorName = "TGC Player";
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Player_DarkOutline.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = "tgc-player";
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = "tgc-player.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadGameCreatorPlayerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(2, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(3, maxFrame), Mathf.Min(14, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", Mathf.Min(15, maxFrame), Mathf.Min(15, maxFrame), maxFrame);
        AddTgcClipRow("Jump Down", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Fall", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", Mathf.Min(17, maxFrame), Mathf.Min(20, maxFrame), maxFrame);
        AddTgcClipRow("Jump Shoot", Mathf.Min(21, maxFrame), Mathf.Min(24, maxFrame), maxFrame);
        AddTgcClipRow("Death", Mathf.Min(16, maxFrame), Mathf.Min(17, maxFrame), maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadStickmanEditorDefaults()
    {
        _animationEditorName = "Playable Scout";
        _animationEditorSource = "assets/third_party/stickman-pack-v0.1/thin-*.png";
        _animationEditorSourceKind = "admitted-third-party";
        _animationEditorSourceId = "stickman-v0.1";
        _animationEditorFolder = "stickman-pack-v0.1";
        _animationEditorFileName = "stickman-thin.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadStickmanFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(6, maxFrame), Mathf.Min(14, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", Mathf.Min(15, maxFrame), Mathf.Min(15, maxFrame), maxFrame);
        AddTgcClipRow("Jump Down", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Fall", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", Mathf.Min(6, maxFrame), Mathf.Min(14, maxFrame), maxFrame);
        AddTgcClipRow("Jump Shoot", Mathf.Min(15, maxFrame), Mathf.Min(15, maxFrame), maxFrame);
        AddTgcClipRow("Death", 0, Mathf.Min(5, maxFrame), maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadSunnyDragonEditorDefaults()
    {
        _animationEditorName = "Sunny Dragon";
        _animationEditorSource = "raw base assets/Legacy Collection/Legacy Collection/Assets/Misc/Characters/sunny-dragon/spritesheets/sunny-dragon-fly.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = "sunny-dragon-fly";
        _animationEditorFolder = "legacy-collection-sunny-dragon-prep";
        _animationEditorFileName = "sunny-dragon-fly.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadSunnyDragonFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, maxFrame, maxFrame);
        AddTgcClipRow("Fly", 0, maxFrame, maxFrame);
        AddTgcClipRow("Run", 0, maxFrame, maxFrame);
        AddTgcClipRow("Jump Up", 0, maxFrame, maxFrame);
        AddTgcClipRow("Jump Down", 0, maxFrame, maxFrame);
        AddTgcClipRow("Fall", 0, maxFrame, maxFrame);
        AddTgcClipRow("Run Shoot", 0, maxFrame, maxFrame);
        AddTgcClipRow("Jump Shoot", 0, maxFrame, maxFrame);
        AddTgcClipRow("Death", 0, maxFrame, maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadTgcPlatformerEnemyEditorDefaults(string actorName, string sourceId, AnimationFrameRange idle, AnimationFrameRange run, AnimationFrameRange crawl)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcPlatformerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", Mathf.Min(idle.Start, maxFrame), Mathf.Min(idle.End, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(run.Start, maxFrame), Mathf.Min(run.End, maxFrame), maxFrame);
        AddTgcClipRow("Crawl", Mathf.Min(crawl.Start, maxFrame), Mathf.Min(crawl.End, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", "-", "-", maxFrame);
        AddTgcClipRow("Jump Down", "-", "-", maxFrame);
        AddTgcClipRow("Fall", "-", "-", maxFrame);
        AddTgcClipRow("Run Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Jump Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Death", "-", "-", maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadTgcOrangeWorkerEditorDefaults(string actorName, string sourceId)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcOrangeWorkerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(7, maxFrame), Mathf.Min(12, maxFrame), maxFrame);
        AddTgcClipRow("Crawl", Mathf.Min(7, maxFrame), Mathf.Min(12, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", "-", "-", maxFrame);
        AddTgcClipRow("Jump Down", "-", "-", maxFrame);
        AddTgcClipRow("Fall", "-", "-", maxFrame);
        AddTgcClipRow("Run Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Jump Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Death", "-", "-", maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadTgcRedRunnerEditorDefaults(string actorName, string sourceId)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcRedRunnerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, maxFrame, maxFrame);
        AddTgcClipRow("Run", 0, maxFrame, maxFrame);
        AddTgcClipRow("Crawl", 0, maxFrame, maxFrame);
        AddTgcClipRow("Jump Up", "-", "-", maxFrame);
        AddTgcClipRow("Jump Down", "-", "-", maxFrame);
        AddTgcClipRow("Fall", "-", "-", maxFrame);
        AddTgcClipRow("Run Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Jump Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Death", "-", "-", maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadTgcBlueGuardEditorDefaults(string actorName, string sourceId)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcBlueGuardFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(6, maxFrame), Mathf.Min(11, maxFrame), maxFrame);
        AddTgcClipRow("Crawl", Mathf.Min(6, maxFrame), Mathf.Min(11, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", "-", "-", maxFrame);
        AddTgcClipRow("Jump Down", "-", "-", maxFrame);
        AddTgcClipRow("Fall", "-", "-", maxFrame);
        AddTgcClipRow("Run Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Jump Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Death", "-", "-", maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadTgcGreenCrawlerEditorDefaults(string actorName, string sourceId)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcGreenCrawlerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, maxFrame, maxFrame);
        AddTgcClipRow("Run", 0, maxFrame, maxFrame);
        AddTgcClipRow("Crawl", 0, maxFrame, maxFrame);
        AddTgcClipRow("Jump Up", "-", "-", maxFrame);
        AddTgcClipRow("Jump Down", "-", "-", maxFrame);
        AddTgcClipRow("Fall", "-", "-", maxFrame);
        AddTgcClipRow("Run Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Jump Shoot", "-", "-", maxFrame);
        AddTgcClipRow("Death", "-", "-", maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadShooterBossEditorDefaults()
    {
        _animationEditorName = "Shooter Boss";
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Shooter_Boss_Sprite.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = "tgc-shooter-boss";
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = "tgc-shooter-boss.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcShooterBossFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, maxFrame, maxFrame);
        AddTgcClipRow("Run", 0, maxFrame, maxFrame);
        AddTgcClipRow("Death", 0, maxFrame, maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadShooterFleetEditorDefaults()
    {
        _animationEditorName = "Shooter Fleet";
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Shooter_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = "tgc-shooter-fleet";
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = "tgc-shooter-fleet.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcShooterFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Death", 0, 0, maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void SetAnimationEditorFrames(SpriteFrame[] frames)
    {
        _animationEditorFrameCount = frames.Length;
        _tgcStripPreview?.SetFrames(frames);
    }

    private void ClearTgcClipRows()
    {
        if (_tgcClipRows is not null)
        {
            foreach (TgcClipRow row in _tgcClipRowModels)
                row.Row.QueueFree();
        }

        _tgcClipRowModels.Clear();
    }

    private void ApplyTgcClipRanges()
    {
        AnimationFrameRange idle = FindTgcClipRange(new AnimationFrameRange(0, 2), "idle");
        AnimationFrameRange run = FindTgcClipRange(new AnimationFrameRange(3, 14), "run", "walk");
        AnimationFrameRange jumpUp = FindTgcClipRange(new AnimationFrameRange(15, 15), "jump up", "jump", "rise");
        AnimationFrameRange jumpDown = FindTgcClipRange(new AnimationFrameRange(16, 16), "jump down", "land");
        AnimationFrameRange fall = FindTgcClipRange(jumpDown, "fall", "falling");
        AnimationFrameRange runShoot = FindTgcClipRange(run, "run shoot", "run shooting");
        AnimationFrameRange jumpShoot = FindTgcClipRange(jumpUp, "jump shoot", "air shoot", "jump shooting");
        AnimationFrameRange death = FindTgcClipRange(new AnimationFrameRange(16, 17), "death", "die");
        bool idlePingPong = FindTgcClipPingPong("idle");
        bool runPingPong = FindTgcClipPingPong("run", "walk");
        bool jumpUpPingPong = FindTgcClipPingPong("jump up", "jump", "rise");
        bool jumpDownPingPong = FindTgcClipPingPong("jump down", "land");
        bool fallPingPong = FindTgcClipPingPong("fall", "falling");
        bool runShootPingPong = FindTgcClipPingPong("run shoot", "run shooting");
        bool jumpShootPingPong = FindTgcClipPingPong("jump shoot", "air shoot", "jump shooting");
        bool deathPingPong = FindTgcClipPingPong("death", "die");
        ApplyDeathStrobeSettings();
        UpdateTgcStripPreview();

        SpriteAnimationSet? animationSet;
        switch (_animationEditorSourceId)
        {
            case "stickman-v0.1":
                animationSet = SpriteAnimationSet.TryLoadStickman(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "sunny-dragon-fly":
                animationSet = SpriteAnimationSet.TryLoadSunnyDragon(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-orange-worker":
                animationSet = SpriteAnimationSet.TryLoadTgcOrangeWorker(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-red-runner":
                animationSet = SpriteAnimationSet.TryLoadTgcRedRunner(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-blue-guard":
                animationSet = SpriteAnimationSet.TryLoadTgcBlueGuard(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-green-crawler":
                animationSet = SpriteAnimationSet.TryLoadTgcGreenCrawler(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-shooter-fleet":
                animationSet = SpriteAnimationSet.TryLoadTgcShooterFleet(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "tgc-shooter-boss":
                animationSet = SpriteAnimationSet.TryLoadTgcShooterBoss();
                break;
            default:
                animationSet = SpriteAnimationSet.TryLoadGameCreatorPlayer(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
        }

        string note = $"{_animationEditorName} labels applied.\n\n"
            + $"Idle {idle.Start}-{idle.End}; Run {run.Start}-{run.End}; Jump-up {jumpUp.Start}-{jumpUp.End}; Jump-down {jumpDown.Start}-{jumpDown.End}; Fall {fall.Start}-{fall.End}; Run-shoot {runShoot.Start}-{runShoot.End}; Jump-shoot {jumpShoot.Start}-{jumpShoot.End}; Death {death.Start}-{death.End}.\n\n"
            + "Run Shoot and Jump Shoot now bind while firing. Death can be tested from the strip editor. PingPong turns short ranges into forward/back sequences.";

        if (_selectedActor is not null && !_selectedActor.IsPlayable)
            SetSelectedEnemyAnimation(_animationEditorName, animationSet, note, _animationEditorSourceId);
        else
            SetPlayerCharacter(_animationEditorName, animationSet, note, _animationEditorSourceId);
    }

    private void SetSelectedEnemyAnimation(string actorName, SpriteAnimationSet? animationSet, string note, string animationSourceId)
    {
        if (_selectedActor is null || _selectedActor.IsPlayable)
            return;

        if (animationSet is null)
        {
            _inspectorText.Text = $"{actorName} could not be applied. The animation source may be missing.";
            return;
        }

        _selectedActor.ActorName = actorName;
        _selectedActor.AnimationSourceId = animationSourceId;
        _selectedActor.AnimationSet = animationSet;
        _selectedActor.AnimationClock = 0;
        _selectedActor.MotionState = ActorMotionState.Idle;
        _selectedActor.QueueRedraw();
        RefreshBindingText();
        _inspectorText.Text = note + "\n\nApplied to the selected enemy. Save labels to make this source's frame mapping reusable.";
    }

    private void ReloadSelectedAnimationDefaults()
    {
        if (_selectedActor is null)
            return;

        string sourceId = string.IsNullOrWhiteSpace(_selectedActor.AnimationSourceId)
            ? GuessAnimationSourceId(_selectedActor.ActorName, _selectedActor.IsPlayable)
            : _selectedActor.AnimationSourceId;

        SpriteAnimationSet? animationSet = LoadDefaultAnimationSet(sourceId);
        if (animationSet is null)
        {
            _inspectorText.Text = $"Could not reload defaults for {_selectedActor.ActorName}. The animation source may be missing.";
            return;
        }

        string actorName = _selectedActor.ActorName;
        LoadAnimationEditorForActor(_selectedActor);
        string note = $"{actorName} default animation reloaded.\n\nThis replaces the selected actor's in-memory animation with the current importer defaults. Use this after importer fixes, or when a placed actor is still holding an old frame cut.";

        if (_selectedActor.IsPlayable)
            SetPlayerCharacter(actorName, animationSet, note, sourceId);
        else
            SetSelectedEnemyAnimation(actorName, animationSet, note, sourceId);
    }

    private static SpriteAnimationSet? LoadDefaultAnimationSet(string sourceId)
    {
        return sourceId switch
        {
            "stickman-v0.1" => SpriteAnimationSet.TryLoadStickman(),
            "sunny-dragon-fly" => SpriteAnimationSet.TryLoadSunnyDragon(),
            "tgc-player" => SpriteAnimationSet.TryLoadGameCreatorPlayer(),
            "tgc-orange-worker" => SpriteAnimationSet.TryLoadTgcOrangeWorker(),
            "tgc-red-runner" => SpriteAnimationSet.TryLoadTgcRedRunner(),
            "tgc-blue-guard" => SpriteAnimationSet.TryLoadTgcBlueGuard(),
            "tgc-green-crawler" => SpriteAnimationSet.TryLoadTgcGreenCrawler(),
            "tgc-shooter-boss" => SpriteAnimationSet.TryLoadTgcShooterBoss(),
            "tgc-shooter-fleet" => SpriteAnimationSet.TryLoadTgcShooterFleet(),
            "battle-fleet-red-ship-01" => SpriteAnimationSet.TryLoadBattleFleetRedShip01(),
            _ => null
        };
    }

    private void TriggerDeathAnimation()
    {
        ApplyDeathStrobeSettings();
        _deathTestSeconds = 1.35;
        _playerVelocity = Vector2.Zero;
        _player.MotionState = ActorMotionState.Death;
        _player.AnimationClock = 0;
        _inspectorText.Text = "Death animation test started. Adjust the Death row, STR, and count, then Apply/Save again.";
    }

    private void ApplyDeathStrobeSettings()
    {
        TgcClipRow? deathRow = FindTgcClipRow("death", "die");
        _player.StrobeEnabled = deathRow?.Strobe.ButtonPressed ?? false;
        _player.StrobeCount = Mathf.Clamp(Mathf.RoundToInt((float)(deathRow?.StrobeCount.Value ?? 0)), 0, 20);
    }

    private void SaveTgcClipLabels()
    {
        string outputPath = GetAnimationSavePath();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        int numberBase = Mathf.RoundToInt((float)(_tgcNumberBase?.Value ?? 0));
        int frameCount = Mathf.Max(0, _animationEditorFrameCount);

        List<object> labels = [];
        for (int i = 0; i < _tgcClipRowModels.Count; i++)
        {
            TgcClipRow row = _tgcClipRowModels[i];
            bool unavailable = IsUnavailableClipRow(row);
            AnimationFrameRange editorRange = unavailable
                ? new AnimationFrameRange(-1, -1)
                : EndpointRange(row.Start, row.End);
            AnimationFrameRange internalRange = unavailable
                ? new AnimationFrameRange(-1, -1)
                : DisplayToInternalRange(editorRange, numberBase, frameCount);
            labels.Add(new
            {
                name = string.IsNullOrWhiteSpace(row.Name.Text) ? $"Action {i + 1}" : row.Name.Text.Trim(),
                unavailable,
                editorStart = editorRange.Start,
                editorEnd = editorRange.End,
                internalStart = internalRange.Start,
                internalEnd = internalRange.End,
                pingPong = row.PingPong.ButtonPressed,
                strobe = row.Strobe.ButtonPressed,
                strobeCount = Mathf.Clamp(Mathf.RoundToInt((float)row.StrobeCount.Value), 0, 20),
                color = ClipColor(i).ToHtml(false)
            });
        }

        object manifest = new
        {
            format = "dackanim",
            version = 1,
            sourceName = _animationEditorName,
            sourceId = _animationEditorSourceId,
            source = _animationEditorSource,
            sourceKind = _animationEditorSourceKind,
            frameNumberBase = numberBase,
            note = "editorStart/editorEnd are the numbers shown in the strip editor; internalStart/internalEnd are zero-based detected frame indices used by playback.",
            frames = Enumerable.Range(0, frameCount).Select(index => new
            {
                index,
                displayed = index + numberBase
            }).ToArray(),
            labels
        };

        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(outputPath, JsonSerializer.Serialize(manifest, options));
        _inspectorText.Text = $"{_animationEditorName} animation labels saved.\n\n{outputPath}\n\nThis file records displayed numbers, internal zero-based frame indices, label names, PingPong toggles, and strobe settings.";
    }

    private void LoadAnimationClipLabels()
    {
        TryLoadAnimationClipLabels(showFeedback: true);
    }

    private bool TryLoadAnimationClipLabels(bool showFeedback)
    {
        string inputPath = GetAnimationSavePath();
        if (!File.Exists(inputPath))
        {
            if (showFeedback)
                _inspectorText.Text = $"No saved {_animationEditorName} animation labels found yet.\n\nExpected:\n{inputPath}";
            return false;
        }

        DackAnimManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<DackAnimManifest>(File.ReadAllText(inputPath));
        }
        catch (Exception ex)
        {
            if (showFeedback)
                _inspectorText.Text = $"Could not load animation labels.\n\n{ex.Message}";
            return false;
        }

        if (manifest?.labels is null || manifest.labels.Count == 0)
        {
            if (showFeedback)
                _inspectorText.Text = $"Animation label file loaded, but it had no labels.\n\n{inputPath}";
            return false;
        }

        _tgcNumberBase.Value = manifest.frameNumberBase;
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        int numberBase = Mathf.RoundToInt((float)_tgcNumberBase.Value);
        bool incompatibleFrameNumbers = manifest.labels.Any(label =>
            !label.unavailable
            && (label.editorStart - numberBase > maxFrame || label.editorEnd - numberBase > maxFrame)
        );
        if (incompatibleFrameNumbers)
        {
            if (showFeedback)
                _inspectorText.Text = $"{_animationEditorName} has saved labels, but they were made for a different frame cut.\n\nUsing the current importer defaults instead. Save labels again to update:\n{inputPath}";
            return false;
        }

        ClearTgcClipRows();
        foreach (DackAnimLabel label in manifest.labels)
        {
            TgcClipRow row = BuildEditableClipRow(
                string.IsNullOrWhiteSpace(label.name) ? $"Action {_tgcClipRowModels.Count + 1}" : label.name,
                Mathf.Clamp(label.editorStart, 0, maxFrame),
                Mathf.Clamp(label.editorEnd, 0, maxFrame),
                maxFrame
            );
            if (label.unavailable)
            {
                row.Start.Text = "-";
                row.End.Text = "-";
            }
            row.PingPong.ButtonPressed = label.pingPong;
            row.Strobe.ButtonPressed = label.strobe;
            row.StrobeCount.Value = Mathf.Clamp(label.strobeCount, 0, 20);
            _tgcClipRowModels.Add(row);
            _tgcClipRows.AddChild(row.Row);
        }

        UpdateTgcStripPreview();
        if (showFeedback)
            _inspectorText.Text = $"{_animationEditorName} animation labels loaded.\n\n{inputPath}\n\nPress APPLY ANIM LABELS to use them on the selected actor.";
        return true;
    }

    private string GetAnimationSavePath()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(
            projectRoot,
            "assets",
            "quarantine",
            _animationEditorFolder,
            _animationEditorFileName
        ));
    }

    private void SaveLevel()
    {
        string outputPath = GetDefaultLevelPath();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        DackLevelManifest manifest = new()
        {
            format = "dacklevel",
            version = 1,
            name = "RAD Test Level",
            sourceMode = "current-prototype-snapshot-reference",
            editorMode = _editorMode,
            playsetMode = _playsetMode.ToString(),
            platformerMode = _platformerMode.ToString(),
            safetyFloor = _platformerSafetyFloor,
            textTerrainEnabled = _textTerrainEnabled,
            textDestructionEnabled = _textDestructionEnabled,
            gunEnabled = _gunEnabled,
            enemyAiEnabled = _enemyAiEnabled,
            enemyTracksPlayer = _enemyTracksPlayer,
            enemyProjectilesEnabled = _enemyProjectilesEnabled,
            partialDamageEnabled = _partialDamageEnabled,
            enemyShotRangeUnits = _enemyShotRangeUnits,
            playerMaxHealth = _playerMaxHealth,
            enemyShotDamage = _enemyShotDamage,
            playerShotPower = _playerShotPower,
            explosionsDamageText = _explosionsDamageText,
            platformerScore = _platformerScore,
            platformerLives = _platformerLives,
            playerHealth = _playerHealth,
            platformerDeaths = _platformerDeaths,
            hudX = _platformerHud?.Position.X ?? 18f,
            hudY = _platformerHud?.Position.Y ?? 78f,
            actorSizeMultiplier = _actorSizeMultiplier,
            textUnitPixels = _textUnitPixels,
            worldObjects = _playfield.GetPlacedWorldObjects().Select(LevelWorldObject.FromWorldObject).ToList(),
            actors = _actors
                .Where(actor => actor.Visible || actor.IsPlayable)
                .Select((actor, index) => LevelActor.FromActor(actor, index))
                .ToList()
        };

        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(outputPath, JsonSerializer.Serialize(manifest, options));
        _inspectorText.Text = $"Level saved.\n\n{outputPath}\n\nThis first .dacklevel pass stores placed toolkit objects, route markers, visible actors, player/gameplay toggles, and the current playset settings. Snapshot image packaging comes next.";
    }

    private void LoadLevel()
    {
        string inputPath = GetDefaultLevelPath();
        if (!File.Exists(inputPath))
        {
            _inspectorText.Text = $"No saved level found yet.\n\nExpected:\n{inputPath}";
            return;
        }

        DackLevelManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<DackLevelManifest>(File.ReadAllText(inputPath));
        }
        catch (Exception ex)
        {
            _inspectorText.Text = $"Could not load level.\n\n{ex.Message}";
            return;
        }

        if (manifest is null)
        {
            _inspectorText.Text = "Could not load level: file was empty or invalid.";
            return;
        }

        SetEditorMode(true);
        _platformerSafetyFloor = manifest.safetyFloor;
        _textTerrainEnabled = manifest.textTerrainEnabled;
        _textDestructionEnabled = manifest.textDestructionEnabled;
        _gunEnabled = manifest.gunEnabled;
        _enemyAiEnabled = manifest.enemyAiEnabled;
        _enemyTracksPlayer = manifest.enemyTracksPlayer;
        _enemyProjectilesEnabled = manifest.enemyProjectilesEnabled;
        _partialDamageEnabled = manifest.partialDamageEnabled;
        _enemyShotRangeUnits = manifest.enemyShotRangeUnits <= 0 ? _enemyShotRangeUnits : manifest.enemyShotRangeUnits;
        _playerMaxHealth = manifest.playerMaxHealth <= 0 ? _playerMaxHealth : manifest.playerMaxHealth;
        _enemyShotDamage = manifest.enemyShotDamage <= 0 ? _enemyShotDamage : manifest.enemyShotDamage;
        _playerShotPower = manifest.playerShotPower <= 0 ? _playerShotPower : manifest.playerShotPower;
        _explosionsDamageText = manifest.explosionsDamageText;
        _platformerScore = manifest.platformerScore;
        _platformerLives = manifest.platformerLives <= 0 ? 3 : manifest.platformerLives;
        _playerHealth = manifest.playerHealth <= 0 ? _playerMaxHealth : Mathf.Min(manifest.playerHealth, _playerMaxHealth);
        _platformerDeaths = manifest.platformerDeaths;
        ClearEnemyShots();
        if (_platformerHud is not null && (manifest.hudX > 0 || manifest.hudY > 0))
            _platformerHud.Position = new Vector2(manifest.hudX, manifest.hudY);
        _actorSizeMultiplier = manifest.actorSizeMultiplier <= 0 ? _actorSizeMultiplier : manifest.actorSizeMultiplier;
        if (manifest.textUnitPixels > 0)
            _textUnitPixels = manifest.textUnitPixels;
        _playfield.TextUnitPixels = _textUnitPixels;
        if (_scaleSlider is not null)
            _scaleSlider.Value = _textUnitPixels;

        if (Enum.TryParse(manifest.platformerMode, out PlatformerMode loadedPlatformerMode))
            _platformerMode = loadedPlatformerMode;

        if (Enum.TryParse(manifest.playsetMode, out PlaysetMode loadedPlaysetMode))
            SetPlaysetMode(loadedPlaysetMode);

        _playfield.SetPlacedWorldObjects(manifest.worldObjects.Select(worldObject => worldObject.ToWorldObject()));
        RestoreLevelActors(manifest.actors);
        ApplyActorScale();
        if (_playfield.HasStartMarker())
            SnapPlayerToStart();
        else if (_player is not null)
            _playerPosition = _player.Position;

        SyncEditorModeToScene();
        RefreshMotionText();
        RefreshCockpitStatus();
        _inspectorText.Text = $"Level loaded.\n\n{inputPath}\n\nRestored {manifest.worldObjects.Count} placed objects and {manifest.actors.Count} actors.";
    }

    private void RestoreLevelActors(List<LevelActor> actors)
    {
        foreach (ActorView actor in _actors)
        {
            actor.Visible = actor.IsPlayable;
            if (!actor.IsPlayable)
                actor.AnimationSet = null;
        }

        foreach (LevelActor saved in actors)
        {
            if (saved.index < 0 || saved.index >= _actors.Count)
                continue;

            ActorView actor = _actors[saved.index];
            actor.ActorName = string.IsNullOrWhiteSpace(saved.name) ? actor.ActorName : saved.name;
            actor.AnimationSourceId = string.IsNullOrWhiteSpace(saved.animationSourceId) ? saved.animationSource : saved.animationSourceId;
            actor.Visible = saved.visible || actor.IsPlayable;
            actor.FacingRight = saved.facingRight;
            actor.ManualPlacement = saved.manualPlacement;
            actor.ShotToughness = saved.shotToughness <= 0 ? DefaultEnemyShotToughness(actor.ActorName) : Mathf.Clamp(saved.shotToughness, 1, 9);
            actor.MotionState = Enum.TryParse(saved.motionState, out ActorMotionState motionState) ? motionState : ActorMotionState.Idle;
            actor.Position = new Vector2(saved.x, saved.y);
            actor.Size = new Vector2(saved.width, saved.height);
            actor.CustomMinimumSize = actor.Size;
            actor.AnimationSet = actor.AnimationSourceId switch
            {
                "stickman-v0.1" => SpriteAnimationSet.TryLoadStickman(),
                "sunny-dragon-fly" => SpriteAnimationSet.TryLoadSunnyDragon(),
                "tgc-player" => SpriteAnimationSet.TryLoadGameCreatorPlayer(),
                "tgc-orange-worker" => SpriteAnimationSet.TryLoadTgcOrangeWorker(),
                "tgc-red-runner" => SpriteAnimationSet.TryLoadTgcRedRunner(),
                "tgc-blue-guard" => SpriteAnimationSet.TryLoadTgcBlueGuard(),
                "tgc-green-crawler" => SpriteAnimationSet.TryLoadTgcGreenCrawler(),
                "tgc-shooter-boss" => SpriteAnimationSet.TryLoadTgcShooterBoss(),
                "tgc-shooter-fleet" => SpriteAnimationSet.TryLoadTgcShooterFleet(),
                "battle-fleet-red-ship-01" => SpriteAnimationSet.TryLoadBattleFleetRedShip01(),
                _ => actor.AnimationSet
            };
        }

        SelectActor(_player);
    }

    private static string GetDefaultLevelPath()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(projectRoot, "levels", "rad-test.dacklevel.json"));
    }

    private AnimationFrameRange FindTgcClipRange(AnimationFrameRange fallback, params string[] names)
    {
        int numberBase = Mathf.RoundToInt((float)(_tgcNumberBase?.Value ?? 0));
        int frameCount = Mathf.Max(0, _animationEditorFrameCount);
        foreach (TgcClipRow row in _tgcClipRowModels)
        {
            string normalized = NormalizeClipName(row.Name.Text);
            if (IsUnavailableClipRow(row))
                continue;

            foreach (string name in names)
            {
                if (normalized == NormalizeClipName(name))
                    return DisplayToInternalRange(EndpointRange(row.Start, row.End), numberBase, frameCount);
            }
        }

        return fallback;
    }

    private bool FindTgcClipPingPong(params string[] names)
    {
        foreach (TgcClipRow row in _tgcClipRowModels)
        {
            string normalized = NormalizeClipName(row.Name.Text);
            if (IsUnavailableClipRow(row))
                continue;

            foreach (string name in names)
            {
                if (normalized == NormalizeClipName(name))
                    return row.PingPong.ButtonPressed;
            }
        }

        return false;
    }

    private TgcClipRow? FindTgcClipRow(params string[] names)
    {
        foreach (TgcClipRow row in _tgcClipRowModels)
        {
            string normalized = NormalizeClipName(row.Name.Text);
            foreach (string name in names)
            {
                if (normalized == NormalizeClipName(name))
                    return row;
            }
        }

        return null;
    }

    private void UpdateTgcStripPreview()
    {
        if (_tgcStripPreview is null
            || _tgcNumberBase is null
            || _tgcClipRows is null)
        {
            return;
        }

        _tgcStripPreview.NumberBase = Mathf.RoundToInt((float)_tgcNumberBase.Value);
        _tgcStripPreview.SetLabels(GetTgcClipLabels());
    }

    private static AnimationFrameRange EndpointRange(LineEdit start, LineEdit end)
    {
        return new AnimationFrameRange(
            ParseEndpoint(start.Text),
            ParseEndpoint(end.Text)
        );
    }

    private static int ParseEndpoint(string text)
    {
        return int.TryParse(text.Trim(), out int value) ? value : 0;
    }

    private static bool IsDashEndpoint(LineEdit endpoint)
    {
        string value = endpoint.Text.Trim();
        return value == "-" || value == "—" || value == "–";
    }

    private static bool IsUnavailableClipRow(TgcClipRow row)
    {
        return IsDashEndpoint(row.Start) || IsDashEndpoint(row.End);
    }

    private static AnimationFrameRange DisplayToInternalRange(AnimationFrameRange editorRange, int numberBase, int frameCount)
    {
        if (frameCount <= 0)
            return new AnimationFrameRange(0, 0);

        int start = Mathf.Clamp(editorRange.Start - numberBase, 0, frameCount - 1);
        int end = Mathf.Clamp(editorRange.End - numberBase, 0, frameCount - 1);
        return new AnimationFrameRange(start, end);
    }

    private IReadOnlyList<AnimationClipLabel> GetTgcClipLabels()
    {
        List<AnimationClipLabel> labels = [];
        int numberBase = Mathf.RoundToInt((float)(_tgcNumberBase?.Value ?? 0));
        int frameCount = Mathf.Max(0, _animationEditorFrameCount);
        for (int i = 0; i < _tgcClipRowModels.Count; i++)
        {
            TgcClipRow row = _tgcClipRowModels[i];
            string name = string.IsNullOrWhiteSpace(row.Name.Text) ? $"Action {i + 1}" : row.Name.Text.Trim();
            if (IsUnavailableClipRow(row))
                continue;

            AnimationFrameRange internalRange = DisplayToInternalRange(EndpointRange(row.Start, row.End), numberBase, frameCount);
            labels.Add(new AnimationClipLabel(
                name,
                internalRange,
                ClipColor(i),
                row.PingPong.ButtonPressed,
                row.Strobe.ButtonPressed,
                Mathf.Clamp(Mathf.RoundToInt((float)row.StrobeCount.Value), 0, 20)
            ));
        }

        return labels;
    }

    private void AddTgcClipRow(string name, int start, int end, int maxFrame)
    {
        TgcClipRow row = BuildEditableClipRow(name, start, end, maxFrame);
        _tgcClipRowModels.Add(row);
        _tgcClipRows.AddChild(row.Row);
    }

    private void AddTgcClipRow(string name, string start, string end, int maxFrame)
    {
        TgcClipRow row = BuildEditableClipRow(name, 0, 0, maxFrame);
        row.Start.Text = start;
        row.End.Text = end;
        _tgcClipRowModels.Add(row);
        _tgcClipRows.AddChild(row.Row);
    }

    private string NextMissingPresetLabel()
    {
        foreach (string label in _tgcPresetLabels)
        {
            bool exists = _tgcClipRowModels.Any(row => NormalizeClipName(row.Name.Text) == NormalizeClipName(label));
            if (!exists)
                return label;
        }

        return $"Action {_tgcClipRowModels.Count + 1}";
    }

    private TgcClipRow BuildEditableClipRow(string name, int defaultStart, int defaultEnd, int maxFrame)
    {
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 6);

        LineEdit nameEdit = new()
        {
            Text = name,
            CustomMinimumSize = new Vector2(96, 32),
            PlaceholderText = "Label"
        };
        nameEdit.AddThemeFontSizeOverride("font_size", 12);
        nameEdit.TextChanged += _ => UpdateTgcStripPreview();
        row.AddChild(nameEdit);

        LineEdit start = ClipEndpointEdit(defaultStart, maxFrame);
        LineEdit end = ClipEndpointEdit(defaultEnd, maxFrame);
        start.TextChanged += text => OnClipEndpointTextChanged(start, end, text, maxFrame);
        end.TextChanged += text => OnClipEndpointTextChanged(end, start, text, maxFrame);
        row.AddChild(start);
        row.AddChild(end);

        CheckBox pingPong = new()
        {
            Text = "↔",
            TooltipText = "PingPong: play forward, then reverse the frames.",
            CustomMinimumSize = new Vector2(36, 32),
            FocusMode = FocusModeEnum.None
        };
        pingPong.Pressed += UpdateTgcStripPreview;
        row.AddChild(pingPong);

        CheckBox strobe = new()
        {
            Text = "STR",
            TooltipText = "Strobe this label during test/play effects.",
            CustomMinimumSize = new Vector2(48, 32),
            FocusMode = FocusModeEnum.None
        };
        strobe.Pressed += UpdateTgcStripPreview;
        row.AddChild(strobe);

        SpinBox strobeCount = ClipEndpointSpin(0, 20);
        strobeCount.TooltipText = "Strobe pulse count/intensity, 0-20.";
        strobeCount.ValueChanged += _ => UpdateTgcStripPreview();
        row.AddChild(strobeCount);

        return new TgcClipRow(row, nameEdit, start, end, pingPong, strobe, strobeCount);
    }

    private void OnClipEndpointTextChanged(LineEdit changed, LineEdit partner, string text, int maxFrame)
    {
        if (_syncingClipUnavailable)
            return;

        string value = text.Trim();
        if (value == "-" || value == "—" || value == "–")
        {
            _syncingClipUnavailable = true;
            changed.Text = "-";
            partner.Text = "-";
            _syncingClipUnavailable = false;
            UpdateTgcStripPreview();
            return;
        }

        if (!int.TryParse(value, out int parsed))
        {
            UpdateTgcStripPreview();
            return;
        }

        int clamped = Mathf.Clamp(parsed, 0, Mathf.Max(0, maxFrame));
        if (clamped != parsed)
        {
            _syncingClipUnavailable = true;
            changed.Text = clamped.ToString();
            changed.CaretColumn = changed.Text.Length;
            _syncingClipUnavailable = false;
        }

        if (IsDashEndpoint(partner))
        {
            _syncingClipUnavailable = true;
            partner.Text = clamped.ToString();
            partner.CaretColumn = partner.Text.Length;
            _syncingClipUnavailable = false;
        }

        UpdateTgcStripPreview();
    }

    private static string NormalizeClipName(string value)
    {
        return value.Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");
    }

    private static Color ClipColor(int index)
    {
        Color[] colors =
        [
            new Color("#5CB8A7"),
            new Color("#F4C95D"),
            new Color("#B56CFF"),
            new Color("#5CB8FF"),
            new Color("#FF5C35"),
            new Color("#FF2BD6"),
            new Color("#8A5A37"),
            new Color("#7EE787")
        ];

        return colors[index % colors.Length];
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
        if (_characterNameEdit is not null && _characterNameEdit.Text != _selectedActor.ActorName)
        {
            _syncingCharacterName = true;
            _characterNameEdit.Text = _selectedActor.ActorName;
            _syncingCharacterName = false;
        }

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

    private void CloseSpritePanel()
    {
        if (_sidebar is null)
            return;

        _sidebar.Visible = false;
        if (_spritePanelButton is not null)
            _spritePanelButton.Text = "SHOW SPRITE PAD";
        UpdateCursorMode();
    }

    private void ToggleCockpit()
    {
        if (_cockpit.Visible)
        {
            bool resumePlay = _resumePlayWhenCockpitCloses;
            _resumePlayWhenCockpitCloses = false;
            _cockpit.Visible = false;
            if (resumePlay)
                SetEditorMode(false);
        }
        else
        {
            _resumePlayWhenCockpitCloses = !_editorMode;
            if (!_editorMode)
                SetEditorMode(true);

            _cockpit.Visible = true;
            FitCockpitToViewport();
        }

        _brickbatOverlay.HudEditable = _cockpit.Visible;
        SyncEditorModeToScene();
        _playfield.QueueRedraw();
        RefreshCockpitStatus();
        UpdateCursorMode();
    }

    private void FitCockpitToViewport()
    {
        if (_cockpit is null || _playfield is null)
            return;

        Vector2 available = _playfield.Size;
        if (available.X <= 0 || available.Y <= 0)
            available = GetViewportRect().Size;

        const float edge = 18f;
        Vector2 desired = new(
            Mathf.Clamp(available.X - edge * 2f, 620f, 1160f),
            Mathf.Clamp(available.Y - edge * 2f, 360f, 620f)
        );
        _cockpit.Size = desired;
        _cockpit.CustomMinimumSize = Vector2.Zero;
        _cockpit.Position = new Vector2(
            Mathf.Clamp(_cockpit.Position.X, edge, Mathf.Max(edge, available.X - desired.X - edge)),
            Mathf.Clamp(_cockpit.Position.Y, edge, Mathf.Max(edge, available.Y - desired.Y - edge))
        );
    }

    private void SetEditorMode(bool enabled)
    {
        _editorMode = enabled;
        SyncEditorModeToScene();
        if (_editorMode)
        {
            ClearEnemyShots();
            _enemyVelocities.Clear();
            _enemyPatrolDirections.Clear();
            foreach (ActorView enemy in _defeatedEnemies)
                enemy.Visible = true;
            _defeatedEnemies.Clear();
            _enemyHealth.Clear();
            _platformerStatus = "EDITOR";
            _inspectorText.Text = "Editor mode enabled.\n\nMarkers, enemies, and toolkit objects are draggable/scalable. Enemy AI and shots are paused for safe layout.";
        }
        else
        {
            _resumePlayWhenCockpitCloses = false;
            SetPlaysetMode(PlaysetMode.Platformer);
            _cockpit.Visible = false;
            CloseSpritePanel();
            _platformerLives = Mathf.Max(1, _platformerLives);
            _playerHealth = _playerMaxHealth;
            _goalReached = false;
            _platformerStatus = "PLAY";
            _contactInvulnerabilitySeconds = 1.25;
            _hazardArmDelaySeconds = 1.0;
            ClearPlayerShots();
            ClearEnemyShots();
            _enemyVelocities.Clear();
            _enemyPatrolDirections.Clear();
            _defeatedEnemies.Clear();
            _enemyHealth.Clear();
            SnapPlayerToStart();
            _inspectorText.Text = "Play mode enabled.\n\nStart Point is honored, editor-only markers are hidden, enemy AI/projectiles can run, and collisions count.";
        }

        SyncEditorModeToScene();
        RefreshSessionModeUi();
        RefreshPlatformerHud();
        UpdateCursorMode();
    }

    private void SyncEditorModeToScene()
    {
        _playfield.EditorMode = _editorMode;
        _playfield.ShowEditorOnlyObjects = _editorMode;
        if (_pinballOverlay is not null)
            _pinballOverlay.Paused = _editorMode || _playsetMode != PlaysetMode.Pinball;

        foreach (ActorView actor in _actors)
            actor.EditorMode = _editorMode;

        if (_player is not null)
            _player.CanDragPlayableInEditor = _editorMode && !_playfield.HasStartMarker();
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
        string authority = _editorMode ? "EDIT" : "PLAY";
        _cockpitStatus.Text = $"{authority}  •  {mode}  •  {_playfield.Ocr.StatusText}  •  contextual shelves  •  Esc toggles cockpit";
    }

    private void RefreshSessionModeUi()
    {
        string label = _editorMode ? "Enter Play Mode" : "Return to Editor";
        foreach (Button button in _editorPlayButtons)
            button.Text = label;

        RefreshCockpitStatus();
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

    private void ProbeDisplayLayout()
    {
        int screenCount = DisplayServer.GetScreenCount();
        int primary = DisplayServer.GetPrimaryScreen();
        int current = DisplayServer.WindowGetCurrentScreen();
        List<string> lines = [$"Detected {screenCount} monitor(s). Primary: {primary}. DACK: {current}."];

        for (int i = 0; i < screenCount; i++)
        {
            Vector2I position = DisplayServer.ScreenGetPosition(i);
            Vector2I size = DisplayServer.ScreenGetSize(i);
            Rect2I usable = DisplayServer.ScreenGetUsableRect(i);
            lines.Add($"#{i}: pos {position.X},{position.Y}  size {size.X}×{size.Y}  usable {usable.Size.X}×{usable.Size.Y}");
        }

        _inspectorText.Text = string.Join("\n", lines)
            + "\n\nStarter path: first move this single DACK window between screens; later split Cockpit/Edit and Playfield/Preview into separate windows.";
    }

    private void MoveDackToNextMonitor()
    {
        int screenCount = DisplayServer.GetScreenCount();
        if (screenCount <= 1)
        {
            _inspectorText.Text = "Only one monitor detected. Dual-monitor layout will stay dormant on this machine.";
            return;
        }

        int current = DisplayServer.WindowGetCurrentScreen();
        int next = (Mathf.Max(0, current) + 1) % screenCount;
        Rect2I usable = DisplayServer.ScreenGetUsableRect(next);
        DisplayServer.WindowSetCurrentScreen(next);
        DisplayServer.WindowSetPosition(usable.Position + new Vector2I(24, 24));
        DisplayServer.WindowSetSize(new Vector2I(Mathf.Max(960, usable.Size.X - 96), Mathf.Max(640, usable.Size.Y - 96)));
        _inspectorText.Text = $"Moved DACK to monitor #{next}.\n\nThis proves the first dual-monitor primitive: screen enumeration + window relocation. Next step is separate editor/playfield windows.";
    }

    private void SetPlaysetMode(PlaysetMode mode)
    {
        _playsetMode = mode;
        bool brickbat = mode == PlaysetMode.Brickbat;
        bool pinball = mode == PlaysetMode.Pinball;
        bool showScout = mode is PlaysetMode.Platformer or PlaysetMode.Overhead;
        _player.Visible = showScout;
        _brickbatOverlay.Visible = brickbat;
        _pinballOverlay.Visible = pinball;
        if (mode != PlaysetMode.Overhead)
            _player.DirectionFrameIndex = null;
        ClearPlayerShots();
        ClearEnemyShots();

        if (showScout && ShouldSnapPlayerForModeChange())
            SnapPlayerToStart();

        RefreshCockpitStatus();
        UpdateCockpitToolkitPanels();
        RefreshPlatformerHud();
        UpdateCursorMode();
        SyncEditorModeToScene();
    }

    private bool ShouldSnapPlayerForModeChange()
    {
        if (!_editorMode)
            return true;

        return _playfield.HasStartMarker() || !_player.ManualPlacement;
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

    private void UpdateActorPresentation(float dt)
    {
        if (_player is not null && _editorMode && _player.CanDragPlayableInEditor)
        {
            _playerPosition = _player.Position;
            _player.HomePosition = _player.Position;
        }

        if (_playsetMode is not (PlaysetMode.Brickbat or PlaysetMode.Pinball))
            return;

        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView actor = _actors[i];
            if (!actor.Visible || actor.AnimationSet is null)
                continue;

            actor.MotionState = IsFlyingEnemy(actor) ? ActorMotionState.Idle : ActorMotionState.Run;
            actor.QueueRedraw();
        }
    }

    private void UpdateOverheadPlayer(double delta)
    {
        if (_player is null || _bossMode || _editorMode || _playsetMode != PlaysetMode.Overhead)
            return;

        float dt = (float)delta;
        float motionUnit = Mathf.Max(_textUnitPixels, 10f);
        Vector2 input = new(
            Input.GetAxis("dack_left", "dack_right"),
            Input.GetAxis("dack_up", "dack_down")
        );

        if (input.LengthSquared() > 1f)
            input = input.Normalized();

        float maxSpeed = motionUnit * 23f;
        float acceleration = motionUnit * 88f;
        float drag = motionUnit * 62f;

        if (input.LengthSquared() > 0.001f)
            _playerVelocity = _playerVelocity.MoveToward(input * maxSpeed, acceleration * dt);
        else
            _playerVelocity = _playerVelocity.MoveToward(Vector2.Zero, drag * dt);

        _playerPosition += _playerVelocity * dt;
        Rect2 bounds = _playfield.PlayBounds;
        _playerPosition = new Vector2(
            Mathf.Clamp(_playerPosition.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - _player.Size.X)),
            Mathf.Clamp(_playerPosition.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - _player.Size.Y))
        );

        _player.Position = _playerPosition;
        _player.MotionState = _playerVelocity.LengthSquared() > motionUnit * motionUnit ? ActorMotionState.Run : ActorMotionState.Idle;
        _player.DirectionFrameIndex = HeadingFrameForVector(_playerVelocity);
        _player.FacingRight = _playerVelocity.X >= -0.01f;
        _player.QueueRedraw();
        RefreshMotionText();
    }

    private static int? HeadingFrameForVector(Vector2 velocity)
    {
        if (velocity.LengthSquared() < 4f)
            return null;

        float angle = velocity.Angle();
        float normalized = Mathf.PosMod(angle + Mathf.Pi, Mathf.Tau) / Mathf.Tau;
        return Mathf.Clamp(Mathf.RoundToInt(normalized * 4f), 0, 4);
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
        if (_player is null || _bossMode || _editorMode || _playsetMode != PlaysetMode.Platformer)
            return;

        float dt = (float)delta;
        if (_deathTestSeconds > 0)
        {
            _deathTestSeconds -= delta;
            _player.AnimationClock += delta;
            _player.MotionState = ActorMotionState.Death;
            _player.QueueRedraw();
            if (_deathTestSeconds <= 0)
            {
                _player.StrobeEnabled = false;
                if (_platformerLives <= 0)
                {
                    _platformerLives = 3;
                    _playerHealth = _playerMaxHealth;
                    _platformerStatus = "TRY AGAIN";
                }
                SnapPlayerToStart();
            }

            return;
        }

        _shootAnimSeconds = Mathf.Max(0, (float)_shootAnimSeconds - dt);
        _hazardArmDelaySeconds = Mathf.Max(0, (float)_hazardArmDelaySeconds - dt);
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
            _playerVelocity.X = Mathf.MoveToward(_playerVelocity.X, slideVelocity.X, Mathf.Abs(slideVelocity.X) * 5f * dt + motionUnit * 16f * dt);
            if (slideVelocity.Y > 0)
                _playerVelocity.Y = Mathf.MoveToward(_playerVelocity.Y, slideVelocity.Y, Mathf.Abs(slideVelocity.Y) * 3f * dt + motionUnit * 18f * dt);
        }

        if (conveyorVelocity != Vector2.Zero)
        {
            _playerVelocity += conveyorVelocity * dt;
            if (conveyorVelocity.Y < 0)
                _playerVelocity.Y = Mathf.Min(_playerVelocity.Y, conveyorVelocity.Y * 0.18f);
        }

        Vector2 next = _playerPosition;
        next.X += _playerVelocity.X * dt;
        Rect2 playBounds = _playfield.PlayBounds;
        next.X = Mathf.Clamp(next.X, playBounds.Position.X, Mathf.Max(playBounds.Position.X, playBounds.End.X - _player.Size.X));

        next.Y += _playerVelocity.Y * dt;
        ResolveVerticalCollisions(ref next);

        if (!_platformerSafetyFloor && next.Y > playBounds.End.Y + _fallDeathHeightUnits * _textUnitPixels)
        {
            KillPlayer("FALL DEATH");
            ClearPlayerShots();
            RefreshMotionText();
            return;
        }

        if (next.Y > playBounds.End.Y + _player.Size.Y)
        {
            KillPlayer("FALL DEATH");
            ClearPlayerShots();
            RefreshMotionText();
            return;
        }

        _playerPosition = next;
        _player.Position = _playerPosition;
        _contactInvulnerabilitySeconds = Mathf.Max(0, (float)_contactInvulnerabilitySeconds - dt);
        if (TryReachGoal())
            return;

        if (TryPlayerEnemyContact())
        {
            KillPlayer(PlayerContactDeathReason());
            return;
        }

        if (shootPressed && _gunEnabled)
            FirePlayerShot();

        UpdatePlayerShots(dt);
        UpdatePlayerAnimation(inputX, crawlingText);
        _player.QueueRedraw();
        RefreshMotionText();
    }

    private bool TryReachGoal()
    {
        if (_goalReached || _player is null)
            return false;

        Rect2? goalBounds = _playfield.GetGoalBounds();
        if (goalBounds is null)
            return false;

        Rect2 playerBounds = PlayerHitBounds();
        if (!playerBounds.Intersects(goalBounds.Value, true))
            return false;

        _goalReached = true;
        _platformerScore += 1000;
        _platformerStatus = "GOAL!";
        _contactInvulnerabilitySeconds = 1.0;
        ClearEnemyShots();
        _playfield.ThrowComicImpact(goalBounds.Value.GetCenter(), "GOAL", 1.9f);
        PlaySound("power-up");
        RefreshPlatformerHud("GOAL! +1000");
        return true;
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
        _shootAnimSeconds = 0.18;
        PlaySound("player-shot");
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
            if (shot.Life <= 0f || !playBounds.HasPoint(shot.Position))
            {
                _playerShots.RemoveAt(i);
            }
            else if (TryHitEnemy(shotBounds, out Vector2 enemyImpact))
            {
                AddImpactEffect(enemyImpact);
                _playerShots.RemoveAt(i);
            }
            else if (TryHitTextObject(shotBounds, out Vector2 textImpact))
            {
                AddImpactEffect(textImpact);
                _playerShots.RemoveAt(i);
            }
            else
            {
                _playerShots[i] = shot;
            }
        }

        PushShotPositionsToPlayfield();
    }

    private void UpdateEnemies(float dt)
    {
        if (_player is null || _bossMode || _playsetMode != PlaysetMode.Platformer || !_enemyAiEnabled)
            return;

        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView enemy = _actors[i];
            if (!enemy.Visible || enemy.AnimationSet is null)
                continue;

            if (enemy.HomePosition == Vector2.Zero)
                enemy.HomePosition = enemy.Position;

            if (IsFlyingEnemy(enemy))
                UpdateFlyingEnemy(enemy, dt, i);
            else
                UpdateGroundEnemy(enemy, dt);

            if (_enemyProjectilesEnabled && enemy.CanFireProjectiles && _hazardArmDelaySeconds <= 0)
            {
                float timer = _enemyShotTimers.TryGetValue(enemy, out float existing) ? existing : 0.8f + i * 0.45f;
                timer -= dt;
                if (timer <= 0f)
                {
                    FireEnemyShot(enemy);
                    timer = 1.7f + i * 0.35f;
                }

                _enemyShotTimers[enemy] = timer;
            }
        }
    }

    private void UpdateFlyingEnemy(ActorView enemy, float dt, int index)
    {
        float phase = (float)_elapsed * 1.4f + index * 2.1f;
        Vector2 patrol = new(
            Mathf.Sin(phase) * _textUnitPixels * 5.5f,
            Mathf.Sin(phase * 1.7f) * _textUnitPixels * 1.8f
        );
        if (_enemyTracksPlayer)
        {
            float chaseBias = Mathf.Clamp((_player.Position.X - enemy.HomePosition.X) * 0.18f, -_textUnitPixels * 7f, _textUnitPixels * 7f);
            patrol.X += chaseBias;
        }

        enemy.Position = enemy.HomePosition + patrol;
        if (_enemyTracksPlayer)
            enemy.FacingRight = _player.Position.X > enemy.Position.X;
        enemy.MotionState = ActorMotionState.Idle;
        enemy.AnimationClock += dt;
    }

    private void UpdateGroundEnemy(ActorView enemy, float dt)
    {
        float motionUnit = Mathf.Max(_textUnitPixels, 10f);
        Vector2 velocity = _enemyVelocities.TryGetValue(enemy, out Vector2 existingVelocity) ? existingVelocity : Vector2.Zero;
        float direction = _enemyPatrolDirections.TryGetValue(enemy, out float existingDirection) && !Mathf.IsZeroApprox(existingDirection)
            ? Mathf.Sign(existingDirection)
            : (enemy.FacingRight ? 1f : -1f);

        float patrolRange = motionUnit * 11f;
        float patrolSpeed = motionUnit * 5.2f;
        if (_enemyTracksPlayer && Mathf.Abs(_player.Position.X - enemy.Position.X) < motionUnit * 28f)
            direction = _player.Position.X >= enemy.Position.X ? 1f : -1f;
        else if (enemy.Position.X < enemy.HomePosition.X - patrolRange)
            direction = 1f;
        else if (enemy.Position.X > enemy.HomePosition.X + patrolRange)
            direction = -1f;

        velocity.X = direction * patrolSpeed;
        velocity.Y += motionUnit * 58f * _gravityScale * dt;

        Vector2 next = enemy.Position + velocity * dt;
        Rect2 playBounds = _playfield.PlayBounds;
        next.X = Mathf.Clamp(next.X, playBounds.Position.X, Mathf.Max(playBounds.Position.X, playBounds.End.X - enemy.Size.X));
        bool grounded = ResolveEnemyVerticalCollisions(enemy, ref next, ref velocity);
        if (grounded && ShouldGroundEnemyReverseAtEdge(enemy, next, direction))
        {
            direction *= -1f;
            velocity.X = direction * patrolSpeed;
            next.X = Mathf.Clamp(enemy.Position.X + velocity.X * dt, playBounds.Position.X, Mathf.Max(playBounds.Position.X, playBounds.End.X - enemy.Size.X));
            ResolveEnemyVerticalCollisions(enemy, ref next, ref velocity);
        }

        if (!_platformerSafetyFloor && next.Y > playBounds.End.Y + enemy.Size.Y)
        {
            next = enemy.HomePosition;
            velocity = Vector2.Zero;
        }

        enemy.Position = next;
        enemy.FacingRight = direction > 0;
        enemy.MotionState = grounded && Mathf.Abs(velocity.X) > motionUnit * 0.25f ? ActorMotionState.Run : ActorMotionState.Idle;
        enemy.AnimationClock += dt;
        _enemyVelocities[enemy] = velocity;
        _enemyPatrolDirections[enemy] = direction;
    }

    private bool ShouldGroundEnemyReverseAtEdge(ActorView enemy, Vector2 next, float direction)
    {
        Rect2 playBounds = _playfield.PlayBounds;
        if (direction < 0 && next.X <= playBounds.Position.X + 1f)
            return true;

        if (direction > 0 && next.X + enemy.Size.X >= playBounds.End.X - 1f)
            return true;

        return !HasGroundSupportAhead(new Rect2(next, enemy.Size), direction);
    }

    private bool HasGroundSupportAhead(Rect2 actorBounds, float direction)
    {
        float motionUnit = Mathf.Max(_textUnitPixels, 10f);
        float probeWidth = Mathf.Clamp(actorBounds.Size.X * 0.32f, 4f, motionUnit * 0.9f);
        float lookAhead = Mathf.Clamp(motionUnit * 0.35f, 3f, actorBounds.Size.X * 0.45f);
        float probeDepth = Mathf.Max(5f, motionUnit * 0.65f);
        float probeX = direction >= 0
            ? actorBounds.End.X + lookAhead - probeWidth
            : actorBounds.Position.X - lookAhead;
        Rect2 footProbe = new(new Vector2(probeX, actorBounds.End.Y - 2f), new Vector2(probeWidth, probeDepth));

        foreach (WorldObject surface in GetLineSurfaces())
        {
            float centerX = footProbe.GetCenter().X;
            if (!surface.ContainsXRange(footProbe.Position.X, footProbe.End.X, _textUnitPixels, _playfield.ElapsedSeconds))
                continue;

            float surfaceY = surface.SurfaceYAt(centerX, _textUnitPixels, _playfield.ElapsedSeconds);
            if (surfaceY >= actorBounds.End.Y - 3f && surfaceY <= footProbe.End.Y)
                return true;
        }

        foreach (Rect2 surface in GetSolidSurfaces())
        {
            if (SurfaceSupportsFootProbe(footProbe, actorBounds.End.Y, surface))
                return true;
        }

        return false;
    }

    private static bool SurfaceSupportsFootProbe(Rect2 footProbe, float actorBottom, Rect2 surface)
    {
        if (surface.Position.Y < actorBottom - 3f || surface.Position.Y > footProbe.End.Y)
            return false;

        float overlap = Mathf.Min(footProbe.End.X, surface.End.X) - Mathf.Max(footProbe.Position.X, surface.Position.X);
        return overlap > 0;
    }

    private void FireEnemyShot(ActorView enemy)
    {
        const int maxEnemyShots = 6;
        if (_enemyShots.Count >= maxEnemyShots)
            return;

        Vector2 origin = enemy.Position + enemy.Size * 0.5f;
        Vector2 target = _player.Position + _player.Size * 0.5f;
        float maxRange = Mathf.Max(_textUnitPixels * _enemyShotRangeUnits, 80f);
        if (origin.DistanceTo(target) > maxRange)
            return;

        if (_enemyTracksPlayer)
            enemy.FacingRight = target.X > origin.X;

        Vector2 direction = target - origin;
        if (direction.LengthSquared() <= 0.01f)
            direction = enemy.FacingRight ? Vector2.Right : Vector2.Left;
        else if (!_enemyTracksPlayer)
            direction = enemy.FacingRight ? Vector2.Right : Vector2.Left;
        else
            direction = direction.Normalized();

        float shotSpeed = Mathf.Max(_textUnitPixels * 28f, 190f);
        float shotLife = Mathf.Clamp(maxRange / shotSpeed, 0.45f, 2.8f);
        _enemyShots.Add(new EnemyShot(origin, direction * shotSpeed, shotLife, 0f, enemy.ActorName));
        _platformerStatus = $"{enemy.ActorName.ToUpperInvariant()} FIRED";
        RefreshPlatformerHud();
        PushEnemyShotPositionsToPlayfield();
    }

    private string EnemyRangeButtonText()
    {
        string label = _enemyShotRangeUnits < 28f ? "Near" : _enemyShotRangeUnits < 45f ? "Mid" : "Far";
        return $"Range: {label}";
    }

    private string PlayerShotPowerButtonText() => $"Gun Power: {_playerShotPower}x";

    private void AdjustSelectedEnemyToughness(int delta)
    {
        if (_selectedActor is null || _selectedActor.IsPlayable)
        {
            _inspectorText.Text = "Select an enemy first, then adjust shot toughness.";
            return;
        }

        _selectedActor.ShotToughness = Mathf.Clamp(_selectedActor.ShotToughness + delta, 1, 9);
        _enemyHealth.Remove(_selectedActor);
        _inspectorText.Text = $"{_selectedActor.ActorName} shot toughness set to {_selectedActor.ShotToughness}.\n\nThis means {_selectedActor.ShotToughness} regular 1x shot(s), or fewer with stronger guns.";
        RefreshPlatformerHud();
    }

    private void PushImpactEffectsToPlayfield()
    {
        EffectVisual[] visuals = new EffectVisual[_impactEffects.Count];
        for (int i = 0; i < _impactEffects.Count; i++)
        {
            int frame = Mathf.Clamp(1 + Mathf.FloorToInt(_impactEffects[i].Age / 0.052f), 1, 12);
            visuals[i] = new EffectVisual(_impactEffects[i].Position, frame);
        }

        _playfield.SetImpactEffects(visuals);
        _combatFxOverlay?.SetImpactEffects(visuals);
    }

    private void UpdateEnemyShots(float dt)
    {
        if (_enemyShots.Count == 0)
            return;

        Rect2 playBounds = _playfield.PlayBounds.Grow(20f);
        Rect2 playerBounds = PlayerHitBounds();
        for (int i = _enemyShots.Count - 1; i >= 0; i--)
        {
            EnemyShot shot = _enemyShots[i];
            shot.Position += shot.Velocity * dt;
            shot.Life -= dt;
            shot.Age += dt;
            Rect2 shotBounds = new(shot.Position - new Vector2(4f, 4f), new Vector2(8f, 8f));
            if (shot.Life <= 0f || !playBounds.HasPoint(shot.Position))
            {
                _enemyShots.RemoveAt(i);
            }
            else if (shot.Age > 0.32f && shotBounds.Intersects(playerBounds))
            {
                AddImpactEffect(playerBounds.GetCenter());
                _enemyShots.RemoveAt(i);
                DamagePlayer($"{shot.OwnerName.ToUpperInvariant()} SHOT", _enemyShotDamage);
            }
            else
            {
                _enemyShots[i] = shot;
            }
        }

        PushEnemyShotPositionsToPlayfield();
    }

    private void AddImpactEffect(Vector2 position)
    {
        _impactEffects.Add(new ImpactEffect(position, 0f));
        ApplyExplosionTextBlast(position);
        PushImpactEffectsToPlayfield();
    }

    private void ApplyExplosionTextBlast(Vector2 position)
    {
        if (!_explosionsDamageText || !_textDestructionEnabled || !_playfield.HasCapturedPage)
            return;

        float radius = Mathf.Max(_textUnitPixels * 4.8f, 34f);
        Rect2 blastBounds = new(position - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));
        int removed = 0;
        foreach (Rect2 letter in _playfield.GetTextObjectRegions(TextObjectGranularity.Letter))
        {
            if (removed >= 9)
                break;

            if (!blastBounds.Intersects(letter))
                continue;

            Vector2 offset = letter.GetCenter() - position;
            if (offset.LengthSquared() > radius * radius)
                continue;

            float chance = 0.82f - offset.Length() / radius * 0.45f;
            float hash = Mathf.Abs(Mathf.Sin(letter.Position.X * 12.9898f + letter.Position.Y * 78.233f + (float)_elapsed * 4.113f));
            if (hash > chance)
                continue;

            _playfield.EraseDocumentText(letter.Grow(2.2f));
            removed++;
        }

        if (removed > 0)
        {
            _playfield.ThrowRandomLetters(position, removed);
            _platformerScore += removed * 5;
            _platformerStatus = $"BLAST -{removed} LETTERS";
            RefreshPlatformerHud();
        }
    }

    private void UpdateImpactEffects(float dt)
    {
        if (_impactEffects.Count == 0)
            return;

        for (int i = _impactEffects.Count - 1; i >= 0; i--)
        {
            ImpactEffect effect = _impactEffects[i];
            effect.Age += dt;
            if (effect.Age > 0.62f)
                _impactEffects.RemoveAt(i);
            else
                _impactEffects[i] = effect;
        }

        PushImpactEffectsToPlayfield();
    }

    private bool TryPlayerEnemyContact()
    {
        if (!_enemyAiEnabled || _hazardArmDelaySeconds > 0 || _contactInvulnerabilitySeconds > 0 || _deathTestSeconds > 0)
            return false;

        Rect2 playerBounds = PlayerHitBounds();
        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView enemy = _actors[i];
            if (!enemy.Visible || enemy.AnimationSet is null)
                continue;

            Rect2 enemyBounds = EnemyHitBounds(enemy);
            Rect2 overlap = playerBounds.Intersection(enemyBounds);
            float overlapArea = overlap.Size.X * overlap.Size.Y;
            float playerArea = playerBounds.Size.X * playerBounds.Size.Y;
            float enemyArea = enemyBounds.Size.X * enemyBounds.Size.Y;
            if (overlapArea > Mathf.Min(playerArea, enemyArea) * 0.12f)
                return true;
        }

        return false;
    }

    private string PlayerContactDeathReason()
    {
        Rect2 playerBounds = PlayerHitBounds();
        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView enemy = _actors[i];
            if (!enemy.Visible || enemy.AnimationSet is null)
                continue;

            Rect2 enemyBounds = EnemyHitBounds(enemy);
            if (!playerBounds.Intersects(enemyBounds))
                continue;

            Rect2 overlap = playerBounds.Intersection(enemyBounds);
            float overlapArea = overlap.Size.X * overlap.Size.Y;
            float playerArea = playerBounds.Size.X * playerBounds.Size.Y;
            float enemyArea = enemyBounds.Size.X * enemyBounds.Size.Y;
            if (overlapArea > Mathf.Min(playerArea, enemyArea) * 0.12f)
                return $"{enemy.ActorName.ToUpperInvariant()} CONTACT";
        }

        return "ENEMY CONTACT";
    }

    private bool TryHitEnemy(Rect2 shotBounds, out Vector2 impactPosition)
    {
        impactPosition = shotBounds.GetCenter();
        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView enemy = _actors[i];
            if (!enemy.Visible || enemy.AnimationSet is null)
                continue;

            Rect2 enemyBounds = EnemyHitBounds(enemy);
            if (!shotBounds.Intersects(enemyBounds))
                continue;

            impactPosition = enemyBounds.GetCenter();
            DamageEnemy(enemy, _playerShotPower);
            return true;
        }

        return false;
    }

    private void DamageEnemy(ActorView enemy, int amount)
    {
        int currentHealth = _enemyHealth.TryGetValue(enemy, out int existing) ? existing : Mathf.Clamp(enemy.ShotToughness, 1, 9);
        int shotPower = Mathf.Max(1, amount);
        currentHealth -= shotPower;
        Vector2 impact = enemy.Position + enemy.Size * 0.5f;

        if (currentHealth <= 0)
        {
            _enemyHealth.Remove(enemy);
            _defeatedEnemies.Add(enemy);
            enemy.Visible = false;
            _enemyVelocities.Remove(enemy);
            _enemyPatrolDirections.Remove(enemy);
            _enemyShotTimers.Remove(enemy);
            _platformerScore += 250;
            _platformerStatus = $"DEFEATED {enemy.ActorName.ToUpperInvariant()}";
            _playfield.ThrowComicImpact(impact, RandomComicWord("defeat"), 1.75f);
            PlaySound("enemy-defeat");
        }
        else
        {
            _enemyHealth[enemy] = currentHealth;
            _platformerScore += 50;
            _platformerStatus = $"{enemy.ActorName.ToUpperInvariant()} {currentHealth}/{enemy.ShotToughness}";
            _playfield.ThrowComicImpact(impact, RandomComicWord("hit"), 1.15f);
            PlaySound("enemy-hit");
        }

        RefreshPlatformerHud();
    }

    private static int DefaultEnemyShotToughness(string actorName)
    {
        if (actorName.Contains("Boss", StringComparison.OrdinalIgnoreCase))
            return 4;
        if (actorName.Contains("Dragon", StringComparison.OrdinalIgnoreCase))
            return 2;
        if (actorName.Contains("Fleet", StringComparison.OrdinalIgnoreCase))
            return 2;

        return 1;
    }

    private void DamagePlayer(string reason, int amount)
    {
        if (_editorMode || _playsetMode != PlaysetMode.Platformer)
            return;

        if (_deathTestSeconds > 0 || _contactInvulnerabilitySeconds > 0)
            return;

        if (_partialDamageEnabled)
        {
            _playerHealth = Mathf.Max(0, _playerHealth - Mathf.Max(1, amount));
            _platformerStatus = $"{reason} -{amount}";
            _contactInvulnerabilitySeconds = 0.55;
            _playfield.ThrowComicImpact(_player.Position + _player.Size * 0.5f, RandomComicWord("hurt"), 1.25f);
            if (_playerHealth > 0)
            {
                PlaySound("player-hurt");
                RefreshPlatformerHud();
                return;
            }
        }

        LosePlayerLife(reason);
    }

    private void KillPlayer(string reason)
    {
        if (_editorMode || _playsetMode != PlaysetMode.Platformer)
            return;

        if (_deathTestSeconds > 0 || _contactInvulnerabilitySeconds > 0)
            return;

        LosePlayerLife(reason);
    }

    private void LosePlayerLife(string reason)
    {
        _platformerLives = Mathf.Max(0, _platformerLives - 1);
        _playerHealth = _playerMaxHealth;
        _platformerDeaths++;
        _platformerStatus = reason;
        _contactInvulnerabilitySeconds = 1.0;
        ClearPlayerShots();
        ClearEnemyShots();
        _playfield.ThrowComicImpact(_player.Position + _player.Size * 0.5f, ComicWordForPlayerLoss(reason), 1.65f);
        PlaySound("player-hurt");
        TriggerDeathAnimation();
        RefreshPlatformerHud(reason);
    }

    private string RandomComicWord(string kind)
    {
        string[] words = kind switch
        {
            "defeat" => ["BOOM", "KABOOM", "KAPOW", "WHAM", "BLAMMO"],
            "hit" => ["BANG", "POW", "THWACK", "ZAP", "SMACK"],
            "hurt" => ["OUCH", "OOF", "OW", "YIPE", "BONK"],
            _ => ["POW"]
        };

        int seed = Mathf.Abs(Mathf.RoundToInt((float)(_elapsed * 97.0))) + _platformerScore * 7 + _platformerDeaths * 19;
        return words[seed % words.Length];
    }

    private static string ComicWordForPlayerLoss(string reason)
    {
        string upper = reason.ToUpperInvariant();
        if (upper.Contains("FALL") || upper.Contains("GUTTER") || upper.Contains("PIT") || upper.Contains("OUT"))
            return "OUCH";
        if (upper.Contains("SHOT") || upper.Contains("LASER"))
            return "ZAP";
        if (upper.Contains("CONTACT") || upper.Contains("GUARD") || upper.Contains("DRAGON"))
            return "BONK";

        return "OUCH";
    }

    private Rect2 PlayerHitBounds()
    {
        return new Rect2(_player.Position, _player.Size).Grow(-Mathf.Min(_player.Size.X, _player.Size.Y) * 0.18f);
    }

    private static Rect2 EnemyHitBounds(ActorView enemy)
    {
        return new Rect2(enemy.Position, enemy.Size).Grow(-Mathf.Min(enemy.Size.X, enemy.Size.Y) * 0.18f);
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

    private bool TryHitTextObject(Rect2 shotBounds, out Vector2 impactPosition)
    {
        impactPosition = shotBounds.GetCenter();
        if (!_textDestructionEnabled || !_playfield.HasCapturedPage)
            return false;

        foreach (Rect2 letter in _playfield.GetTextObjectRegions(TextObjectGranularity.Letter))
        {
            if (!shotBounds.Intersects(letter))
                continue;

            impactPosition = letter.GetCenter();
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

    private void ClearEnemyShots()
    {
        if (_enemyShots.Count == 0)
            return;

        _enemyShots.Clear();
        PushEnemyShotPositionsToPlayfield();
    }

    private void PushEnemyShotPositionsToPlayfield()
    {
        Vector2[] positions = new Vector2[_enemyShots.Count];
        for (int i = 0; i < _enemyShots.Count; i++)
            positions[i] = _enemyShots[i].Position;

        _playfield.SetEnemyShotPositions(positions);
    }

    private void RefreshPlatformerHud(string? status = null)
    {
        if (_platformerHudText is null)
            return;

        if (!string.IsNullOrWhiteSpace(status))
            _platformerStatus = status;

        _platformerHud.Visible = _playsetMode == PlaysetMode.Platformer;
        int visibleEnemies = _actors.Skip(1).Count(actor => actor.Visible && actor.AnimationSet is not null);
        _platformerHudText.Text =
            $"SCORE  {_platformerScore}\n"
            + $"LIVES  {_platformerLives}   HP {_playerHealth}/{_playerMaxHealth}   DEATHS {_platformerDeaths}\n"
            + $"ENEMY  {visibleEnemies}   SHOTS {_enemyShots.Count}   RNG {_enemyShotRangeUnits:0}\n"
            + _platformerStatus;
    }

    private void UpdatePlayerAnimation(float inputX, bool crawlingText)
    {
        _player.AnimationClock = _elapsed;

        if (crawlingText)
        {
            _player.MotionState = ActorMotionState.Crawl;
            return;
        }

        if (_gunEnabled && _shootAnimSeconds > 0)
        {
            _player.MotionState = _playerOnGround ? ActorMotionState.RunShoot : ActorMotionState.JumpShoot;
            return;
        }

        if (!_playerOnGround)
        {
            _player.MotionState = _playerVelocity.Y < 0 ? ActorMotionState.JumpUp : ActorMotionState.Fall;
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

        foreach (WorldObject surface in GetLineSurfaces())
        {
            float centerX = next.X + _player.Size.X * 0.5f;
            float surfaceY = surface.SurfaceYAt(centerX, _textUnitPixels, _playfield.ElapsedSeconds);
            if (_playerVelocity.Y >= 0
                && previousBottom <= surfaceY + _textUnitPixels * 0.4f
                && nextBounds.End.Y >= surfaceY
                && surface.ContainsXRange(nextBounds.Position.X, nextBounds.End.X, _textUnitPixels, _playfield.ElapsedSeconds)
                && HasEnoughLandingSupport(nextBounds, surface.Bounds(_textUnitPixels, _playfield.ElapsedSeconds)))
            {
                next.Y = surfaceY - _player.Size.Y;
                _playerVelocity.Y = 0;
                _playerOnGround = true;
                nextBounds = new Rect2(next, _player.Size);
            }
        }

        if (!_playerOnGround)
        {
            foreach (Rect2 surface in GetSolidSurfaces())
            {
                if (_playerVelocity.Y >= 0
                    && previousBottom <= surface.Position.Y + 2f
                    && nextBounds.End.Y >= surface.Position.Y
                    && nextBounds.Position.X < surface.End.X
                    && nextBounds.End.X > surface.Position.X
                    && HasEnoughLandingSupport(nextBounds, surface))
                {
                    next.Y = surface.Position.Y - _player.Size.Y;
                    _playerVelocity.Y = 0;
                    _playerOnGround = true;
                    nextBounds = new Rect2(next, _player.Size);
                }
            }
        }

        if (next.Y < 30)
        {
            next.Y = 30;
            _playerVelocity.Y = 0;
        }
    }

    private bool ResolveEnemyVerticalCollisions(ActorView enemy, ref Vector2 next, ref Vector2 velocity)
    {
        bool grounded = false;
        Rect2 nextBounds = new(next, enemy.Size);
        float previousBottom = enemy.Position.Y + enemy.Size.Y;

        foreach (WorldObject surface in GetLineSurfaces())
        {
            float centerX = next.X + enemy.Size.X * 0.5f;
            float surfaceY = surface.SurfaceYAt(centerX, _textUnitPixels, _playfield.ElapsedSeconds);
            if (velocity.Y >= 0
                && previousBottom <= surfaceY + _textUnitPixels * 0.4f
                && nextBounds.End.Y >= surfaceY
                && surface.ContainsXRange(nextBounds.Position.X, nextBounds.End.X, _textUnitPixels, _playfield.ElapsedSeconds)
                && HasEnoughLandingSupport(nextBounds, surface.Bounds(_textUnitPixels, _playfield.ElapsedSeconds)))
            {
                next.Y = surfaceY - enemy.Size.Y;
                velocity.Y = 0;
                grounded = true;
                nextBounds = new Rect2(next, enemy.Size);
            }
        }

        if (!grounded)
        {
            foreach (Rect2 surface in GetSolidSurfaces())
            {
                if (velocity.Y >= 0
                    && previousBottom <= surface.Position.Y + 2f
                    && nextBounds.End.Y >= surface.Position.Y
                    && nextBounds.Position.X < surface.End.X
                    && nextBounds.End.X > surface.Position.X
                    && HasEnoughLandingSupport(nextBounds, surface))
                {
                    next.Y = surface.Position.Y - enemy.Size.Y;
                    velocity.Y = 0;
                    grounded = true;
                    nextBounds = new Rect2(next, enemy.Size);
                }
            }
        }

        return grounded;
    }

    private static bool IsFlyingEnemy(ActorView enemy)
    {
        string name = enemy.ActorName;
        return name.Contains("Dragon", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Ship", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Fleet", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Boss", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Fly", StringComparison.OrdinalIgnoreCase);
    }

    private bool HasEnoughLandingSupport(Rect2 actorBounds, Rect2 surface)
    {
        float overlap = Mathf.Min(actorBounds.End.X, surface.End.X) - Mathf.Max(actorBounds.Position.X, surface.Position.X);
        if (overlap <= 0)
            return false;

        float required = Mathf.Clamp(_minLandingSupportRatio, 0.05f, 1f) * actorBounds.Size.X;
        return overlap >= required;
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
            foreach (Rect2 word in _playfield.GetTextObjectRegions(TextObjectGranularity.Word))
                yield return new Rect2(
                    word.Position + new Vector2(-_textUnitPixels * 0.18f, 2f),
                    new Vector2(word.Size.X + _textUnitPixels * 0.36f, Mathf.Max(2f, Mathf.Min(word.Size.Y, _textUnitPixels * 0.45f)))
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
        Vector2 playerSize = new(
            Mathf.Round(_textUnitPixels * 3.0f * _actorSizeMultiplier),
            Mathf.Round(_textUnitPixels * 4.6f * _actorSizeMultiplier)
        );
        Vector2 cardSize = new(_textUnitPixels * 6.5f, _textUnitPixels * 7f);

        for (int i = 0; i < _actors.Count; i++)
        {
            _actors[i].Size = i == 0 ? playerSize : cardSize;
            _actors[i].CustomMinimumSize = _actors[i].Size;
        }
    }

    private void SetActorSizeMultiplier(float multiplier)
    {
        if (_selectedActor is not null && !ReferenceEquals(_selectedActor, _player))
        {
            ScaleSelectedEnemy(multiplier);
            return;
        }

        _actorSizeMultiplier = multiplier;
        ApplyActorScale();
        SnapPlayerToStart();
        _playfield.QueueRedraw();
        RefreshMotionText();
    }

    private void ScaleSelectedEnemy(float multiplier)
    {
        float baseHeight = Mathf.Max(_textUnitPixels * 7f, 1f);
        float currentBottom = _selectedActor.Position.Y + _selectedActor.Size.Y;
        float aspect = _selectedActor.Size.X > 0 && _selectedActor.Size.Y > 0
            ? _selectedActor.Size.X / _selectedActor.Size.Y
            : 1f;
        float newHeight = Mathf.Round(baseHeight * multiplier);
        float newWidth = Mathf.Round(newHeight * aspect);
        _selectedActor.Size = new Vector2(newWidth, newHeight);
        _selectedActor.CustomMinimumSize = _selectedActor.Size;
        _selectedActor.Position = new Vector2(_selectedActor.Position.X, currentBottom - newHeight);
        _selectedActor.ManualPlacement = true;
        _selectedActor.QueueRedraw();
        RefreshMotionText();
        _inspectorText.Text = $"{_selectedActor.ActorName} scaled to {multiplier:0.##}x enemy size.\n\nActor size buttons affect the selected enemy when an enemy is selected; select the player to tune the player/text ratio.";
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
        if (!_editorMode || _playfield.HasStartMarker() || !_player.ManualPlacement)
            SnapPlayerToStart();

        _playfield.QueueRedraw();
    }

    private void RefreshMotionText()
    {
        if (_motionLabel is null || _player is null)
            return;

        string mode = _platformerMode == PlatformerMode.Horizontal ? "horizontal run" : "vertical climb";
        string ground = _playerOnGround ? "grounded" : "airborne";
        _motionLabel.Text = $"{mode}  |  text unit {_textUnitPixels:0}px  |  actor {_player.Size.Y:0}px tall ({_actorSizeMultiplier:0.##}x)  |  gravity {_gravityScale:0.00}x  |  support {_minLandingSupportRatio * 100f:0}%  |  fall death {_fallDeathHeightUnits:0}u  |  {(_gunEnabled ? "gun" : "no gun")}  |  {ground}";
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
            _speedSlider.MinValue = -180;
            _speedSlider.MaxValue = 180;
            _speedSlider.Value = 0;
            _thicknessSlider.MaxValue = 3.0;
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
        _speedSlider.MinValue = -180;
        _speedSlider.MaxValue = 180;
        _thicknessSlider.Editable = true;
        _thicknessSlider.MaxValue = selected.Kind == WorldObjectKind.Ladder ? 2.5 : 3.0;
        _rangeSlider.Editable = selected.Kind == WorldObjectKind.Elevator;
        _opacitySlider.Editable = true;
        _tintPicker.Disabled = false;
        _customTintCheck.Disabled = false;
        _customTintCheck.ButtonPressed = selected.UseCustomTint;
        _speedSlider.Value = selected.SpeedUnits;
        _thicknessSlider.Value = Mathf.Clamp(selected.ThicknessUnits, 0.3f, (float)_thicknessSlider.MaxValue);
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
            + "A/B endpoints rotate and scale line tools. Rotate nudges most selected objects around their center. Ladders stay vertical and use thickness as climb width.";
        _updatingAttributeControls = false;
    }

    private static void EnsureInputActions()
    {
        EnsureAction("dack_left", Key.A, Key.Left);
        EnsureAction("dack_right", Key.D, Key.Right);
        EnsureAction("dack_up", Key.W, Key.Up);
        EnsureAction("dack_down", Key.S, Key.Down);
        EnsureAction("dack_jump", Key.Space);
        EnsureAction("dack_shoot", Key.J, Key.X);
    }

    private static void EnsureAction(string actionName, params Key[] keys)
    {
        StringName action = actionName;
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);
        else
            InputMap.ActionEraseEvents(action);

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

    private static HBoxContainer ButtonRow(params Button[] buttons)
    {
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 6);
        foreach (Button button in buttons)
        {
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(button);
        }

        return row;
    }

    private static HBoxContainer BuildClipRangeRow(string labelText, out SpinBox start, out SpinBox end, int defaultStart, int defaultEnd, int maxFrame)
    {
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 6);

        Label label = new()
        {
            Text = labelText,
            CustomMinimumSize = new Vector2(64, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", new Color("#202A34"));
        row.AddChild(label);

        start = ClipEndpointSpin(defaultStart, maxFrame);
        end = ClipEndpointSpin(defaultEnd, maxFrame);
        row.AddChild(start);
        row.AddChild(end);
        return row;
    }

    private static SpinBox ClipEndpointSpin(int value, int maxFrame)
    {
        SpinBox spin = new()
        {
            MinValue = 0,
            MaxValue = Mathf.Max(0, maxFrame),
            Value = Mathf.Clamp(value, 0, Mathf.Max(0, maxFrame)),
            Step = 1,
            CustomMinimumSize = new Vector2(70, 32),
            FocusMode = FocusModeEnum.Click
        };
        spin.GetLineEdit().FocusMode = FocusModeEnum.Click;
        spin.GetLineEdit().SelectAllOnFocus = true;
        return spin;
    }

    private static LineEdit ClipEndpointEdit(int value, int maxFrame)
    {
        LineEdit edit = new()
        {
            Text = Mathf.Clamp(value, 0, Mathf.Max(0, maxFrame)).ToString(),
            PlaceholderText = "-",
            CustomMinimumSize = new Vector2(70, 32),
            FocusMode = FocusModeEnum.Click,
            SelectAllOnFocus = true,
            TooltipText = "Frame number, or '-' if this character does not use this animation."
        };
        edit.AddThemeFontSizeOverride("font_size", 12);
        return edit;
    }

    private Button ShelfButton(string text, WorldObjectKind kind, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Platformer);
            _playfield.AddPlacedObject(kind);
            SyncEditorModeToScene();
            _inspectorText.Text = $"{text} placed.\n\n{description}\n\nDrag either A/B endpoint handle on the playfield to move, scale, or angle it.";
            RefreshCockpitStatus();
        };
        return button;
    }

    private Button PinballShelfButton(string text, WorldObjectKind kind, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Pinball);
            _playfield.AddPlacedObject(kind);
            _inspectorText.Text = $"{text} placed.\n\n{description}\n\nThis is a pinball construction placeholder: draggable now, physics binding later.";
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
            WorldObjectKind.GoalPoint => new Color("#F4C95D"),
            WorldObjectKind.HiddenSwitch => new Color("#FF2BD6"),
            WorldObjectKind.Checkpoint => new Color("#5CB8A7"),
            WorldObjectKind.PinballFlipper => new Color("#FF5C35"),
            WorldObjectKind.PinballBumper => new Color("#5CB8A7"),
            WorldObjectKind.PinballPlunger => new Color("#B56CFF"),
            WorldObjectKind.PinballDrain => new Color("#202A34"),
            WorldObjectKind.PinballRollover => new Color("#F4C95D"),
            WorldObjectKind.PinballGate => new Color("#5CB8FF"),
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

    private struct EnemyShot(Vector2 position, Vector2 velocity, float life, float age, string ownerName)
    {
        public Vector2 Position = position;
        public Vector2 Velocity = velocity;
        public float Life = life;
        public float Age = age;
        public string OwnerName = ownerName;
    }

    private struct ImpactEffect(Vector2 position, float age)
    {
        public Vector2 Position = position;
        public float Age = age;
    }

    private sealed class DackAnimManifest
    {
        public string sourceId { get; set; } = "";
        public int frameNumberBase { get; set; }
        public List<DackAnimLabel> labels { get; set; } = [];
    }

    private sealed class DackAnimLabel
    {
        public string name { get; set; } = "";
        public bool unavailable { get; set; }
        public int editorStart { get; set; }
        public int editorEnd { get; set; }
        public bool pingPong { get; set; }
        public bool strobe { get; set; }
        public int strobeCount { get; set; }
    }

    private sealed class DackLevelManifest
    {
        public string format { get; set; } = "dacklevel";
        public int version { get; set; } = 1;
        public string name { get; set; } = "";
        public string sourceMode { get; set; } = "";
        public bool editorMode { get; set; } = true;
        public string playsetMode { get; set; } = "";
        public string platformerMode { get; set; } = "";
        public bool safetyFloor { get; set; }
        public bool textTerrainEnabled { get; set; }
        public bool textDestructionEnabled { get; set; }
        public bool gunEnabled { get; set; }
        public bool enemyAiEnabled { get; set; } = true;
        public bool enemyTracksPlayer { get; set; } = true;
        public bool enemyProjectilesEnabled { get; set; } = true;
        public bool partialDamageEnabled { get; set; } = true;
        public float enemyShotRangeUnits { get; set; } = 34f;
        public int playerMaxHealth { get; set; } = 3;
        public int playerHealth { get; set; } = 3;
        public int enemyShotDamage { get; set; } = 1;
        public int playerShotPower { get; set; } = 1;
        public bool explosionsDamageText { get; set; } = true;
        public int platformerScore { get; set; }
        public int platformerLives { get; set; } = 3;
        public int platformerDeaths { get; set; }
        public float hudX { get; set; } = 18f;
        public float hudY { get; set; } = 78f;
        public float actorSizeMultiplier { get; set; }
        public float textUnitPixels { get; set; }
        public List<LevelWorldObject> worldObjects { get; set; } = [];
        public List<LevelActor> actors { get; set; } = [];
    }

    private sealed class LevelWorldObject
    {
        public string kind { get; set; } = "";
        public string markerRole { get; set; } = "";
        public float startX { get; set; }
        public float startY { get; set; }
        public float endX { get; set; }
        public float endY { get; set; }
        public float thicknessUnits { get; set; }
        public float speedUnits { get; set; }
        public float phase { get; set; }
        public float rangeUnits { get; set; }
        public bool visibleInPlay { get; set; }
        public bool useCustomTint { get; set; }
        public string tint { get; set; } = "";
        public float opacity { get; set; }

        public static LevelWorldObject FromWorldObject(WorldObject worldObject)
        {
            return new LevelWorldObject
            {
                kind = worldObject.Kind.ToString(),
                markerRole = worldObject.MarkerRole.ToString(),
                startX = worldObject.Start.X,
                startY = worldObject.Start.Y,
                endX = worldObject.End.X,
                endY = worldObject.End.Y,
                thicknessUnits = worldObject.ThicknessUnits,
                speedUnits = worldObject.SpeedUnits,
                phase = worldObject.Phase,
                rangeUnits = worldObject.RangeUnits,
                visibleInPlay = worldObject.VisibleInPlay,
                useCustomTint = worldObject.UseCustomTint,
                tint = worldObject.Tint.ToHtml(false),
                opacity = worldObject.Opacity
            };
        }

        public WorldObject ToWorldObject()
        {
            WorldObjectKind parsedKind = Enum.TryParse(kind, out WorldObjectKind worldObjectKind)
                ? worldObjectKind
                : WorldObjectKind.Platform;
            MarkerRole parsedRole = Enum.TryParse(markerRole, out MarkerRole role)
                ? role
                : MarkerRole.None;
            Color parsedTint = string.IsNullOrWhiteSpace(tint) ? default : new Color("#" + tint);

            return new WorldObject(
                parsedKind,
                new Vector2(startX, startY),
                new Vector2(endX, endY),
                thicknessUnits,
                speedUnits,
                phase,
                rangeUnits,
                parsedRole,
                visibleInPlay,
                useCustomTint,
                parsedTint,
                opacity
            );
        }
    }

    private sealed class LevelActor
    {
        public int index { get; set; }
        public string name { get; set; } = "";
        public string animationSourceId { get; set; } = "";
        public string animationSource { get; set; } = "";
        public string motionState { get; set; } = "";
        public bool visible { get; set; }
        public bool playable { get; set; }
        public bool facingRight { get; set; }
        public bool manualPlacement { get; set; }
        public int shotToughness { get; set; } = 1;
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }

        public static LevelActor FromActor(ActorView actor, int index)
        {
            return new LevelActor
            {
                index = index,
                name = actor.ActorName,
                animationSourceId = GuessAnimationSource(actor),
                animationSource = GuessAnimationSource(actor),
                motionState = actor.MotionState.ToString(),
                visible = actor.Visible,
                playable = actor.IsPlayable,
                facingRight = actor.FacingRight,
                manualPlacement = actor.ManualPlacement,
                shotToughness = actor.ShotToughness,
                x = actor.Position.X,
                y = actor.Position.Y,
                width = actor.Size.X,
                height = actor.Size.Y
            };
        }

        private static string GuessAnimationSource(ActorView actor)
        {
            if (!string.IsNullOrWhiteSpace(actor.AnimationSourceId))
                return actor.AnimationSourceId;

            return GuessAnimationSourceId(actor.ActorName, actor.IsPlayable);
        }
    }

    private sealed record TgcClipRow(
        HBoxContainer Row,
        LineEdit Name,
        LineEdit Start,
        LineEdit End,
        CheckBox PingPong,
        CheckBox Strobe,
        SpinBox StrobeCount
    );
}
