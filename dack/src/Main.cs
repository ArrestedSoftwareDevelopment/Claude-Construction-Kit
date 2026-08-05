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
    private readonly DackUiState _uiState = new();
    private readonly RandomNumberGenerator _enemyPlacementRandom = new();
    private readonly List<PlayerShot> _playerShots = [];
    private readonly List<EnemyShot> _enemyShots = [];
    private readonly List<ImpactEffect> _impactEffects = [];
    private readonly Dictionary<ActorView, float> _enemyShotTimers = [];
    private readonly Dictionary<ActorView, Vector2> _enemyVelocities = [];
    private readonly Dictionary<ActorView, float> _enemyPatrolDirections = [];
    private readonly Dictionary<ActorView, int> _enemyHealth = [];
    private readonly HashSet<ActorView> _defeatedEnemies = [];
    private readonly Dictionary<string, AudioStreamPlayer> _soundPlayers = [];
    private readonly Dictionary<string, string> _soundCardBindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _legacySoundFallbacks = new(StringComparer.OrdinalIgnoreCase);
    private SoundCardPlayer _soundCardPlayer = null!;
    private int _kenneyAuditionSourcesLoaded;
    private readonly Vector2[] _actorAnchors =
    [
        new(0.16f, 0.66f),
        new(0.43f, 0.34f),
        new(0.70f, 0.62f)
    ];

    private Control _workspace = null!;
    private Control _bossOverlay = null!;
    private PanelContainer _playfieldFrame = null!;
    private PlayfieldSurface _playfield = null!;
    private CombatFxOverlay _combatFxOverlay = null!;
    private SpritePad _spritePad = null!;
    private Label _selectionLabel = null!;
    private LineEdit _characterNameEdit = null!;
    private Label _bindingLabel = null!;
    private Label _focusedClipLabel = null!;
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
    private Control _launchScreen = null!;
    private TextureRect _launchLogo = null!;
    private Label _launchHint = null!;
    private PanelContainer _cockpit = null!;
    private TabContainer _cockpitTabs = null!;
    private Control _platformerPanel = null!;
    private Control _brickbatPanel = null!;
    private Control _pinballPanel = null!;
    private Control _overheadPanel = null!;
    private Label _legacyLibraryStatus = null!;
    private List<LegacyAssetBundle> _legacyBundles = [];
    private CharacterPreviewPanel _characterPreview = null!;
    private CharacterPreviewPanel _spriteEditorPreview = null!;
    private Label _characterWorkbenchStatus = null!;
    private readonly List<Button> _editorPlayButtons = [];
    private Label _cockpitStatus = null!;
    private bool _sessionDirty;
    private string _sessionDirtyReason = "";
    private Button _transportModeButton = null!;
    private Button _transportFreezeButton = null!;
    private Button _transportStopButton = null!;
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
    private SpriteFrame[] _animationEditorFrames = [];
    private SpriteFrame[] _focusedClipFrames = [];
    private TgcClipRow? _focusedClipRow;
    private int _focusedClipFrameIndex = -1;
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
        "Punch",
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
    private bool _desktopParked;
    private bool _launchScreenActive = true;
    private double _launchScreenClock;
    private double _elapsed;
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private bool _playerOnGround;
    private bool _platformerSafetyFloor = true;
    private bool _textTerrainEnabled = true;
    private bool _textDestructionEnabled = true;
    private bool _gunEnabled = true;
    private bool _editorMode = true;
    private bool _simulationFrozen = true;
    private bool _simulationStopped;

    private const int FileOpenCommand = 1;
    private const int FileSaveCommand = 2;
    private const int FileSnapshotCommand = 3;
    private const int FileResetCommand = 4;
    private const int FileSnapshotHistoryCommand = 5;
    private const int FileDesktopCommand = 6;
    private const int TransportRunCommand = 1;
    private const int TransportFreezeCommand = 2;
    private const int TransportStopCommand = 3;
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
    private double _punchPreviewSeconds;
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
        UpdateLaunchScreen((float)delta);
        if (_cockpit is not null && _cockpit.Visible)
        {
            FitCockpitToViewport();
            RefreshCockpitStatus();
        }

        if (_editorMode)
        {
            // Build mode remains interactive even while simulation is frozen:
            // placement, selection, and animation previews still need to update.
            UpdateActorPresentation((float)delta);
            UpdatePunchPreview(delta);
        }
        else if (!_simulationFrozen)
        {
            UpdatePlayer(delta);
            UpdateOverheadPlayer(delta);
            UpdateActorPresentation((float)delta);
            UpdatePunchPreview(delta);

            if (!_editorMode)
            {
                UpdateEnemies((float)delta);
                UpdateEnemyShots((float)delta);
            }

            UpdateImpactEffects((float)delta);
        }

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
            DismissLaunchScreen();
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
        else if (@event is InputEventKey playModeKey
                 && playModeKey.Pressed
                 && !playModeKey.Echo
                 && playModeKey.Keycode == Key.F6)
        {
            DismissLaunchScreen();
            ToggleBuildPlayMode();
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventKey freezeKey
                 && freezeKey.Pressed
                 && !freezeKey.Echo
                 && freezeKey.Keycode == Key.F7)
        {
            DismissLaunchScreen();
            ToggleSimulationFreeze();
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventKey escKey
                 && escKey.Pressed
                 && !escKey.Echo
                 && escKey.Keycode == Key.Escape)
        {
            DismissLaunchScreen();
            if (_sidebar is not null && _sidebar.Visible)
                CloseSpritePanel();
            else
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
            Text = "RAD 01   â€¢   LOCAL PLAYFIELD",
            VerticalAlignment = VerticalAlignment.Center
        };
        status.AddThemeColorOverride("font_color", new Color("#AAB7C4"));
        status.AddThemeFontSizeOverride("font_size", 13);
        headerRow.AddChild(status);

        Button bossButton = Button("Boss Key  Ctrl+Alt+B");
        bossButton.Pressed += ToggleBossMode;
        headerRow.AddChild(bossButton);

        _spritePanelButton = Button("Show Sprite Pad");
        _spritePanelButton.Pressed += ToggleSpritePanel;
        headerRow.AddChild(_spritePanelButton);

        HSplitContainer body = new()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SplitOffsets = [1280]
        };
        _workspace.AddChild(body);

        _playfieldFrame = new PanelContainer
        {
            CustomMinimumSize = Vector2.Zero
        };
        _playfieldFrame.AddThemeStyleboxOverride("panel", FlatStyle("#111820", 0));
        body.AddChild(_playfieldFrame);

        MarginContainer playfieldMargin = Margins(0, 0, 0, 0);
        _playfieldFrame.AddChild(playfieldMargin);
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
        _playfield.CardDroppedOnPlayfield += OnPlayfieldCardDropped;
        BuildAudioDeck();
        BuildKenneySoundCardDeck();
        BuildPlaysetToolbar();
        BuildCockpit();
        BuildPlatformerHud();
        BuildLaunchScreen();

        _sidebar = new PanelContainer();
        _sidebar.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _sidebar.SizeFlagsVertical = SizeFlags.ExpandFill;
        _sidebar.AddThemeStyleboxOverride("panel", FlatStyle("#F2EFE8", 0));
        body.AddChild(_sidebar);

        MarginContainer sidebarMargin = Margins(18, 14, 18, 14);
        _sidebar.AddChild(sidebarMargin);

        VBoxContainer sidebarRoot = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        sidebarRoot.AddThemeConstantOverride("separation", 10);
        sidebarMargin.AddChild(sidebarRoot);

        HBoxContainer spriteEditorTop = new();
        spriteEditorTop.AddThemeConstantOverride("separation", 8);
        sidebarRoot.AddChild(spriteEditorTop);

        Label spriteEditorTitle = Heading("SPRITE / ANIMATION EDITOR");
        spriteEditorTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        spriteEditorTop.AddChild(spriteEditorTitle);

        Button closeSpriteEditor = Button("Ã—");
        closeSpriteEditor.TooltipText = "Close Sprite / Animation Editor";
        closeSpriteEditor.CustomMinimumSize = new Vector2(42, 34);
        closeSpriteEditor.Pressed += CloseSpritePanel;
        spriteEditorTop.AddChild(closeSpriteEditor);

        ScrollContainer spriteEditorScroll = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        sidebarRoot.AddChild(spriteEditorScroll);

        VBoxContainer side = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        side.AddThemeConstantOverride("separation", 8);
        spriteEditorScroll.AddChild(side);

        HBoxContainer spriteEditorTopBlock = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        spriteEditorTopBlock.AddThemeConstantOverride("separation", 14);
        side.AddChild(spriteEditorTopBlock);

        _spriteEditorPreview = new CharacterPreviewPanel
        {
            CustomMinimumSize = new Vector2(340, 240),
            BackgroundColor = new Color("#FFFFFF"),
            BorderColor = new Color("#D9DEE5"),
            TitleColor = new Color("#202A34"),
            TextColor = new Color("#202A34"),
            MutedTextColor = new Color("#52606D")
        };
        spriteEditorTopBlock.AddChild(_spriteEditorPreview);

        VBoxContainer editSpriteColumn = new()
        {
            CustomMinimumSize = new Vector2(260, 0)
        };
        editSpriteColumn.AddThemeConstantOverride("separation", 6);
        spriteEditorTopBlock.AddChild(editSpriteColumn);

        editSpriteColumn.AddChild(Heading("EDIT FRAME SPRITE"));
        _spritePad = new SpritePad
        {
            DisplaySize = 240f,
            CustomMinimumSize = new Vector2(240, 240)
        };
        editSpriteColumn.AddChild(_spritePad);

        HBoxContainer tools = new();
        tools.AddThemeConstantOverride("separation", 6);
        editSpriteColumn.AddChild(tools);

        Button paint = Button("Paint");
        paint.Pressed += () =>
        {
            _spritePad.Erasing = false;
            _toolLabel.Text = "Tool: paint";
        };
        tools.AddChild(paint);

        Button erase = Button("Erase");
        erase.Pressed += () =>
        {
            _spritePad.Erasing = true;
            _toolLabel.Text = "Tool: erase";
        };
        tools.AddChild(erase);

        HBoxContainer swatches = new();
        swatches.AddThemeConstantOverride("separation", 5);
        editSpriteColumn.AddChild(swatches);
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
                CustomMinimumSize = new Vector2(34, 34),
                TooltipText = $"Paint {color.ToHtml(false)}"
            };
            swatch.AddThemeStyleboxOverride("normal", FlatStyle(color.ToHtml(false), 3));
            swatch.AddThemeStyleboxOverride("hover", FlatStyle(color.Lightened(0.15f).ToHtml(false), 3));
            swatch.Pressed += () =>
            {
                _spritePad.PaintColor = color;
                _spritePad.Erasing = false;
                _toolLabel.Text = $"Tool: paint #{color.ToHtml(false)}";
            };
            swatches.AddChild(swatch);
        }

        _toolLabel = new Label
        {
            Text = "Tool: paint",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _toolLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _toolLabel.AddThemeFontSizeOverride("font_size", 12);
        editSpriteColumn.AddChild(_toolLabel);

        VBoxContainer spriteEditorActorInfo = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        spriteEditorActorInfo.AddThemeConstantOverride("separation", 8);
        spriteEditorTopBlock.AddChild(spriteEditorActorInfo);

        Label selectedHeading = Heading("SELECTED ACTOR");
        spriteEditorActorInfo.AddChild(selectedHeading);

        _selectionLabel = new Label();
        _selectionLabel.AddThemeFontSizeOverride("font_size", 20);
        _selectionLabel.AddThemeColorOverride("font_color", new Color("#202A34"));
        spriteEditorActorInfo.AddChild(_selectionLabel);

        _characterNameEdit = new LineEdit
        {
            PlaceholderText = "Character name",
            CustomMinimumSize = new Vector2(0, 32),
            SelectAllOnFocus = true
        };
        _characterNameEdit.TextChanged += RenameSelectedCharacter;
        spriteEditorActorInfo.AddChild(_characterNameEdit);

        _bindingLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _bindingLabel.AddThemeColorOverride("font_color", new Color("#52606D"));
        _bindingLabel.AddThemeFontSizeOverride("font_size", 12);
        spriteEditorActorInfo.AddChild(_bindingLabel);

        _focusedClipLabel = new Label
        {
            Text = "Focused clip: click an animation row",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _focusedClipLabel.AddThemeColorOverride("font_color", new Color("#202A34"));
        _focusedClipLabel.AddThemeFontSizeOverride("font_size", 13);
        spriteEditorActorInfo.AddChild(_focusedClipLabel);

        Button previousFrame = Button("Prev Frame");
        previousFrame.TooltipText = "Step backward through the focused animation sequence.";
        previousFrame.Pressed += () => StepFocusedClipFrame(-1);

        Button playFocusedClip = Button("Play Clip");
        playFocusedClip.TooltipText = "Return the preview to looping the focused animation sequence.";
        playFocusedClip.Pressed += PlayFocusedClipSequence;

        Button nextFrame = Button("Next Frame");
        nextFrame.TooltipText = "Step forward through the focused animation sequence.";
        nextFrame.Pressed += () => StepFocusedClipFrame(1);
        spriteEditorActorInfo.AddChild(ButtonRow(previousFrame, playFocusedClip, nextFrame));

        side.AddChild(Heading("CHARACTER PICKER"));

        GridContainer characterPicker = new()
        {
            Columns = 3,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        characterPicker.AddThemeConstantOverride("h_separation", 6);
        characterPicker.AddThemeConstantOverride("v_separation", 6);
        side.AddChild(characterPicker);

        Button stickman2 = Button("Stickman 2.0");
        stickman2.TooltipText = "Use the OctoPyte v0.2 stick figure set with Melee/Punch and Death sheets.";
        stickman2.Pressed += () =>
        {
            LoadStickmanV2EditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(stickman2);

        Button stickman = Button("Classic Stickman");
        stickman.TooltipText = "Use the OctoPyte v0.1 thin stick figure animation set.";
        stickman.Pressed += () =>
        {
            LoadStickmanEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(stickman);

        Button gameCreatorPlayer = Button("TGC Player");
        gameCreatorPlayer.TooltipText = "Use The Game Creator's Pack player strip via local blob-detected frames.";
        gameCreatorPlayer.Pressed += () =>
        {
            LoadTgcEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(gameCreatorPlayer);

        Button dungeonRunner = Button("Dungeon Runner");
        dungeonRunner.TooltipText = "Use the CC0 8-Bit Dungeon climber with Lode Runner-style ladder frames.";
        dungeonRunner.Pressed += () =>
        {
            LoadDungeonRunnerEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(dungeonRunner);

        Button knight = Button("Knight");
        knight.TooltipText = "Use the discrete transparent Knight strips with Idle, Run, Jump/Fall, Roll, Attack, Shield, and Death animations.";
        knight.Pressed += () =>
        {
            LoadKnightEditorDefaults();
            ApplyTgcClipRanges();
        };
        characterPicker.AddChild(knight);

        Button sunnyDragon = Button("Sunny Dragon");
        sunnyDragon.TooltipText = "Add the Legacy Collection Sunny Dragon fly strip as the first animated enemy.";
        sunnyDragon.Pressed += () =>
        {
            LoadSunnyDragonEditorDefaults();
            SetEnemyCharacter("Sunny Dragon", SpriteAnimationSet.TryLoadSunnyDragon(), "Sunny Dragon added as the first animated enemy. Its fly strip is a 9-frame grid source that can be renamed, labeled, saved, and loaded.");
        };
        characterPicker.AddChild(sunnyDragon);

        Button tgcOrange = Button("Orange Worker");
        tgcOrange.TooltipText = "Add the TGC Orange Worker as an enemy.";
        tgcOrange.Pressed += () => AddTgcEnemy(
            "Orange Worker",
            SpriteAnimationSet.TryLoadTgcOrangeWorker(),
            "TGC Orange Worker added as an enemy. Good for simple ground patrol/blocker roles."
        );
        characterPicker.AddChild(tgcOrange);

        Button tgcRed = Button("Red Runner");
        tgcRed.TooltipText = "Add the TGC Red Runner as an enemy.";
        tgcRed.Pressed += () => AddTgcEnemy(
            "Red Runner",
            SpriteAnimationSet.TryLoadTgcRedRunner(),
            "TGC Red Runner added as an enemy. Good for faster patrol/chase roles."
        );
        characterPicker.AddChild(tgcRed);

        Button tgcBlue = Button("Blue Guard");
        tgcBlue.TooltipText = "Add the TGC Blue Guard as an enemy.";
        tgcBlue.Pressed += () => AddTgcEnemy(
            "Blue Guard",
            SpriteAnimationSet.TryLoadTgcBlueGuard(),
            "TGC Blue Guard added as an enemy. Good for patrol/guard roles."
        );
        characterPicker.AddChild(tgcBlue);

        Button tgcGreen = Button("Green Crawler");
        tgcGreen.TooltipText = "Add the TGC Green Crawler as an enemy.";
        tgcGreen.Pressed += () => AddTgcEnemy(
            "Green Crawler",
            SpriteAnimationSet.TryLoadTgcGreenCrawler(),
            "TGC Green Crawler added as an enemy. Good for crawling/slime/insect-style hazards."
        );
        characterPicker.AddChild(tgcGreen);

        Button tgcSnake = Button("Green Snake");
        tgcSnake.TooltipText = "Add the lower TGC snake/crawler animation as its own enemy.";
        tgcSnake.Pressed += () => AddTgcEnemy(
            "Green Snake",
            SpriteAnimationSet.TryLoadTgcGreenSnake(),
            "TGC Green Snake added as an enemy. Good for snake/maze/crawler experiments."
        );
        characterPicker.AddChild(tgcSnake);

        Button tgcBoss = Button("Shooter Boss");
        tgcBoss.TooltipText = "Add the TGC Shooter Boss as a static/large enemy.";
        tgcBoss.Pressed += () => AddTgcEnemy(
            "Shooter Boss",
            SpriteAnimationSet.TryLoadTgcShooterBoss(),
            "TGC Shooter Boss added as an enemy/boss/pinball toy candidate."
        );
        characterPicker.AddChild(tgcBoss);

        Button tgcFleet = Button("Shooter Fleet");
        tgcFleet.TooltipText = "Add the TGC Shooter Fleet sheet as an enemy.";
        tgcFleet.Pressed += () => AddTgcEnemy(
            "Shooter Fleet",
            SpriteAnimationSet.TryLoadTgcShooterFleet(),
            "TGC Shooter Fleet added as an enemy. This is a rough first atlas import; exact per-ship slicing comes later."
        );
        characterPicker.AddChild(tgcFleet);

        Button battleShip = Button("Battle Ship");
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

        Button addPreset = Button("Add Preset Label");
        addPreset.Pressed += () =>
        {
            string label = NextMissingPresetLabel();
            int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
            int frame = Mathf.Clamp(_tgcClipRowModels.Count * 2, 0, maxFrame);
            AddTgcClipRow(label, frame, frame, maxFrame);
            UpdateTgcStripPreview();
            _inspectorText.Text = $"{label} animation label added from the preset vocabulary. Edit its frame numbers to match the strip.";
        };

        Button addLabel = Button("Add Label");
        addLabel.Pressed += () =>
        {
            int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
            int frame = Mathf.Clamp(_tgcClipRowModels.Count * 2, 0, maxFrame);
            AddTgcClipRow($"Action {_tgcClipRowModels.Count + 1}", frame, frame, maxFrame);
            UpdateTgcStripPreview();
            _inspectorText.Text = "New animation label added. Rename it, then edit its start/end frame numbers.";
        };
        side.AddChild(ButtonRow(addPreset, addLabel));

        Button applyTgcClips = Button("Apply Anim Labels");
        applyTgcClips.Pressed += ApplyTgcClipRanges;

        Button reloadDefaultAnim = Button("Reload Default Anim");
        reloadDefaultAnim.Pressed += ReloadSelectedAnimationDefaults;
        side.AddChild(ButtonRow(applyTgcClips, reloadDefaultAnim));

        Button testDeath = Button("Test Death");
        testDeath.Pressed += TriggerDeathAnimation;

        Button loadTgcClips = Button("Load Anim Labels");
        loadTgcClips.Pressed += LoadAnimationClipLabels;

        Button saveTgcClips = Button("Save Anim Labels");
        saveTgcClips.Pressed += SaveTgcClipLabels;
        side.AddChild(ButtonRow(testDeath, loadTgcClips, saveTgcClips));

        HBoxContainer actions = new();
        side.AddChild(actions);

        Button fork = Button("Fork Selected");
        fork.TooltipText = "Give this actor its own independent sprite.";
        fork.Pressed += ForkSelected;
        actions.AddChild(fork);

        Button reset = Button("Reset Figure");
        reset.Pressed += () => _selectedActor.Model.ResetToProcedural();
        actions.AddChild(reset);

        Label transparentNote = new()
        {
            Text = "32 Ã— 32 live pad  â€¢  white imports as transparent",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        transparentNote.AddThemeColorOverride("font_color", new Color("#737F8C"));
        transparentNote.AddThemeFontSizeOverride("font_size", 12);
        side.AddChild(transparentNote);

        Label motionHeading = Heading("CHARACTER MOTION");
        side.AddChild(motionHeading);

        HBoxContainer motionButtons = new();
        side.AddChild(motionButtons);

        Button horizontal = Button("Pitfall");
        horizontal.TooltipText = "Horizontal platformer tuning.";
        horizontal.Pressed += () => SetPlatformerMode(PlatformerMode.Horizontal);
        motionButtons.AddChild(horizontal);

        Button vertical = Button("Climber");
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
        Button halfSize = Button("Actor 1/2x");
        halfSize.Pressed += () => SetActorSizeMultiplier(0.5f);
        actorSizeRow.AddChild(halfSize);
        Button normalSize = Button("Actor 1x");
        normalSize.Pressed += () => SetActorSizeMultiplier(1f);
        actorSizeRow.AddChild(normalSize);
        Button doubleSize = Button("Actor 2x");
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

    private void BuildKenneySoundCardDeck()
    {
        const string root = "res://assets/third_party/kenney-audio";
        _soundCardPlayer = new SoundCardPlayer { Name = "SoundCardPlayer" };
        AddChild(_soundCardPlayer);

        void Register(SoundCardDefinition card)
        {
            _kenneyAuditionSourcesLoaded += _soundCardPlayer.RegisterCard(card);
        }

        Register(new SoundCardDefinition(
            "kenney.ui.accept",
            "UI Accept",
            "DACK UI",
            "A compact confirmation click for Cockpit buttons and card activation.",
            [
                new($"{root}/ui-pack/ui-accept.ogg", "UI Pack / click-a"),
                new($"{root}/ui-pack/ui-accept-v2.ogg", "UI Pack / click-b"),
                new($"{root}/ui-pack/ui-accept-v3.ogg", "UI Pack / tap-a")
            ]
        )
        {
            Tags = ["interface", "accept", "click"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -15f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.ui.toggle",
            "UI Toggle",
            "DACK UI",
            "A physical switch sound for checkboxes, playset rules, and Inspector toggles.",
            [
                new($"{root}/ui-pack/ui-toggle.ogg", "UI Pack / switch-a"),
                new($"{root}/ui-pack/ui-toggle-v2.ogg", "UI Pack / switch-b"),
                new($"{root}/ui-pack/ui-toggle-v3.ogg", "UI Pack / tap-b")
            ]
        )
        {
            Tags = ["interface", "toggle", "switch"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -15f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.document.book-open",
            "Document / Book Open",
            "Document / RPG",
            "Document-native punctuation for opening a page level, book, Snapshot, or inventory panel.",
            [new($"{root}/rpg-audio/document-rpg-interaction.ogg", "RPG Audio / bookOpen")]
        )
        {
            Tags = ["document", "book", "page", "rpg"],
            SelectionMode = SoundVariantSelectionMode.Fixed,
            VolumeDb = -12f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.player-shot",
            "Player Shot",
            "Platformer",
            "First candidate for the player projectile fire slot.",
            [
                new($"{root}/desert-shooter/player-shot.ogg", "Desert Shooter / shoot-a"),
                new($"{root}/desert-shooter/player-shot-v2.ogg", "Desert Shooter / shoot-b"),
                new($"{root}/desert-shooter/player-shot-v3.ogg", "Desert Shooter / shoot-c")
            ]
        )
        {
            Tags = ["platformer", "projectile", "fire"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -11f,
            PitchMin = 0.97f,
            PitchMax = 1.03f,
            MaxVoices = 3,
            CooldownSeconds = 0.03f
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.contact-hit",
            "Enemy / Contact Hit",
            "Platformer",
            "Short bump candidate for enemy contact, armor, or a nonlethal collision.",
            [
                new($"{root}/new-platformer/enemy-contact-hit.ogg", "New Platformer / sfx_bump"),
                new($"{root}/new-platformer/enemy-contact-hit-v2.ogg", "Impact Sounds / generic light 000"),
                new($"{root}/new-platformer/enemy-contact-hit-v3.ogg", "Impact Sounds / generic light 001")
            ]
        )
        {
            Tags = ["platformer", "enemy", "contact", "hit"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -9f,
            PitchMin = 0.96f,
            PitchMax = 1.04f,
            MaxVoices = 2,
            CooldownSeconds = 0.05f
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.enemy-defeat",
            "Enemy Defeat",
            "Platformer",
            "A clean disappearance cue for an enemy reaching zero toughness.",
            [
                new($"{root}/new-platformer/enemy-defeat.ogg", "New Platformer / sfx_disappear"),
                new($"{root}/new-platformer/enemy-defeat-v2.ogg", "Desert Shooter / explosion-a"),
                new($"{root}/new-platformer/enemy-defeat-v3.ogg", "Desert Shooter / explosion-b")
            ]
        )
        {
            Tags = ["platformer", "enemy", "defeat"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -8f,
            PitchMin = 0.96f,
            PitchMax = 1.04f,
            MaxVoices = 3
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.player-hurt",
            "Player Hurt",
            "Platformer",
            "Damage cue for hearts or instant-death modes.",
            [
                new($"{root}/new-platformer/player-hurt.ogg", "New Platformer / sfx_hurt"),
                new($"{root}/new-platformer/player-hurt-v2.ogg", "Desert Shooter / hurt-a"),
                new($"{root}/new-platformer/player-hurt-v3.ogg", "Desert Shooter / hurt-b")
            ]
        )
        {
            Tags = ["platformer", "player", "hurt"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -8f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.power-up",
            "Power-Up / Magic",
            "Platformer",
            "Power-up candidate for player cards, pickups, and word bonuses.",
            [
                new($"{root}/new-platformer/power-up.ogg", "New Platformer / sfx_magic"),
                new($"{root}/new-platformer/power-up-v2.ogg", "New Platformer / sfx_coin"),
                new($"{root}/new-platformer/power-up-v3.ogg", "New Platformer / sfx_gem")
            ]
        )
        {
            Tags = ["platformer", "power-up", "magic", "pickup"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -9f,
            MaxVoices = 2
        });

        Register(new SoundCardDefinition(
            "kenney.platformer.jump",
            "Platformer Jump",
            "Platformer",
            "First jump-slot candidate for player movement cards.",
            [
                new($"{root}/new-platformer/platformer-jump.ogg", "New Platformer / sfx_jump"),
                new($"{root}/new-platformer/platformer-jump-v2.ogg", "New Platformer / sfx_jump-high"),
                new($"{root}/new-platformer/platformer-jump-v3.ogg", "Desert Shooter / jump-a")
            ]
        )
        {
            Tags = ["platformer", "player", "jump"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -10f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.brickbat.text-hit",
            "Brickbat Text Hit",
            "Brickbat",
            "Dry retro impact for a ball or projectile striking a letter target.",
            [
                new($"{root}/retro-sounds-2/brickbat-text-hit.ogg", "Retro Sounds 2 / hit1"),
                new($"{root}/retro-sounds-2/brickbat-text-hit-v2.ogg", "Retro Sounds 2 / hit2"),
                new($"{root}/retro-sounds-2/brickbat-text-hit-v3.ogg", "Retro Sounds 2 / hit3")
            ]
        )
        {
            Tags = ["brickbat", "text", "impact"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -11f,
            PitchMin = 0.97f,
            PitchMax = 1.03f,
            MaxVoices = 3,
            CooldownSeconds = 0.025f
        });

        Register(new SoundCardDefinition(
            "kenney.brickbat.word-break",
            "Brickbat Word Break",
            "Brickbat",
            "Retro explosion candidate for a destroyed word or literary bonus target.",
            [
                new($"{root}/retro-sounds-2/brickbat-word-break.ogg", "Retro Sounds 2 / explosion1"),
                new($"{root}/retro-sounds-2/brickbat-word-break-v2.ogg", "Retro Sounds 2 / explosion2"),
                new($"{root}/retro-sounds-2/brickbat-word-break-v3.ogg", "Retro Sounds 2 / explosion3")
            ]
        )
        {
            Tags = ["brickbat", "word", "break", "explosion"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -10f,
            PitchMin = 0.96f,
            PitchMax = 1.04f,
            MaxVoices = 3,
            CooldownSeconds = 0.04f
        });

        Register(new SoundCardDefinition(
            "kenney.brickbat.ball-lost",
            "Brickbat Ball Lost",
            "Brickbat",
            "Short lose cue for a drained ball or exhausted reserve.",
            [
                new($"{root}/retro-sounds-2/ball-lost.ogg", "Retro Sounds 2 / lose1"),
                new($"{root}/retro-sounds-2/ball-lost-v2.ogg", "Retro Sounds 2 / lose2"),
                new($"{root}/retro-sounds-2/ball-lost-v3.ogg", "Retro Sounds 2 / lose3")
            ]
        )
        {
            Tags = ["brickbat", "ball", "lost"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -9f,
            MaxVoices = 1
        });

        Register(new SoundCardDefinition(
            "kenney.brickbat.laser",
            "Brickbat Laser",
            "Brickbat",
            "Large sci-fi laser candidate for the percentage-strength text-clearing beam.",
            [
                new($"{root}/sci-fi-sounds/brickbat-laser.ogg", "Sci-Fi Sounds / laserLarge 000"),
                new($"{root}/sci-fi-sounds/brickbat-laser-v2.ogg", "Sci-Fi Sounds / laserLarge 001"),
                new($"{root}/sci-fi-sounds/brickbat-laser-v3.ogg", "Sci-Fi Sounds / laserLarge 002")
            ]
        )
        {
            Tags = ["brickbat", "laser", "power-up"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -10f,
            PitchMin = 0.98f,
            PitchMax = 1.02f,
            MaxVoices = 2
        });

        Register(new SoundCardDefinition(
            "kenney.pinball.light-rail",
            "Paddle / Light Rail",
            "Pinball",
            "Light metal impact for Brickbat paddles, pinball rails, gates, or small flipper contact.",
            [
                new($"{root}/impact-sounds/paddle-light-rail.ogg", "Impact Sounds / metal light 000"),
                new($"{root}/impact-sounds/paddle-light-rail-v2.ogg", "Impact Sounds / metal light 001"),
                new($"{root}/impact-sounds/paddle-light-rail-v3.ogg", "Impact Sounds / metal light 002")
            ]
        )
        {
            Tags = ["pinball", "brickbat", "metal", "rail", "paddle"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -13f,
            PitchMin = 0.98f,
            PitchMax = 1.02f,
            MaxVoices = 4,
            CooldownSeconds = 0.02f
        });

        Register(new SoundCardDefinition(
            "kenney.pinball.bumper",
            "Pinball Bell Bumper",
            "Pinball",
            "Bell-like heavy impact for a pop bumper, score target, or bonus insert.",
            [
                new($"{root}/impact-sounds/pinball-bumper.ogg", "Impact Sounds / bell heavy 000"),
                new($"{root}/impact-sounds/pinball-bumper-v2.ogg", "Impact Sounds / bell heavy 001"),
                new($"{root}/impact-sounds/pinball-bumper-v3.ogg", "Impact Sounds / bell heavy 002")
            ]
        )
        {
            Tags = ["pinball", "bumper", "bell", "score"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -11f,
            PitchMin = 0.98f,
            PitchMax = 1.02f,
            MaxVoices = 4,
            CooldownSeconds = 0.025f
        });

        Register(new SoundCardDefinition(
            "kenney.space.projectile",
            "Space / Actor Projectile",
            "Space / Combat",
            "Small laser candidate for ships, turrets, guards, and compact enemy fire.",
            [
                new($"{root}/sci-fi-sounds/space-actor-projectile.ogg", "Sci-Fi Sounds / laserSmall 000"),
                new($"{root}/sci-fi-sounds/space-actor-projectile-v2.ogg", "Sci-Fi Sounds / laserSmall 001"),
                new($"{root}/sci-fi-sounds/space-actor-projectile-v3.ogg", "Sci-Fi Sounds / laserSmall 002")
            ]
        )
        {
            Tags = ["space", "combat", "projectile", "laser"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -11f,
            PitchMin = 0.97f,
            PitchMax = 1.03f,
            MaxVoices = 4,
            CooldownSeconds = 0.025f
        });

        Register(new SoundCardDefinition(
            "kenney.combat.explosion",
            "Projectile Explosion",
            "Space / Combat",
            "Crunchy sci-fi explosion for projectile impact, enemy defeat, and letter-shrapnel blasts.",
            [
                new($"{root}/sci-fi-sounds/projectile-explosion.ogg", "Sci-Fi Sounds / explosionCrunch 000"),
                new($"{root}/sci-fi-sounds/projectile-explosion-v2.ogg", "Sci-Fi Sounds / explosionCrunch 001"),
                new($"{root}/sci-fi-sounds/projectile-explosion-v3.ogg", "Sci-Fi Sounds / explosionCrunch 002")
            ]
        )
        {
            Tags = ["space", "combat", "projectile", "explosion"],
            SelectionMode = SoundVariantSelectionMode.RandomNoRepeat,
            VolumeDb = -9f,
            PitchMin = 0.96f,
            PitchMax = 1.04f,
            MaxVoices = 3,
            CooldownSeconds = 0.04f
        });

        Register(new SoundCardDefinition(
            "kenney.racing.engine",
            "Retro Racing Engine",
            "Racing",
            "First low-cost engine candidate for track-builder and vehicle prototypes; not yet a seamless loop.",
            [new($"{root}/retro-sounds-2/racing-engine.ogg", "Retro Sounds 2 / engine1")]
        )
        {
            Tags = ["racing", "vehicle", "engine"],
            SelectionMode = SoundVariantSelectionMode.Fixed,
            VolumeDb = -12f,
            MaxVoices = 1
        });

        BindApprovedSoundCards();
    }

    private void BindApprovedSoundCards()
    {
        _soundCardBindings.Clear();

        _soundCardBindings["ui-accept"] = "kenney.ui.accept";
        _soundCardBindings["ui-toggle"] = "kenney.ui.toggle";
        _soundCardBindings["document-book-open"] = "kenney.document.book-open";

        _soundCardBindings["player-shot"] = "kenney.platformer.player-shot";
        _soundCardBindings["enemy-shot"] = "kenney.space.projectile";
        _soundCardBindings["enemy-contact"] = "kenney.platformer.contact-hit";
        _soundCardBindings["enemy-hit"] = "kenney.platformer.contact-hit";
        _soundCardBindings["enemy-defeat"] = "kenney.platformer.enemy-defeat";
        _soundCardBindings["player-hurt"] = "kenney.platformer.player-hurt";
        _soundCardBindings["power-up"] = "kenney.platformer.power-up";
        _soundCardBindings["platformer-jump"] = "kenney.platformer.jump";
        _soundCardBindings["combat-explosion"] = "kenney.combat.explosion";

        _soundCardBindings["brickbat-paddle"] = "kenney.pinball.light-rail";
        _soundCardBindings["brickbat-text-hit"] = "kenney.brickbat.text-hit";
        _soundCardBindings["brickbat-word-break"] = "kenney.brickbat.word-break";
        _soundCardBindings["brickbat-enemy-defeat"] = "kenney.combat.explosion";
        _soundCardBindings["brickbat-laser"] = "kenney.brickbat.laser";
        _soundCardBindings["brickbat-ball-lost"] = "kenney.brickbat.ball-lost";

        _soundCardBindings["pinball-launch"] = "kenney.pinball.light-rail";
        _soundCardBindings["pinball-flipper"] = "kenney.pinball.light-rail";
        _soundCardBindings["pinball-flipper-hit"] = "kenney.pinball.light-rail";
        _soundCardBindings["pinball-bumper"] = "kenney.pinball.bumper";
        _soundCardBindings["pinball-rollover"] = "kenney.ui.toggle";
        _soundCardBindings["pinball-text-plow"] = "kenney.brickbat.text-hit";
        _soundCardBindings["pinball-drain"] = "kenney.brickbat.ball-lost";

        _legacySoundFallbacks.Clear();
        _legacySoundFallbacks["enemy-shot"] = "player-shot";
        _legacySoundFallbacks["enemy-contact"] = "enemy-hit";
        _legacySoundFallbacks["combat-explosion"] = "enemy-defeat";
        _legacySoundFallbacks["platformer-jump"] = "power-up";
        _legacySoundFallbacks["brickbat-enemy-defeat"] = "brickbat-word-break";
        _legacySoundFallbacks["pinball-launch"] = "brickbat-paddle";
        _legacySoundFallbacks["pinball-flipper"] = "brickbat-paddle";
        _legacySoundFallbacks["pinball-flipper-hit"] = "brickbat-paddle";
        _legacySoundFallbacks["pinball-bumper"] = "power-up";
        _legacySoundFallbacks["pinball-rollover"] = "brickbat-text-hit";
        _legacySoundFallbacks["pinball-text-plow"] = "brickbat-text-hit";
        _legacySoundFallbacks["pinball-drain"] = "brickbat-ball-lost";
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
        if (!_soundEnabled)
            return;

        if (_soundCardBindings.TryGetValue(key, out string? cardId)
            && _soundCardPlayer.AvailableVariantCount(cardId) > 0)
        {
            _soundCardPlayer.TryPlayCard(cardId, out _);
            return;
        }

        string legacyKey = _legacySoundFallbacks.TryGetValue(key, out string? fallbackKey) ? fallbackKey : key;
        if (!_soundPlayers.TryGetValue(legacyKey, out AudioStreamPlayer? player))
            return;

        if (player.Playing)
            player.Stop();
        player.Play();
    }

    private void StopAllAudio()
    {
        if (_soundCardPlayer is not null)
            _soundCardPlayer.StopAll();
        foreach (AudioStreamPlayer player in _soundPlayers.Values)
            player.Stop();
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
            Text = "Esc hides cockpit  â€¢  Ctrl+Alt+B Boss Key",
            VerticalAlignment = VerticalAlignment.Center
        };
        _cockpitStatus.AddThemeColorOverride("font_color", new Color("#AAB7C4"));
        _cockpitStatus.AddThemeFontSizeOverride("font_size", 12);
        top.AddChild(_cockpitStatus);

        Button close = Button("Ã—");
        close.TooltipText = "Close Cockpit (Esc)";
        close.CustomMinimumSize = new Vector2(36, 34);
        close.Pressed += ToggleCockpit;
        top.AddChild(close);

        root.AddChild(BuildMenuBar());
        root.AddChild(BuildTransportBar());

        HBoxContainer cockpitBody = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        cockpitBody.AddThemeConstantOverride("separation", 12);
        root.AddChild(cockpitBody);

        _cockpitTabs = new TabContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        cockpitBody.AddChild(_cockpitTabs);

        _platformerPanel = BuildShelfPanel();
        _brickbatPanel = BuildBrickbatPanel();
        _pinballPanel = BuildPinballPanel();
        _overheadPanel = BuildOverheadPanel();

        AddCockpitTab(_cockpitTabs, "Player", BuildPlayerPanel());
        AddCockpitTab(_cockpitTabs, "Side View", _platformerPanel);
        AddCockpitTab(_cockpitTabs, "Paddle", _brickbatPanel);
        AddCockpitTab(_cockpitTabs, "Ball / Table", _pinballPanel);
        AddCockpitTab(_cockpitTabs, "Overhead", _overheadPanel);
        AddCockpitTab(_cockpitTabs, "Assets", BuildLegacyLibraryPanel());
        AddCockpitTab(_cockpitTabs, "Enemies", BuildEnemiesPanel());
        AddCockpitTab(_cockpitTabs, "Projectiles", BuildProjectilesPanel());
        AddCockpitTab(_cockpitTabs, "Sounds", BuildSoundsPanel());
        AddCockpitTab(_cockpitTabs, "Objects", BuildObjectsPanel());
        AddCockpitTab(_cockpitTabs, "Builder", BuildCharacterWorkbenchPanel());
        AddCockpitTab(_cockpitTabs, "Understand", BuildUnderstandingPanel());

        ScrollContainer inspectorScroll = new()
        {
            CustomMinimumSize = new Vector2(300, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        inspectorScroll.AddChild(BuildInspectorPanel());
        cockpitBody.AddChild(inspectorScroll);

        FitCockpitToViewport();
        UpdateCockpitToolkitPanels();
    }

    private Control BuildMenuBar()
    {
        MenuBar menuBar = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        MenuButton fileMenu = new() { Text = "File" };
        PopupMenu filePopup = fileMenu.GetPopup();
        filePopup.AddItem("Open Level", FileOpenCommand);
        filePopup.AddItem("Save Level", FileSaveCommand);
        filePopup.AddItem("Save Snapshot", FileSnapshotCommand);
        filePopup.AddItem("Reset Working Clone", FileResetCommand);
        filePopup.AddSeparator();
        filePopup.AddItem("Snapshot History", FileSnapshotHistoryCommand);
        filePopup.AddSeparator();
        filePopup.AddItem("Return to Desktop", FileDesktopCommand);
        filePopup.IdPressed += id => HandleFileMenuCommand((int)id);
        menuBar.AddChild(fileMenu);

        MenuButton transportMenu = new() { Text = "Transport" };
        PopupMenu transportPopup = transportMenu.GetPopup();
        transportPopup.AddItem("Run / Build (F6)", TransportRunCommand);
        transportPopup.AddItem("Freeze / Resume (F7)", TransportFreezeCommand);
        transportPopup.AddItem("Stop", TransportStopCommand);
        transportPopup.IdPressed += id => HandleTransportMenuCommand((int)id);
        menuBar.AddChild(transportMenu);

        MenuButton viewMenu = new() { Text = "View" };
        PopupMenu viewPopup = viewMenu.GetPopup();
        viewPopup.AddItem("Toggle Cockpit (Esc)", 1);
        viewPopup.AddItem("Toggle Sprite Pad", 2);
        viewPopup.AddItem("Toggle Playset Toolbar (F1)", 3);
        viewPopup.AddItem("Boss Key (Ctrl+Alt+B)", 4);
        viewPopup.IdPressed += id =>
        {
            switch ((int)id)
            {
                case 1:
                    ToggleCockpit();
                    break;
                case 2:
                    ToggleSpritePanel();
                    break;
                case 3:
                    TogglePlaysetToolbar();
                    break;
                case 4:
                    ToggleBossMode();
                    break;
            }
        };
        menuBar.AddChild(viewMenu);

        return menuBar;
    }

    private void HandleFileMenuCommand(int command)
    {
        switch (command)
        {
            case FileOpenCommand:
                ConfirmSessionAction("Open level", "Open the saved level and discard unsaved layout changes?", LoadLevel);
                break;
            case FileSaveCommand:
                SaveLevel();
                break;
            case FileSnapshotCommand:
                SaveSnapshot();
                break;
            case FileResetCommand:
                ConfirmSessionAction("Reset working clone", "Restore the captured Snapshot and reset the active game?", ResetSession);
                break;
            case FileSnapshotHistoryCommand:
                ShowSnapshotHistory();
                break;
            case FileDesktopCommand:
                ReturnToDesktop();
                break;
        }
    }

    private void HandleTransportMenuCommand(int command)
    {
        switch (command)
        {
            case TransportRunCommand:
                ToggleBuildPlayMode();
                break;
            case TransportFreezeCommand:
                ToggleSimulationFreeze();
                break;
            case TransportStopCommand:
                StopSimulation();
                break;
        }
    }

    private void ConfirmSessionAction(string title, string message, Action action)
    {
        if (!_sessionDirty)
        {
            action();
            return;
        }

        ConfirmationDialog dialog = new()
        {
            Title = title,
            DialogText = $"{message}\n\nUnsaved change: {_sessionDirtyReason}",
            OkButtonText = "Continue",
            CancelButtonText = "Cancel"
        };
        dialog.Confirmed += () =>
        {
            dialog.QueueFree();
            action();
        };
        dialog.Canceled += dialog.QueueFree;
        _workspace.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(520, 220));
    }

    private void MarkSessionDirty(string reason)
    {
        _sessionDirty = true;
        _sessionDirtyReason = reason;
        RefreshCockpitStatus();
    }

    private void MarkSessionClean()
    {
        _sessionDirty = false;
        _sessionDirtyReason = "";
        RefreshCockpitStatus();
    }

    private void ShowSnapshotHistory()
    {
        string directory = GetSnapshotDirectory();
        if (!Directory.Exists(directory))
        {
            _inspectorText.Text = "Snapshot history is empty. Save a Snapshot to create the first native-resolution clone.";
            return;
        }

        string[] snapshots = Directory.GetFiles(directory, "rad-snapshot-*.png")
            .OrderByDescending(path => File.GetLastWriteTimeUtc(path))
            .Take(12)
            .Select(path => Path.GetFileName(path))
            .ToArray();
        _inspectorText.Text = snapshots.Length == 0
            ? "Snapshot history is empty."
            : "SNAPSHOT HISTORY\n\n" + string.Join("\n", snapshots) + "\n\nThe latest Snapshot remains the active working clone; older entries are retained for comparison and future restore UI.";
    }

    private Control BuildTransportBar()
    {
        PanelContainer bar = new();
        bar.AddThemeStyleboxOverride("panel", FlatStyle("#293641", 6));

        MarginContainer margin = Margins(8, 6, 8, 6);
        bar.AddChild(margin);
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 6);
        margin.AddChild(row);

        row.AddChild(Heading("FILE"));

        Button open = Button("Open");
        open.TooltipText = "Open the current RAD level manifest.";
        open.Pressed += () => ConfirmSessionAction("Open level", "Open the saved level and discard unsaved layout changes?", LoadLevel);
        row.AddChild(open);

        Button save = Button("Save");
        save.TooltipText = "Save the current level recipe.";
        save.Pressed += SaveLevel;
        row.AddChild(save);

        Button snapshot = Button("Snapshot");
        snapshot.TooltipText = "Freeze the current working clone as a native-resolution Snapshot.";
        snapshot.Pressed += SaveSnapshot;
        row.AddChild(snapshot);

        Button reset = Button("Reset");
        reset.TooltipText = "Restore the working clone and reset the current game without changing families.";
        reset.Pressed += () => ConfirmSessionAction("Reset working clone", "Restore the captured Snapshot and reset the active game?", ResetSession);
        row.AddChild(reset);

        VSeparator separator = new();
        separator.CustomMinimumSize = new Vector2(10, 26);
        row.AddChild(separator);
        row.AddChild(Heading("TRANSPORT"));

        _transportModeButton = Button("Run (F6)");
        _transportModeButton.TooltipText = "Switch between Build and Play without changing the selected game type.";
        _transportModeButton.Pressed += ToggleBuildPlayMode;
        row.AddChild(_transportModeButton);

        _transportFreezeButton = Button("Resume (F7)");
        _transportFreezeButton.TooltipText = "Freeze or resume the active simulation.";
        _transportFreezeButton.Pressed += ToggleSimulationFreeze;
        row.AddChild(_transportFreezeButton);

        _transportStopButton = Button("Stop");
        _transportStopButton.TooltipText = "Stop the simulation and return to a safe Build state.";
        _transportStopButton.Pressed += StopSimulation;
        row.AddChild(_transportStopButton);

        Button desktop = Button("Desktop");
        desktop.TooltipText = "Park DACK and return input to the desktop without deleting the session.";
        desktop.Pressed += ReturnToDesktop;
        row.AddChild(desktop);

        return bar;
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

    private static VBoxContainer FamilySectionBody()
    {
        VBoxContainer body = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        body.AddThemeConstantOverride("separation", 7);
        return body;
    }

    private Button FamilyTabButton(string text, string targetTab, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            SelectCockpitTab(targetTab);
            _inspectorText.Text = description;
        };
        return button;
    }

    private Control BuildShelfPanel()
    {
        PanelContainer panel = CockpitPanel(300);
        FamilyPageShell shell = new("Side View", "Platformer");
        panel.AddChild(shell);

        VBoxContainer overview = FamilySectionBody();
        AddGameTypeSessionBlock(overview, PlaysetMode.Platformer, "Use Platformer Preset");
        overview.AddChild(CockpitNote("Horizontal and vertical text-native platforming. The selected Snapshot, cards, and clone mutations survive family navigation."));
        shell.AddSection("Overview & Transport", "preset and session", overview, expanded: true);

        VBoxContainer player = FamilySectionBody();
        player.AddChild(FamilyTabButton("Open Player Cards", "Player", "Choose, place, scale, and edit the active player character."));
        player.AddChild(CockpitNote("Movement, spawn, scale, climb, crawl, gravity, inertia, and input bindings belong here."));
        shell.AddSection("Player", "character, controls, movement", player);

        VBoxContainer actors = FamilySectionBody();
        actors.AddChild(FamilyTabButton("Open Enemy Cards", "Enemies", "Choose enemies, configure contact/shooter behavior, and place repeated instances."));
        Button enemyAi = Button(_enemyAiEnabled ? "Enemy AI: On" : "Enemy AI: Off");
        enemyAi.Pressed += () =>
        {
            _enemyAiEnabled = !_enemyAiEnabled;
            enemyAi.Text = _enemyAiEnabled ? "Enemy AI: On" : "Enemy AI: Off";
            MarkSessionDirty("Enemy AI rule changed");
            _inspectorText.Text = _enemyAiEnabled
                ? "Enemy AI enabled. Enemies patrol/hover, collide, and can block the route."
                : "Enemy AI disabled. Enemies stay placed for editing.";
        };
        Button enemyTrack = Button(_enemyTracksPlayer ? "Enemy Track: On" : "Enemy Track: Off");
        enemyTrack.Pressed += () =>
        {
            _enemyTracksPlayer = !_enemyTracksPlayer;
            enemyTrack.Text = _enemyTracksPlayer ? "Enemy Track: On" : "Enemy Track: Off";
            MarkSessionDirty("Enemy tracking rule changed");
            _inspectorText.Text = _enemyTracksPlayer
                ? "Enemy tracking enabled. Enemies bias patrol/facing toward the player."
                : "Enemy tracking disabled. Enemies keep their patrol/guard behavior.";
        };
        Button enemyShots = Button(_enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off");
        enemyShots.Pressed += () =>
        {
            _enemyProjectilesEnabled = !_enemyProjectilesEnabled;
            enemyShots.Text = _enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off";
            ClearEnemyShots();
            MarkSessionDirty("Enemy projectile rule changed");
        };
        Button enemyRange = Button(EnemyRangeButtonText());
        enemyRange.Pressed += () =>
        {
            _enemyShotRangeUnits = _enemyShotRangeUnits < 28f ? 34f : _enemyShotRangeUnits < 45f ? 55f : 18f;
            enemyRange.Text = EnemyRangeButtonText();
            ClearEnemyShots();
            MarkSessionDirty("Enemy perception range changed");
            _inspectorText.Text = $"Enemy shot range set to {_enemyShotRangeUnits:0} text units.";
        };
        actors.AddChild(ButtonRow(enemyAi, enemyTrack));
        actors.AddChild(ButtonRow(enemyShots, enemyRange));
        shell.AddSection("Actors", "enemies, AI, perception", actors);

        VBoxContainer world = FamilySectionBody();
        world.AddChild(ButtonRow(
            ShelfButton("Ladder", WorldObjectKind.Ladder, "Vertical climb volume. Drag A/B to set height; thickness roughly matches player width."),
            ShelfButton("Ramp", WorldObjectKind.Ramp, "Static angled standable line for paragraph slants / Donkey Kong feel.")));
        world.AddChild(ButtonRow(
            ShelfButton("Slide", WorldObjectKind.Slide, "Downhill acceleration surface. Slides always push toward the lower endpoint."),
            ShelfButton("Conveyor", WorldObjectKind.Conveyor, "Powered belt/line surface with intentionally strong force; rotate it for angled belts.")));
        world.AddChild(ShelfButton("Elevator", WorldObjectKind.Elevator, "Moving platform with editable range, direction, and speed."));
        shell.AddSection("World", "terrain and moving surfaces", world, expanded: true);

        VBoxContainer effects = FamilySectionBody();
        Button gun = Button(_gunEnabled ? "Player Weapon: On" : "Player Weapon: Off");
        gun.Pressed += () =>
        {
            _gunEnabled = !_gunEnabled;
            gun.Text = _gunEnabled ? "Player Weapon: On" : "Player Weapon: Off";
            ClearPlayerShots();
            MarkSessionDirty("Player weapon rule changed");
            _inspectorText.Text = _gunEnabled
                ? "Weapon enabled. Run Shoot / Jump Shoot labels and assigned projectiles can fire."
                : "Weapon disabled. The preset becomes a jump/climb/dig style game.";
        };
        effects.AddChild(gun);
        effects.AddChild(ButtonRow(
            FamilyTabButton("Projectiles", "Projectiles", "Assign projectile and impact/explosion cards."),
            FamilyTabButton("Sounds", "Sounds", "Assign firing, impact, movement, and death sounds.")));
        shell.AddSection("Weapons & Effects", "shots, explosions, sound", effects);

        VBoxContainer logic = FamilySectionBody();
        logic.AddChild(ButtonRow(
            ShelfButton("Start", WorldObjectKind.StartPoint, "Editor-only spawn marker. Visible while building, hidden during play."),
            ShelfButton("Checkpoint", WorldObjectKind.Checkpoint, "Visible marker now; spawn binding comes next.")));
        logic.AddChild(ButtonRow(
            ShelfButton("Goal", WorldObjectKind.GoalPoint, "Visible level objective marker: the first complete test level spine is Start -> Midpoint -> Goal."),
            ShelfButton("Hidden Switch", WorldObjectKind.HiddenSwitch, "Invisible gameplay logic: visible in editor, hidden from the player.")));
        logic.AddChild(ShelfButton("Enemy Spawn", WorldObjectKind.EnemySpawnPoint, "Editor-only spawn flag with assignable enemy, interval, burst count, max active count, speed, and behavior."));
        shell.AddSection("Markers & Logic", "start, goal, triggers, spawns", logic);

        VBoxContainer text = FamilySectionBody();
        Button textTerrain = Button(_textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off");
        textTerrain.Pressed += () =>
        {
            _textTerrainEnabled = !_textTerrainEnabled;
            textTerrain.Text = _textTerrainEnabled ? "Text Terrain: On" : "Text Terrain: Off";
            MarkSessionDirty("Text terrain rule changed");
            _inspectorText.Text = _textTerrainEnabled
                ? "Captured letters and words can support actors."
                : "Only explicit construction objects support actors.";
        };
        Button textCrawl = Button(_playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off");
        textCrawl.Pressed += () =>
        {
            _playfield.TextCrawlEnabled = !_playfield.TextCrawlEnabled;
            textCrawl.Text = _playfield.TextCrawlEnabled ? "Text Crawl: On" : "Text Crawl: Off";
            MarkSessionDirty("Text crawl rule changed");
        };
        Button textDestruction = Button(_textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off");
        textDestruction.Pressed += () =>
        {
            _textDestructionEnabled = !_textDestructionEnabled;
            textDestruction.Text = _textDestructionEnabled ? "Shot Text Damage: On" : "Shot Text Damage: Off";
            MarkSessionDirty("Text destruction rule changed");
        };
        text.AddChild(ButtonRow(textTerrain, textCrawl));
        text.AddChild(textDestruction);
        shell.AddSection("Text & Source", "terrain, crawl, destruction", text);

        VBoxContainer rules = FamilySectionBody();
        Button floor = Button(_platformerSafetyFloor ? "Safety Floor: On" : "Safety Floor: Off");
        floor.Pressed += () =>
        {
            _platformerSafetyFloor = !_platformerSafetyFloor;
            floor.Text = _platformerSafetyFloor ? "Safety Floor: On" : "Safety Floor: Off";
            SetPlaysetMode(PlaysetMode.Platformer);
            SnapPlayerToStart();
            MarkSessionDirty("Safety floor rule changed");
            _inspectorText.Text = _platformerSafetyFloor
                ? "Platformer safety floor enabled. Falling below the document catches the player."
                : "Platformer safety floor disabled. Gutter/plunge/death-pit levels can now work.";
        };
        Button damageModel = Button(_partialDamageEnabled ? "Damage: Hearts" : "Damage: Instant");
        damageModel.Pressed += () =>
        {
            _partialDamageEnabled = !_partialDamageEnabled;
            damageModel.Text = _partialDamageEnabled ? "Damage: Hearts" : "Damage: Instant";
            _playerHealth = _playerMaxHealth;
            RefreshPlatformerHud();
            MarkSessionDirty("Damage model changed");
            _inspectorText.Text = _partialDamageEnabled
                ? "Partial damage enabled. Enemy shots remove health before costing a life."
                : "Instant damage enabled. Enemy shots kill like old-school hazards.";
        };
        Button clear = Button("Clear Placed Parts");
        clear.Pressed += () =>
        {
            _playfield.ClearPlacedObjects();
            MarkSessionDirty("Placed world parts cleared");
            SyncEditorModeToScene();
            _inspectorText.Text = "Placed toolkit parts cleared. Captured document pixels and Brickbat mutations remain separate.";
        };
        rules.AddChild(ButtonRow(floor, damageModel));
        rules.AddChild(clear);
        shell.AddSection("Scoring & Rules", "damage, lives, fail states", rules);

        VBoxContainer understand = FamilySectionBody();
        understand.AddChild(FamilyTabButton("Open Understand Tools", "Understand", "Inspect detected text, whitespace, icons, regions, collision, and source interpretation."));
        understand.AddChild(CockpitNote("Use this section to validate text floors, gutters, climb bands, and collision before testing."));
        shell.AddSection("Understand & Test", "overlays and diagnostics", understand);

        return panel;
    }

    private Control BuildBrickbatPanel()
    {
        PanelContainer panel = CockpitPanel(300);
        FamilyPageShell shell = new("Paddle / Clearing", "Brickbat");
        panel.AddChild(shell);

        VBoxContainer overview = FamilySectionBody();
        AddGameTypeSessionBlock(overview, PlaysetMode.Brickbat, "Use Brickbat Preset");
        overview.AddChild(CockpitNote("One ball in play, three-ball reserve rules, literary bonuses, and persistent document deformation."));
        shell.AddSection("Overview & Transport", "preset and session", overview, expanded: true);

        VBoxContainer player = FamilySectionBody();
        Button paddle = Button(_brickbatOverlay.SidePaddle ? "Paddle: Side" : "Paddle: Bottom");
        paddle.Pressed += () =>
        {
            _brickbatOverlay.SidePaddle = !_brickbatOverlay.SidePaddle;
            paddle.Text = _brickbatOverlay.SidePaddle ? "Paddle: Side" : "Paddle: Bottom";
            SetPlaysetMode(PlaysetMode.Brickbat);
            _brickbatOverlay.ResetGame();
            MarkSessionDirty("Brickbat paddle orientation changed");
            _inspectorText.Text = _brickbatOverlay.SidePaddle
                ? "Brickbat side-paddle mode. Useful for vertical/side-wall target clearing."
                : "Brickbat bottom-paddle mode. Standard document brick-clearing layout.";
        };
        player.AddChild(paddle);
        player.AddChild(CockpitNote("Paddle placement, input edge, size, speed, and rebound shaping live here."));
        shell.AddSection("Player", "paddle, input, rebound", player);

        VBoxContainer actors = FamilySectionBody();
        actors.AddChild(FamilyTabButton("Open Enemy Cards", "Enemies", "Add animated destructible enemies and moving targets to Brickbat."));
        shell.AddSection("Actors", "enemies and moving targets", actors);

        VBoxContainer world = FamilySectionBody();
        world.AddChild(CockpitNote("The captured document is the target wall. Object cards can add barricades, gates, icons, and shaped bonus zones."));
        world.AddChild(FamilyTabButton("Open Object Cards", "Objects", "Add reusable objects to the Brickbat field."));
        shell.AddSection("World", "target wall and objects", world);

        VBoxContainer effects = FamilySectionBody();
        effects.AddChild(ButtonRow(
            FamilyTabButton("Projectiles", "Projectiles", "Configure lasers, shots, impacts, and column-clearing effects."),
            FamilyTabButton("Sounds", "Sounds", "Assign paddle, bounce, target, power-up, and loss sounds.")));
        effects.AddChild(CockpitNote("Multiball, laser strength, word explosions, letter shrapnel, and psychedelic scoring effects belong here."));
        shell.AddSection("Weapons & Effects", "laser, multiball, bursts", effects);

        VBoxContainer logic = FamilySectionBody();
        logic.AddChild(CockpitNote("Conditional target zones, protected regions, bonus triggers, and enemy spawn rules will use shared Marker/Logic Cards."));
        shell.AddSection("Markers & Logic", "zones, triggers, protection", logic);

        VBoxContainer text = FamilySectionBody();

        Button grain = Button(_brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter ? "Targets: Letters" : "Targets: Words");
        grain.Pressed += () =>
        {
            _brickbatOverlay.BrickGranularity = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter
                ? TextObjectGranularity.Word
                : TextObjectGranularity.Letter;
            grain.Text = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter ? "Targets: Letters" : "Targets: Words";
            SetPlaysetMode(PlaysetMode.Brickbat);
            _brickbatOverlay.ResetGame();
            MarkSessionDirty("Brickbat text target granularity changed");
            _inspectorText.Text = _brickbatOverlay.BrickGranularity == TextObjectGranularity.Letter
                ? "Brickbat target grain set to letters. Fine-grained page destruction; OCR labels can still bleed in from nearby word regions."
                : "Brickbat target grain set to words. Larger targets, +50 scoring, stronger Word Sense / found-poem behavior.";
        };
        text.AddChild(grain);

        Button textCollision = Button(_brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce ? "Text Physics: Bounce" : "Text Physics: Pierce");
        textCollision.Pressed += () =>
        {
            _brickbatOverlay.TextCollisionMode = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce
                ? TextCollisionMode.PassThrough
                : TextCollisionMode.Bounce;
            textCollision.Text = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce ? "Text Physics: Bounce" : "Text Physics: Pierce";
            SetPlaysetMode(PlaysetMode.Brickbat);
            MarkSessionDirty("Brickbat text collision rule changed");
            _inspectorText.Text = _brickbatOverlay.TextCollisionMode == TextCollisionMode.Bounce
                ? "Brickbat text collision set to Bounce. Letters/words act like solid targets and deflect the ball."
                : "Brickbat text collision set to Pierce. The ball erases and scores text but keeps traveling through it.";
        };
        text.AddChild(textCollision);
        text.AddChild(CockpitNote("OCR remains lazy and optional; letter/word geometry continues working without recognized text."));
        shell.AddSection("Text & Source", "targets, OCR, erasure", text, expanded: true);

        VBoxContainer rules = FamilySectionBody();
        Button resetHud = Button("Auto-Place Score");
        resetHud.Pressed += () =>
        {
            _brickbatOverlay.ResetHudPosition();
            _inspectorText.Text = "Brickbat score panel returned to auto whitespace placement. Open the Cockpit in Brickbat and drag the panel to pin it somewhere else.";
        };
        rules.AddChild(resetHud);
        rules.AddChild(CockpitNote("Lives, reserves, cooldowns, score multipliers, win/lose conditions, and found-word ticker policy live here."));
        shell.AddSection("Scoring & Rules", "lives, score, cooldowns", rules);

        VBoxContainer understand = FamilySectionBody();
        understand.AddChild(FamilyTabButton("Open Understand Tools", "Understand", "Inspect missed letters, connected ink, background regions, OCR targets, and collision masks."));
        shell.AddSection("Understand & Test", "target and erasure diagnostics", understand);

        return panel;
    }

    private Control BuildPinballPanel()
    {
        PanelContainer panel = CockpitPanel(300);
        FamilyPageShell shell = new("Ball / Table", "Pinball");
        panel.AddChild(shell);

        VBoxContainer overview = FamilySectionBody();
        AddGameTypeSessionBlock(overview, PlaysetMode.Pinball, "Use Pinball Preset");
        overview.AddChild(CockpitNote("Generated table shell, document-plowing ball, construction parts, ANSI underlay, and office-themed board rules."));
        shell.AddSection("Overview & Transport", "preset and session", overview, expanded: true);

        VBoxContainer player = FamilySectionBody();
        player.AddChild(PinballShelfButton("Add Plunger", WorldObjectKind.PinballPlunger, "Launch lane/plunger. A/B handles define lane and launch direction."));
        player.AddChild(CockpitNote("Ball card, plunger strength, launch direction, nudge, and input bindings belong here."));
        shell.AddSection("Player", "ball, plunger, nudge", player);

        VBoxContainer actors = FamilySectionBody();
        actors.AddChild(FamilyTabButton("Open Enemy Cards", "Enemies", "Add animated targets, roaming hazards, and character bumpers to the table."));
        shell.AddSection("Actors", "animated targets and hazards", actors);

        VBoxContainer world = FamilySectionBody();
        world.AddChild(ButtonRow(
            PinballShelfButton("Add Flipper", WorldObjectKind.PinballFlipper, "Pivot-to-tip flipper. A/B handles define pivot, length, and resting angle."),
            PinballShelfButton("Add Bumper", WorldObjectKind.PinballBumper, "Circular pop bumper. Drag A/B to place and scale radius.")));
        world.AddChild(PinballShelfButton("Add Drain", WorldObjectKind.PinballDrain, "Drain/outlane. A/B handles set drain width."));
        world.AddChild(FamilyTabButton("Open Object Cards", "Objects", "Add rails, barricades, gems, inserts, and table furniture."));
        shell.AddSection("World", "table shell and physical parts", world, expanded: true);

        VBoxContainer effects = FamilySectionBody();
        effects.AddChild(ButtonRow(
            FamilyTabButton("Projectiles", "Projectiles", "Assign ball trails, impacts, explosions, and special shots."),
            FamilyTabButton("Sounds", "Sounds", "Assign flipper, bumper, plunger, drain, rollover, and jackpot sounds.")));
        effects.AddChild(CockpitNote("Glow, analog text bursts, letter spirals, backglass effects, and multiball presentation live here."));
        shell.AddSection("Weapons & Effects", "impacts, lights, sound", effects);

        VBoxContainer logic = FamilySectionBody();
        logic.AddChild(ButtonRow(
            PinballShelfButton("Add Rollover", WorldObjectKind.PinballRollover, "Small scoring/lit insert strip. A/B handles set position and width."),
            PinballShelfButton("Add Gate", WorldObjectKind.PinballGate, "One-way gate. A/B direction points toward allowed travel.")));
        logic.AddChild(CockpitNote("Switch banks, jackpots, locks, lanes, missions, and conditional areas will use shared logic cards."));
        shell.AddSection("Markers & Logic", "switches, lanes, jackpots", logic);

        VBoxContainer text = FamilySectionBody();
        text.AddChild(CockpitNote("Pinball defaults to plowing through detected text. Conditional bounce/pierce regions and ANSI/ASCII underlays remain explicit table rules."));
        shell.AddSection("Text & Source", "pierce, regions, ANSI underlay", text);

        VBoxContainer rules = FamilySectionBody();
        rules.AddChild(CockpitNote("Gravity, friction, elasticity, tilt, ball count, drains, scoring, and table completion rules live here."));
        shell.AddSection("Scoring & Rules", "physics, balls, score", rules);

        VBoxContainer understand = FamilySectionBody();
        understand.AddChild(FamilyTabButton("Open Understand Tools", "Understand", "Inspect table boundaries, whitespace, detected objects, text collision, and underlay alignment."));
        shell.AddSection("Understand & Test", "geometry and physics diagnostics", understand);
        return panel;
    }

    private Control BuildOverheadPanel()
    {
        PanelContainer panel = CockpitPanel(300);
        FamilyPageShell shell = new("Overhead", "Combat / Driving / RPG");
        panel.AddChild(shell);

        VBoxContainer overview = FamilySectionBody();
        AddGameTypeSessionBlock(overview, PlaysetMode.Overhead, "Use Overhead Preset");
        overview.AddChild(CockpitNote("Shared top-down foundation for Combat, driving, aircraft, spaceships, RPG actors, animals, insects, escort, and hordes."));
        shell.AddSection("Overview & Transport", "preset and session", overview, expanded: true);

        VBoxContainer player = FamilySectionBody();
        player.AddChild(FamilyTabButton("Open Player Cards", "Player", "Choose a tank, car, ship, character, animal, insect, or glyph-derived player."));
        player.AddChild(CockpitNote("Heading frames, inertia, thrust, steering, localized gravity, and input bindings live here."));
        shell.AddSection("Player", "vehicle/actor and movement", player);

        VBoxContainer actors = FamilySectionBody();
        actors.AddChild(FamilyTabButton("Open Enemy Cards", "Enemies", "Configure patrol, defend, pursue, flee, flock, horde, radar, and projectile behavior."));
        shell.AddSection("Actors", "AI, perception, groups", actors, expanded: true);

        VBoxContainer world = FamilySectionBody();
        world.AddChild(FamilyTabButton("Open Object Cards", "Objects", "Add cover, barricades, pickups, resource nodes, hazards, and route furniture."));
        world.AddChild(CockpitNote("Rectangular/hex grids, paths, regions, roads, space fields, and localized physics zones belong here."));
        shell.AddSection("World", "arena, routes, terrain", world);

        VBoxContainer effects = FamilySectionBody();
        effects.AddChild(ButtonRow(
            FamilyTabButton("Projectiles", "Projectiles", "Assign bullets, shells, missiles, beams, mines, impacts, and explosions."),
            FamilyTabButton("Sounds", "Sounds", "Assign engine, thrust, weapon, impact, pickup, and destruction sounds.")));
        shell.AddSection("Weapons & Effects", "weapons, impacts, audio", effects);

        VBoxContainer logic = FamilySectionBody();
        logic.AddChild(CockpitNote("Start, checkpoint, goal, defend area, escort route, spawn waves, safe zones, and capture points use shared Marker/Logic Cards."));
        shell.AddSection("Markers & Logic", "objectives, zones, waves", logic);

        VBoxContainer text = FamilySectionBody();
        text.AddChild(CockpitNote("Words can become resources, hazards, objectives, cover, mines, or protected targets. OCR remains an optional semantic layer."));
        shell.AddSection("Text & Source", "semantic targets and terrain", text);

        VBoxContainer rules = FamilySectionBody();
        rules.AddChild(CockpitNote("Health, lives, teams, friendly fire, inertia, gravity, score, victory, defeat, waves, and intensity belong here."));
        shell.AddSection("Scoring & Rules", "combat and mission rules", rules);

        VBoxContainer understand = FamilySectionBody();
        understand.AddChild(FamilyTabButton("Open Understand Tools", "Understand", "Inspect regions, text, icons, navigable areas, cover, paths, and source interpretation."));
        shell.AddSection("Understand & Test", "navigation and perception diagnostics", understand);
        return panel;
    }

    private Control BuildSoundsPanel()
    {
        PanelContainer panel = CockpitPanel(360);
        VBoxContainer sounds = PanelVBox(panel);
        sounds.AddChild(CockpitHeading("SOUND CARDS / AUDITION SHELF"));
        sounds.AddChild(CockpitNote(
            $"Kenney CC0 deck: {_soundCardPlayer.Cards.Count} approved cards • {_kenneyAuditionSourcesLoaded} sources loaded. "
            + "Choose a game family, then a semantic card. The same cards now drive live gameplay through editable event bindings."
        ));

        sounds.AddChild(CockpitHeading("PICKER"));
        OptionButton familyPicker = SoundPicker("Game family");
        OptionButton cardPicker = SoundPicker("Sound Card");
        sounds.AddChild(familyPicker);
        sounds.AddChild(cardPicker);

        Label details = CockpitNote("");
        details.CustomMinimumSize = new Vector2(0, 132);
        sounds.AddChild(details);

        Button audition = Button("Audition");
        Button nextVariant = Button("Next Variant");
        Button stop = Button("Stop");
        Button soundToggle = Button(_soundEnabled ? "Sound: On" : "Sound: Off");
        sounds.AddChild(ButtonRow(audition, nextVariant, stop, soundToggle));

        Label auditionStatus = CockpitNote("Ready. Auditions stop when the Cockpit closes or the Boss Key is used.");
        sounds.AddChild(auditionStatus);

        sounds.AddChild(CockpitHeading("CARD POLICY"));
        sounds.AddChild(CockpitNote(
            "Sound Cards own variants, shuffle/random policy, gain, pitch range, cooldown, voice cap, loop intent, semantic tags, and provenance. "
            + "Approved cards now drive live game events; closely matched source variants can be admitted without changing those event bindings."
        ));

        sounds.AddChild(CockpitHeading("LIVE BINDINGS"));
        sounds.AddChild(CockpitNote(
            "Platformer jump, player/enemy fire, contact, hurt, defeat and power-up; Brickbat paddle, text, word, laser and drain; "
            + "and pinball launch, flipper, bumper, rollover, text plow and drain now route through semantic Sound Card bindings."
        ));

        List<SoundCardDefinition> visibleCards = [];
        int auditionVariantIndex = 0;

        string[] familyOrder =
        [
            "All",
            "DACK UI",
            "Document / RPG",
            "Platformer",
            "Brickbat",
            "Pinball",
            "Space / Combat",
            "Racing"
        ];
        foreach (string family in familyOrder)
        {
            if (family == "All" || _soundCardPlayer.Cards.Any(card => card.Family == family))
                familyPicker.AddItem(family);
        }

        SoundCardDefinition? SelectedCard()
        {
            int selected = cardPicker.Selected;
            return selected >= 0 && selected < visibleCards.Count ? visibleCards[selected] : null;
        }

        void RefreshDetails()
        {
            SoundCardDefinition? selected = SelectedCard();
            if (selected is null)
            {
                details.Text = "No Sound Card is available for this family.";
                audition.Disabled = true;
                nextVariant.Disabled = true;
                return;
            }

            int available = _soundCardPlayer.AvailableVariantCount(selected.Id);
            string boundEvents = string.Join(", ", _soundCardBindings
                .Where(binding => binding.Value.Equals(selected.Id, StringComparison.OrdinalIgnoreCase))
                .Select(binding => binding.Key)
                .OrderBy(eventKey => eventKey));
            string sourceSummary = selected.Variants.Count <= 1
                ? selected.Variants[0].DisplayName
                : $"{selected.Variants[0].DisplayName} (+{selected.Variants.Count - 1} variants)";
            details.Text = $"{selected.DisplayName}  //  {selected.Family}\n"
                + $"{selected.Description}\n\n"
                + $"Variants {available}/{selected.Variants.Count}  •  gain {selected.VolumeDb:0.#} dB  •  voices {selected.MaxVoices}  •  "
                + $"{selected.SelectionMode}\n"
                + $"Tags: {string.Join(", ", selected.Tags)}\n"
                + $"Events: {(string.IsNullOrWhiteSpace(boundEvents) ? "audition only" : boundEvents)}\n"
                + $"Source: {sourceSummary}  •  Kenney CC0 1.0";
            audition.Disabled = available == 0;
            nextVariant.Disabled = available == 0;
        }

        void RefreshCards()
        {
            string family = familyPicker.Selected >= 0 ? familyPicker.GetItemText(familyPicker.Selected) : "All";
            visibleCards = _soundCardPlayer.Cards
                .Where(card => family == "All" || card.Family == family)
                .OrderBy(card => card.DisplayName)
                .ToList();
            cardPicker.Clear();
            foreach (SoundCardDefinition card in visibleCards)
                cardPicker.AddItem(card.DisplayName);
            if (visibleCards.Count > 0)
                cardPicker.Select(0);
            auditionVariantIndex = 0;
            RefreshDetails();
        }

        familyPicker.ItemSelected += _ => RefreshCards();
        cardPicker.ItemSelected += _ =>
        {
            auditionVariantIndex = 0;
            RefreshDetails();
        };

        audition.Pressed += () =>
        {
            SoundCardDefinition? selected = SelectedCard();
            if (selected is null)
                return;
            if (!_soundEnabled)
            {
                auditionStatus.Text = "Sound is off. Turn it on to audition this card.";
                return;
            }

            _soundCardPlayer.StopAll();
            bool played = _soundCardPlayer.TryPlayCard(selected.Id, out string variantName);
            auditionStatus.Text = played
                ? $"Playing {selected.DisplayName} — {variantName}."
                : $"Could not play {selected.DisplayName}: {variantName}.";
            _inspectorText.Text = $"Sound Card audition\n\n{selected.DisplayName}\n{selected.Description}\n\n{variantName}\nKenney / CC0 1.0.";
        };

        nextVariant.Pressed += () =>
        {
            SoundCardDefinition? selected = SelectedCard();
            if (selected is null)
                return;
            if (!_soundEnabled)
            {
                auditionStatus.Text = "Sound is off. Turn it on to audition variants.";
                return;
            }

            int available = _soundCardPlayer.AvailableVariantCount(selected.Id);
            if (available == 0)
                return;
            auditionVariantIndex = (auditionVariantIndex + 1) % available;
            _soundCardPlayer.StopAll();
            bool played = _soundCardPlayer.TryPlayCard(selected.Id, out string variantName, auditionVariantIndex);
            auditionStatus.Text = played
                ? $"Variant {auditionVariantIndex + 1}/{available}: {variantName}."
                : $"Could not play {selected.DisplayName}: {variantName}.";
        };

        stop.Pressed += () =>
        {
            _soundCardPlayer.StopAll();
            auditionStatus.Text = "Audition stopped.";
        };

        soundToggle.Pressed += () =>
        {
            _soundEnabled = !_soundEnabled;
            soundToggle.Text = _soundEnabled ? "Sound: On" : "Sound: Off";
            if (!_soundEnabled)
            {
                _soundCardPlayer.StopAll();
                foreach (AudioStreamPlayer player in _soundPlayers.Values)
                    player.Stop();
            }
            auditionStatus.Text = _soundEnabled ? "Sound enabled." : "All sound disabled.";
        };

        familyPicker.Select(0);
        RefreshCards();
        return panel;
    }

    private Control BuildLegacyLibraryPanel()
    {
        _legacyBundles = LoadLegacyBundles();

        PanelContainer panel = CockpitPanel(330);
        VBoxContainer library = PanelVBox(panel);
        library.AddChild(CockpitHeading("ASSET LIBRARY"));
        _legacyLibraryStatus = CockpitNote(LegacyLibraryStatusText());
        library.AddChild(_legacyLibraryStatus);

        Button refresh = Button("Refresh Legacy Catalog");
        refresh.TooltipText = "Reload the generated Legacy Collection bundle manifest from quarantine.";
        refresh.Pressed += () =>
        {
            _legacyBundles = LoadLegacyBundles();
            _legacyLibraryStatus.Text = LegacyLibraryStatusText();
            _inspectorText.Text = "Legacy asset catalog refreshed.\n\nThe visible shelf is built when the Cockpit opens; close/reopen the Cockpit after regenerating the catalog to rebuild the candidate buttons.";
        };
        library.AddChild(refresh);

        AddLegacyBundleGroup(
            library,
            "EFFECTS / PROJECTILES",
            bundle => bundle.primaryCategory is "effects" or "projectiles",
            "Reusable FX deck: assign to projectiles, impacts, deaths, Brickbat bursts, Pinball hits, and text shrapnel."
        );

        AddLegacyBundleGroup(
            library,
            "SIDE VIEW ACTORS",
            bundle => bundle.bundleRoot.StartsWith("Gothicvania/Characters", StringComparison.OrdinalIgnoreCase)
                || bundle.bundleRoot.StartsWith("Misc/Characters", StringComparison.OrdinalIgnoreCase),
            "Side-view shelf: platformer enemies, bosses, flyers, animated Brickbat/Pinball targets, and side-scroller player tests."
        );

        AddLegacyBundleGroup(
            library,
            "OVERHEAD / SPACE / TANK",
            bundle => bundle.bundleRoot.StartsWith("Warped/Characters", StringComparison.OrdinalIgnoreCase)
                || bundle.bundleRoot.Contains("asteroid-fighter", StringComparison.OrdinalIgnoreCase),
            "Overhead shelf: Combat, Robotron, tank/vehicle, flying, space, Lunar Lander, and invasion-style playsets."
        );

        AddLegacyBundleGroup(
            library,
            "RPG / MAZE / MONSTERS",
            bundle => bundle.bundleRoot.StartsWith("TinyRPG/Characters", StringComparison.OrdinalIgnoreCase),
            "RPG shelf: Rogue/Hack/Pac-like/Snake/maze/escort actors, dungeon robots, battle sprites, and monster tokens."
        );

        AddLegacyBundleGroup(
            library,
            "OBJECTS / TILES / WORLDS",
            bundle => bundle.primaryCategory is "objects_and_pickups" or "tiles_and_surfaces"
                || bundle.bundleRoot.Contains("/Environments/", StringComparison.OrdinalIgnoreCase),
            "Object shelf: pickups, office/game furniture, solids, decorative underlays, board/table parts, and future tile/world slices.",
            limit: 8
        );

        library.AddChild(CockpitNote("This panel reads quarantine metadata only. Promoting art into the playable runtime remains explicit, curated, and provenance-recorded."));
        return panel;
    }

    private void AddLegacyBundleGroup(VBoxContainer library, string heading, Func<LegacyAssetBundle, bool> predicate, string intent, int limit = 6)
    {
        List<LegacyAssetBundle> bundles = _legacyBundles
            .Where(predicate)
            .OrderBy(bundle => LegacyBundleQualityRank(bundle.quality))
            .ThenByDescending(bundle => bundle.imageFiles)
            .ThenBy(bundle => bundle.bundleRoot, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();

        library.AddChild(CockpitHeading(heading));
        if (bundles.Count == 0)
        {
            library.AddChild(CockpitNote("No candidate bundles found yet. Regenerate the Legacy catalog if new assets were added."));
            return;
        }

        library.AddChild(CockpitNote(intent));
        for (int i = 0; i < bundles.Count; i += 2)
        {
            Button first = LegacyBundleButton(bundles[i], intent);
            if (i + 1 < bundles.Count)
                library.AddChild(ButtonRow(first, LegacyBundleButton(bundles[i + 1], intent)));
            else
                library.AddChild(first);
        }
    }

    private Button LegacyBundleButton(LegacyAssetBundle bundle, string intent)
    {
        Button button = Button(CompactLegacyBundleName(bundle.displayName));
        button.TooltipText = $"{bundle.bundleRoot}\n{bundle.primaryCategory}, {bundle.quality}, {bundle.imageFiles} images";
        button.Pressed += () => SelectLegacyBundle(bundle, intent);
        return button;
    }

    private void SelectLegacyBundle(LegacyAssetBundle bundle, string intent)
    {
        string dimensions = bundle.commonDimensions.Count > 0
            ? string.Join(", ", bundle.commonDimensions.Take(3).Select(pair => $"{pair.Key} ({pair.Value})"))
            : "mixed/unknown";
        string sheets = bundle.spriteSheets.Count > 0
            ? string.Join("\n", bundle.spriteSheets.Take(4).Select(path => "- " + path))
            : "- no obvious spritesheet; likely sequence/manual review";
        string previews = bundle.previews.Count > 0
            ? string.Join("\n", bundle.previews.Take(3).Select(path => "- " + path))
            : "- no preview GIF/preview file detected";
        string runtimeHint = LegacyRuntimeHint(bundle);

        _inspectorText.Text =
            $"Legacy bundle selected: {bundle.displayName}\n\n"
            + $"{bundle.bundleRoot}\n\n"
            + $"Shelf intent: {intent}\n\n"
            + $"Readiness: {bundle.quality}\n"
            + $"Category: {bundle.primaryCategory}\n"
            + $"Images: {bundle.imageFiles}\n"
            + $"Common sizes: {dimensions}\n\n"
            + $"Sheets:\n{sheets}\n\n"
            + $"Previews:\n{previews}\n\n"
            + runtimeHint;
    }

    private string LegacyRuntimeHint(LegacyAssetBundle bundle)
    {
        if (bundle.bundleRoot.Contains("sunny-dragon", StringComparison.OrdinalIgnoreCase))
            return "Already partly wired: use Enemies -> Sunny Dragon to place it now. Next step is to let this Library button promote/open it directly.";
        if (bundle.bundleRoot.Contains("top-down-shooter-ship", StringComparison.OrdinalIgnoreCase))
            return "Already partly wired: use Character Editor -> Battle Ship Player for the red ship test. This is our overhead/space heading-bin prototype.";
        if (bundle.primaryCategory is "effects" or "projectiles")
            return "Best next wiring: feed this into the Projectile/Explosion shelf as assignable projectile art, impact frames, blast radius, sound hooks, and text-shrapnel rules.";
        if (bundle.bundleRoot.StartsWith("Warped/Characters", StringComparison.OrdinalIgnoreCase))
            return "Best next wiring: import as an Overhead actor preset with movement physics chosen by role: ship inertia, tank drive, walker patrol, flyer hover, or boss pattern.";
        if (bundle.bundleRoot.StartsWith("TinyRPG/Characters", StringComparison.OrdinalIgnoreCase))
            return "Best next wiring: import as RPG/Maze actor tokens with simple direction/facing frames, contact rules, chase/avoid/defend AI, and text-goal behavior.";
        if (bundle.primaryCategory is "objects_and_pickups" or "tiles_and_surfaces")
            return "Best next wiring: slice as shelf objects, then tag each as pickup, solid, cover, hazard, decoration, underlay, or trigger.";
        return "Best next wiring: manual review, then promote as a curated DACK preset if it earns a shelf slot.";
    }

    private string LegacyLibraryStatusText()
    {
        if (_legacyBundles.Count == 0)
            return "Legacy catalog not found yet. Run the cataloger to populate the shelf.";

        int spritesheetReady = _legacyBundles.Count(bundle => bundle.quality == "spritesheet_ready");
        int sequenceReady = _legacyBundles.Count(bundle => bundle.quality == "sequence_ready");
        return $"{_legacyBundles.Count} bundles loaded from quarantine. {spritesheetReady} spritesheet-ready, {sequenceReady} sequence-ready.";
    }

    private static int LegacyBundleQualityRank(string quality)
    {
        return quality switch
        {
            "spritesheet_ready" => 0,
            "sequence_ready" => 1,
            "manual_review" => 2,
            _ => 3,
        };
    }

    private static string CompactLegacyBundleName(string name)
    {
        string cleaned = name
            .Replace("-Files", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" Files", "", StringComparison.OrdinalIgnoreCase)
            .Replace("_", " ")
            .Replace("-", " ")
            .Trim();
        return cleaned.Length <= 22 ? cleaned : cleaned[..21] + "â€¦";
    }

    private List<LegacyAssetBundle> LoadLegacyBundles()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string manifestPath = Path.GetFullPath(Path.Combine(
            projectRoot,
            "assets",
            "quarantine",
            "legacy-collection-prep",
            "legacy-collection-bundles.json"
        ));

        if (!File.Exists(manifestPath))
            return [];

        try
        {
            string json = File.ReadAllText(manifestPath);
            LegacyBundleManifest? manifest = JsonSerializer.Deserialize<LegacyBundleManifest>(json);
            return manifest?.bundles ?? [];
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Could not load Legacy Collection bundle catalog: {ex.Message}");
            return [];
        }
    }

    private Control BuildEnemiesPanel()
    {
        PanelContainer panel = CockpitPanel(270);
        VBoxContainer enemies = PanelVBox(panel);
        enemies.AddChild(CockpitHeading("ENEMIES"));
        enemies.AddChild(CockpitNote("Pick by role first. The same art can later be reassigned, but these buttons give useful starter defaults: contact, shooter, flyer/ship, vehicle, boss."));

        CardShelf enemyShelf = CreateCardShelf("Enemy Cards", EnemyCardDefinitions(), definition => ActivateEnemyCard(definition.EffectiveId, null));
        enemies.AddChild(enemyShelf);

        enemies.AddChild(CockpitHeading("BEHAVIOR STARTERS"));
        Button enemyAi = Button(_enemyAiEnabled ? "AI: On" : "AI: Off");
        enemyAi.Pressed += () =>
        {
            _enemyAiEnabled = !_enemyAiEnabled;
            enemyAi.Text = _enemyAiEnabled ? "AI: On" : "AI: Off";
            _inspectorText.Text = _enemyAiEnabled
                ? "Enemy AI enabled. Enemies patrol/hover, collide, and can block routes."
                : "Enemy AI disabled. Enemies stay placed for editing.";
        };

        Button enemyTrack = Button(_enemyTracksPlayer ? "Track Player: On" : "Track Player: Off");
        enemyTrack.Pressed += () =>
        {
            _enemyTracksPlayer = !_enemyTracksPlayer;
            enemyTrack.Text = _enemyTracksPlayer ? "Track Player: On" : "Track Player: Off";
            _inspectorText.Text = _enemyTracksPlayer
                ? "Enemy tracking enabled. Enemies bias facing/firing toward the player."
                : "Enemy tracking disabled. Enemies keep patrol/guard facing.";
        };

        Button selectedShoots = Button("Selected Shoots: Toggle");
        selectedShoots.Pressed += ToggleSelectedEnemyProjectileAbility;

        Button toughness = Button("Toughness +");
        toughness.Pressed += () => AdjustSelectedEnemyToughness(1);

        Button radarNearer = Button("Radar -");
        radarNearer.Pressed += () => AdjustSelectedEnemyRadar(-6f);

        Button radarFarther = Button("Radar +");
        radarFarther.Pressed += () => AdjustSelectedEnemyRadar(6f);

        enemies.AddChild(ButtonRow(enemyAi, enemyTrack));
        enemies.AddChild(ButtonRow(selectedShoots, toughness));
        enemies.AddChild(ButtonRow(radarNearer, radarFarther));
        enemies.AddChild(CockpitNote("Behavior menu seeds: patrol, chase, defend, horde/flock, ground-only, flying, shooter, collision hazard, escort blocker. Radar is invisible during play: smarter enemies simply notice the player from farther away."));
        return panel;
    }

    private CardShelf CreateCardShelf(string title, IEnumerable<CardDefinition> definitions, Action<CardDefinition> activate)
    {
        CardShelf shelf = new(title, definitions);
        shelf.Activated += activate;
        shelf.Forked += fork =>
        {
            MarkSessionDirty($"Forked card {fork.Title}");
            _inspectorText.Text = $"{fork.Title} created as a project-local card. It still uses the source definition until its bindings are edited in the Builder.";
        };
        return shelf;
    }

    private static CardDefinition Card(
        string kind,
        string id,
        string title,
        string subtitle,
        string details,
        string category,
        string provenance,
        string license,
        string exportStatus,
        string action,
        params string[] tags)
    {
        return new CardDefinition(kind, id, title, subtitle, details, category, provenance, license, exportStatus, tags, PrimaryAction: action);
    }

    private static CardDefinition[] PlayerCardDefinitions() =>
    [
        Card("player-character", "stickman-v0.2", "Stickman 2.0", "Fuller action scout", "Idle, Run, Jump, Melee/Punch, and Death sheets.", "Stick Figures", "OctoPyte Stickman Pack", "Commercial use cleared", "Project-cleared", "Apply", "side-view", "melee", "animated"),
        Card("player-character", "stickman-v0.1", "Classic Stickman", "Original thin scout", "Classic compatibility baseline and live sprite-pad subject.", "Stick Figures", "OctoPyte Stickman Pack", "Commercial use cleared", "Project-cleared", "Apply", "side-view", "minimal", "animated"),
        Card("player-character", "tgc-player", "TGC Player", "Imported platformer body", "Human-scale platformer strip used to test editable animation labels.", "Platformer", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Apply", "side-view", "shooter", "animated"),
        Card("player-character", "8-bit-dungeon-runner", "Dungeon Runner", "8-bit climber / rope scout", "Idle, Run, Fall, Rope, and two-frame Climb animation.", "Platformer", "Jamie Cross dungeon tiles", "CC0", "Hub-safe", "Apply", "climb", "8-bit", "animated"),
        Card("player-character", "knight-player", "Knight", "Melee / shield platformer", "Idle, Run, Jump/Fall, Roll, Attack, Shield, and Death strips.", "Platformer", "Knight asset pack", "License recorded", "Project-cleared", "Apply", "melee", "shield", "animated"),
        Card("player-character", "battle-fleet-red-ship-01", "Battle Ship Player", "Overhead / space pilot", "Heading-bin ship frames for Combat, space, and Lunar Lander experiments.", "Overhead", "Legacy Collection", "License recorded", "Review on export", "Apply", "space", "vehicle", "heading-frames")
    ];

    private static CardDefinition[] EnemyCardDefinitions() =>
    [
        Card("enemy-character", "tgc-red-runner", "Red Runner", "Fast ground contact", "Patrol/chase blocker with projectile ability off by default.", "Ground Contact", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "grounded", "contact", "fast"),
        Card("enemy-character", "tgc-green-crawler", "Green Crawler", "Low crawling hazard", "Slime/insect-style contact enemy with projectile ability off.", "Ground Contact", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "grounded", "crawl", "contact"),
        Card("enemy-character", "tgc-green-snake", "Green Snake", "Low snake hazard", "Crawling contact enemy and future Snake/Maze candidate.", "Ground Contact", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "grounded", "snake", "contact"),
        Card("enemy-character", "tgc-orange-worker", "Orange Shooter", "Ground shooter / guard", "Ground patrol with projectile ability on by default.", "Ground Shooters", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "grounded", "shooter", "guard"),
        Card("enemy-character", "tgc-blue-guard", "Blue Guard", "Ground guard / defender", "Ground defender with projectile ability and medium radar.", "Ground Shooters", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "grounded", "shooter", "defend"),
        Card("enemy-character", "sunny-dragon-fly", "Sunny Dragon", "Flying shooter", "Cross-family flyer for Side View, Paddle, Ball/Table, and Overhead.", "Flyers / Ships", "Legacy Collection", "License recorded", "Review on export", "Place", "flying", "shooter", "animated"),
        Card("enemy-character", "tgc-shooter-fleet", "Shooter Fleet", "Space / bullet-hell group", "Rough fleet atlas used for overhead and invasion tests.", "Flyers / Ships", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "flying", "space", "shooter"),
        Card("enemy-character", "tgc-shooter-boss", "Shooter Boss", "Large shooter / table target", "Boss hazard usable as a shooter or large Pinball/Brickbat target.", "Bosses", "The Game Creator's Pack", "User-owned license", "Project-cleared", "Place", "boss", "shooter", "large")
    ];

    private static CardDefinition[] ObjectCardDefinitions() =>
    [
        Card("world-object", "coin", "Coin", "Score pickup", "Reusable score pickup placeholder.", "Pickups", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "pickup", "score"),
        Card("world-object", "gem", "Gem", "Rare pickup / resource", "Bonus, key, fuel, spell-word, or rare target placeholder.", "Pickups", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "pickup", "resource"),
        Card("world-object", "barricade", "Barricade", "Solid cover", "Draggable/scalable obstacle and standable side-view surface.", "Solids / Cover", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "solid", "cover"),
        Card("world-object", "dungeon-coin", "Dungeon Coin", "8-bit collection role", "Coin behavior prepared for Dungeon Runner levels.", "Dungeon", "Jamie Cross role mapping", "CC0 reference", "Hub-safe", "Place", "pickup", "8-bit"),
        Card("world-object", "dungeon-jewel", "Dungeon Jewel", "Treasure role", "Gem behavior prepared for Lode Runner-style collection loops.", "Dungeon", "Jamie Cross role mapping", "CC0 reference", "Hub-safe", "Place", "pickup", "treasure"),
        Card("world-object", "dungeon-door", "Dungeon Door", "Visible goal / exit", "Goal marker behavior prepared for dungeon exits.", "Dungeon", "Jamie Cross role mapping", "CC0 reference", "Hub-safe", "Place", "goal", "door"),
        Card("world-object", "dungeon-spike", "Dungeon Spike", "Solid hazard role", "Barricade behavior prepared for a later damage binding.", "Dungeon", "Jamie Cross role mapping", "CC0 reference", "Hub-safe", "Place", "hazard", "solid")
    ];

    private static CardDefinition[] SideViewConstructionCardDefinitions() =>
    [
        Card("world-object", "ladder", "Ladder", "Vertical climb volume", "Drag endpoints to set height; width stays near actor scale.", "Traversal", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "climb", "vertical"),
        Card("world-object", "ramp", "Ramp", "Static angled platform", "Standable paragraph-slant surface for Donkey Kong-style layouts.", "Traversal", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "platform", "angled"),
        Card("world-object", "slide", "Slide", "Downhill acceleration", "Pushes actors toward the lower endpoint.", "Motion", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "downhill", "motion"),
        Card("world-object", "conveyor", "Conveyor", "Powered reversible surface", "Strong directional surface with editable speed and direction.", "Motion", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "powered", "reversible"),
        Card("world-object", "elevator", "Elevator", "Moving platform", "Editable travel range, direction, phase, and speed.", "Motion", "DACK procedural object", "Project-owned", "Hub-safe", "Place", "moving-platform", "range")
    ];

    private static CardDefinition[] MarkerCardDefinitions() =>
    [
        Card("marker", "start", "Start", "Player spawn marker", "Editor-visible and hidden during play.", "Route", "DACK logic primitive", "Project-owned", "Hub-safe", "Place", "spawn", "editor-only"),
        Card("marker", "checkpoint", "Checkpoint", "Mid-route marker", "Visible midpoint/checkpoint role with future respawn binding.", "Route", "DACK logic primitive", "Project-owned", "Hub-safe", "Place", "checkpoint", "visible"),
        Card("marker", "goal", "Goal", "Level objective", "Visible endpoint used by the current completion rule.", "Route", "DACK logic primitive", "Project-owned", "Hub-safe", "Place", "goal", "objective"),
        Card("marker", "hidden-switch", "Hidden Switch", "Invisible trigger", "Visible in Build and hidden from the player.", "Logic", "DACK logic primitive", "Project-owned", "Hub-safe", "Place", "trigger", "editor-only"),
        Card("marker", "enemy-spawn", "Enemy Spawn", "Configurable spawn flag", "Future binding includes enemy card, interval, burst, count, speed, and behavior.", "Logic", "DACK logic primitive", "Project-owned", "Hub-safe", "Place", "spawn", "enemy", "editor-only")
    ];

    private static CardDefinition[] PinballConstructionCardDefinitions() =>
    [
        Card("world-object", "pinball-plunger", "Plunger", "Ball launch lane", "Endpoints define lane and launch direction.", "Player", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "launch", "pinball"),
        Card("world-object", "pinball-flipper", "Flipper", "Pivoting bat", "Endpoints define pivot, length, and resting angle.", "Table Parts", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "flipper", "physics"),
        Card("world-object", "pinball-bumper", "Bumper", "Pop bumper", "Circular impulse/scoring part scaled from its endpoints.", "Table Parts", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "bumper", "score"),
        Card("world-object", "pinball-drain", "Drain", "Ball loss region", "Endpoints define drain/outlane width.", "Table Parts", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "drain", "loss"),
        Card("marker", "pinball-rollover", "Rollover", "Scoring insert", "Small lit scoring/logic strip.", "Logic", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "switch", "score"),
        Card("marker", "pinball-gate", "Gate", "One-way table gate", "Endpoint direction indicates allowed travel.", "Logic", "DACK pinball primitive", "Project-owned", "Hub-safe", "Place", "gate", "one-way")
    ];

    private static CardDefinition[] ProjectileCardDefinitions() =>
    [
        Card("projectile", "player-basic-shot", "Player Basic Shot", "Reusable straight projectile", "Enables the player weapon and current Run Shoot / Jump Shoot hooks.", "Projectiles", "DACK runtime primitive", "Project-owned", "Hub-safe", "Apply", "player", "straight", "text-damage"),
        Card("projectile", "enemy-fireball", "Enemy Fireball", "Aimed enemy projectile", "Binds projectile ability to the selected enemy; global Enemy Shots must also be enabled.", "Projectiles", "DACK runtime + cleared explosion strip", "License recorded", "Project-cleared", "Apply", "enemy", "aimed", "explosive"),
        Card("effect", "explosion-pack-b", "Explosion B", "Projectile impact / blast", "Projectile frame plus impact/explosion frames and optional letter shrapnel.", "Effects", "Itch explosion pack", "User-cleared license", "Project-cleared", "Apply", "explosion", "impact", "letter-shrapnel"),
        Card("effect", "psychedelic-word-burst", "Word Burst", "Procedural literary FX", "Solid-color word/letter burst with rotation, vector/spline motion, and fading.", "Effects", "DACK procedural effect", "Project-owned", "Hub-safe", "Apply", "word", "procedural", "psychedelic")
    ];

    private void ActivateEnemyCard(string cardId, Vector2? dropPosition)
    {
        (string name, SpriteAnimationSet? animation, bool shoots, string role) = cardId switch
        {
            "tgc-red-runner" => ("Red Runner", SpriteAnimationSet.TryLoadTgcRedRunner(), false, "fast ground contact / patrol"),
            "tgc-green-crawler" => ("Green Crawler", SpriteAnimationSet.TryLoadTgcGreenCrawler(), false, "low crawling contact hazard"),
            "tgc-green-snake" => ("Green Snake", SpriteAnimationSet.TryLoadTgcGreenSnake(), false, "snake / crawling contact hazard"),
            "tgc-orange-worker" => ("Orange Shooter", SpriteAnimationSet.TryLoadTgcOrangeWorker(), true, "ground shooter / guard"),
            "tgc-blue-guard" => ("Blue Guard", SpriteAnimationSet.TryLoadTgcBlueGuard(), true, "defender / ground shooter"),
            "sunny-dragon-fly" => ("Sunny Dragon", SpriteAnimationSet.TryLoadSunnyDragon(), true, "flying shooter"),
            "tgc-shooter-fleet" => ("Shooter Fleet", SpriteAnimationSet.TryLoadTgcShooterFleet(), true, "space / bullet-hell shooter"),
            "tgc-shooter-boss" => ("Shooter Boss", SpriteAnimationSet.TryLoadTgcShooterBoss(), true, "large shooter / boss"),
            _ => ("", null, false, "")
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            _inspectorText.Text = $"Unknown enemy card: {cardId}";
            return;
        }

        AddEnemyCharacter(
            name,
            animation,
            $"{name} placed from its reusable Enemy Card.",
            $"Starter role: {role}. Repeated Apply, Duplicate, or drag/drop creates another independent instance.",
            shoots,
            cardId,
            dropPosition
        );
        MarkSessionDirty($"Placed enemy card {name}");
    }

    private void ActivateProjectileCard(string cardId)
    {
        switch (cardId)
        {
            case "player-basic-shot":
                _gunEnabled = true;
                ClearPlayerShots();
                _inspectorText.Text = "Player Basic Shot applied. The player weapon and Run Shoot / Jump Shoot hooks are enabled.";
                break;
            case "enemy-fireball":
                if (_selectedActor is null || _selectedActor.IsPlayable)
                {
                    _inspectorText.Text = "Select an enemy before applying the Enemy Fireball card.";
                    return;
                }
                _selectedActor.CanFireProjectiles = true;
                _enemyProjectilesEnabled = true;
                _inspectorText.Text = $"Enemy Fireball applied to {_selectedActor.ActorName}. Global enemy shots are enabled.";
                break;
            case "explosion-pack-b":
                _explosionsDamageText = true;
                _inspectorText.Text = "Explosion B applied as the current impact profile. Blast-letter shrapnel is enabled.";
                break;
            case "psychedelic-word-burst":
                _inspectorText.Text = "Procedural Word Burst selected for the current effects vocabulary.";
                break;
            default:
                _inspectorText.Text = $"Unknown projectile/effect card: {cardId}";
                return;
        }

        MarkSessionDirty($"Applied projectile/effect card {cardId}");
        RefreshCharacterWorkbenchStatus();
    }

    private void ActivateWorldObjectCard(string cardId, Vector2? dropPosition)
    {
        WorldObjectKind? kind = cardId switch
        {
            "ladder" => WorldObjectKind.Ladder,
            "ramp" => WorldObjectKind.Ramp,
            "slide" => WorldObjectKind.Slide,
            "conveyor" => WorldObjectKind.Conveyor,
            "elevator" => WorldObjectKind.Elevator,
            "start" => WorldObjectKind.StartPoint,
            "checkpoint" => WorldObjectKind.Checkpoint,
            "goal" => WorldObjectKind.GoalPoint,
            "hidden-switch" => WorldObjectKind.HiddenSwitch,
            "enemy-spawn" => WorldObjectKind.EnemySpawnPoint,
            "pinball-flipper" => WorldObjectKind.PinballFlipper,
            "pinball-bumper" => WorldObjectKind.PinballBumper,
            "pinball-plunger" => WorldObjectKind.PinballPlunger,
            "pinball-drain" => WorldObjectKind.PinballDrain,
            "pinball-rollover" => WorldObjectKind.PinballRollover,
            "pinball-gate" => WorldObjectKind.PinballGate,
            "coin" or "dungeon-coin" => WorldObjectKind.Coin,
            "gem" or "dungeon-jewel" => WorldObjectKind.Gem,
            "dungeon-door" => WorldObjectKind.GoalPoint,
            "barricade" or "dungeon-spike" => WorldObjectKind.Barricade,
            _ => null
        };

        if (!kind.HasValue)
        {
            _inspectorText.Text = $"Unknown world/logic card: {cardId}";
            return;
        }

        _playfield.AddPlacedObject(kind.Value, dropPosition);
        MarkSessionDirty($"Placed {cardId} card");
        SyncEditorModeToScene();
        _inspectorText.Text = $"{cardId} placed as an independent instance. Duplicate or drag the card again to place another.";
    }

    private Control BuildProjectilesPanel()
    {
        PanelContainer panel = CockpitPanel(260);
        VBoxContainer projectiles = PanelVBox(panel);
        projectiles.AddChild(CockpitHeading("PROJECTILES"));
        projectiles.AddChild(CockpitNote("Shared projectile and explosion rules for players, enemies, Brickbat-like modes, pinball toys, and future overhead/space games."));
        CardShelf projectileShelf = CreateCardShelf("Projectile / Effect Cards", ProjectileCardDefinitions(), definition => ActivateProjectileCard(definition.EffectiveId));
        projectiles.AddChild(projectileShelf);

        projectiles.AddChild(CockpitHeading("GLOBAL RULES"));
        Button playerGun = Button(_gunEnabled ? "Player Gun: On" : "Player Gun: Off");
        playerGun.Pressed += () =>
        {
            _gunEnabled = !_gunEnabled;
            playerGun.Text = _gunEnabled ? "Player Gun: On" : "Player Gun: Off";
            ClearPlayerShots();
            _inspectorText.Text = _gunEnabled
                ? "Player gun enabled. Characters can use Run Shoot / Jump Shoot labels and fire projectiles."
                : "Player gun disabled. This character/game now leans Mario/climber/digger instead of Contra.";
        };

        Button enemyShots = Button(_enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off");
        enemyShots.Pressed += () =>
        {
            _enemyProjectilesEnabled = !_enemyProjectilesEnabled;
            enemyShots.Text = _enemyProjectilesEnabled ? "Enemy Shots: On" : "Enemy Shots: Off";
            ClearEnemyShots();
            _inspectorText.Text = _enemyProjectilesEnabled
                ? "Enemy projectile system enabled. Individual enemies still need projectile ability."
                : "Enemy projectile system disabled. Contact danger can remain, but enemies will not fire.";
        };

        projectiles.AddChild(ButtonRow(playerGun, enemyShots));

        Button shotPower = Button(PlayerShotPowerButtonText());
        shotPower.Pressed += () =>
        {
            _playerShotPower = _playerShotPower >= 4 ? 1 : _playerShotPower + 1;
            shotPower.Text = PlayerShotPowerButtonText();
            _inspectorText.Text = $"Player gun power set to {_playerShotPower}x. Enemy toughness is reduced by that amount per hit.";
        };

        Button enemyDamage = Button(EnemyShotDamageButtonText());
        enemyDamage.Pressed += () =>
        {
            _enemyShotDamage = _enemyShotDamage >= 3 ? 1 : _enemyShotDamage + 1;
            enemyDamage.Text = EnemyShotDamageButtonText();
            _inspectorText.Text = $"Enemy shot damage set to {_enemyShotDamage}. Heart games can tune this separately from instant-death games.";
        };

        projectiles.AddChild(ButtonRow(shotPower, enemyDamage));

        Button enemyRange = Button(EnemyRangeButtonText());
        enemyRange.Pressed += () =>
        {
            _enemyShotRangeUnits = _enemyShotRangeUnits < 28f ? 34f : _enemyShotRangeUnits < 45f ? 55f : 18f;
            enemyRange.Text = EnemyRangeButtonText();
            ClearEnemyShots();
            _inspectorText.Text = $"Enemy shot range set to {_enemyShotRangeUnits:0} text units.";
        };

        Button blastText = Button(_explosionsDamageText ? "Blast Letters: On" : "Blast Letters: Off");
        blastText.Pressed += () =>
        {
            _explosionsDamageText = !_explosionsDamageText;
            blastText.Text = _explosionsDamageText ? "Blast Letters: On" : "Blast Letters: Off";
            _inspectorText.Text = _explosionsDamageText
                ? "Explosion blast radius can throw/remove letters in the cloned playfield."
                : "Explosion visuals stay cosmetic; text is not damaged by blast radius.";
        };

        projectiles.AddChild(ButtonRow(enemyRange, blastText));

        Button clearShots = Button("Clear All Shots");
        clearShots.Pressed += () =>
        {
            ClearPlayerShots();
            ClearEnemyShots();
            _impactEffects.Clear();
            PushImpactEffectsToPlayfield();
            _inspectorText.Text = "All active player shots, enemy shots, and explosion visuals cleared.";
        };
        projectiles.AddChild(clearShots);
        projectiles.AddChild(CockpitNote("Next workbench step: assign projectile sprite, impact frame, explosion strip, fire sound, hit sound, speed, range, damage, blast radius, and text rules per actor."));
        return panel;
    }

    private Control BuildObjectsPanel()
    {
        PanelContainer panel = CockpitPanel(260);
        VBoxContainer objects = PanelVBox(panel);
        objects.AddChild(CockpitHeading("OBJECTS"));
        objects.AddChild(CockpitNote("Level furniture and pickups: gems, coins, barricades, keys, doors, office junk, bonus icons, and future text-bound rewards."));

        CardShelf objectShelf = CreateCardShelf("Object Cards", ObjectCardDefinitions(), definition => ActivateWorldObjectCard(definition.EffectiveId, null));
        objects.AddChild(objectShelf);

        objects.AddChild(CockpitHeading("FUTURE OBJECT SET"));
        objects.AddChild(CockpitNote(
            "- keys / locks / doors\n"
            + "- health, ammo, shield, fuel\n"
            + "- office items: staple, paperclip, pushpin, folder, coffee, sticky note\n"
            + "- text-bound pickups: collect this word, protect that word\n"
            + "- visible / invisible triggers using the same object model"
        ));
        return panel;
    }

    private Control BuildPlayerPanel()
    {
        PanelContainer panel = CockpitPanel(340);
        VBoxContainer player = PanelVBox(panel);
        player.AddChild(CockpitHeading("PLAYER"));
        player.AddChild(CockpitNote("Choose the protagonist as a card. Press Use to apply it, or drag a card onto the playfield to apply and place it."));

        _characterPreview = new CharacterPreviewPanel();
        player.AddChild(_characterPreview);

        _characterWorkbenchStatus = CockpitNote("No actor selected yet.");
        player.AddChild(_characterWorkbenchStatus);

        CardShelf playerShelf = CreateCardShelf("Player Cards", PlayerCardDefinitions(), definition => ActivatePlayerCharacterCard(definition.EffectiveId, null));
        player.AddChild(playerShelf);

        player.AddChild(CockpitHeading("PLAYER TOOLS"));
        Button selectPlayer = Button("Select Player");
        selectPlayer.Pressed += () => SelectActor(_player);

        Button openSpritePad = Button("Open Sprite / Anim Editor");
        openSpritePad.Pressed += () =>
        {
            SelectActor(_player);
            if (!_sidebar.Visible)
                ToggleSpritePanel();
            _inspectorText.Text = "Player sprite/animation editor opened. Player remains its own top-level card tab; frame editing stays in the detailed sprite page.";
        };
        player.AddChild(ButtonRow(selectPlayer, openSpritePad));

        Button playerGun = Button(_gunEnabled ? "Gun: On" : "Gun: Off");
        playerGun.Pressed += () =>
        {
            _gunEnabled = !_gunEnabled;
            playerGun.Text = _gunEnabled ? "Gun: On" : "Gun: Off";
            ClearPlayerShots();
            RefreshMotionText();
            RefreshCharacterWorkbenchStatus();
            _inspectorText.Text = _gunEnabled
                ? "Player gun enabled. This is the Contra-ish branch of the player card."
                : "Player gun disabled. This is the Mario/Pitfall-ish branch of the player card.";
        };

        Button testPunch = Button("Test Melee");
        testPunch.Pressed += TriggerPunchPreview;

        Button halfSize = Button("Player 1/2x");
        halfSize.Pressed += () =>
        {
            SelectActor(_player);
            SetActorSizeMultiplier(0.5f);
        };
        Button normalSize = Button("Player 1x");
        normalSize.Pressed += () =>
        {
            SelectActor(_player);
            SetActorSizeMultiplier(1f);
        };
        Button doubleSize = Button("Player 2x");
        doubleSize.Pressed += () =>
        {
            SelectActor(_player);
            SetActorSizeMultiplier(2f);
        };
        player.AddChild(ButtonRow(playerGun, testPunch));
        player.AddChild(ButtonRow(halfSize, normalSize, doubleSize));

        player.AddChild(CockpitHeading("NEXT CARD SLOTS"));
        player.AddChild(CockpitNote(
            "- movement card: platformer, climber, overhead, thrust, lunar lander\n"
            + "- weapon/tool card: none, gun, melee/sword, dig, climb, harvest, shield\n"
            + "- text-rule card: stand, crawl, destroy, harvest, ignore\n"
            + "- sound/effect card: jump, fire, hurt, death, power-up"
        ));
        return panel;
    }

    private Control BuildCharacterWorkbenchPanel()
    {
        PanelContainer panel = CockpitPanel(340);
        VBoxContainer workbench = PanelVBox(panel);
        workbench.AddChild(CockpitHeading("BUILDER"));
        workbench.AddChild(CockpitNote("Composition workbench: wire the selected actor/object into projectiles, effects, AI, sounds, text rules, and future spawn/level cards. Player selection now has its own top-level tab."));

        workbench.AddChild(CockpitHeading("SELECTED ACTOR"));
        Button openSpritePad = Button("Open Sprite / Anim Editor");
        openSpritePad.Pressed += () =>
        {
            if (!_sidebar.Visible)
                ToggleSpritePanel();
            _inspectorText.Text = "Sprite/animation editor opened. This remains the detailed frame-label editor while the Workbench becomes the higher-level wiring surface.";
        };

        Button selectPlayer = Button("Select Player");
        selectPlayer.Pressed += () => SelectActor(_player);
        workbench.AddChild(ButtonRow(selectPlayer, openSpritePad));

        workbench.AddChild(CockpitHeading("SLOTS / SHELVES"));
        workbench.AddChild(CharacterSlotShelf(
            "Projectile",
            "Current: " + (_selectedActor is not null && !_selectedActor.IsPlayable && _selectedActor.CanFireProjectiles ? "Enemy fireball" : _gunEnabled ? "Player shot" : "None"),
            "Assign projectile art, speed, range, damage, muzzle point, fire sound, and text rules."
        ));
        workbench.AddChild(CharacterSlotShelf(
            "Explosion",
            "Current: Fireball impact profile",
            "Assign explosion strip, blast radius, letter shrapnel, impact sound, and screen/effects response."
        ));
        workbench.AddChild(CharacterSlotShelf(
            "AI / Behavior",
            "Current: " + (_selectedActor is not null && !_selectedActor.IsPlayable ? "Role default" : "Player input"),
            "Assign patrol, chase, defend, contact, shooter, flyer, tank, horde/flock, or boss logic."
        ));
        workbench.AddChild(CharacterSlotShelf(
            "Sounds",
            "Current: starter sound deck",
            "Assign jump, land, fire, hurt, defeat, pickup, alert, and ambient/idle sounds.",
            "Sounds"
        ));
        workbench.AddChild(CharacterSlotShelf(
            "Text Rules",
            "Current: game defaults",
            "Assign text-solid, text-crawl, text-destroy, text-harvest, protected-word, and word-target behavior."
        ));

        Button selectedShoots = Button("Toggle Selected Shots");
        selectedShoots.Pressed += ToggleSelectedEnemyProjectileAbility;

        Button reloadAnims = Button("Reload Default Anims");
        reloadAnims.Pressed += ReloadSelectedAnimationDefaults;
        workbench.AddChild(ButtonRow(selectedShoots, reloadAnims));

        Button saveAnims = Button("Save Anim Labels");
        saveAnims.Pressed += SaveTgcClipLabels;
        Button loadAnims = Button("Load Anim Labels");
        loadAnims.Pressed += LoadAnimationClipLabels;
        workbench.AddChild(ButtonRow(saveAnims, loadAnims));

        return panel;
    }

    private void OnPlayfieldCardDropped(Godot.Collections.Dictionary card, Vector2 position)
    {
        string kind = card.ContainsKey("dackCardKind") ? card["dackCardKind"].AsString() : "";
        string cardId = card.ContainsKey("dackCardSourceId")
            ? card["dackCardSourceId"].AsString()
            : card.ContainsKey("dackCardId") ? card["dackCardId"].AsString() : "";
        switch (kind)
        {
            case "player-character":
                ActivatePlayerCharacterCard(cardId, position);
                break;
            case "enemy-character":
                ActivateEnemyCard(cardId, position);
                break;
            case "world-object":
            case "marker":
                ActivateWorldObjectCard(cardId, position);
                break;
            case "projectile":
            case "effect":
                ActivateProjectileCard(cardId);
                break;
        }
    }

    private void ActivatePlayerCharacterCard(string cardId, Vector2? dropPosition)
    {
        switch (cardId)
        {
            case "stickman-v0.2":
                SetPlayerCharacter(
                    "Stickman 2.0",
                    SpriteAnimationSet.TryLoadStickmanV2(),
                    "Stickman 2.0 Player Card applied. Melee/Punch and Death are now available as imported sheets.",
                    "stickman-v0.2"
                );
                LoadStickmanV2EditorDefaults();
                break;
            case "stickman-v0.1":
                SetPlayerCharacter(
                    "Classic Stickman",
                    SpriteAnimationSet.TryLoadStickman(),
                    "Classic Stickman Player Card applied.",
                    "stickman-v0.1"
                );
                LoadStickmanEditorDefaults();
                break;
            case "tgc-player":
                SetPlayerCharacter(
                    "TGC Player",
                    SpriteAnimationSet.TryLoadGameCreatorPlayer(),
                    "TGC Player Card applied.",
                    "tgc-player"
                );
                LoadTgcEditorDefaults();
                break;
            case "8-bit-dungeon-runner":
                SetPlayerCharacter(
                    "Dungeon Runner",
                    SpriteAnimationSet.TryLoadDungeonRunner(),
                    "Dungeon Runner Player Card applied. This is our first climb-native 8-bit player: ideal for ladders, ropes, gutters, and Lode Runner-style text dungeons.",
                    "8-bit-dungeon-runner"
                );
                LoadDungeonRunnerEditorDefaults();
                break;
            case "knight-player":
                SetPlayerCharacter(
                    "Knight",
                    SpriteAnimationSet.TryLoadKnight(),
                    "Knight Player Card applied. Melee, roll/crawl, shield, jump/fall, and death strips are available in the animation editor.",
                    "knight-player"
                );
                LoadKnightEditorDefaults();
                break;
            case "battle-fleet-red-ship-01":
                SetPlayerCharacter(
                    "Battle Ship 01",
                    SpriteAnimationSet.TryLoadBattleFleetRedShip01(),
                    "Battle Ship Player Card applied. Its frames are heading bins, not walk frames.",
                    "battle-fleet-red-ship-01"
                );
                break;
            default:
                if (_inspectorText is not null)
                    _inspectorText.Text = $"Unknown player card: {cardId}";
                return;
        }

        if (dropPosition.HasValue)
            PlacePlayerFromCardDrop(dropPosition.Value);

        MarkSessionDirty($"Player card changed to {_player.ActorName}");
    }

    private void PlacePlayerFromCardDrop(Vector2 playfieldPosition)
    {
        if (_playfield.HasStartMarker())
        {
            _inspectorText.Text += "\n\nCard drop selected this player. Existing Start Point still controls play-mode spawn; remove or move the Start Point to change the spawn.";
            return;
        }

        Rect2 bounds = _playfield.PlayBounds;
        Vector2 desired = playfieldPosition - _player.Size * 0.5f;
        desired = new Vector2(
            Mathf.Clamp(desired.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - _player.Size.X)),
            Mathf.Clamp(desired.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - _player.Size.Y))
        );

        _playerPosition = desired;
        _player.Position = desired;
        _player.HomePosition = desired;
        _player.ManualPlacement = true;
        _player.CanDragPlayableInEditor = _editorMode;
        SelectActor(_player);
        RefreshMotionText();
        _inspectorText.Text += "\n\nDropped onto the playfield: player card is now the active player and has been placed here.";
    }

    private void AddGameTypeSessionBlock(VBoxContainer page, PlaysetMode mode, string enterText)
    {
        page.AddChild(CockpitHeading("SESSION"));

        Button enter = Button(enterText);
        enter.Pressed += () =>
        {
            SetPlaysetMode(mode);
            PlaySound("ui-accept");
            _inspectorText.Text = $"{PlaysetModeLabel(mode)} toolkit selected.";
        };

        page.AddChild(enter);
        Label transportHint = new()
        {
            Text = "Use the shared File / Transport bar above for Open, Save, Snapshot, Reset, Run, Freeze, Stop, and Desktop.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        transportHint.AddThemeColorOverride("font_color", new Color("#5D6975"));
        transportHint.AddThemeFontSizeOverride("font_size", 11);
        page.AddChild(transportHint);
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
        Button tougher = Button("Toughness +");
        tougher.Pressed += () => AdjustSelectedEnemyToughness(1);
        Button weaker = Button("Toughness -");
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

        Button clearTint = Button("Default Color");
        clearTint.Pressed += () =>
        {
            _playfield.ClearSelectedTint();
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(clearTint);

        Button reverse = Button("Reverse Direction");
        reverse.Pressed += () =>
        {
            _playfield.ReverseSelectedDirection();
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(reverse);

        Button rotateLeft = Button("Rotate -15Â°");
        rotateLeft.Pressed += () =>
        {
            _playfield.RotateSelected(-15f);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        Button rotateRight = Button("Rotate +15Â°");
        rotateRight.Pressed += () =>
        {
            _playfield.RotateSelected(15f);
            UpdateAttributeControls(_playfield.GetSelectedWorldObject());
        };
        inspector.AddChild(ButtonRow(rotateLeft, rotateRight));

        Button normalize = Button("Ramp Up / Slide Down");
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

        Button spritePad = Button("Toggle Sprite Pad");
        spritePad.Pressed += ToggleSpritePanel;
        inspector.AddChild(spritePad);

        Button platformer = Button("Platformer Mode");
        platformer.Pressed += () => SetPlaysetMode(PlaysetMode.Platformer);
        inspector.AddChild(platformer);

        Button resetScout = Button("Reset Scout Start");
        resetScout.Pressed += SnapPlayerToStart;
        inspector.AddChild(resetScout);

        inspector.AddChild(CockpitHeading("PINBALL ASSET NOTE"));
        inspector.AddChild(CockpitNote(
            "VerzatileDev pinball source PNGs are huge (~3937Ã—3937 / ~118 MB each). "
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

        Button platformer = Button("Platformer");
        platformer.Pressed += () => SetPlaysetMode(PlaysetMode.Platformer);
        _playsetToolbarRow.AddChild(platformer);

        Button brickbat = Button("Brickbat");
        brickbat.Pressed += () => SetPlaysetMode(PlaysetMode.Brickbat);
        _playsetToolbarRow.AddChild(brickbat);

        Button pinball = Button("Pinball");
        pinball.Pressed += () => SetPlaysetMode(PlaysetMode.Pinball);
        _playsetToolbarRow.AddChild(pinball);

        Button overhead = Button("Overhead");
        overhead.Pressed += () => SetPlaysetMode(PlaysetMode.Overhead);
        _playsetToolbarRow.AddChild(overhead);

        Button reset = Button("Reset");
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

        Button cockpit = Button("Cockpit");
        cockpit.Pressed += ToggleCockpit;
        _playsetToolbarRow.AddChild(cockpit);

        Button boss = Button("Boss");
        boss.Pressed += ToggleBossMode;
        _playsetToolbarRow.AddChild(boss);
    }

    private void BuildLaunchScreen()
    {
        _launchScreen = new Control
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = 200
        };
        _launchScreen.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_launchScreen);

        CenterContainer center = new();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _launchScreen.AddChild(center);

        VBoxContainer stack = new()
        {
            CustomMinimumSize = new Vector2(360, 0),
            Alignment = BoxContainer.AlignmentMode.Center
        };
        stack.AddThemeConstantOverride("separation", 16);
        center.AddChild(stack);

        _launchLogo = new TextureRect
        {
            Texture = GD.Load<Texture2D>("res://icon.svg"),
            CustomMinimumSize = new Vector2(168, 168),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Modulate = new Color(1f, 1f, 1f, 0.42f),
            PivotOffset = new Vector2(84, 84),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        stack.AddChild(_launchLogo);

        _launchHint = new Label
        {
            Text = "CTRL+ALT+B  —  SHOW / HIDE DACK",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(360, 30),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _launchHint.AddThemeColorOverride("font_color", new Color(0.95f, 0.96f, 0.98f, 0.72f));
        _launchHint.AddThemeFontSizeOverride("font_size", 15);
        stack.AddChild(_launchHint);

        _playsetToolbar.Visible = false;
        _platformerHud.Visible = false;
    }

    private void UpdateLaunchScreen(float delta)
    {
        if (!_launchScreenActive || _launchScreen is null || !_launchScreen.Visible)
            return;

        _launchScreenClock += delta;
        float breathe = Mathf.Sin((float)_launchScreenClock * 1.35f) * 0.018f;
        _launchLogo.Scale = Vector2.One * (1f + breathe);
        _launchLogo.Rotation = Mathf.Sin((float)_launchScreenClock * 0.72f) * 0.012f;
        float alpha = 0.38f + Mathf.Sin((float)_launchScreenClock * 1.1f) * 0.05f;
        _launchLogo.Modulate = new Color(1f, 1f, 1f, alpha);
    }

    private void DismissLaunchScreen()
    {
        if (!_launchScreenActive)
            return;

        _launchScreenActive = false;
        if (_desktopParked)
        {
            _desktopParked = false;
            if (_workspace is not null)
                _workspace.Visible = true;
        }
        if (_launchScreen is not null)
            _launchScreen.Visible = false;
        if (_playsetToolbar is not null)
            _playsetToolbar.Visible = true;
        RefreshPlatformerHud();
        UpdateCursorMode();
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
            Text = "\nQ3 priorities\n\nâ€¢ Review staffing plan and delivery milestones\nâ€¢ Reconcile the operating forecast\nâ€¢ Prepare notes for Monday's status meeting\n\n"
                 + "Draft workspace â€” press Ctrl+Alt+B to return.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        copy.AddThemeFontSizeOverride("font_size", 16);
        copy.AddThemeColorOverride("font_color", new Color("#485664"));
        document.AddChild(copy);
    }

    private void CreateActors()
    {
        _enemyPlacementRandom.Randomize();
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
        _player.ActorName = "Stickman 2.0";
        _player.AnimationSourceId = "stickman-v0.2";
        _player.IsPlayable = true;
        _player.AnimationSet = SpriteAnimationSet.TryLoadStickmanV2() ?? SpriteAnimationSet.TryLoadStickman();
        ApplyActorScale();
        SetPlatformerMode(PlatformerMode.Horizontal);

        _toolLabel.Text = loadedThirdPartyAsset
            ? "OctoPyte CC BY 4.0 figure loaded"
            : "Procedural project figure loaded  â€¢  export-safe";
    }

    private void SelectActor(ActorView actor)
    {
        _selectedActor = actor;
        foreach (ActorView candidate in _actors)
            candidate.Selected = candidate == actor;

        _spritePad.Model = actor.Model;
        LoadAnimationEditorForActor(actor);
        RefreshBindingText();
        if (_characterPreview is not null)
            _characterPreview.Actor = actor;
        if (_spriteEditorPreview is not null)
            _spriteEditorPreview.Actor = actor;
        RefreshCharacterWorkbenchStatus();
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
        RefreshCharacterWorkbenchStatus();
        _selectedActor.TooltipText = $"Select {trimmed}";
    }

    private void LoadAnimationEditorForActor(ActorView actor)
    {
        string sourceId = string.IsNullOrWhiteSpace(actor.AnimationSourceId)
            ? GuessAnimationSourceId(actor.ActorName, actor.IsPlayable)
            : actor.AnimationSourceId;

        switch (sourceId)
        {
            case "stickman-v0.2":
                LoadStickmanV2EditorDefaults();
                break;
            case "stickman-v0.1":
                LoadStickmanEditorDefaults();
                break;
            case "tgc-player":
                LoadTgcEditorDefaults();
                break;
            case "8-bit-dungeon-runner":
                LoadDungeonRunnerEditorDefaults();
                break;
            case "knight-player":
                LoadKnightEditorDefaults();
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
            case "tgc-green-snake":
                LoadTgcGreenSnakeEditorDefaults(actor.ActorName, sourceId);
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
        if (name.Contains("knight"))
            return "knight-player";
        if (name.Contains("sunny") || name.Contains("dragon"))
            return "sunny-dragon-fly";
        if (name.Contains("orange"))
            return "tgc-orange-worker";
        if (name.Contains("red"))
            return "tgc-red-runner";
        if (name.Contains("blue"))
            return "tgc-blue-guard";
        if (name.Contains("snake"))
            return "tgc-green-snake";
        if (name.Contains("green") || name.Contains("crawler"))
            return "tgc-green-crawler";
        if (name.Contains("boss"))
            return "tgc-shooter-boss";
        if (name.Contains("fleet") || name.Contains("ship"))
            return "tgc-shooter-fleet";
        if (name.Contains("battle"))
            return "battle-fleet-red-ship-01";
        if (name.Contains("dungeon") || name.Contains("runner"))
            return "8-bit-dungeon-runner";
        if (name.Contains("tgc"))
            return "tgc-player";
        return playable ? "stickman-v0.2" : "";
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

    private void AddEnemyCharacter(string actorName, SpriteAnimationSet? animationSet, string note, string footer, bool canFireProjectiles, string animationSourceId = "", Vector2? dropPosition = null)
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
        enemy.RadarRangeUnits = DefaultEnemyRadarRange(actorName);
        enemy.ManualPlacement = true;
        int slotIndex = _actors.IndexOf(enemy);
        enemy.Size = EnemyDefaultSize();
        enemy.CustomMinimumSize = enemy.Size;
        enemy.Position = dropPosition.HasValue
            ? ClampActorToPlayfield(dropPosition.Value - enemy.Size * 0.5f, enemy.Size)
            : EnemySpawnPosition(slotIndex, enemy.Size);
        enemy.HomePosition = enemy.Position;
        enemy.TooltipText = $"Select {actorName}";
        SelectActor(enemy);
        _enemyHealth.Remove(enemy);
        _defeatedEnemies.Remove(enemy);
        _inspectorText.Text = note + "\n\n" + footer + $"\n\nProjectile capable: {(canFireProjectiles ? "yes" : "no")}.\nShot toughness: {enemy.ShotToughness}.\nRadar: {enemy.RadarRangeUnits:0} text units.";
    }

    private ActorView NextEnemySlot()
    {
        for (int i = 1; i < _actors.Count; i++)
        {
            if (!_actors[i].Visible)
                return _actors[i];
        }

        return CreateEnemySlot();
    }

    private ActorView CreateEnemySlot()
    {
        ActorView enemy = new()
        {
            ActorName = $"Enemy {_actors.Count}",
            Model = _initialModel,
            Visible = false
        };
        enemy.SelectionRequested += SelectActor;
        _actors.Add(enemy);
        _playfield.AddChild(enemy);
        return enemy;
    }

    private Vector2 EnemyDefaultSize()
    {
        float height = Mathf.Max(_textUnitPixels * 7f, 52f);
        return new Vector2(height, height);
    }

    private Vector2 ClampActorToPlayfield(Vector2 position, Vector2 size)
    {
        Rect2 bounds = _playfield.PlayBounds;
        return new Vector2(
            Mathf.Clamp(position.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - size.X)),
            Mathf.Clamp(position.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - size.Y))
        );
    }

    private Vector2 EnemySpawnPosition(int slotIndex, Vector2 size)
    {
        Rect2 bounds = _playfield.PlayBounds;
        float horizontalMargin = Mathf.Min(bounds.Size.X * 0.12f, _textUnitPixels * 18f);
        float verticalMargin = Mathf.Min(bounds.Size.Y * 0.12f, _textUnitPixels * 12f);
        float minX = bounds.Position.X + horizontalMargin;
        float maxX = bounds.End.X - horizontalMargin - size.X;
        float minY = bounds.Position.Y + verticalMargin;
        float maxY = bounds.End.Y - verticalMargin - size.Y;

        if (maxX <= minX || maxY <= minY)
            return new Vector2(bounds.Position.X, bounds.Position.Y);

        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2 candidate = new(
                _enemyPlacementRandom.RandfRange(minX, maxX),
                _enemyPlacementRandom.RandfRange(minY, maxY)
            );
            Rect2 candidateRect = new(candidate, size);
            bool overlaps = false;
            foreach (ActorView actor in _actors)
            {
                if (!actor.Visible || actor == _player || actor.Size == Vector2.Zero)
                    continue;

                if (candidateRect.Grow(_textUnitPixels * 1.5f).Intersects(new Rect2(actor.Position, actor.Size)))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                return candidate;
        }

        int index = Mathf.Max(1, slotIndex);
        int column = (index - 1) % 4;
        int row = (index - 1) / 4;
        Vector2 fallback = new(
            minX + (column * Mathf.Max(_textUnitPixels * 8f, (maxX - minX) / 4f)),
            minY + (row * Mathf.Max(_textUnitPixels * 7f, (maxY - minY) / 4f))
        );
        return new Vector2(Mathf.Clamp(fallback.X, minX, maxX), Mathf.Clamp(fallback.Y, minY, maxY));
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

    private void LoadStickmanV2EditorDefaults()
    {
        _animationEditorName = "Stickman 2.0";
        _animationEditorSource = "assets/third_party/stickman-pack-v0.2/*.png";
        _animationEditorSourceKind = "admitted-third-party";
        _animationEditorSourceId = "stickman-v0.2";
        _animationEditorFolder = "stickman-pack-v0.2";
        _animationEditorFileName = "stickman-2.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadStickmanV2Frames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(5, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(6, maxFrame), Mathf.Min(14, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", Mathf.Min(15, maxFrame), Mathf.Min(15, maxFrame), maxFrame);
        AddTgcClipRow("Jump Down", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Fall", Mathf.Min(16, maxFrame), Mathf.Min(16, maxFrame), maxFrame);
        AddTgcClipRow("Punch", Mathf.Min(17, maxFrame), Mathf.Min(28, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", Mathf.Min(17, maxFrame), Mathf.Min(28, maxFrame), maxFrame);
        AddTgcClipRow("Jump Shoot", Mathf.Min(17, maxFrame), Mathf.Min(28, maxFrame), maxFrame);
        AddTgcClipRow("Death", Mathf.Min(29, maxFrame), maxFrame, maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadDungeonRunnerEditorDefaults()
    {
        _animationEditorName = "Dungeon Runner";
        _animationEditorSource = "assets/third_party/8-bit-dungeon/player-*.png";
        _animationEditorSourceKind = "admitted-third-party";
        _animationEditorSourceId = "8-bit-dungeon-runner";
        _animationEditorFolder = "8-bit-dungeon";
        _animationEditorFileName = "dungeon-runner.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadDungeonRunnerFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, 0, maxFrame);
        AddTgcClipRow("Run", Mathf.Min(1, maxFrame), Mathf.Min(4, maxFrame), maxFrame);
        AddTgcClipRow("Crawl", Mathf.Min(5, maxFrame), Mathf.Min(6, maxFrame), maxFrame);
        AddTgcClipRow("Climb", Mathf.Min(5, maxFrame), Mathf.Min(6, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", Mathf.Min(7, maxFrame), Mathf.Min(7, maxFrame), maxFrame);
        AddTgcClipRow("Jump Down", Mathf.Min(7, maxFrame), Mathf.Min(7, maxFrame), maxFrame);
        AddTgcClipRow("Fall", Mathf.Min(7, maxFrame), Mathf.Min(7, maxFrame), maxFrame);
        AddTgcClipRow("Melee / Rope", Mathf.Min(8, maxFrame), Mathf.Min(10, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", Mathf.Min(8, maxFrame), Mathf.Min(10, maxFrame), maxFrame);
        AddTgcClipRow("Jump Shoot", Mathf.Min(9, maxFrame), Mathf.Min(9, maxFrame), maxFrame);
        AddTgcClipRow("Death", Mathf.Min(7, maxFrame), Mathf.Min(7, maxFrame), maxFrame);
        TryLoadAnimationClipLabels(showFeedback: false);
        UpdateTgcStripPreview();
    }

    private void LoadKnightEditorDefaults()
    {
        _animationEditorName = "Knight";
        _animationEditorSource = "assets/project/knight/knight-*.png";
        _animationEditorSourceKind = "repo-dev-test";
        _animationEditorSourceId = "knight-player";
        _animationEditorFolder = "knight-player-prep";
        _animationEditorFileName = "knight-player.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadKnightFrames(out SpriteFrame[] loaded) ? loaded : [];
        SetAnimationEditorFrames(frames);
        int maxFrame = Mathf.Max(0, _animationEditorFrameCount - 1);
        ClearTgcClipRows();
        AddTgcClipRow("Idle", 0, Mathf.Min(14, maxFrame), maxFrame);
        AddTgcClipRow("Run", Mathf.Min(15, maxFrame), Mathf.Min(22, maxFrame), maxFrame);
        AddTgcClipRow("Jump Anticipation", Mathf.Min(23, maxFrame), Mathf.Min(25, maxFrame), maxFrame);
        AddTgcClipRow("Jump Up", Mathf.Min(26, maxFrame), Mathf.Min(29, maxFrame), maxFrame);
        AddTgcClipRow("Jump Down", Mathf.Min(30, maxFrame), Mathf.Min(32, maxFrame), maxFrame);
        AddTgcClipRow("Fall", Mathf.Min(30, maxFrame), Mathf.Min(32, maxFrame), maxFrame);
        AddTgcClipRow("Land / Recovery", Mathf.Min(33, maxFrame), Mathf.Min(36, maxFrame), maxFrame);
        AddTgcClipRow("Roll / Crawl", Mathf.Min(37, maxFrame), Mathf.Min(51, maxFrame), maxFrame);
        AddTgcClipRow("Attack / Melee", Mathf.Min(52, maxFrame), Mathf.Min(73, maxFrame), maxFrame);
        AddTgcClipRow("Run Shoot", Mathf.Min(52, maxFrame), Mathf.Min(73, maxFrame), maxFrame);
        AddTgcClipRow("Jump Shoot", Mathf.Min(52, maxFrame), Mathf.Min(73, maxFrame), maxFrame);
        AddTgcClipRow("Shield", Mathf.Min(74, maxFrame), Mathf.Min(80, maxFrame), maxFrame);
        AddTgcClipRow("Death", Mathf.Min(81, maxFrame), Mathf.Min(95, maxFrame), maxFrame);
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

    private void LoadTgcGreenSnakeEditorDefaults(string actorName, string sourceId)
    {
        _animationEditorName = actorName;
        _animationEditorSource = "raw base assets/The Game Creator's Pack/The Game Creator's Pack/Graphic Pack/Platformer_SpriteSheet.png";
        _animationEditorSourceKind = "raw-local-evaluation";
        _animationEditorSourceId = sourceId;
        _animationEditorFolder = "game-creators-pack-graphics-prep";
        _animationEditorFileName = $"{sourceId}.dackanim.json";

        SpriteFrame[] frames = SpriteAnimationSet.TryLoadTgcGreenSnakeFrames(out SpriteFrame[] loaded) ? loaded : [];
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
        _animationEditorFrames = frames;
        _animationEditorFrameCount = frames.Length;
        _focusedClipRow = null;
        _focusedClipFrames = [];
        _focusedClipFrameIndex = -1;
        RefreshFocusedClipLabel();
        _tgcStripPreview?.SetFrames(frames);
        if (_tgcStripPreview is not null)
            _tgcStripPreview.FacingRight = _selectedActor?.FacingRight ?? true;
        if (_spriteEditorPreview is not null)
            _spriteEditorPreview.Actor = _selectedActor;
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
        AnimationFrameRange runShoot = FindTgcClipRange(run, "run shoot", "run shooting", "attack / melee", "attack", "melee", "punch");
        AnimationFrameRange jumpShoot = FindTgcClipRange(jumpUp, "jump shoot", "air shoot", "jump shooting", "attack / melee", "attack", "melee", "punch");
        AnimationFrameRange death = FindTgcClipRange(new AnimationFrameRange(16, 17), "death", "die");
        AnimationFrameRange roll = FindTgcClipRange(run, "roll / crawl", "roll", "crawl", "dodge");
        AnimationFrameRange shield = FindTgcClipRange(idle, "shield", "block", "defend");
        bool idlePingPong = FindTgcClipPingPong("idle");
        bool runPingPong = FindTgcClipPingPong("run", "walk");
        bool jumpUpPingPong = FindTgcClipPingPong("jump up", "jump", "rise");
        bool jumpDownPingPong = FindTgcClipPingPong("jump down", "land");
        bool fallPingPong = FindTgcClipPingPong("fall", "falling");
        bool runShootPingPong = FindTgcClipPingPong("run shoot", "run shooting");
        bool jumpShootPingPong = FindTgcClipPingPong("jump shoot", "air shoot", "jump shooting");
        bool deathPingPong = FindTgcClipPingPong("death", "die");
        bool rollPingPong = FindTgcClipPingPong("roll / crawl", "roll", "crawl", "dodge");
        bool shieldPingPong = FindTgcClipPingPong("shield", "block", "defend");
        ApplyDeathStrobeSettings();
        UpdateTgcStripPreview();

        SpriteAnimationSet? animationSet;
        switch (_animationEditorSourceId)
        {
            case "stickman-v0.2":
                animationSet = SpriteAnimationSet.TryLoadStickmanV2();
                break;
            case "stickman-v0.1":
                animationSet = SpriteAnimationSet.TryLoadStickman(idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong, runShootPingPong, jumpShootPingPong, deathPingPong);
                break;
            case "8-bit-dungeon-runner":
                animationSet = SpriteAnimationSet.TryLoadDungeonRunner();
                break;
            case "knight-player":
                animationSet = SpriteAnimationSet.TryLoadKnight(
                    idle, run, jumpUp, jumpDown, fall, runShoot, jumpShoot, death, roll, shield,
                    idlePingPong, runPingPong, jumpUpPingPong, jumpDownPingPong, fallPingPong,
                    runShootPingPong, jumpShootPingPong, deathPingPong, rollPingPong, shieldPingPong
                );
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
            case "tgc-green-snake":
                animationSet = SpriteAnimationSet.TryLoadTgcGreenSnake();
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
            + $"Idle {idle.Start}-{idle.End}; Run {run.Start}-{run.End}; Jump-up {jumpUp.Start}-{jumpUp.End}; Jump-down {jumpDown.Start}-{jumpDown.End}; Fall {fall.Start}-{fall.End}; Run-shoot/attack {runShoot.Start}-{runShoot.End}; Jump-shoot/attack {jumpShoot.Start}-{jumpShoot.End}; Roll {roll.Start}-{roll.End}; Shield {shield.Start}-{shield.End}; Death {death.Start}-{death.End}.\n\n"
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
            "stickman-v0.2" => SpriteAnimationSet.TryLoadStickmanV2(),
            "stickman-v0.1" => SpriteAnimationSet.TryLoadStickman(),
            "8-bit-dungeon-runner" => SpriteAnimationSet.TryLoadDungeonRunner(),
            "knight-player" => SpriteAnimationSet.TryLoadKnight(),
            "sunny-dragon-fly" => SpriteAnimationSet.TryLoadSunnyDragon(),
            "tgc-player" => SpriteAnimationSet.TryLoadGameCreatorPlayer(),
            "tgc-orange-worker" => SpriteAnimationSet.TryLoadTgcOrangeWorker(),
            "tgc-red-runner" => SpriteAnimationSet.TryLoadTgcRedRunner(),
            "tgc-blue-guard" => SpriteAnimationSet.TryLoadTgcBlueGuard(),
            "tgc-green-crawler" => SpriteAnimationSet.TryLoadTgcGreenCrawler(),
            "tgc-green-snake" => SpriteAnimationSet.TryLoadTgcGreenSnake(),
            "tgc-shooter-boss" => SpriteAnimationSet.TryLoadTgcShooterBoss(),
            "tgc-shooter-fleet" => SpriteAnimationSet.TryLoadTgcShooterFleet(),
            "battle-fleet-red-ship-01" => SpriteAnimationSet.TryLoadBattleFleetRedShip01(),
            _ => null
        };
    }

    private void TriggerDeathAnimation()
    {
        ApplyDeathStrobeSettings();
        _deathTestSeconds = 1.55;
        _playerVelocity = Vector2.Zero;
        _player.MotionState = ActorMotionState.Death;
        _player.AnimationClock = 0;
        _inspectorText.Text = "Death animation test started. Adjust the Death row, STR, and count, then Apply/Save again.";
    }

    private void TriggerPunchPreview()
    {
        SelectActor(_player);
        _punchPreviewSeconds = 1.25;
        _player.AnimationClock = 0;
        _player.MotionState = ActorMotionState.Punch;
        _player.QueueRedraw();
        _inspectorText.Text = "Melee preview started. Stickman 2.0 calls this source animation Punch, but the gameplay card can become punch, sword, bite, club, wand, or other close-action rules.";
    }

    private void UpdatePunchPreview(double delta)
    {
        if (_player is null || _punchPreviewSeconds <= 0)
            return;

        _punchPreviewSeconds -= delta;
        _player.MotionState = ActorMotionState.Punch;
        _player.QueueRedraw();
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
        MarkSessionClean();
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
        MarkSessionClean();
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
            if (saved.index < 0)
                continue;

            while (saved.index >= _actors.Count)
                CreateEnemySlot();

            ActorView actor = _actors[saved.index];
            actor.ActorName = string.IsNullOrWhiteSpace(saved.name) ? actor.ActorName : saved.name;
            actor.AnimationSourceId = string.IsNullOrWhiteSpace(saved.animationSourceId) ? saved.animationSource : saved.animationSourceId;
            actor.Visible = saved.visible || actor.IsPlayable;
            actor.FacingRight = saved.facingRight;
            actor.ManualPlacement = saved.manualPlacement;
            actor.ShotToughness = saved.shotToughness <= 0 ? DefaultEnemyShotToughness(actor.ActorName) : Mathf.Clamp(saved.shotToughness, 1, 9);
            actor.RadarRangeUnits = saved.radarRangeUnits <= 0 ? DefaultEnemyRadarRange(actor.ActorName) : Mathf.Clamp(saved.radarRangeUnits, 6f, 90f);
            actor.MotionState = Enum.TryParse(saved.motionState, out ActorMotionState motionState) ? motionState : ActorMotionState.Idle;
            actor.Position = new Vector2(saved.x, saved.y);
            actor.Size = new Vector2(saved.width, saved.height);
            actor.CustomMinimumSize = actor.Size;
            actor.AnimationSet = actor.AnimationSourceId switch
            {
                "stickman-v0.1" => SpriteAnimationSet.TryLoadStickman(),
                "stickman-v0.2" => SpriteAnimationSet.TryLoadStickmanV2(),
                "8-bit-dungeon-runner" => SpriteAnimationSet.TryLoadDungeonRunner(),
                "knight-player" => SpriteAnimationSet.TryLoadKnight(),
                "sunny-dragon-fly" => SpriteAnimationSet.TryLoadSunnyDragon(),
                "tgc-player" => SpriteAnimationSet.TryLoadGameCreatorPlayer(),
                "tgc-orange-worker" => SpriteAnimationSet.TryLoadTgcOrangeWorker(),
                "tgc-red-runner" => SpriteAnimationSet.TryLoadTgcRedRunner(),
                "tgc-blue-guard" => SpriteAnimationSet.TryLoadTgcBlueGuard(),
                "tgc-green-crawler" => SpriteAnimationSet.TryLoadTgcGreenCrawler(),
                "tgc-green-snake" => SpriteAnimationSet.TryLoadTgcGreenSnake(),
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

    private static string GetDefaultSnapshotPath()
    {
        return Path.Combine(GetSnapshotDirectory(), "rad-snapshot-latest.png");
    }

    private static string GetSnapshotDirectory()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(projectRoot, "snapshots"));
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
        _tgcStripPreview.FacingRight = _selectedActor?.FacingRight ?? true;
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
        return value == "-" || value == "â€”" || value == "â€“";
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
        row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddThemeConstantOverride("separation", 6);

        LineEdit nameEdit = new()
        {
            Text = name,
            CustomMinimumSize = new Vector2(118, 30),
            PlaceholderText = "Label"
        };
        nameEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        nameEdit.AddThemeFontSizeOverride("font_size", 12);
        StyleLightEditorLineEdit(nameEdit);
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
            Text = "Ping",
            TooltipText = "PingPong: play forward, then reverse the frames.",
            CustomMinimumSize = new Vector2(58, 30),
            FocusMode = FocusModeEnum.None
        };
        StyleLightEditorCheckBox(pingPong);
        pingPong.Pressed += UpdateTgcStripPreview;
        row.AddChild(pingPong);

        CheckBox strobe = new()
        {
            Text = "Strobe",
            TooltipText = "Strobe this label during test/play effects.",
            CustomMinimumSize = new Vector2(70, 30),
            FocusMode = FocusModeEnum.None
        };
        StyleLightEditorCheckBox(strobe);
        strobe.Pressed += UpdateTgcStripPreview;
        row.AddChild(strobe);

        SpinBox strobeCount = ClipEndpointSpin(0, 20);
        strobeCount.TooltipText = "Strobe pulse count/intensity, 0-20.";
        strobeCount.ValueChanged += _ => UpdateTgcStripPreview();
        row.AddChild(strobeCount);

        TgcClipRow clipRow = new(row, nameEdit, start, end, pingPong, strobe, strobeCount);
        row.GuiInput += inputEvent =>
        {
            if (inputEvent is InputEventMouseButton mouseButton
                && mouseButton.ButtonIndex == MouseButton.Left
                && mouseButton.Pressed)
            {
                PreviewTgcClipRow(clipRow);
            }
        };
        nameEdit.FocusEntered += () => PreviewTgcClipRow(clipRow);
        start.FocusEntered += () => PreviewTgcClipRow(clipRow);
        end.FocusEntered += () => PreviewTgcClipRow(clipRow);
        return clipRow;
    }

    private void PreviewTgcClipRow(TgcClipRow row)
    {
        if (_spriteEditorPreview is null)
            return;

        if (IsUnavailableClipRow(row) || _animationEditorFrames.Length == 0)
        {
            _focusedClipRow = row;
            _focusedClipFrames = [];
            _focusedClipFrameIndex = -1;
            RefreshFocusedClipLabel();
            _spriteEditorPreview.Actor = _selectedActor;
            _inspectorText.Text = $"{row.Name.Text} is unavailable for this character. Preview falls back to the selected actor's default idle animation.";
            return;
        }

        SpriteFrame[] frames = BuildFramesForClipRow(row);
        string label = FocusedClipDisplayName(row);
        _focusedClipRow = row;
        _focusedClipFrames = frames;
        _focusedClipFrameIndex = -1;
        RefreshFocusedClipLabel();
        _spriteEditorPreview.ShowFrames(frames, label);
        _inspectorText.Text = $"Focused clip: {label}, frames {row.Start.Text}–{row.End.Text}.\n\nUse Prev/Next Frame to inspect individual frames, or Play Clip to loop just this sequence.";
    }

    private SpriteFrame[] BuildFramesForClipRow(TgcClipRow row)
    {
        if (_animationEditorFrames.Length == 0 || IsUnavailableClipRow(row))
            return [];

        int numberBase = Mathf.RoundToInt((float)(_tgcNumberBase?.Value ?? 0));
        AnimationFrameRange range = DisplayToInternalRange(EndpointRange(row.Start, row.End), numberBase, _animationEditorFrames.Length);
        int start = Mathf.Clamp(Mathf.Min(range.Start, range.End), 0, _animationEditorFrames.Length - 1);
        int end = Mathf.Clamp(Mathf.Max(range.Start, range.End), 0, _animationEditorFrames.Length - 1);
        List<SpriteFrame> frames = [];
        for (int i = start; i <= end; i++)
            frames.Add(_animationEditorFrames[i]);

        if (row.PingPong.ButtonPressed && frames.Count > 1)
        {
            for (int i = frames.Count - 2; i > 0; i--)
                frames.Add(frames[i]);
        }

        return frames.ToArray();
    }

    private void StepFocusedClipFrame(int delta)
    {
        if (_focusedClipRow is null)
        {
            _inspectorText.Text = "Click an animation label row first, then use Prev/Next Frame to inspect that sequence.";
            return;
        }

        _focusedClipFrames = BuildFramesForClipRow(_focusedClipRow);
        if (_focusedClipFrames.Length == 0)
        {
            RefreshFocusedClipLabel();
            _inspectorText.Text = $"{FocusedClipDisplayName(_focusedClipRow)} has no editable frames for this character.";
            return;
        }

        if (_focusedClipFrameIndex < 0)
            _focusedClipFrameIndex = delta >= 0 ? 0 : _focusedClipFrames.Length - 1;
        else
            _focusedClipFrameIndex = Mathf.PosMod(_focusedClipFrameIndex + delta, _focusedClipFrames.Length);

        string label = FocusedClipDisplayName(_focusedClipRow);
        _spriteEditorPreview.ShowFrames(_focusedClipFrames, label, _focusedClipFrameIndex);
        RefreshFocusedClipLabel();
        _inspectorText.Text = $"{label}: showing frame {_focusedClipFrameIndex + 1} of {_focusedClipFrames.Length}.\n\nThis is the focused animation sequence; edits should stay scoped to this clip.";
    }

    private void PlayFocusedClipSequence()
    {
        if (_focusedClipRow is null)
        {
            _inspectorText.Text = "Click an animation label row first, then Play Clip will loop only that sequence.";
            return;
        }

        _focusedClipFrames = BuildFramesForClipRow(_focusedClipRow);
        _focusedClipFrameIndex = -1;
        string label = FocusedClipDisplayName(_focusedClipRow);
        _spriteEditorPreview.ShowFrames(_focusedClipFrames, label);
        RefreshFocusedClipLabel();
        _inspectorText.Text = $"{label} is looping as the focused clip.";
    }

    private void RefreshFocusedClipLabel()
    {
        if (_focusedClipLabel is null)
            return;

        if (_focusedClipRow is null)
        {
            _focusedClipLabel.Text = "Focused clip: click an animation row";
            return;
        }

        string label = FocusedClipDisplayName(_focusedClipRow);
        string frame = _focusedClipFrameIndex >= 0 && _focusedClipFrames.Length > 0
            ? $"frame {_focusedClipFrameIndex + 1}/{_focusedClipFrames.Length}"
            : $"{_focusedClipFrames.Length} frame sequence";
        _focusedClipLabel.Text = $"Focused clip: {label}\n{frame}";
    }

    private static string FocusedClipDisplayName(TgcClipRow row)
    {
        return string.IsNullOrWhiteSpace(row.Name.Text) ? "Animation" : row.Name.Text.Trim();
    }

    private void OnClipEndpointTextChanged(LineEdit changed, LineEdit partner, string text, int maxFrame)
    {
        if (_syncingClipUnavailable)
            return;

        string value = text.Trim();
        if (value == "-" || value == "â€”" || value == "â€“")
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
            ? $"LIVE LINK ACTIVE â€” edits update {linkedActors} actors instantly. Fork to make this actor independent."
            : "INDEPENDENT SPRITE â€” edits affect only this actor.";
    }

    private void ToggleBossMode()
    {
        DismissLaunchScreen();
        _bossMode = !_bossMode;
        if (_bossMode)
            StopAllAudio();
        _workspace.Visible = !_bossMode;
        _bossOverlay.Visible = _bossMode;
        RefreshUiState();
        UpdateCursorMode();
    }

    private void ToggleSpritePanel()
    {
        DismissLaunchScreen();
        bool opening = !_sidebar.Visible;
        _sidebar.Visible = opening;
        if (_playfieldFrame is not null)
            _playfieldFrame.Visible = !opening;
        _spritePanelButton.Text = _sidebar.Visible ? "Hide Sprite Pad" : "Show Sprite Pad";

        if (opening && _cockpit is not null && _cockpit.Visible)
        {
            _soundCardPlayer.StopAll();
            _cockpit.Visible = false;
            _resumePlayWhenCockpitCloses = false;
            _brickbatOverlay.HudEditable = false;
            SyncEditorModeToScene();
            _playfield.QueueRedraw();
        }

        UpdateCursorMode();
    }

    private void CloseSpritePanel()
    {
        if (_sidebar is null)
            return;

        _sidebar.Visible = false;
        if (_playfieldFrame is not null)
            _playfieldFrame.Visible = true;
        if (_spritePanelButton is not null)
            _spritePanelButton.Text = "Show Sprite Pad";
        RefreshUiState();
        UpdateCursorMode();
    }

    private void ToggleCockpit()
    {
        DismissLaunchScreen();
        if (_cockpit.Visible)
        {
            bool resumePlay = _resumePlayWhenCockpitCloses;
            _resumePlayWhenCockpitCloses = false;
            _soundCardPlayer.StopAll();
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
            PlaySound("document-book-open");
        }

        _brickbatOverlay.HudEditable = _cockpit.Visible;
        SyncEditorModeToScene();
        _playfield.QueueRedraw();
        RefreshCockpitStatus();
        RefreshUiState();
        UpdateCursorMode();
    }

    private void ToggleBuildPlayMode()
    {
        if (_editorMode)
        {
            _resumePlayWhenCockpitCloses = false;
            _simulationStopped = false;
            SetEditorMode(false);
            return;
        }

        _resumePlayWhenCockpitCloses = true;
        SetEditorMode(true);
        _cockpit.Visible = true;
        FitCockpitToViewport();
        _brickbatOverlay.HudEditable = true;
        PlaySound("document-book-open");
        SyncEditorModeToScene();
        RefreshCockpitStatus();
        UpdateCursorMode();
    }

    private void ToggleSimulationFreeze()
    {
        if (_simulationStopped)
        {
            _inspectorText.Text = "Simulation is stopped. Press F6 to return to Play mode.";
            RefreshCockpitStatus();
            return;
        }

        if (_editorMode)
        {
            _simulationFrozen = true;
            _inspectorText.Text = "Simulation is already frozen in Build mode.";
        }
        else
        {
            _simulationFrozen = !_simulationFrozen;
            _inspectorText.Text = _simulationFrozen
                ? "Simulation frozen. Press F7 to resume."
                : "Simulation resumed.";
        }

        SyncEditorModeToScene();
        RefreshCockpitStatus();
        RefreshPlatformerHud();
        UpdateCursorMode();
    }

    private void StopSimulation()
    {
        SetEditorMode(true);
        _simulationStopped = true;
        _cockpit.Visible = true;
        FitCockpitToViewport();
        _brickbatOverlay.HudEditable = true;
        _inspectorText.Text = "Simulation stopped. Build mode is active and the current level remains intact. Press F6 to run it again.";
        SyncEditorModeToScene();
        RefreshCockpitStatus();
        UpdateCursorMode();
    }

    private void ReturnToDesktop()
    {
        DismissLaunchScreen();
        StopAllAudio();
        _simulationFrozen = true;
        _desktopParked = true;
        _cockpit.Visible = false;
        _sidebar.Visible = false;
        _playfieldFrame.Visible = true;
        _brickbatOverlay.HudEditable = false;
        _workspace.Visible = false;
        _launchScreenActive = true;
        _launchScreen.Visible = true;
        _launchHint.Text = "DACK parked — press Esc, F6, or F7 to return to your desktop game session";
        SyncEditorModeToScene();
        UpdateCursorMode();
    }

    private void ResetSession()
    {
        SetEditorMode(true);
        _simulationStopped = false;
        _playfield.ResetDocumentImage();
        ClearPlayerShots();
        ClearEnemyShots();
        _impactEffects.Clear();
        ResetCurrentPlayset(_playsetMode);
        _inspectorText.Text = $"{PlaysetModeLabel(_playsetMode)} reset. The working clone returned to its captured Snapshot; placed cards and level structure remain.";
        SyncEditorModeToScene();
        RefreshCockpitStatus();
        RefreshPlatformerHud();
    }

    private void SaveSnapshot()
    {
        string directory = GetSnapshotDirectory();
        Directory.CreateDirectory(directory);
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string outputPath = Path.Combine(directory, $"rad-snapshot-{stamp}.png");
        int collision = 2;
        while (File.Exists(outputPath))
            outputPath = Path.Combine(directory, $"rad-snapshot-{stamp}-{collision++}.png");

        if (!_playfield.TrySaveWorkingSnapshot(outputPath))
        {
            _inspectorText.Text = "Snapshot could not be saved because no captured playfield is available.";
            return;
        }

        DackSnapshotManifest manifest = new()
        {
            format = "dacksnapshot",
            version = 1,
            imagePath = Path.GetRelativePath(ProjectSettings.GlobalizePath("res://"), outputPath).Replace('\\', '/'),
            sourceName = _playfield.CapturedPageSourceName,
            playsetMode = _playsetMode.ToString(),
            nativeWidth = _playfield.CapturedPageSize.X,
            nativeHeight = _playfield.CapturedPageSize.Y
        };
        string metadataPath = Path.ChangeExtension(outputPath, ".json");
        File.WriteAllText(metadataPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        string latestPath = GetDefaultSnapshotPath();
        File.Copy(outputPath, latestPath, true);
        File.Copy(metadataPath, Path.ChangeExtension(latestPath, ".json"), true);
        _simulationFrozen = true;
        SyncEditorModeToScene();
        _inspectorText.Text = $"Snapshot saved.\n\n{outputPath}\n\nThe original source remains untouched; this native-resolution working clone is now the level's reusable Snapshot.";
        RefreshCockpitStatus();
    }

    private void FitCockpitToViewport()
    {
        if (_cockpit is null || _playfield is null)
            return;

        Vector2 available = _playfield.Size;
        if (available.X <= 0 || available.Y <= 0)
            available = GetViewportRect().Size;

        const float edge = 8f;
        Vector2 desired = new(
            Mathf.Max(620f, available.X - edge * 2f),
            Mathf.Max(360f, available.Y - edge * 2f)
        );
        _cockpit.Size = desired;
        _cockpit.CustomMinimumSize = Vector2.Zero;
        _cockpit.Position = new Vector2(edge, edge);
    }

    private void SetEditorMode(bool enabled)
    {
        _editorMode = enabled;
        _simulationFrozen = enabled;
        _simulationStopped = false;
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
            _soundCardPlayer.StopAll();
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
        _playfield.SimulationPaused = _simulationFrozen;
        _brickbatOverlay.Paused = _editorMode || _simulationFrozen || _playsetMode != PlaysetMode.Brickbat;
        if (_pinballOverlay is not null)
            _pinballOverlay.Paused = _editorMode || _simulationFrozen || _playsetMode != PlaysetMode.Pinball;

        foreach (ActorView actor in _actors)
        {
            actor.EditorMode = _editorMode;
            actor.AnimationPaused = _simulationFrozen && !_editorMode;
        }

        if (_player is not null)
            _player.CanDragPlayableInEditor = _editorMode && !_playfield.HasStartMarker();

        RefreshUiState();
    }

    private void RefreshUiState()
    {
        if (_workspace is null)
            return;

        DackOwnedSurface surface = _sidebar is not null && _sidebar.Visible
            ? DackOwnedSurface.SpriteStudio
            : _cockpit is not null && _cockpit.Visible
                ? DackOwnedSurface.Cockpit
                : DackOwnedSurface.Canvas;

        DackSimulationState simulation = _simulationStopped
            ? DackSimulationState.Stopped
            : _simulationFrozen
                ? DackSimulationState.Frozen
                : DackSimulationState.Running;

        _uiState.Set(
            simulation,
            _editorMode ? DackAuthoringMode.Build : DackAuthoringMode.Play,
            surface,
            _bossMode ? DackSafetyState.Boss : DackSafetyState.Normal);
    }

    private void TogglePlaysetToolbar()
    {
        bool launchWasActive = _launchScreenActive;
        DismissLaunchScreen();
        if (launchWasActive)
        {
            for (int i = 1; i < _playsetToolbarRow.GetChildCount(); i++)
                _playsetToolbarRow.GetChild<Control>(i).Visible = true;

            _playsetToolbarToggle.Text = "-";
            UpdateCursorMode();
            return;
        }

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
        string authority = _editorMode ? "BUILD" : "PLAY";
        string simulation = _simulationStopped ? "STOPPED" : _editorMode || _simulationFrozen ? "FROZEN" : "RUNNING";
        string dirty = _sessionDirty ? "DIRTY" : "SAVED";
        _cockpitStatus.Text = $"{authority}  â€¢  {simulation}  â€¢  {mode}  â€¢  {dirty}  â€¢  {_playfield.Ocr.StatusText}  â€¢  F6 build/play  â€¢  F7 freeze  â€¢  Esc cockpit";

        if (_transportModeButton is not null)
            _transportModeButton.Text = _editorMode ? "Run (F6)" : "Build (F6)";
        if (_transportFreezeButton is not null)
        {
            _transportFreezeButton.Text = _simulationFrozen ? "Resume (F7)" : "Freeze (F7)";
            _transportFreezeButton.Disabled = _editorMode || _simulationStopped;
        }
        if (_transportStopButton is not null)
            _transportStopButton.Disabled = _editorMode && _simulationStopped;
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
        if (_cockpitTabs is null)
            return;

        string target = _playsetMode switch
        {
            PlaysetMode.Brickbat => "Paddle",
            PlaysetMode.Pinball => "Ball / Table",
            PlaysetMode.Overhead => "Overhead",
            _ => "Side View"
        };

        SelectCockpitTab(target);
    }

    private void SelectCockpitTab(string target)
    {
        if (_cockpitTabs is null)
            return;

        for (int i = 0; i < _cockpitTabs.GetTabCount(); i++)
        {
            if (_cockpitTabs.GetTabTitle(i) == target || _cockpitTabs.GetChild(i).Name == target)
            {
                _cockpitTabs.CurrentTab = i;
                return;
            }
        }
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
            lines.Add($"#{i}: pos {position.X},{position.Y}  size {size.X}Ã—{size.Y}  usable {usable.Size.X}Ã—{usable.Size.Y}");
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

        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView actor = _actors[i];
            if (!actor.Visible || actor.AnimationSet is null)
                continue;

            if (!_editorMode && _playsetMode == PlaysetMode.Platformer)
                continue;

            if (!_editorMode && _enemyAiEnabled)
            {
                UpdateAmbientEnemyPresentation(actor, dt, i);
                continue;
            }

            actor.MotionState = IsFlyingEnemy(actor) ? ActorMotionState.Idle : ActorMotionState.Run;
            actor.AnimationClock += dt * 0.65f;
            actor.QueueRedraw();
        }
    }

    private void UpdateAmbientEnemyPresentation(ActorView actor, float dt, int index)
    {
        if (actor.HomePosition == Vector2.Zero)
            actor.HomePosition = actor.Position;

        float unit = Mathf.Max(_textUnitPixels, 10f);
        Rect2 bounds = _playfield.PlayBounds;
        bool flying = IsFlyingEnemy(actor) || _playsetMode is PlaysetMode.Overhead or PlaysetMode.Pinball or PlaysetMode.Brickbat;
        if (flying)
        {
            float phase = (float)_elapsed * (0.9f + index * 0.04f) + index * 1.7f;
            Vector2 drift = new(
                Mathf.Sin(phase) * unit * 5.2f,
                Mathf.Sin(phase * 1.37f) * unit * 2.4f
            );
            Vector2 next = actor.HomePosition + drift;
            actor.Position = new Vector2(
                Mathf.Clamp(next.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - actor.Size.X)),
                Mathf.Clamp(next.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - actor.Size.Y))
            );
            actor.FacingRight = Mathf.Cos(phase) >= 0f;
            actor.MotionState = ActorMotionState.Idle;
        }
        else
        {
            float direction = _enemyPatrolDirections.TryGetValue(actor, out float existingDirection) && !Mathf.IsZeroApprox(existingDirection)
                ? Mathf.Sign(existingDirection)
                : (actor.FacingRight ? 1f : -1f);
            float patrolRange = unit * 10f;
            float speed = unit * 3.2f;
            Vector2 next = actor.Position + new Vector2(direction * speed * dt, 0f);
            if (next.X < actor.HomePosition.X - patrolRange || next.X > actor.HomePosition.X + patrolRange)
            {
                direction *= -1f;
                next = actor.Position + new Vector2(direction * speed * dt, 0f);
            }

            actor.Position = new Vector2(
                Mathf.Clamp(next.X, bounds.Position.X, Mathf.Max(bounds.Position.X, bounds.End.X - actor.Size.X)),
                Mathf.Clamp(next.Y, bounds.Position.Y, Mathf.Max(bounds.Position.Y, bounds.End.Y - actor.Size.Y))
            );
            actor.FacingRight = direction > 0f;
            actor.MotionState = ActorMotionState.Run;
            _enemyPatrolDirections[actor] = direction;
        }

        actor.AnimationClock += dt;
        actor.QueueRedraw();
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
            {
                _playerVelocity.Y = -jumpSpeed;
                PlaySound("platformer-jump");
            }

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
            PlaySound("enemy-contact");
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
            else if (TryHitEnemy(shotBounds, out Vector2 enemyImpact, out bool defeatedEnemy))
            {
                if (!defeatedEnemy)
                    AddImpactEffect(enemyImpact);
                _playerShots.RemoveAt(i);
            }
            else if (TryHitTextObject(shotBounds, out Vector2 textImpact))
            {
                AddImpactEffect(textImpact);
                PlaySound("combat-explosion");
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

            bool playerInRadar = IsPlayerInsideEnemyRadar(enemy);
            if (_enemyProjectilesEnabled && enemy.CanFireProjectiles && playerInRadar && _hazardArmDelaySeconds <= 0)
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
        bool playerInRadar = IsPlayerInsideEnemyRadar(enemy);
        float phase = (float)_elapsed * 1.4f + index * 2.1f;
        Vector2 patrol = new(
            Mathf.Sin(phase) * _textUnitPixels * 5.5f,
            Mathf.Sin(phase * 1.7f) * _textUnitPixels * 1.8f
        );
        if (_enemyTracksPlayer && playerInRadar)
        {
            float chaseBias = Mathf.Clamp((_player.Position.X - enemy.HomePosition.X) * 0.18f, -_textUnitPixels * 7f, _textUnitPixels * 7f);
            patrol.X += chaseBias;
        }

        enemy.Position = enemy.HomePosition + patrol;
        if (_enemyTracksPlayer && playerInRadar)
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
        bool playerInRadar = IsPlayerInsideEnemyRadar(enemy);

        float patrolRange = motionUnit * 11f;
        float patrolSpeed = motionUnit * 5.2f;
        if (_enemyTracksPlayer && playerInRadar)
            direction = _player.Position.X >= enemy.Position.X ? 1f : -1f;
        else if (enemy.Position.X < enemy.HomePosition.X - patrolRange)
            direction = 1f;
        else if (enemy.Position.X > enemy.HomePosition.X + patrolRange)
            direction = -1f;

        velocity.X = direction * patrolSpeed;
        velocity.Y += motionUnit * 58f * _gravityScale * dt;

        Rect2 actorBounds = new(enemy.Position, enemy.Size);
        Vector2 slideVelocity = _playfield.GetSlideVelocity(actorBounds);
        if (slideVelocity != Vector2.Zero)
        {
            velocity.X = Mathf.MoveToward(velocity.X, slideVelocity.X, Mathf.Abs(slideVelocity.X) * 5f * dt + motionUnit * 16f * dt);
            if (slideVelocity.Y > 0)
                velocity.Y = Mathf.MoveToward(velocity.Y, slideVelocity.Y, Mathf.Abs(slideVelocity.Y) * 3f * dt + motionUnit * 18f * dt);
        }

        Vector2 conveyorVelocity = _playfield.GetConveyorVelocity(actorBounds);
        if (conveyorVelocity != Vector2.Zero)
        {
            velocity += conveyorVelocity * dt;
            if (conveyorVelocity.Y < 0)
                velocity.Y = Mathf.Min(velocity.Y, conveyorVelocity.Y * 0.18f);
        }

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
        float rangeUnits = Mathf.Max(_enemyShotRangeUnits, enemy.RadarRangeUnits);
        float maxRange = Mathf.Max(_textUnitPixels * rangeUnits, 80f);
        if (origin.DistanceTo(target) > maxRange)
            return;

        if (_enemyTracksPlayer && IsPlayerInsideEnemyRadar(enemy))
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
        PlaySound("enemy-shot");
        _platformerStatus = $"{enemy.ActorName.ToUpperInvariant()} FIRED";
        RefreshPlatformerHud();
        PushEnemyShotPositionsToPlayfield();
    }

    private bool IsPlayerInsideEnemyRadar(ActorView enemy)
    {
        if (_player is null)
            return false;

        Vector2 origin = enemy.Position + enemy.Size * 0.5f;
        Vector2 target = _player.Position + _player.Size * 0.5f;
        float rangeUnits = enemy.RadarRangeUnits > 0 ? enemy.RadarRangeUnits : DefaultEnemyRadarRange(enemy.ActorName);
        float maxRange = Mathf.Max(_textUnitPixels * rangeUnits, 80f);
        return origin.DistanceTo(target) <= maxRange;
    }

    private string EnemyRangeButtonText()
    {
        string label = _enemyShotRangeUnits < 28f ? "Near" : _enemyShotRangeUnits < 45f ? "Mid" : "Far";
        return $"Range: {label}";
    }

    private string PlayerShotPowerButtonText() => $"Gun Power: {_playerShotPower}x";
    private string EnemyShotDamageButtonText() => $"Enemy Damage: {_enemyShotDamage}";

    private void ToggleSelectedEnemyProjectileAbility()
    {
        if (_selectedActor is null)
        {
            _inspectorText.Text = "Select an enemy first, then toggle its projectile ability.";
            return;
        }

        if (_selectedActor.IsPlayable)
        {
            _inspectorText.Text = "The selected actor is the player. Use Projectiles -> Player Gun for the player-wide gun toggle.";
            return;
        }

        _selectedActor.CanFireProjectiles = !_selectedActor.CanFireProjectiles;
        ClearEnemyShots();
        _inspectorText.Text = $"{_selectedActor.ActorName} projectile ability: {(_selectedActor.CanFireProjectiles ? "ON" : "OFF")}.\n\nGlobal Enemy Shots must also be on for this enemy to fire during play.";
        RefreshCharacterWorkbenchStatus();
    }

    private void RefreshCharacterWorkbenchStatus()
    {
        if (_characterWorkbenchStatus is null)
            return;

        if (_selectedActor is null)
        {
            _characterWorkbenchStatus.Text = "No actor selected yet.";
            return;
        }

        string role = _selectedActor.IsPlayable ? "Player" : "Enemy / object actor";
        string source = string.IsNullOrWhiteSpace(_selectedActor.AnimationSourceId) ? "unlabeled source" : _selectedActor.AnimationSourceId;
        string projectile = _selectedActor.IsPlayable
            ? (_gunEnabled ? "player gun on" : "player gun off")
            : (_selectedActor.CanFireProjectiles ? "can fire" : "no shots");
        string toughness = _selectedActor.IsPlayable ? $"HP {_playerHealth}/{_playerMaxHealth}" : $"toughness {_selectedActor.ShotToughness}";
        string radar = _selectedActor.IsPlayable ? "player senses" : $"radar {_selectedActor.RadarRangeUnits:0}u";
        _characterWorkbenchStatus.Text =
            $"Selected: {_selectedActor.ActorName}\n"
            + $"Role: {role}\n"
            + $"Animation: {source}\n"
            + $"Combat: {projectile}, {toughness}, {radar}\n"
            + $"Size: {_selectedActor.Size.X:0} Ã— {_selectedActor.Size.Y:0}";
    }

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
        RefreshCharacterWorkbenchStatus();
        RefreshPlatformerHud();
    }

    private void AdjustSelectedEnemyRadar(float delta)
    {
        if (_selectedActor is null || _selectedActor.IsPlayable)
        {
            _inspectorText.Text = "Select an enemy first, then adjust its invisible radar range.";
            return;
        }

        _selectedActor.RadarRangeUnits = Mathf.Clamp(_selectedActor.RadarRangeUnits + delta, 6f, 90f);
        ClearEnemyShots();
        _inspectorText.Text = $"{_selectedActor.ActorName} radar set to {_selectedActor.RadarRangeUnits:0} text units.\n\nTracking and firing now wait until the player enters this invisible awareness bubble. Bigger numbers make the enemy feel smarter or more alert.";
        RefreshCharacterWorkbenchStatus();
        RefreshPlatformerHud();
    }

    private void PushImpactEffectsToPlayfield()
    {
        EffectVisual[] visuals = new EffectVisual[_impactEffects.Count];
        for (int i = 0; i < _impactEffects.Count; i++)
        {
            int frame = _impactEffects[i].Kind == EffectVisualKind.LegacyEnemyDeath
                ? Mathf.Clamp(1 + Mathf.FloorToInt(_impactEffects[i].Age / 0.075f), 1, 6)
                : Mathf.Clamp(1 + Mathf.FloorToInt(_impactEffects[i].Age / 0.052f), 1, 12);
            visuals[i] = new EffectVisual(_impactEffects[i].Position, frame, _impactEffects[i].Kind);
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
                PlaySound("combat-explosion");
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

    private void AddImpactEffect(Vector2 position, EffectVisualKind kind = EffectVisualKind.FireballImpact)
    {
        _impactEffects.Add(new ImpactEffect(position, 0f, kind));
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

    private bool TryHitEnemy(Rect2 shotBounds, out Vector2 impactPosition, out bool defeated)
    {
        impactPosition = shotBounds.GetCenter();
        defeated = false;
        for (int i = 1; i < _actors.Count; i++)
        {
            ActorView enemy = _actors[i];
            if (!enemy.Visible || enemy.AnimationSet is null)
                continue;

            Rect2 enemyBounds = EnemyHitBounds(enemy);
            if (!shotBounds.Intersects(enemyBounds))
                continue;

            impactPosition = enemyBounds.GetCenter();
            defeated = DamageEnemy(enemy, _playerShotPower);
            return true;
        }

        return false;
    }

    private bool DamageEnemy(ActorView enemy, int amount)
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
            AddImpactEffect(impact, EffectVisualKind.LegacyEnemyDeath);
            _playfield.ThrowComicImpact(impact + new Vector2(0f, -enemy.Size.Y * 0.35f), RandomComicWord("defeat"), 1.05f);
            PlaySound("enemy-defeat");
            RefreshPlatformerHud();
            return true;
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
        return false;
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

    private static float DefaultEnemyRadarRange(string actorName)
    {
        if (actorName.Contains("Boss", StringComparison.OrdinalIgnoreCase))
            return 55f;
        if (actorName.Contains("Dragon", StringComparison.OrdinalIgnoreCase)
            || actorName.Contains("Ship", StringComparison.OrdinalIgnoreCase)
            || actorName.Contains("Fleet", StringComparison.OrdinalIgnoreCase)
            || actorName.Contains("Guard", StringComparison.OrdinalIgnoreCase))
            return 42f;
        if (actorName.Contains("Shooter", StringComparison.OrdinalIgnoreCase)
            || actorName.Contains("Orange", StringComparison.OrdinalIgnoreCase))
            return 34f;

        return 24f;
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

        _platformerHud.Visible = !_launchScreenActive && _playsetMode == PlaysetMode.Platformer;
        int visibleEnemies = _actors.Skip(1).Count(actor => actor.Visible && actor.AnimationSet is not null);
        _platformerHudText.Text =
            $"SCORE  {_platformerScore}\n"
            + $"LIVES  {_platformerLives}   HP {_playerHealth}/{_playerMaxHealth}   DEATHS {_platformerDeaths}\n"
            + $"ENEMY  {visibleEnemies}   SHOTS {_enemyShots.Count}   RNG {_enemyShotRangeUnits:0}\n"
            + _platformerStatus;
    }

    private void UpdatePlayerAnimation(float inputX, bool crawlingText)
    {
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

            foreach (WorldObject barricade in _playfield.GetBarricades())
                yield return barricade.Bounds(_textUnitPixels, _playfield.ElapsedSeconds);

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

        foreach (WorldObject barricade in _playfield.GetBarricades())
            yield return barricade.Bounds(_textUnitPixels, _playfield.ElapsedSeconds);

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
            _speedSlider.Step = 1;
            _speedSlider.Value = 0;
            _thicknessSlider.MinValue = 0.3;
            _thicknessSlider.MaxValue = 3.0;
            _thicknessSlider.Step = 0.1;
            _thicknessSlider.Value = 0.8;
            _rangeSlider.MinValue = 0;
            _rangeSlider.MaxValue = 16;
            _rangeSlider.Step = 0.5;
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

        bool isEnemySpawn = selected.Kind == WorldObjectKind.EnemySpawnPoint;
        _speedSlider.Editable = true;
        _speedSlider.MinValue = isEnemySpawn ? 1 : -180;
        _speedSlider.MaxValue = isEnemySpawn ? 10 : 180;
        _speedSlider.Step = 1;
        _thicknessSlider.Editable = true;
        _thicknessSlider.MinValue = isEnemySpawn ? 1 : 0.3;
        _thicknessSlider.MaxValue = isEnemySpawn ? 10 : selected.Kind == WorldObjectKind.Ladder ? 2.5 : 3.0;
        _thicknessSlider.Step = isEnemySpawn ? 1 : 0.1;
        _rangeSlider.Editable = selected.Kind == WorldObjectKind.Elevator || isEnemySpawn;
        _rangeSlider.MinValue = isEnemySpawn ? 1 : 0;
        _rangeSlider.MaxValue = isEnemySpawn ? 10 : 16;
        _rangeSlider.Step = isEnemySpawn ? 1 : 0.5;
        _opacitySlider.Editable = true;
        _tintPicker.Disabled = false;
        _customTintCheck.Disabled = false;
        _customTintCheck.ButtonPressed = selected.UseCustomTint;
        _speedSlider.Value = selected.SpeedUnits;
        _thicknessSlider.Value = Mathf.Clamp(selected.ThicknessUnits, 0.3f, (float)_thicknessSlider.MaxValue);
        _rangeSlider.Value = selected.RangeUnits;
        _opacitySlider.Value = selected.Opacity;
        _tintPicker.Color = selected.UseCustomTint ? selected.Tint : DefaultWorldObjectColor(selected.Kind);
        _attributeText.Text = isEnemySpawn
            ? "Enemy Spawn Point\n"
                + $"Spawn interval: {Mathf.Clamp(Mathf.Round(selected.SpeedUnits), 1f, 10f):0} sec\n"
                + $"Burst count: {Mathf.Clamp(Mathf.Round(selected.ThicknessUnits), 1f, 10f):0}\n"
                + $"Max active: {Mathf.Clamp(Mathf.Round(selected.RangeUnits), 1f, 10f):0}\n"
                + $"Opacity: {selected.Opacity * 100f:0}%\n"
                + $"Color: {(selected.UseCustomTint ? "custom" : "default")}\n"
                + "Editor-only: visible while building, hidden during play.\n"
                + "Next pass: bind enemy type/graphic, spawn speed, and AI preset. Counts stay small; 10 is the hard ceiling."
            : $"{selected.Kind}\n"
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
            FocusMode = FocusModeEnum.None,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
        button.AddThemeFontSizeOverride("font_size", 12);
        button.AddThemeColorOverride("font_color", new Color("#202A34"));
        button.AddThemeColorOverride("font_hover_color", new Color("#101820"));
        button.AddThemeColorOverride("font_pressed_color", new Color("#101820"));
        button.AddThemeColorOverride("font_focus_color", new Color("#101820"));
        button.AddThemeColorOverride("font_disabled_color", new Color("#737F8C"));
        button.AddThemeStyleboxOverride("normal", FlatStyle("#FFFFFF", 6));
        button.AddThemeStyleboxOverride("hover", FlatStyle("#EAF2FF", 6));
        button.AddThemeStyleboxOverride("pressed", FlatStyle("#D9E8FF", 6));
        button.AddThemeStyleboxOverride("disabled", FlatStyle("#E4E8ED", 6));
        return button;
    }

    private static OptionButton SoundPicker(string tooltip)
    {
        OptionButton picker = new()
        {
            TooltipText = tooltip,
            CustomMinimumSize = new Vector2(280, 36),
            FocusMode = FocusModeEnum.None,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
        picker.AddThemeFontSizeOverride("font_size", 12);
        picker.AddThemeColorOverride("font_color", new Color("#202A34"));
        picker.AddThemeColorOverride("font_hover_color", new Color("#101820"));
        picker.AddThemeColorOverride("font_pressed_color", new Color("#101820"));
        picker.AddThemeStyleboxOverride("normal", FlatStyle("#FFFFFF", 6));
        picker.AddThemeStyleboxOverride("hover", FlatStyle("#EAF2FF", 6));
        picker.AddThemeStyleboxOverride("pressed", FlatStyle("#D9E8FF", 6));
        return picker;
    }

    private static HBoxContainer ButtonRow(params Button[] buttons)
    {
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 6);
        foreach (Button button in buttons)
        {
            button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
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
            CustomMinimumSize = new Vector2(58, 30),
            FocusMode = FocusModeEnum.Click
        };
        spin.AddThemeColorOverride("font_color", new Color("#202A34"));
        spin.AddThemeColorOverride("font_hover_color", new Color("#101820"));
        spin.AddThemeColorOverride("font_focus_color", new Color("#101820"));
        spin.GetLineEdit().FocusMode = FocusModeEnum.Click;
        spin.GetLineEdit().SelectAllOnFocus = true;
        StyleLightEditorLineEdit(spin.GetLineEdit());
        return spin;
    }

    private static LineEdit ClipEndpointEdit(int value, int maxFrame)
    {
        LineEdit edit = new()
        {
            Text = Mathf.Clamp(value, 0, Mathf.Max(0, maxFrame)).ToString(),
            PlaceholderText = "-",
            CustomMinimumSize = new Vector2(52, 30),
            FocusMode = FocusModeEnum.Click,
            SelectAllOnFocus = true,
            TooltipText = "Frame number, or '-' if this character does not use this animation."
        };
        edit.AddThemeFontSizeOverride("font_size", 12);
        StyleLightEditorLineEdit(edit);
        return edit;
    }

    private static void StyleLightEditorCheckBox(CheckBox checkBox)
    {
        checkBox.AddThemeColorOverride("font_color", new Color("#202A34"));
        checkBox.AddThemeColorOverride("font_hover_color", new Color("#101820"));
        checkBox.AddThemeColorOverride("font_pressed_color", new Color("#101820"));
        checkBox.AddThemeColorOverride("font_focus_color", new Color("#101820"));
        checkBox.AddThemeColorOverride("font_disabled_color", new Color("#737F8C"));
        checkBox.AddThemeFontSizeOverride("font_size", 12);
    }

    private static void StyleLightEditorLineEdit(LineEdit lineEdit)
    {
        lineEdit.AddThemeColorOverride("font_color", new Color("#202A34"));
        lineEdit.AddThemeColorOverride("font_placeholder_color", new Color("#737F8C"));
        lineEdit.AddThemeColorOverride("font_selected_color", new Color("#FFFFFF"));
        lineEdit.AddThemeColorOverride("selection_color", new Color("#4378B8"));
        lineEdit.AddThemeFontSizeOverride("font_size", 12);
    }

    private Button ShelfButton(string text, WorldObjectKind kind, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            SetPlaysetMode(PlaysetMode.Platformer);
            _playfield.AddPlacedObject(kind);
            MarkSessionDirty($"Placed {text}");
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
            MarkSessionDirty($"Placed {text}");
            _inspectorText.Text = $"{text} placed.\n\n{description}\n\nThis is a pinball construction placeholder: draggable now, physics binding later.";
            RefreshCockpitStatus();
        };
        return button;
    }

    private Button ObjectShelfButton(string text, WorldObjectKind kind, string description)
    {
        Button button = Button(text);
        button.TooltipText = description;
        button.Pressed += () =>
        {
            _playfield.AddPlacedObject(kind);
            MarkSessionDirty($"Placed {text}");
            SyncEditorModeToScene();
            _inspectorText.Text = $"{text} placed.\n\n{description}\n\nObjects are shared level furniture: they can belong to platformer, pinball, Brickbat, overhead, text quests, or later builder presets.";
            RefreshCockpitStatus();
        };
        return button;
    }

    private Control CharacterSlotShelf(string title, string current, string description, string? targetTab = null)
    {
        Button shelf = Button($"{title}\n{current}");
        shelf.CustomMinimumSize = new Vector2(0, 54);
        shelf.TooltipText = description;
        shelf.Pressed += () =>
        {
            if (!string.IsNullOrWhiteSpace(targetTab))
                SelectCockpitTab(targetTab);
            _inspectorText.Text = $"{title} shelf\n\n{description}\n\nNext pass: drag a card here from Projectiles, Effects, Sounds, AI, or Text Rules. For now, use the matching Cockpit page and the selected actor.";
        };
        shelf.AddThemeFontSizeOverride("font_size", 11);
        return shelf;
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
            WorldObjectKind.EnemySpawnPoint => new Color("#FF2B2B"),
            WorldObjectKind.Checkpoint => new Color("#5CB8A7"),
            WorldObjectKind.PinballFlipper => new Color("#FF5C35"),
            WorldObjectKind.PinballBumper => new Color("#5CB8A7"),
            WorldObjectKind.PinballPlunger => new Color("#B56CFF"),
            WorldObjectKind.PinballDrain => new Color("#202A34"),
            WorldObjectKind.PinballRollover => new Color("#F4C95D"),
            WorldObjectKind.PinballGate => new Color("#5CB8FF"),
            WorldObjectKind.Coin => new Color("#F4C95D"),
            WorldObjectKind.Gem => new Color("#B56CFF"),
            WorldObjectKind.Barricade => new Color("#8A5A37"),
            _ => new Color("#5CB8A7")
        };
    }

    private static PanelContainer CockpitPanel(float width)
    {
        PanelContainer panel = new()
        {
            CustomMinimumSize = new Vector2(width, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride("panel", FlatStyle("#F7F5EF", 8));
        return panel;
    }

    private static void AddCockpitTab(TabContainer tabs, string title, Control content)
    {
        ScrollContainer scroll = new()
        {
            Name = title,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        content.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.AddChild(content);
        tabs.AddChild(scroll);
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

    private struct ImpactEffect(Vector2 position, float age, EffectVisualKind kind)
    {
        public Vector2 Position = position;
        public float Age = age;
        public EffectVisualKind Kind = kind;
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

    private sealed class DackSnapshotManifest
    {
        public string format { get; set; } = "dacksnapshot";
        public int version { get; set; } = 1;
        public string imagePath { get; set; } = "";
        public string sourceName { get; set; } = "";
        public string playsetMode { get; set; } = "";
        public int nativeWidth { get; set; }
        public int nativeHeight { get; set; }
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
        public float radarRangeUnits { get; set; } = 28f;
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
                radarRangeUnits = actor.RadarRangeUnits,
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

    private sealed class LegacyBundleManifest
    {
        public string sourceRoot { get; set; } = "";
        public string generatedUtc { get; set; } = "";
        public string note { get; set; } = "";
        public List<LegacyAssetBundle> bundles { get; set; } = [];
    }

    private sealed class LegacyAssetBundle
    {
        public string bundleRoot { get; set; } = "";
        public string displayName { get; set; } = "";
        public string primaryCategory { get; set; } = "";
        public string quality { get; set; } = "";
        public int imageFiles { get; set; }
        public Dictionary<string, int> categoryCounts { get; set; } = [];
        public Dictionary<string, int> animationHintCounts { get; set; } = [];
        public Dictionary<string, int> commonDimensions { get; set; } = [];
        public List<string> spriteSheets { get; set; } = [];
        public List<string> previews { get; set; } = [];
        public List<string> sampleFiles { get; set; } = [];
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
