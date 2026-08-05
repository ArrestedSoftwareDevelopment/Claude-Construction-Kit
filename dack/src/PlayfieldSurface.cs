using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public partial class PlayfieldSurface : Control
{
    private Texture2D? _brick;
    private Texture2D? _platform;
    private Texture2D? _window;
    private Texture2D? _fireballExplosion;
    private Texture2D? _legacyEnemyDeath;
    private CapturedPageFrame? _capturedPage;
    private readonly PsychedelicEffects _letterEffects = new();
    private readonly List<WorldObject> _placedWorldObjects = [];
    private readonly RandomNumberGenerator _placementRandom = new();
    private Vector2[] _playerShotPositions = [];
    private Vector2[] _enemyShotPositions = [];
    private EffectVisual[] _impactEffects = [];
    private int _selectedWorldObjectIndex = -1;
    private int _draggedHandle = -1;
    private Vector2 _dragBodyOffset;
    private double _elapsed;

    public float TextUnitPixels { get; set; } = 7f;
    public PlatformerMode Mode { get; set; } = PlatformerMode.Horizontal;
    public LazyOcrService Ocr { get; } = new();
    public bool HasCapturedPage => _capturedPage is not null;
    public string CapturedPageSourceName => _capturedPage?.SourceName ?? string.Empty;
    public Vector2I CapturedPageSize => _capturedPage?.PixelSize ?? Vector2I.Zero;
    public PlayfieldProfile? Profile => _capturedPage?.Profile;
    public bool ShowEditorOnlyObjects { get; set; }
    public bool EditorMode { get; set; } = true;
    public bool SimulationPaused { get; set; } = true;
    public bool TextCrawlEnabled { get; set; } = true;
    public Rect2 PlayBounds => _capturedPage is not null ? GetCapturedPageDrawRect(_capturedPage) : new Rect2(Vector2.Zero, Size);
    public event Action<string>? WorldObjectSelectionChanged;
    public event Action<WorldObject?>? WorldObjectSelectionObjectChanged;
    public event Action<WorldObject?>? WorldObjectInspectionRequested;
    public event Action<string>? WorldObjectChanged;
    public event Action<Godot.Collections.Dictionary, Vector2>? CardDroppedOnPlayfield;
    public float ElapsedSeconds
    {
        get => (float)_elapsed;
        set
        {
            _elapsed = value;
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Pass;
        _placementRandom.Randomize();
        _brick = LoadPng("res://assets/third_party/8-bit-dungeon/brick-solid.png");
        _platform = LoadPng("res://assets/third_party/8-bit-dungeon/platform.png");
        _window = LoadPng("res://assets/third_party/8-bit-dungeon/window-2.png");
        _fireballExplosion = LoadPng("res://assets/project/effects/fireball-impact-explosion.png");
        _legacyEnemyDeath = LoadPng("res://assets/project/effects/legacy-enemy-death.png");
        _capturedPage = CapturedPageImportModule.TryLoadDefault();
        Resized += QueueRedraw;
        SetProcess(true);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return false;

        Godot.Collections.Dictionary card = data.AsGodotDictionary();
        return card.ContainsKey("dackCardKind") && card.ContainsKey("dackCardId");
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return;

        CardDroppedOnPlayfield?.Invoke(data.AsGodotDictionary(), atPosition);
    }

    public override void _Process(double delta)
    {
        if (!SimulationPaused)
            _letterEffects.Update((float)delta);
        if (_letterEffects.HasActiveEffects)
            QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (!EditorMode)
            return;

        if (inputEvent is InputEventMouseButton inspectButton
            && inspectButton.ButtonIndex == MouseButton.Right
            && inspectButton.Pressed)
        {
            if (TrySelectWorldObject(inspectButton.Position))
            {
                WorldObjectInspectionRequested?.Invoke(GetSelectedWorldObject());
                AcceptEvent();
            }
        }
        else if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                if (TryBeginWorldObjectDrag(mouseButton.Position))
                    AcceptEvent();
            }
            else if (_draggedHandle != -1)
            {
                _draggedHandle = -1;
                AcceptEvent();
            }
        }
        else if (inputEvent is InputEventMouseMotion motion && _selectedWorldObjectIndex >= 0 && _draggedHandle != -1)
        {
            if (_draggedHandle == 2)
                DragSelectedBody(motion.Position);
            else
                DragSelectedHandle(motion.Position);
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        DrawBackground();

        if (_capturedPage is not null)
        {
            DrawWorldObjects();
            DrawPlayerShots();
            return;
        }

        float floorY = GetFloor().Position.Y;
        if (_platform is not null)
        {
            for (float x = 0; x < Size.X; x += 64)
                DrawTextureRect(_platform, new Rect2(x, floorY, 64, 64), false);
        }

        if (_brick is not null)
        {
            DrawTextureRect(_brick, new Rect2(Size.X * 0.34f, floorY - 54, 54, 54), false);
            DrawTextureRect(_brick, new Rect2(Size.X * 0.34f + 54, floorY - 54, 54, 54), false);
        }

        foreach (Rect2 platform in GetPlatforms())
            DrawPlatform(platform);

        DrawWorldObjects();

        DrawPlayerShots();

        DrawRect(new Rect2(0, 0, Size.X, 30), new Color("#26313C"), true);
        DrawRect(new Rect2(14, 9, 12, 12), new Color("#FF5C35"), true);
        DrawRect(new Rect2(34, 9, 12, 12), new Color("#F4C95D"), true);
        DrawRect(new Rect2(54, 9, 12, 12), new Color("#5CB8A7"), true);
    }

    public Rect2 GetFloor()
    {
        return new Rect2(0, Size.Y - TextUnitPixels * 10f, Size.X, TextUnitPixels * 10f);
    }

    public Rect2[] GetPlatforms()
    {
        if (_capturedPage is not null)
            return GetCapturedTextPlatforms(_capturedPage);

        float unit = TextUnitPixels;
        if (Mode == PlatformerMode.Vertical)
        {
            return
            [
                new Rect2(Size.X * 0.16f, Size.Y * 0.72f, unit * 14f, unit * 0.8f),
                new Rect2(Size.X * 0.50f, Size.Y * 0.54f, unit * 12f, unit * 0.8f),
                new Rect2(Size.X * 0.24f, Size.Y * 0.36f, unit * 13f, unit * 0.8f)
            ];
        }

        return
        [
            new Rect2(Size.X * 0.22f, Size.Y * 0.68f, unit * 13f, unit * 0.8f),
            new Rect2(Size.X * 0.58f, Size.Y * 0.50f, unit * 12f, unit * 0.8f)
        ];
    }

    public Rect2[] GetBrickRegions()
    {
        return GetTextObjectRegions(TextObjectGranularity.Letter);
    }

    public Rect2[] GetTextObjectRegions(TextObjectGranularity granularity)
    {
        if (_capturedPage is not null)
        {
            Rect2[] sourceRects = granularity switch
            {
                TextObjectGranularity.BonusAnchor => _capturedPage.BonusAnchors,
                TextObjectGranularity.Word => _capturedPage.TextWords,
                TextObjectGranularity.Line => _capturedPage.TextLines,
                _ => _capturedPage.TextBricks
            };

            return GetCapturedRects(_capturedPage, sourceRects, activeOnly: true);
        }

        return GetPlatforms();
    }

    public bool IsTextRegionStillActive(Rect2 displayRegion)
    {
        if (_capturedPage is null)
            return true;

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, displayRegion).Grow(2f);
        return HasCurrentInkInRegion(_capturedPage, sourceRegion);
    }

    public Rect2 FindWhitespaceRect(Vector2 desiredSize)
    {
        Rect2 fallback = new(Mathf.Max(0, Size.X - desiredSize.X - 16f), 16f, desiredSize.X, desiredSize.Y);
        if (_capturedPage is null)
            return fallback;

        Rect2 document = PlayBounds;
        Rect2[] candidates =
        [
            new Rect2(document.End.X + 14f, document.Position.Y + 18f, desiredSize.X, desiredSize.Y),
            new Rect2(document.Position.X + 14f, document.End.Y + 10f, desiredSize.X, desiredSize.Y),
            new Rect2(document.End.X - desiredSize.X - 18f, document.End.Y - desiredSize.Y - 18f, desiredSize.X, desiredSize.Y),
            new Rect2(document.Position.X + 18f, document.End.Y - desiredSize.Y - 18f, desiredSize.X, desiredSize.Y),
            new Rect2(document.End.X - desiredSize.X - 18f, document.Position.Y + 18f, desiredSize.X, desiredSize.Y)
        ];

        Rect2 best = fallback;
        float bestScore = float.NegativeInfinity;
        foreach (Rect2 candidate in candidates)
        {
            if (candidate.End.X > Size.X || candidate.End.Y > Size.Y || candidate.Position.X < 0 || candidate.Position.Y < 0)
                continue;

            float score = ScoreWhitespace(candidate);
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    public Color GetDocumentBackgroundColor(Rect2 region)
    {
        if (_capturedPage is null)
            return new Color("#FBFBF8");

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, region);

        return SampleBackgroundColor(_capturedPage.OriginalImage, sourceRegion);
    }

    public void EraseDocumentText(Rect2 region)
    {
        if (_capturedPage is null)
            return;

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, region);
        Color fill = SampleBackgroundColor(_capturedPage.OriginalImage, sourceRegion.Grow(4f));
        FloodEraseConnectedInk(_capturedPage, sourceRegion, fill);
        CleanupTinyInkSpecks(_capturedPage, sourceRegion.Grow(9f), fill);

        if (_capturedPage.Texture is ImageTexture imageTexture)
            imageTexture.Update(_capturedPage.Image);

        QueueRedraw();
    }

    public bool TryCreateOcrSample(Rect2 displayRegion, out Image? sample)
    {
        sample = null;
        if (_capturedPage is null)
            return false;

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, displayRegion).Grow(4f);
        Rect2I bounds = ClampToImage(_capturedPage.OriginalImage, sourceRegion);
        if (bounds.Size.X < 2 || bounds.Size.Y < 2)
            return false;

        sample = _capturedPage.OriginalImage.GetRegion(bounds);
        sample.Convert(Image.Format.Rgba8);

        if (sample.GetHeight() < 32)
        {
            int scale = sample.GetHeight() < 18 ? 4 : 3;
            sample.Resize(sample.GetWidth() * scale, sample.GetHeight() * scale, Image.Interpolation.Nearest);
        }

        return true;
    }

    public void ResetDocumentImage()
    {
        if (_capturedPage is null)
            return;

        _capturedPage.Image.BlitRect(
            _capturedPage.OriginalImage,
            new Rect2I(Vector2I.Zero, _capturedPage.PixelSize),
            Vector2I.Zero
        );

        if (_capturedPage.Texture is ImageTexture imageTexture)
            imageTexture.Update(_capturedPage.Image);

        QueueRedraw();
    }

    public bool TrySaveWorkingSnapshot(string outputPath)
    {
        if (_capturedPage is null || string.IsNullOrWhiteSpace(outputPath))
            return false;

        Error result = _capturedPage.Image.SavePng(outputPath);
        return result == Error.Ok;
    }

    public void SetPlayerShotPositions(IReadOnlyList<Vector2> positions)
    {
        if (positions.Count == 0)
        {
            _playerShotPositions = [];
            QueueRedraw();
            return;
        }

        Vector2[] copy = new Vector2[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            copy[i] = positions[i];

        _playerShotPositions = copy;
        QueueRedraw();
    }

    public void SetEnemyShotPositions(IReadOnlyList<Vector2> positions)
    {
        if (positions.Count == 0)
        {
            _enemyShotPositions = [];
            QueueRedraw();
            return;
        }

        Vector2[] copy = new Vector2[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            copy[i] = positions[i];

        _enemyShotPositions = copy;
        QueueRedraw();
    }

    public void SetImpactEffects(IReadOnlyList<EffectVisual> effects)
    {
        if (effects.Count == 0)
        {
            _impactEffects = [];
            QueueRedraw();
            return;
        }

        EffectVisual[] copy = new EffectVisual[effects.Count];
        for (int i = 0; i < effects.Count; i++)
            copy[i] = effects[i];

        _impactEffects = copy;
        QueueRedraw();
    }

    public void AddPlacedObject(WorldObjectKind kind, Vector2? preferredCenter = null)
    {
        Rect2 bounds = PlayBounds;
        float unit = TextUnitPixels;
        Vector2 center = preferredCenter.HasValue
            ? ClampPlacementCenter(preferredCenter.Value, kind, bounds, unit)
            : GetRandomPlacementCenter(kind, bounds, unit);

        WorldObject placed = kind switch
        {
            WorldObjectKind.Ladder => new WorldObject(kind, center + new Vector2(-unit * 2f, unit * 8f), center + new Vector2(-unit * 2f, -unit * 8f), 2.0f),
            WorldObjectKind.Ramp => new WorldObject(kind, center + new Vector2(-unit * 12f, unit * 5f), center + new Vector2(unit * 12f, -unit * 4f), 0.9f),
            WorldObjectKind.Slide => new WorldObject(kind, center + new Vector2(-unit * 12f, -unit * 4f), center + new Vector2(unit * 12f, unit * 5f), 0.9f, 22f),
            WorldObjectKind.Conveyor => new WorldObject(kind, center + new Vector2(-unit * 13f, unit * 10f), center + new Vector2(unit * 13f, unit * 10f), 0.9f, 90f),
            WorldObjectKind.Elevator => new WorldObject(kind, center + new Vector2(-unit * 8f, unit * 2f), center + new Vector2(unit * 8f, unit * 2f), 0.9f, 1.6f, _placedWorldObjects.Count * 0.45f),
            WorldObjectKind.Checkpoint => CreateMarker(center, unit, MarkerRole.Midpoint, true),
            WorldObjectKind.StartPoint => CreateMarker(center, unit, MarkerRole.Start, false),
            WorldObjectKind.GoalPoint => CreateMarker(center, unit, MarkerRole.End, true),
            WorldObjectKind.HiddenSwitch => CreateMarker(center, unit, MarkerRole.Switch, false),
            WorldObjectKind.EnemySpawnPoint => CreateMarker(center, unit, MarkerRole.EnemySpawn, false) with { SpeedUnits = 5f, ThicknessUnits = 1f, RangeUnits = 3f },
            WorldObjectKind.PinballFlipper => new WorldObject(kind, center + new Vector2(-unit * 8f, unit * 10f), center + new Vector2(unit * 10f, unit * 13f), 1.25f, 12f, 0f, 5f, MarkerRole.None, true, false, default, 0.92f),
            WorldObjectKind.PinballBumper => new WorldObject(kind, center, center + new Vector2(unit * 5f, 0), 1.2f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.92f),
            WorldObjectKind.PinballPlunger => new WorldObject(kind, center + new Vector2(unit * 18f, unit * 16f), center + new Vector2(unit * 18f, -unit * 12f), 1.1f, 12f, 0f, 5f, MarkerRole.None, true, false, default, 0.9f),
            WorldObjectKind.PinballDrain => new WorldObject(kind, center + new Vector2(-unit * 12f, unit * 18f), center + new Vector2(unit * 12f, unit * 18f), 1.1f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.9f),
            WorldObjectKind.PinballRollover => new WorldObject(kind, center + new Vector2(-unit * 5f, -unit * 4f), center + new Vector2(unit * 5f, -unit * 4f), 0.85f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.88f),
            WorldObjectKind.PinballGate => new WorldObject(kind, center + new Vector2(-unit * 5f, unit * 2f), center + new Vector2(unit * 6f, -unit * 4f), 0.75f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.88f),
            WorldObjectKind.Coin => new WorldObject(kind, center - new Vector2(unit * 2.1f, unit * 2.1f), center + new Vector2(unit * 2.1f, unit * 2.1f), 0.8f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.96f),
            WorldObjectKind.Gem => new WorldObject(kind, center - new Vector2(unit * 2.4f, unit * 2.4f), center + new Vector2(unit * 2.4f, unit * 2.4f), 0.8f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.96f),
            WorldObjectKind.Barricade => new WorldObject(kind, center + new Vector2(-unit * 7f, unit * 4f), center + new Vector2(unit * 7f, unit * 4f), 1.6f, 0f, 0f, 5f, MarkerRole.None, true, false, default, 0.92f),
            _ => new WorldObject(kind, center - new Vector2(unit * 8f, 0), center + new Vector2(unit * 8f, 0), 0.8f)
        };

        _placedWorldObjects.Add(placed);
        _selectedWorldObjectIndex = _placedWorldObjects.Count - 1;
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    private static Vector2 ClampPlacementCenter(Vector2 center, WorldObjectKind kind, Rect2 bounds, float unit)
    {
        float horizontalMargin = unit * (kind == WorldObjectKind.PinballPlunger ? 20f : 3f);
        float verticalMargin = unit * (kind is WorldObjectKind.PinballPlunger or WorldObjectKind.PinballDrain ? 20f : 3f);
        return new Vector2(
            Mathf.Clamp(center.X, bounds.Position.X + horizontalMargin, Mathf.Max(bounds.Position.X + horizontalMargin, bounds.End.X - horizontalMargin)),
            Mathf.Clamp(center.Y, bounds.Position.Y + verticalMargin, Mathf.Max(bounds.Position.Y + verticalMargin, bounds.End.Y - verticalMargin))
        );
    }

    private Vector2 GetRandomPlacementCenter(WorldObjectKind kind, Rect2 bounds, float unit)
    {
        // Leave enough room for the longest default line objects and keep the
        // first placement away from the exact center. The creator can then
        // drag/rotate the new instance precisely; repeated clicks create new
        // instances instead of reusing a single center slot.
        float horizontalMargin = unit * (kind == WorldObjectKind.PinballPlunger ? 20f : 16f);
        float verticalMargin = unit * (kind is WorldObjectKind.PinballPlunger or WorldObjectKind.PinballDrain ? 20f : 14f);
        float minX = bounds.Position.X + horizontalMargin;
        float maxX = bounds.End.X - horizontalMargin;
        float minY = bounds.Position.Y + verticalMargin;
        float maxY = bounds.End.Y - verticalMargin;

        if (maxX <= minX || maxY <= minY)
            return bounds.GetCenter();

        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2 candidate = new(
                _placementRandom.RandfRange(minX, maxX),
                _placementRandom.RandfRange(minY, maxY)
            );

            bool tooClose = false;
            foreach (WorldObject existing in _placedWorldObjects)
            {
                if (existing.Center.DistanceTo(candidate) < unit * 5f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return candidate;
        }

        // Crowded levels still get a predictable, usable fallback rather than
        // stacking another object directly on the previous selection.
        int fallbackIndex = _placedWorldObjects.Count;
        Vector2 fallback = bounds.GetCenter() + new Vector2(
            ((fallbackIndex % 5) - 2) * unit * 6f,
            ((fallbackIndex / 5) % 4 - 1.5f) * unit * 5f
        );
        return new Vector2(
            Mathf.Clamp(fallback.X, minX, maxX),
            Mathf.Clamp(fallback.Y, minY, maxY)
        );
    }

    public void AddMarker(MarkerRole role, bool visibleInPlay)
    {
        Rect2 bounds = PlayBounds;
        float unit = TextUnitPixels;
        WorldObjectKind markerKind = role switch
        {
            MarkerRole.Switch => WorldObjectKind.HiddenSwitch,
            MarkerRole.EnemySpawn => WorldObjectKind.EnemySpawnPoint,
            MarkerRole.Start => WorldObjectKind.StartPoint,
            MarkerRole.End => WorldObjectKind.GoalPoint,
            _ => WorldObjectKind.Checkpoint
        };
        Vector2 center = GetRandomPlacementCenter(markerKind, bounds, unit);
        WorldObject marker = CreateMarker(center, unit, role, visibleInPlay);
        _placedWorldObjects.Add(marker);
        _selectedWorldObjectIndex = _placedWorldObjects.Count - 1;
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    private static WorldObject CreateMarker(Vector2 center, float unit, MarkerRole role, bool visibleInPlay)
    {
        WorldObjectKind kind = role switch
        {
            MarkerRole.Switch => WorldObjectKind.HiddenSwitch,
            MarkerRole.EnemySpawn => WorldObjectKind.EnemySpawnPoint,
            MarkerRole.Start => WorldObjectKind.StartPoint,
            MarkerRole.End => WorldObjectKind.GoalPoint,
            _ => WorldObjectKind.Checkpoint
        };
        return new WorldObject(
            kind,
            center + new Vector2(0, unit * 6f),
            center,
            0.8f,
            0f,
            0f,
            5f,
            role,
            visibleInPlay
        );
    }

    public void ClearPlacedObjects()
    {
        _placedWorldObjects.Clear();
        _selectedWorldObjectIndex = -1;
        _draggedHandle = -1;
        _dragBodyOffset = Vector2.Zero;
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    public WorldObject[] GetLadders()
    {
        List<WorldObject> ladders = ObjectsOfKind(WorldObjectKind.Ladder);

        if (_capturedPage is null && Mode == PlatformerMode.Vertical)
        {
            ladders.Add(new WorldObject(WorldObjectKind.Ladder, new Vector2(Size.X * 0.29f, Size.Y * 0.72f), new Vector2(Size.X * 0.29f, Size.Y * 0.36f), 2.0f));
            ladders.Add(new WorldObject(WorldObjectKind.Ladder, new Vector2(Size.X * 0.62f, Size.Y * 0.88f), new Vector2(Size.X * 0.62f, Size.Y * 0.54f), 2.0f));
        }

        return ladders.ToArray();
    }

    public WorldObject[] GetRamps()
    {
        List<WorldObject> ramps = ObjectsOfKind(WorldObjectKind.Ramp);
        ramps.AddRange(ObjectsOfKind(WorldObjectKind.Slide));

        float unit = TextUnitPixels;
        if (_capturedPage is null && Mode == PlatformerMode.Vertical)
        {
            ramps.Add(new WorldObject(WorldObjectKind.Ramp, new Vector2(Size.X * 0.42f, Size.Y * 0.56f), new Vector2(Size.X * 0.58f, Size.Y * 0.39f), 0.9f));
        }
        else if (_capturedPage is null)
        {
            ramps.Add(new WorldObject(WorldObjectKind.Ramp, new Vector2(Size.X * 0.39f, Size.Y * 0.66f), new Vector2(Size.X * 0.55f, Size.Y * 0.56f), 0.9f));
        }

        return ramps.ToArray();
    }

    public WorldObject[] GetConveyors()
    {
        List<WorldObject> conveyors = ObjectsOfKind(WorldObjectKind.Conveyor);

        if (_capturedPage is null)
            conveyors.Add(new WorldObject(WorldObjectKind.Conveyor, new Vector2(Size.X * 0.66f, Size.Y * 0.78f), new Vector2(Size.X * 0.85f, Size.Y * 0.78f), 0.9f, Mode == PlatformerMode.Horizontal ? -90f : 90f));

        return conveyors.ToArray();
    }

    public WorldObject[] GetElevators()
    {
        List<WorldObject> elevators = ObjectsOfKind(WorldObjectKind.Elevator);

        if (_capturedPage is null)
            elevators.Add(new WorldObject(WorldObjectKind.Elevator, new Vector2(Size.X * 0.74f, Size.Y * 0.40f), new Vector2(Size.X * 0.86f, Size.Y * 0.40f), 0.9f, 1.6f, 0.25f));

        return elevators.ToArray();
    }

    public WorldObject[] GetBarricades()
    {
        return ObjectsOfKind(WorldObjectKind.Barricade).ToArray();
    }

    public bool IsTouchingLadder(Rect2 actorBounds)
    {
        if (Mode != PlatformerMode.Vertical)
            return false;

        foreach (WorldObject ladder in GetLadders())
        {
            if (ActorTouchesLadder(actorBounds, ladder))
                return true;
        }

        if (_capturedPage is not null)
            return IsTouchingTextCrawlSurface(actorBounds);

        return false;
    }

    private bool ActorTouchesLadder(Rect2 actorBounds, WorldObject ladder)
    {
        return LadderRect(ladder).Intersects(actorBounds, true);
    }

    private bool IsTouchingTextCrawlSurface(Rect2 actorBounds)
    {
        if (!TextCrawlEnabled || Mode != PlatformerMode.Vertical)
            return false;

        Rect2 probe = actorBounds.Grow(TextUnitPixels * 0.6f);
        foreach (Rect2 line in GetTextObjectRegions(TextObjectGranularity.Line))
        {
            if (line.Size.Y > TextUnitPixels * 2.2f)
                continue;

            if (line.Intersects(probe, true))
                return true;
        }

        return false;
    }

    public bool IsTouchingRamp(Rect2 actorBounds)
    {
        foreach (WorldObject ramp in ObjectsOfKind(WorldObjectKind.Ramp))
        {
            if (ramp.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
                return true;
        }

        return false;
    }

    public Vector2 GetSlideVelocity(Rect2 actorBounds)
    {
        foreach (WorldObject slide in ObjectsOfKind(WorldObjectKind.Slide))
        {
            if (slide.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
            {
                float speed = Mathf.Abs(slide.SpeedUnits) > 0.001f ? Mathf.Abs(slide.SpeedUnits) : 7f;
                return slide.DownhillDirection(TextUnitPixels, ElapsedSeconds) * speed * TextUnitPixels;
            }
        }

        return Vector2.Zero;
    }

    public Vector2 GetConveyorVelocity(Rect2 actorBounds)
    {
        foreach (WorldObject conveyor in GetConveyors())
        {
            if (conveyor.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
                return conveyor.MotionDirection(TextUnitPixels, ElapsedSeconds) * Mathf.Abs(conveyor.SpeedUnits) * TextUnitPixels;
        }

        return Vector2.Zero;
    }

    public Vector2? GetEditorStartPosition(Vector2 actorSize)
    {
        WorldObject? start = null;
        foreach (WorldObject worldObject in _placedWorldObjects)
        {
            if (worldObject.MarkerRole == MarkerRole.Start || worldObject.Kind == WorldObjectKind.StartPoint)
                start = worldObject;
        }

        if (start is null)
            return null;

        Vector2 center = start.Center;
        return new Vector2(center.X - actorSize.X * 0.5f, center.Y - actorSize.Y);
    }

    public bool HasStartMarker()
    {
        foreach (WorldObject worldObject in _placedWorldObjects)
        {
            if (worldObject.MarkerRole == MarkerRole.Start || worldObject.Kind == WorldObjectKind.StartPoint)
                return true;
        }

        return false;
    }

    public WorldObject? GetSelectedWorldObject()
    {
        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
            return null;

        return _placedWorldObjects[_selectedWorldObjectIndex];
    }

    public Rect2? GetGoalBounds()
    {
        WorldObject? goal = null;
        foreach (WorldObject worldObject in _placedWorldObjects)
        {
            if (worldObject.MarkerRole == MarkerRole.End || worldObject.Kind == WorldObjectKind.GoalPoint)
                goal = worldObject;
        }

        return goal?.Bounds(TextUnitPixels, ElapsedSeconds).Grow(TextUnitPixels * 0.45f);
    }

    public WorldObject[] GetPlacedWorldObjects()
    {
        return _placedWorldObjects.ToArray();
    }

    public void SetPlacedWorldObjects(IEnumerable<WorldObject> objects)
    {
        _placedWorldObjects.Clear();
        _placedWorldObjects.AddRange(objects);
        _selectedWorldObjectIndex = _placedWorldObjects.Count > 0 ? _placedWorldObjects.Count - 1 : -1;
        _draggedHandle = -1;
        _dragBodyOffset = Vector2.Zero;
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    public void SetSelectedSpeed(float speedUnits)
    {
        UpdateSelected(selected => selected.Kind == WorldObjectKind.EnemySpawnPoint
            ? selected with { SpeedUnits = Mathf.Clamp(Mathf.Round(speedUnits), 1f, 10f) }
            : selected with { SpeedUnits = speedUnits });
    }

    public void SetSelectedThickness(float thicknessUnits)
    {
        UpdateSelected(selected =>
        {
            if (selected.Kind == WorldObjectKind.EnemySpawnPoint)
                return selected with { ThicknessUnits = Mathf.Clamp(Mathf.Round(thicknessUnits), 1f, 10f) };

            float maxThickness = selected.Kind == WorldObjectKind.Ladder ? 2.5f : 3.0f;
            return selected with { ThicknessUnits = Mathf.Clamp(thicknessUnits, 0.3f, maxThickness) };
        });
    }

    public void SetSelectedRange(float rangeUnits)
    {
        UpdateSelected(selected => selected.Kind switch
        {
            WorldObjectKind.Elevator => selected with { RangeUnits = Mathf.Clamp(rangeUnits, 0f, 16f) },
            WorldObjectKind.EnemySpawnPoint => selected with { RangeUnits = Mathf.Clamp(Mathf.Round(rangeUnits), 1f, 10f) },
            _ => selected
        });
    }

    public void SetSelectedTint(Color tint)
    {
        UpdateSelected(selected => selected with { Tint = tint, UseCustomTint = true });
    }

    public void SetSelectedOpacity(float opacity)
    {
        UpdateSelected(selected => selected with { Opacity = Mathf.Clamp(opacity, 0f, 1f) });
    }

    public void ClearSelectedTint()
    {
        UpdateSelected(selected => selected with { UseCustomTint = false });
    }

    public void ReverseSelectedDirection()
    {
        UpdateSelected(selected =>
        {
            if (selected.Kind == WorldObjectKind.Conveyor)
                return selected with { SpeedUnits = Mathf.Abs(selected.SpeedUnits) < 0.001f ? -90f : -selected.SpeedUnits };

            return selected with { Start = selected.End, End = selected.Start };
        });
    }

    public void RotateSelected(float degrees)
    {
        UpdateSelected(selected =>
        {
            if (selected.IsMarker || selected.Kind == WorldObjectKind.Ladder)
                return selected;

            float radians = Mathf.DegToRad(degrees);
            Vector2 center = selected.Center;
            Vector2 start = center + (selected.Start - center).Rotated(radians);
            Vector2 end = center + (selected.End - center).Rotated(radians);
            return selected with
            {
                Start = SnapToPlayBounds(start),
                End = SnapToPlayBounds(end)
            };
        });
    }

    public void NormalizeSelectedSlope()
    {
        UpdateSelected(selected =>
        {
            if (selected.Kind == WorldObjectKind.Ramp && selected.End.Y > selected.Start.Y)
                return selected with { Start = selected.End, End = selected.Start };

            if (selected.Kind == WorldObjectKind.Slide && selected.End.Y < selected.Start.Y)
                return selected with { Start = selected.End, End = selected.Start };

            return selected;
        });
    }

    private void UpdateSelected(Func<WorldObject, WorldObject> update)
    {
        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
            return;

        _placedWorldObjects[_selectedWorldObjectIndex] = update(_placedWorldObjects[_selectedWorldObjectIndex]);
        WorldObjectChanged?.Invoke($"{_placedWorldObjects[_selectedWorldObjectIndex].Kind} changed");
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    public Vector2 GetSpawnPosition(Vector2 actorSize)
    {
        foreach (Rect2 platform in GetPlatforms())
        {
            if (platform.Size.X < actorSize.X * 1.5f || platform.Position.Y < 120f)
                continue;

            if (platform.Position.X > Size.X - actorSize.X || platform.End.X < actorSize.X)
                continue;

            float x = Mathf.Clamp(platform.Position.X + 4f, 0, Mathf.Max(0, Size.X - actorSize.X));
            return new Vector2(x, platform.Position.Y - actorSize.Y);
        }

        return new Vector2(Size.X * 0.18f, GetFloor().Position.Y - actorSize.Y);
    }

    private void DrawWindow(Rect2 rect, Color body)
    {
        DrawRect(rect with { Position = rect.Position + new Vector2(5, 7) }, new Color(0, 0, 0, 0.13f), true);
        DrawRect(rect, body, true);
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X, 24)), new Color("#52606D"), true);
        DrawRect(rect, new Color("#8A97A5"), false, 2f);

        for (int i = 0; i < 4; i++)
        {
            float width = rect.Size.X * (0.58f + i * 0.07f);
            DrawRect(
                new Rect2(rect.Position + new Vector2(18, 42 + i * 22), new Vector2(width, 5)),
                new Color(0.25f, 0.31f, 0.36f, 0.17f),
                true
            );
        }
    }

    private void DrawBackground()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), _capturedPage is not null ? new Color("#202A34") : new Color("#D9E3EA"), true);

        if (_capturedPage is not null)
        {
            DrawCapturedPage(_capturedPage);
            DrawToolkitMargins(GetCapturedPageDrawRect(_capturedPage));
            return;
        }

        // A deliberately generic "office desktop" made of fixed window geometry.
        DrawWindow(new Rect2(38, 44, Size.X * 0.53f, Size.Y * 0.46f), new Color("#F9FAFB"));
        DrawWindow(new Rect2(Size.X * 0.48f, 88, Size.X * 0.42f, Size.Y * 0.39f), new Color("#FFFDF8"));
        DrawWindow(new Rect2(96, Size.Y * 0.55f, Size.X * 0.52f, Size.Y * 0.30f), new Color("#EEF5F2"));

        if (_window is not null)
            DrawTextureRect(_window, new Rect2(Size.X - 148, Size.Y - 164, 94, 94), false);
    }

    private void DrawCapturedPage(CapturedPageFrame frame)
    {
        DrawTextureRect(frame.Texture, GetCapturedPageDrawRect(frame), false);
    }

    private void DrawPlayerShots()
    {
        foreach (Vector2 position in _playerShotPositions)
        {
            DrawCircle(position, 3.8f, new Color("#FFF0A8"));
            DrawCircle(position, 1.7f, new Color("#FF5C35"));
        }

        foreach (Vector2 position in _enemyShotPositions)
        {
            DrawCircle(position, 8.5f, new Color("#202A34"));
            DrawCircle(position, 6.2f, new Color("#FF2B2B"));
            DrawProjectileFrame(position, 1, 0.82f);
            DrawCircle(position, 2.2f, new Color("#FFF0A8"));
        }

        foreach (EffectVisual effect in _impactEffects)
            DrawEffectFrame(effect, 1.0f);

        _letterEffects.Draw(this);
    }

    public void ThrowRandomLetters(Vector2 position, int count)
    {
        if (count <= 0)
            return;

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string letters = "";
        int seed = Mathf.Abs(Mathf.RoundToInt(position.X * 17f + position.Y * 31f + ElapsedSeconds * 101f));
        for (int i = 0; i < Mathf.Min(count, 12); i++)
            letters += alphabet[(seed + i * 7) % alphabet.Length];

        _letterEffects.ExplodeWord(letters, position, new Color("#FFF0A8"), Mathf.Clamp(0.85f + count * 0.08f, 0.9f, 1.8f));
    }

    public void ThrowDeathPhrase(Vector2 position, string reason)
    {
        string phrase = string.IsNullOrWhiteSpace(reason) ? "DEATH" : reason;
        _letterEffects.ExplodeWord(phrase, position, new Color("#FF2BD6"), 1.45f);
    }

    public void ThrowComicImpact(Vector2 position, string word, float intensity = 1.35f)
    {
        _letterEffects.ComicImpact(position, word, intensity);
    }

    private void DrawEffectFrame(EffectVisual effect, float scale)
    {
        if (effect.Kind == EffectVisualKind.LegacyEnemyDeath)
        {
            DrawLegacyEnemyDeathFrame(effect.Position, effect.Frame, scale * 2.25f);
            return;
        }

        DrawProjectileFrame(effect.Position, effect.Frame, scale);
    }

    private void DrawLegacyEnemyDeathFrame(Vector2 position, int frame, float scale)
    {
        if (_legacyEnemyDeath is null)
        {
            DrawCircle(position, 18f, new Color("#FFF0A8", 0.7f));
            DrawCircle(position, 9f, new Color("#FF2BD6", 0.92f));
            return;
        }

        const int frameWidth = 48;
        const int frameHeight = 48;
        frame = Mathf.Clamp(frame, 0, 7);
        Rect2 source = new(frame * frameWidth, 0, frameWidth, frameHeight);
        Vector2 size = new Vector2(frameWidth, frameHeight) * scale;
        DrawTextureRectRegion(_legacyEnemyDeath, new Rect2(position - size * 0.5f, size), source);
    }

    private void DrawProjectileFrame(Vector2 position, int frame, float scale)
    {
        if (_fireballExplosion is null)
        {
            DrawCircle(position, 4.4f, new Color("#FF2BD6"));
            DrawCircle(position, 2.0f, new Color("#202A34"));
            return;
        }

        const int frameWidth = 80;
        const int frameHeight = 48;
        frame = Mathf.Clamp(frame, 0, 12);
        Rect2 source = new(frame * frameWidth, 0, frameWidth, frameHeight);
        Vector2 size = new Vector2(frameWidth, frameHeight) * scale;
        DrawTextureRectRegion(_fireballExplosion, new Rect2(position - size * 0.5f, size), source);
    }

    private void DrawWorldObjects()
    {
        foreach (WorldObject ladder in GetLadders())
            DrawLadder(ladder);

        foreach (WorldObject ramp in GetRamps())
        {
            if (ramp.Kind == WorldObjectKind.Slide)
                DrawSlide(ramp);
            else
                DrawRamp(ramp);
        }

        foreach (WorldObject conveyor in GetConveyors())
            DrawConveyor(conveyor);

        foreach (WorldObject elevator in GetElevators())
            DrawElevator(elevator);

        foreach (WorldObject checkpoint in ObjectsOfKind(WorldObjectKind.Checkpoint))
            DrawCheckpoint(checkpoint);

        foreach (WorldObject goal in ObjectsOfKind(WorldObjectKind.GoalPoint))
            DrawCheckpoint(goal);

        foreach (WorldObject start in ObjectsOfKind(WorldObjectKind.StartPoint))
            DrawEditorOnlyObject(start, "START", new Color("#B56CFF"));

        foreach (WorldObject hiddenSwitch in ObjectsOfKind(WorldObjectKind.HiddenSwitch))
            DrawEditorOnlyObject(hiddenSwitch, "SWITCH", new Color("#FF2BD6"));

        foreach (WorldObject spawn in ObjectsOfKind(WorldObjectKind.EnemySpawnPoint))
            DrawEnemySpawnPoint(spawn);

        foreach (WorldObject flipper in ObjectsOfKind(WorldObjectKind.PinballFlipper))
            DrawPinballFlipper(flipper);

        foreach (WorldObject bumper in ObjectsOfKind(WorldObjectKind.PinballBumper))
            DrawPinballBumper(bumper);

        foreach (WorldObject plunger in ObjectsOfKind(WorldObjectKind.PinballPlunger))
            DrawPinballPlunger(plunger);

        foreach (WorldObject drain in ObjectsOfKind(WorldObjectKind.PinballDrain))
            DrawPinballDrain(drain);

        foreach (WorldObject rollover in ObjectsOfKind(WorldObjectKind.PinballRollover))
            DrawPinballRollover(rollover);

        foreach (WorldObject gate in ObjectsOfKind(WorldObjectKind.PinballGate))
            DrawPinballGate(gate);

        foreach (WorldObject coin in ObjectsOfKind(WorldObjectKind.Coin))
            DrawCoin(coin);

        foreach (WorldObject gem in ObjectsOfKind(WorldObjectKind.Gem))
            DrawGem(gem);

        foreach (WorldObject barricade in ObjectsOfKind(WorldObjectKind.Barricade))
            DrawBarricade(barricade);

        DrawSelectedWorldObjectHandles();
    }

    private List<WorldObject> ObjectsOfKind(WorldObjectKind kind)
    {
        List<WorldObject> objects = [];
        foreach (WorldObject worldObject in _placedWorldObjects)
        {
            if (worldObject.IsEditorOnly && !ShowEditorOnlyObjects)
                continue;

            if (worldObject.Kind == kind)
                objects.Add(worldObject);
        }

        return objects;
    }

    private bool TryBeginWorldObjectDrag(Vector2 position)
    {
        for (int i = _placedWorldObjects.Count - 1; i >= 0; i--)
        {
            WorldObject worldObject = _placedWorldObjects[i];
            if (HandleRect(worldObject.Start).HasPoint(position))
            {
                _selectedWorldObjectIndex = i;
                _draggedHandle = 0;
                PublishWorldObjectSelection();
                QueueRedraw();
                return true;
            }

            if (HandleRect(worldObject.End).HasPoint(position))
            {
                _selectedWorldObjectIndex = i;
                _draggedHandle = 1;
                PublishWorldObjectSelection();
                QueueRedraw();
                return true;
            }

            if (WorldObjectBodyHitTest(worldObject, position))
            {
                _selectedWorldObjectIndex = i;
                _draggedHandle = 2;
                _dragBodyOffset = position - worldObject.Center;
                PublishWorldObjectSelection();
                QueueRedraw();
                return true;
            }
        }

            _selectedWorldObjectIndex = -1;
            _draggedHandle = -1;
            _dragBodyOffset = Vector2.Zero;
            PublishWorldObjectSelection();
        QueueRedraw();
        return false;
    }

    private bool TrySelectWorldObject(Vector2 position)
    {
        for (int i = _placedWorldObjects.Count - 1; i >= 0; i--)
        {
            WorldObject worldObject = _placedWorldObjects[i];
            if (!WorldObjectBodyHitTest(worldObject, position)
                && !HandleRect(worldObject.Start).HasPoint(position)
                && !HandleRect(worldObject.End).HasPoint(position))
                continue;

            _selectedWorldObjectIndex = i;
            _draggedHandle = -1;
            _dragBodyOffset = Vector2.Zero;
            PublishWorldObjectSelection();
            QueueRedraw();
            return true;
        }

        return false;
    }

    private bool WorldObjectBodyHitTest(WorldObject worldObject, Vector2 position)
    {
        if (worldObject.IsMarker || worldObject.Kind == WorldObjectKind.PinballBumper)
            return worldObject.Bounds(TextUnitPixels, ElapsedSeconds).Grow(5f).HasPoint(position);

        if (IsLineLikeWorldObject(worldObject.Kind))
            return DistanceToWorldObjectSegment(worldObject, position) <= WorldObjectPickRadius(worldObject);

        return worldObject.Bounds(TextUnitPixels, ElapsedSeconds).Grow(5f).HasPoint(position);
    }

    private static bool IsLineLikeWorldObject(WorldObjectKind kind)
    {
        return kind is WorldObjectKind.Ladder
            or WorldObjectKind.Ramp
            or WorldObjectKind.Slide
            or WorldObjectKind.Conveyor
            or WorldObjectKind.Elevator
            or WorldObjectKind.PinballFlipper
            or WorldObjectKind.PinballPlunger
            or WorldObjectKind.PinballDrain
            or WorldObjectKind.PinballRollover
            or WorldObjectKind.PinballGate
            or WorldObjectKind.Barricade;
    }

    private float DistanceToWorldObjectSegment(WorldObject worldObject, Vector2 position)
    {
        Vector2 start = worldObject.ResolvePoint(worldObject.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = worldObject.ResolvePoint(worldObject.End, TextUnitPixels, ElapsedSeconds);
        Vector2 segment = end - start;
        if (segment.LengthSquared() <= 0.001f)
            return position.DistanceTo(start);

        float t = Mathf.Clamp((position - start).Dot(segment) / segment.LengthSquared(), 0f, 1f);
        return position.DistanceTo(start + segment * t);
    }

    private float WorldObjectPickRadius(WorldObject worldObject)
    {
        float thickness = worldObject.Kind == WorldObjectKind.Ladder
            ? Mathf.Clamp(worldObject.ThicknessUnits, 0.6f, 2.5f)
            : worldObject.ThicknessUnits;

        return Mathf.Clamp(TextUnitPixels * thickness * 0.55f, 6f, TextUnitPixels * 1.35f);
    }

    private void DragSelectedHandle(Vector2 position)
    {
        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
            return;

        WorldObject selected = _placedWorldObjects[_selectedWorldObjectIndex];
        Vector2 snapped = SnapToPlayBounds(position);
        if (selected.Kind == WorldObjectKind.Ladder)
        {
            float x = selected.Center.X;
            selected = _draggedHandle == 0
                ? selected with { Start = new Vector2(x, snapped.Y) }
                : selected with { End = new Vector2(x, snapped.Y) };
        }
        else
        {
            selected = _draggedHandle == 0
                ? selected with { Start = snapped }
                : selected with { End = snapped };
        }

        _placedWorldObjects[_selectedWorldObjectIndex] = selected;
        WorldObjectChanged?.Invoke($"{selected.Kind} geometry changed");
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    private void DragSelectedBody(Vector2 position)
    {
        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
            return;

        WorldObject selected = _placedWorldObjects[_selectedWorldObjectIndex];
        Vector2 desiredCenter = SnapBodyCenterToPlayBounds(selected, position - _dragBodyOffset);
        Vector2 delta = desiredCenter - selected.Center;
        _placedWorldObjects[_selectedWorldObjectIndex] = selected.Translated(delta);
        WorldObjectChanged?.Invoke($"{selected.Kind} moved");
        PublishWorldObjectSelection();
        QueueRedraw();
    }

    private Vector2 SnapBodyCenterToPlayBounds(WorldObject selected, Vector2 desiredCenter)
    {
        Rect2 bounds = PlayBounds;
        Vector2 halfSpan = new(
            Mathf.Abs(selected.End.X - selected.Start.X) * 0.5f,
            Mathf.Abs(selected.End.Y - selected.Start.Y) * 0.5f
        );

        return new Vector2(
            Mathf.Clamp(desiredCenter.X, bounds.Position.X + halfSpan.X, bounds.End.X - halfSpan.X),
            Mathf.Clamp(desiredCenter.Y, bounds.Position.Y + halfSpan.Y, bounds.End.Y - halfSpan.Y)
        );
    }

    private Vector2 SnapToPlayBounds(Vector2 position)
    {
        Rect2 bounds = PlayBounds;
        return new Vector2(
            Mathf.Clamp(position.X, bounds.Position.X, bounds.End.X),
            Mathf.Clamp(position.Y, bounds.Position.Y, bounds.End.Y)
        );
    }

    private Rect2 HandleRect(Vector2 point)
    {
        float radius = Mathf.Max(7f, TextUnitPixels * 0.9f);
        return new Rect2(point - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));
    }

    private void PublishWorldObjectSelection()
    {
        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
        {
            WorldObjectSelectionChanged?.Invoke("No placed toolkit object selected.\n\nClick/drag a placed ladder/ramp/elevator/etc. body to move it, or drag its endpoint handles to scale/angle it.");
            WorldObjectSelectionObjectChanged?.Invoke(null);
            return;
        }

        WorldObject selected = _placedWorldObjects[_selectedWorldObjectIndex];
        float length = selected.Start.DistanceTo(selected.End);
        string hidden = selected.IsEditorOnly
            ? "\nEditor-only: visible while building, hidden during play.\n"
            : "";
        WorldObjectSelectionChanged?.Invoke(
            $"{selected.Kind} selected\n\n"
            + $"Start: {Mathf.RoundToInt(selected.Start.X)}, {Mathf.RoundToInt(selected.Start.Y)}\n"
            + $"End: {Mathf.RoundToInt(selected.End.X)}, {Mathf.RoundToInt(selected.End.Y)}\n"
            + $"Length: {Mathf.RoundToInt(length)} px\n\n"
            + $"Speed: {selected.SpeedUnits:0.0} units\n"
            + $"Thickness: {selected.ThicknessUnits:0.0} units\n"
            + $"Range: {selected.RangeUnits:0.0} units\n"
            + $"Opacity: {selected.Opacity * 100f:0}%\n"
            + hidden
            + "Drag the object body to move it. Drag either square handle to scale/angle it. Collision updates immediately."
        );
        WorldObjectSelectionObjectChanged?.Invoke(selected);
    }

    private Rect2[] GetCapturedTextPlatforms(CapturedPageFrame frame)
    {
        Rect2 imageRect = GetCapturedPageDrawRect(frame);
        Vector2 sourceSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = imageRect.Size.X / sourceSize.X;
        Rect2[] mapped = new Rect2[frame.TextPlatforms.Length];

        for (int i = 0; i < frame.TextPlatforms.Length; i++)
        {
            Rect2 source = frame.TextPlatforms[i];
            mapped[i] = new Rect2(
                imageRect.Position + source.Position * scale,
                new Vector2(source.Size.X * scale, Mathf.Max(2f, TextUnitPixels * 0.7f))
            );
        }

        return mapped;
    }

    private Rect2[] GetCapturedTextBricks(CapturedPageFrame frame)
    {
        return GetCapturedRects(frame, frame.TextBricks);
    }

    private Rect2[] GetCapturedRects(CapturedPageFrame frame, Rect2[] sourceRects, bool activeOnly = false)
    {
        Rect2 imageRect = GetCapturedPageDrawRect(frame);
        Vector2 sourceSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = imageRect.Size.X / sourceSize.X;
        List<Rect2> mapped = [];

        for (int i = 0; i < sourceRects.Length; i++)
        {
            Rect2 source = sourceRects[i];
            if (activeOnly && !HasCurrentInkInRegion(frame, source.Grow(2f)))
                continue;

            mapped.Add(new Rect2(
                imageRect.Position + source.Position * scale,
                source.Size * scale
            ));
        }

        return mapped.ToArray();
    }

    private Rect2 GetCapturedPageDrawRect(CapturedPageFrame frame)
    {
        return new Rect2(Vector2.Zero, new Vector2(frame.PixelSize.X, frame.PixelSize.Y));
    }

    private Rect2 DisplayToSourceRect(CapturedPageFrame frame, Rect2 displayRegion)
    {
        Rect2 imageRect = GetCapturedPageDrawRect(frame);
        Vector2 sourceSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = imageRect.Size.X / sourceSize.X;
        return new Rect2(
            (displayRegion.Position - imageRect.Position) / scale,
            displayRegion.Size / scale
        );
    }

    private static Color SampleBackgroundColor(Image image, Rect2 sourceRegion)
    {
        float grow = Mathf.Max(8f, Mathf.Max(sourceRegion.Size.X, sourceRegion.Size.Y) * 0.75f);
        Rect2 expanded = sourceRegion.Grow(grow);
        int minX = Mathf.Clamp(Mathf.FloorToInt(expanded.Position.X), 0, image.GetWidth() - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(expanded.Position.Y), 0, image.GetHeight() - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(expanded.End.X), 0, image.GetWidth() - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(expanded.End.Y), 0, image.GetHeight() - 1);

        Dictionary<int, ColorBucket> buckets = [];

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (IsLikelyTextPixel(pixel))
                    continue;

                int key = QuantizeColor(pixel);
                buckets.TryGetValue(key, out ColorBucket bucket);
                bucket.Add(pixel);
                buckets[key] = bucket;
            }
        }

        if (buckets.Count == 0)
            return new Color("#FBFBF8");

        ColorBucket best = default;
        foreach (ColorBucket bucket in buckets.Values)
        {
            if (bucket.Count > best.Count)
                best = bucket;
        }

        return best.Average();
    }

    private static bool IsLikelyTextPixel(Color pixel)
    {
        if (pixel.A < 0.5f)
            return false;

        float luminance = pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
        float chroma = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
        return luminance < 0.50f && (luminance < 0.42f || chroma < 0.22f);
    }

    private static bool IsCurrentInkPixel(CapturedPageFrame frame, int x, int y)
    {
        Color current = frame.Image.GetPixel(x, y);
        if (current.A < 0.5f)
            return false;

        Color originalBackground = EstimateLocalBackgroundColor(frame.OriginalImage, x, y);
        if (ColorDistance(current, originalBackground) < 0.035f)
            return false;

        if (IsLikelyTextPixel(current))
            return true;

        float currentLuminance = Luminance(current);
        float backgroundLuminance = Luminance(originalBackground);
        float chroma = Math.Max(current.R, Math.Max(current.G, current.B)) - Math.Min(current.R, Math.Min(current.G, current.B));
        float contrast = backgroundLuminance - currentLuminance;
        return contrast > 0.075f && currentLuminance < 0.78f && chroma < 0.26f;
    }

    private static Color EstimateLocalBackgroundColor(Image image, int x, int y)
    {
        Dictionary<int, ColorBucket> buckets = [];
        for (int dy = -8; dy <= 8; dy += 4)
        {
            for (int dx = -8; dx <= 8; dx += 4)
            {
                if (Math.Abs(dx) <= 2 && Math.Abs(dy) <= 2)
                    continue;

                int sx = Mathf.Clamp(x + dx, 0, image.GetWidth() - 1);
                int sy = Mathf.Clamp(y + dy, 0, image.GetHeight() - 1);
                Color sample = image.GetPixel(sx, sy);
                if (IsLikelyTextPixel(sample))
                    continue;

                int key = QuantizeColor(sample);
                buckets.TryGetValue(key, out ColorBucket bucket);
                bucket.Add(sample);
                buckets[key] = bucket;
            }
        }

        if (buckets.Count == 0)
            return new Color("#FBFBF8");

        ColorBucket best = default;
        foreach (ColorBucket bucket in buckets.Values)
        {
            if (bucket.Count > best.Count)
                best = bucket;
        }

        return best.Average();
    }

    private static void FloodEraseConnectedInk(CapturedPageFrame frame, Rect2 sourceRegion, Color fill)
    {
        Rect2 seedRegion = sourceRegion.Grow(2.5f);
        Rect2 floodRegion = sourceRegion.Grow(8f);
        Rect2I floodBounds = ClampToImage(frame.Image, floodRegion);
        Rect2I seedBounds = ClampToImage(frame.Image, seedRegion);
        Queue<Vector2I> pending = [];
        HashSet<int> visited = [];

        for (int y = seedBounds.Position.Y; y < seedBounds.End.Y; y++)
        {
            for (int x = seedBounds.Position.X; x < seedBounds.End.X; x++)
            {
                Vector2I point = new(x, y);
                if (IsEraseCandidate(frame, point, fill))
                    Enqueue(point);
            }
        }

        while (pending.Count > 0)
        {
            Vector2I point = pending.Dequeue();
            frame.Image.SetPixel(point.X, point.Y, fill);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2I next = point + new Vector2I(dx, dy);
                    if (!floodBounds.HasPoint(next) || !IsEraseCandidate(frame, next, fill))
                        continue;

                    Enqueue(next);
                }
            }
        }

        void Enqueue(Vector2I point)
        {
            int key = point.Y * frame.Image.GetWidth() + point.X;
            if (!visited.Add(key))
                return;

            pending.Enqueue(point);
        }
    }

    private static Rect2I ClampToImage(Image image, Rect2 region)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(region.Position.X), 0, image.GetWidth() - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(region.Position.Y), 0, image.GetHeight() - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(region.End.X) + 1, minX + 1, image.GetWidth());
        int maxY = Mathf.Clamp(Mathf.CeilToInt(region.End.Y) + 1, minY + 1, image.GetHeight());
        return new Rect2I(minX, minY, maxX - minX, maxY - minY);
    }

    private static bool IsEraseCandidate(CapturedPageFrame frame, Vector2I point, Color fill)
    {
        Color original = frame.OriginalImage.GetPixel(point.X, point.Y);
        Color current = frame.Image.GetPixel(point.X, point.Y);

        if (ColorDistance(current, fill) < 0.03f)
            return false;

        return IsCurrentInkPixel(frame, point.X, point.Y)
            || IsLikelyTextPixel(original)
            || IsLikelyInkEdge(original, fill)
            || IsLikelyTextPixel(current)
            || IsLikelyInkEdge(current, fill);
    }

    private static bool HasCurrentInkInRegion(CapturedPageFrame frame, Rect2 sourceRegion)
    {
        Rect2I bounds = ClampToImage(frame.Image, sourceRegion);
        int inkPixels = 0;
        int requiredPixels = Mathf.Clamp((bounds.Size.X * bounds.Size.Y) / 220, 2, 10);
        for (int y = bounds.Position.Y; y < bounds.End.Y; y++)
        {
            for (int x = bounds.Position.X; x < bounds.End.X; x++)
            {
                Color current = frame.Image.GetPixel(x, y);
                Color original = frame.OriginalImage.GetPixel(x, y);
                if (!IsFastCurrentInkPixel(current, original))
                    continue;

                inkPixels++;
                if (inkPixels >= requiredPixels)
                    return true;
            }
        }

        return false;
    }

    private float ScoreWhitespace(Rect2 displayRegion)
    {
        if (_capturedPage is null || !PlayBounds.Intersects(displayRegion, true))
            return 10_000f;

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, displayRegion);
        Rect2I bounds = ClampToImage(_capturedPage.Image, sourceRegion);
        int ink = 0;
        int samples = 0;
        int step = 4;

        for (int y = bounds.Position.Y; y < bounds.End.Y; y += step)
        {
            for (int x = bounds.Position.X; x < bounds.End.X; x += step)
            {
                samples++;
                Color current = _capturedPage.Image.GetPixel(x, y);
                Color original = _capturedPage.OriginalImage.GetPixel(x, y);
                if (IsFastCurrentInkPixel(current, original))
                    ink++;
            }
        }

        float lowerRightBias = displayRegion.GetCenter().X * 0.001f + displayRegion.GetCenter().Y * 0.001f;
        return samples == 0 ? lowerRightBias : -((float)ink / samples) + lowerRightBias;
    }

    private static bool IsFastCurrentInkPixel(Color current, Color original)
    {
        if (current.A < 0.5f)
            return false;

        float currentLuminance = Luminance(current);
        if (currentLuminance < 0.42f)
            return true;

        float originalLuminance = Luminance(original);
        float chroma = Math.Max(current.R, Math.Max(current.G, current.B)) - Math.Min(current.R, Math.Min(current.G, current.B));
        return originalLuminance - currentLuminance > 0.075f && currentLuminance < 0.78f && chroma < 0.26f;
    }

    private static void CleanupTinyInkSpecks(CapturedPageFrame frame, Rect2 sourceRegion, Color fill)
    {
        Rect2I bounds = ClampToImage(frame.Image, sourceRegion);
        HashSet<int> visited = [];
        List<Vector2I> component = [];
        Queue<Vector2I> pending = [];
        const int maxSpeckPixels = 5;

        for (int y = bounds.Position.Y; y < bounds.End.Y; y++)
        {
            for (int x = bounds.Position.X; x < bounds.End.X; x++)
            {
                Vector2I start = new(x, y);
                int startKey = Key(start);
                if (visited.Contains(startKey) || !IsRemainingInkSpeckCandidate(frame, start, fill))
                    continue;

                component.Clear();
                pending.Clear();
                visited.Add(startKey);
                pending.Enqueue(start);

                while (pending.Count > 0)
                {
                    Vector2I point = pending.Dequeue();
                    component.Add(point);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            Vector2I next = point + new Vector2I(dx, dy);
                            int nextKey = Key(next);
                            if (!bounds.HasPoint(next) || visited.Contains(nextKey) || !IsRemainingInkSpeckCandidate(frame, next, fill))
                                continue;

                            visited.Add(nextKey);
                            pending.Enqueue(next);
                        }
                    }
                }

                if (component.Count > maxSpeckPixels)
                    continue;

                foreach (Vector2I point in component)
                    frame.Image.SetPixel(point.X, point.Y, fill);
            }
        }

        int Key(Vector2I point) => point.Y * frame.Image.GetWidth() + point.X;
    }

    private static bool IsRemainingInkSpeckCandidate(CapturedPageFrame frame, Vector2I point, Color fill)
    {
        Color current = frame.Image.GetPixel(point.X, point.Y);
        if (ColorDistance(current, fill) < 0.03f)
            return false;

        return IsCurrentInkPixel(frame, point.X, point.Y) || IsLikelyInkEdge(current, fill);
    }

    private static float Luminance(Color pixel)
    {
        return pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
    }

    private static bool IsLikelyInkEdge(Color pixel, Color background)
    {
        float luminance = pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
        float backgroundLuminance = background.R * 0.2126f + background.G * 0.7152f + background.B * 0.0722f;
        return luminance < backgroundLuminance - 0.08f;
    }

    private static float ColorDistance(Color a, Color b)
    {
        float dr = a.R - b.R;
        float dg = a.G - b.G;
        float db = a.B - b.B;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }

    private static int QuantizeColor(Color pixel)
    {
        int r = Mathf.Clamp(Mathf.RoundToInt(pixel.R * 15f), 0, 15);
        int g = Mathf.Clamp(Mathf.RoundToInt(pixel.G * 15f), 0, 15);
        int b = Mathf.Clamp(Mathf.RoundToInt(pixel.B * 15f), 0, 15);
        return (r << 8) | (g << 4) | b;
    }

    private struct ColorBucket
    {
        private double _r;
        private double _g;
        private double _b;
        private double _a;

        public int Count { get; private set; }

        public void Add(Color color)
        {
            _r += color.R;
            _g += color.G;
            _b += color.B;
            _a += color.A;
            Count++;
        }

        public readonly Color Average()
        {
            if (Count == 0)
                return new Color("#FBFBF8");

            return new Color(
                (float)(_r / Count),
                (float)(_g / Count),
                (float)(_b / Count),
                Math.Max(1f, (float)(_a / Count))
            );
        }
    }

    private void DrawToolkitMargins(Rect2 documentRect)
    {
        Rect2 right = new(documentRect.End.X, 0, Mathf.Max(0, Size.X - documentRect.End.X), Size.Y);
        Rect2 bottom = new(0, documentRect.End.Y, Mathf.Min(Size.X, documentRect.Size.X), Mathf.Max(0, Size.Y - documentRect.End.Y));

        if (right.Size.X > 1f)
            DrawToolkitPanel(right, "TOOLKIT MARGIN");

        if (bottom.Size.Y > 1f)
            DrawToolkitPanel(bottom, "STATUS / POWER-UPS");
    }

    private void DrawToolkitPanel(Rect2 rect, string label)
    {
        DrawRect(rect, new Color("#202A34"), true);
        DrawRect(rect, new Color("#52606D"), false, 1f);

        if (rect.Size.X < 90f || rect.Size.Y < 28f)
            return;

        DrawString(
            ThemeDB.FallbackFont,
            rect.Position + new Vector2(16, 28),
            label,
            HorizontalAlignment.Left,
            rect.Size.X - 24,
            13,
            new Color("#D9E3EA", 0.72f)
        );
    }

    private void DrawPlatform(Rect2 rect)
    {
        DrawRect(rect with { Position = rect.Position + new Vector2(4, 6) }, new Color(0, 0, 0, 0.16f), true);

        if (_platform is not null)
        {
            for (float x = rect.Position.X; x < rect.End.X; x += TextUnitPixels * 2f)
            {
                float width = Mathf.Min(TextUnitPixels * 2f, rect.End.X - x);
                DrawTextureRect(_platform, new Rect2(x, rect.Position.Y, width, TextUnitPixels * 2f), false);
            }
        }
        else
        {
            DrawRect(rect, new Color("#435463"), true);
        }
    }

    private void DrawLadder(WorldObject ladder)
    {
        Rect2 rect = LadderRect(ladder);
        if (rect.Size.Y <= 0.001f)
            return;

        Color rail = ladder.Styled(new Color("#8A5A37"));
        float railWidth = Mathf.Clamp(TextUnitPixels * 0.18f, 2f, 4f);
        float leftX = rect.Position.X;
        float rightX = rect.End.X;

        DrawLine(new Vector2(leftX, rect.Position.Y), new Vector2(leftX, rect.End.Y), rail, railWidth);
        DrawLine(new Vector2(rightX, rect.Position.Y), new Vector2(rightX, rect.End.Y), rail, railWidth);

        float rungSpacing = Mathf.Max(TextUnitPixels, 8f);
        for (float y = rect.Position.Y + rungSpacing; y < rect.End.Y; y += rungSpacing)
        {
            DrawLine(new Vector2(leftX, y), new Vector2(rightX, y), rail, railWidth);
        }
    }

    private Rect2 LadderRect(WorldObject ladder)
    {
        Vector2 start = ladder.ResolvePoint(ladder.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = ladder.ResolvePoint(ladder.End, TextUnitPixels, ElapsedSeconds);
        float centerX = ladder.Center.X;
        float halfWidth = LadderHalfWidthPixels(ladder);
        float top = Mathf.Min(start.Y, end.Y);
        float bottom = Mathf.Max(start.Y, end.Y);
        return new Rect2(centerX - halfWidth, top, halfWidth * 2f, bottom - top);
    }

    private float LadderHalfWidthPixels(WorldObject ladder)
    {
        float cappedThickness = Mathf.Clamp(ladder.ThicknessUnits, 0.6f, 2.5f);
        return Mathf.Clamp(TextUnitPixels * cappedThickness * 0.5f, TextUnitPixels * 0.55f, TextUnitPixels * 1.35f);
    }

    private void DrawRamp(WorldObject ramp)
    {
        Vector2 start = ramp.ResolvePoint(ramp.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = ramp.ResolvePoint(ramp.End, TextUnitPixels, ElapsedSeconds);
        Color body = ramp.Styled(new Color("#5CB8A7"));
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.45f);
        DrawLine(start, end, body, TextUnitPixels * 0.42f);
        DrawLine(start, end, new Color("#F7F5EF"), 2f);
    }

    private void DrawSlide(WorldObject slide)
    {
        Vector2 start = slide.ResolvePoint(slide.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = slide.ResolvePoint(slide.End, TextUnitPixels, ElapsedSeconds);
        Color body = slide.Styled(new Color("#FF5C35"));
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.5f);
        DrawLine(start, end, body, TextUnitPixels * 0.45f);
        DrawLine(start, end, new Color("#FFF0A8"), 2f);
    }

    private void DrawConveyor(WorldObject conveyor)
    {
        Vector2 start = conveyor.ResolvePoint(conveyor.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = conveyor.ResolvePoint(conveyor.End, TextUnitPixels, ElapsedSeconds);
        Color body = conveyor.Styled(new Color("#4378B8"));
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.55f);
        DrawLine(start, end, body, TextUnitPixels * 0.5f);

        Vector2 direction = conveyor.Direction(TextUnitPixels, ElapsedSeconds);
        float length = start.DistanceTo(end);
        for (float offset = TextUnitPixels; offset < length; offset += TextUnitPixels * 2f)
        {
            Vector2 center = start + direction * offset;
            DrawLine(center - direction * TextUnitPixels * 0.35f, center + direction * TextUnitPixels * 0.35f, new Color("#F7F5EF"), 2f);
        }
    }

    private void DrawElevator(WorldObject elevator)
    {
        Vector2 start = elevator.ResolvePoint(elevator.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = elevator.ResolvePoint(elevator.End, TextUnitPixels, ElapsedSeconds);
        Vector2 range = elevator.ElevatorRangeVector(TextUnitPixels);
        Vector2 baseCenter = elevator.Center;
        Color body = elevator.Styled(new Color("#F4C95D"));

        DrawLine(baseCenter - range + new Vector2(0, 2), baseCenter + range + new Vector2(0, 2), new Color(0, 0, 0, 0.18f), 4f);
        DrawLine(baseCenter - range, baseCenter + range, new Color("#B56CFF", 0.62f), 2f);
        DrawCircle(baseCenter - range, 4f, body);
        DrawCircle(baseCenter + range, 4f, body);

        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.65f);
        DrawLine(start, end, body, TextUnitPixels * 0.6f);
        DrawLine(start, end, new Color("#202A34"), 2f);
    }

    private void DrawCheckpoint(WorldObject checkpoint)
    {
        Vector2 basePoint = checkpoint.ResolvePoint(checkpoint.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 topPoint = checkpoint.ResolvePoint(checkpoint.End, TextUnitPixels, ElapsedSeconds);
        Color defaultFlag = checkpoint.MarkerRole == MarkerRole.End ? new Color("#F4C95D") : new Color("#5CB8A7");
        Color flag = checkpoint.Styled(defaultFlag);
        DrawLine(basePoint + new Vector2(2, 3), topPoint + new Vector2(2, 3), new Color(0, 0, 0, 0.22f), 3f);
        DrawLine(basePoint, topPoint, new Color("#202A34"), 3f);
        Vector2 flagA = topPoint;
        Vector2 flagB = topPoint + new Vector2(TextUnitPixels * 4.5f, TextUnitPixels * 1.4f);
        Vector2 flagC = topPoint + new Vector2(0, TextUnitPixels * 2.8f);
        DrawColoredPolygon([flagA, flagB, flagC], flag);
        DrawPolyline([flagA, flagB, flagC, flagA], new Color("#F7F5EF"), 1.5f);
        if (checkpoint.MarkerRole == MarkerRole.End)
            DrawString(ThemeDB.FallbackFont, topPoint + new Vector2(TextUnitPixels * 1.0f, TextUnitPixels * 2.3f), "GOAL", HorizontalAlignment.Left, TextUnitPixels * 8f, 12, new Color("#202A34"));
    }

    private void DrawPinballFlipper(WorldObject flipper)
    {
        Vector2 start = flipper.ResolvePoint(flipper.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = flipper.ResolvePoint(flipper.End, TextUnitPixels, ElapsedSeconds);
        Color body = flipper.Styled(new Color("#FF5C35"));
        DrawCircle(start + new Vector2(2, 3), TextUnitPixels * 1.45f, new Color(0, 0, 0, 0.24f));
        DrawLine(start + new Vector2(3, 4), end + new Vector2(3, 4), new Color(0, 0, 0, 0.20f), TextUnitPixels * 1.25f);
        DrawLine(start, end, body, TextUnitPixels * 1.12f);
        DrawLine(start, end, new Color("#FFF0A8"), 2f);
        DrawCircle(start, TextUnitPixels * 1.15f, new Color("#202A34"));
        DrawCircle(start, TextUnitPixels * 1.15f, new Color("#F7F5EF"), false, 1.5f);
    }

    private void DrawPinballBumper(WorldObject bumper)
    {
        Vector2 center = bumper.ResolvePoint(bumper.Start, TextUnitPixels, ElapsedSeconds);
        float radius = Mathf.Max(TextUnitPixels * 3f, center.DistanceTo(bumper.End));
        Color body = bumper.Styled(new Color("#5CB8A7"));
        DrawCircle(center + new Vector2(3, 4), radius + 3f, new Color(0, 0, 0, 0.22f));
        DrawCircle(center, radius, body);
        DrawCircle(center, radius * 0.63f, new Color("#FFF0A8", body.A * 0.82f));
        DrawCircle(center, radius, new Color("#F7F5EF"), false, 2f);
        DrawString(ThemeDB.FallbackFont, center + new Vector2(-radius * 0.55f, 4f), "POP", HorizontalAlignment.Center, radius * 1.1f, 12, new Color("#202A34"));
    }

    private void DrawPinballPlunger(WorldObject plunger)
    {
        Vector2 start = plunger.ResolvePoint(plunger.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = plunger.ResolvePoint(plunger.End, TextUnitPixels, ElapsedSeconds);
        Color body = plunger.Styled(new Color("#B56CFF"));
        DrawLine(start + new Vector2(3, 4), end + new Vector2(3, 4), new Color(0, 0, 0, 0.22f), TextUnitPixels * 1.15f);
        DrawLine(start, end, new Color("#52606D", body.A * 0.75f), TextUnitPixels * 0.75f);
        DrawLine(start, end, body, 3f);
        DrawCircle(start, TextUnitPixels * 1.7f, new Color("#FFF0A8", body.A));
        DrawCircle(end, TextUnitPixels * 0.85f, new Color("#F7F5EF", body.A));
    }

    private void DrawPinballDrain(WorldObject drain)
    {
        Vector2 start = drain.ResolvePoint(drain.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = drain.ResolvePoint(drain.End, TextUnitPixels, ElapsedSeconds);
        Color body = drain.Styled(new Color("#202A34"));
        DrawLine(start + new Vector2(2, 4), end + new Vector2(2, 4), new Color(0, 0, 0, 0.28f), TextUnitPixels * 1.55f);
        DrawLine(start, end, body, TextUnitPixels * 1.35f);
        DrawLine(start, end, new Color("#FF2BD6", body.A), 2.5f);
        DrawString(ThemeDB.FallbackFont, (start + end) * 0.5f + new Vector2(-34f, -8f), "DRAIN", HorizontalAlignment.Center, 68f, 12, new Color("#FFF0A8", body.A));
    }

    private void DrawPinballRollover(WorldObject rollover)
    {
        Vector2 start = rollover.ResolvePoint(rollover.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = rollover.ResolvePoint(rollover.End, TextUnitPixels, ElapsedSeconds);
        Color body = rollover.Styled(new Color("#F4C95D"));
        DrawLine(start + new Vector2(2, 3), end + new Vector2(2, 3), new Color(0, 0, 0, 0.20f), TextUnitPixels * 0.9f);
        DrawLine(start, end, body, TextUnitPixels * 0.72f);
        DrawLine(start, end, new Color("#F7F5EF", body.A), 1.5f);
    }

    private void DrawPinballGate(WorldObject gate)
    {
        Vector2 start = gate.ResolvePoint(gate.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = gate.ResolvePoint(gate.End, TextUnitPixels, ElapsedSeconds);
        Vector2 direction = (end - start).LengthSquared() < 0.01f ? Vector2.Right : (end - start).Normalized();
        Vector2 normal = new(-direction.Y, direction.X);
        Color body = gate.Styled(new Color("#5CB8FF"));
        DrawLine(start + new Vector2(2, 3), end + new Vector2(2, 3), new Color(0, 0, 0, 0.20f), TextUnitPixels * 0.6f);
        DrawLine(start, end, body, TextUnitPixels * 0.45f);
        DrawLine(end - direction * TextUnitPixels * 1.6f + normal * TextUnitPixels * 0.85f, end, new Color("#FFF0A8", body.A), 2f);
        DrawLine(end - direction * TextUnitPixels * 1.6f - normal * TextUnitPixels * 0.85f, end, new Color("#FFF0A8", body.A), 2f);
    }

    private void DrawCoin(WorldObject coin)
    {
        Rect2 bounds = coin.Bounds(TextUnitPixels, ElapsedSeconds);
        Vector2 center = bounds.GetCenter();
        float radius = Mathf.Max(TextUnitPixels * 1.35f, Mathf.Min(bounds.Size.X, bounds.Size.Y) * 0.33f);
        Color body = coin.Styled(new Color("#F4C95D"));
        DrawCircle(center + new Vector2(2, 3), radius + 2f, new Color(0, 0, 0, 0.24f));
        DrawCircle(center, radius, body);
        DrawCircle(center, radius * 0.64f, new Color("#FFF0A8", body.A * 0.92f));
        DrawCircle(center, radius, new Color("#202A34", body.A * 0.70f), false, 1.4f);
        DrawString(ThemeDB.FallbackFont, center + new Vector2(-radius * 0.42f, radius * 0.38f), "$", HorizontalAlignment.Center, radius * 0.84f, 13, new Color("#202A34", body.A));
    }

    private void DrawGem(WorldObject gem)
    {
        Rect2 bounds = gem.Bounds(TextUnitPixels, ElapsedSeconds);
        Vector2 center = bounds.GetCenter();
        float radius = Mathf.Max(TextUnitPixels * 1.55f, Mathf.Min(bounds.Size.X, bounds.Size.Y) * 0.35f);
        Color body = gem.Styled(new Color("#B56CFF"));
        Vector2[] points =
        [
            center + new Vector2(0, -radius),
            center + new Vector2(radius * 0.92f, -radius * 0.18f),
            center + new Vector2(radius * 0.50f, radius * 0.92f),
            center + new Vector2(-radius * 0.50f, radius * 0.92f),
            center + new Vector2(-radius * 0.92f, -radius * 0.18f)
        ];
        Vector2 shadowOffset = new(2, 3);
        Vector2[] shadowPoints =
        [
            points[0] + shadowOffset,
            points[1] + shadowOffset,
            points[2] + shadowOffset,
            points[3] + shadowOffset,
            points[4] + shadowOffset
        ];
        DrawColoredPolygon(shadowPoints, new Color(0, 0, 0, 0.24f));
        DrawColoredPolygon(points, body);
        DrawPolyline([points[0], points[1], points[2], points[3], points[4], points[0]], new Color("#F7F5EF", body.A), 1.5f);
        DrawLine(points[0], points[2], new Color("#FFF0A8", body.A * 0.72f), 1.2f);
        DrawLine(points[0], points[3], new Color("#FFF0A8", body.A * 0.72f), 1.2f);
    }

    private void DrawBarricade(WorldObject barricade)
    {
        Vector2 start = barricade.ResolvePoint(barricade.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = barricade.ResolvePoint(barricade.End, TextUnitPixels, ElapsedSeconds);
        Color body = barricade.Styled(new Color("#8A5A37"));
        float thickness = Mathf.Max(TextUnitPixels * barricade.ThicknessUnits, 10f);
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.28f), thickness * 1.15f);
        DrawLine(start, end, new Color("#202A34", body.A * 0.98f), thickness * 1.05f);
        DrawLine(start, end, body, thickness * 0.82f);
        DrawLine(start, end, new Color("#FFF0A8", body.A * 0.78f), 2f);
        Vector2 direction = (end - start).LengthSquared() < 0.01f ? Vector2.Right : (end - start).Normalized();
        Vector2 normal = new(-direction.Y, direction.X);
        float length = start.DistanceTo(end);
        int slats = Mathf.Clamp(Mathf.RoundToInt(length / Mathf.Max(14f, TextUnitPixels * 2.4f)), 1, 12);
        for (int i = 0; i <= slats; i++)
        {
            float t = slats == 0 ? 0f : i / (float)slats;
            Vector2 point = start.Lerp(end, t);
            DrawLine(point - normal * thickness * 0.46f, point + normal * thickness * 0.46f, new Color("#202A34", body.A * 0.75f), 1.6f);
        }
    }

    private void DrawEditorOnlyObject(WorldObject worldObject, string label, Color color)
    {
        if (!ShowEditorOnlyObjects)
            return;

        Vector2 start = worldObject.ResolvePoint(worldObject.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = worldObject.ResolvePoint(worldObject.End, TextUnitPixels, ElapsedSeconds);
        Vector2 center = (start + end) * 0.5f;
        float radius = Mathf.Max(TextUnitPixels * 1.7f, 12f);
        Color styled = worldObject.Styled(color);
        DrawLine(start, end, WithAlpha(styled, styled.A * 0.62f), Mathf.Max(2f, TextUnitPixels * 0.28f));
        DrawCircle(center + new Vector2(2, 3), radius + 3f, new Color(0, 0, 0, 0.30f));
        DrawCircle(center, radius, WithAlpha(styled, styled.A * 0.62f));
        DrawCircle(center, radius, new Color("#F7F5EF"), false, 2f);
        DrawString(ThemeDB.FallbackFont, center + new Vector2(radius + 5f, 4f), label, HorizontalAlignment.Left, 90f, 12, new Color("#FFF0A8"));
    }

    private void DrawEnemySpawnPoint(WorldObject spawn)
    {
        if (!ShowEditorOnlyObjects)
            return;

        Vector2 basePoint = spawn.ResolvePoint(spawn.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 topPoint = spawn.ResolvePoint(spawn.End, TextUnitPixels, ElapsedSeconds);
        Vector2 center = (basePoint + topPoint) * 0.5f;
        Color body = spawn.Styled(new Color("#FF2B2B"));
        float unit = TextUnitPixels;
        float maxActive = Mathf.Clamp(Mathf.Round(spawn.RangeUnits), 1f, 10f);
        float burst = Mathf.Clamp(Mathf.Round(spawn.ThicknessUnits), 1f, 10f);
        float interval = Mathf.Clamp(Mathf.Round(Mathf.Abs(spawn.SpeedUnits)), 1f, 10f);

        DrawLine(basePoint + new Vector2(2, 3), topPoint + new Vector2(2, 3), new Color(0, 0, 0, 0.28f), 3f);
        DrawLine(basePoint, topPoint, new Color("#202A34"), 3f);

        Vector2 flagA = topPoint;
        Vector2 flagB = topPoint + new Vector2(unit * 5.2f, unit * 1.4f);
        Vector2 flagC = topPoint + new Vector2(unit * 1.0f, unit * 3.0f);
        DrawColoredPolygon([flagA, flagB, flagC], WithAlpha(body, body.A * 0.82f));
        DrawPolyline([flagA, flagB, flagC, flagA], new Color("#F7F5EF"), 1.5f);

        float radius = Mathf.Max(unit * 2.0f, 13f);
        DrawCircle(center + new Vector2(2, 3), radius + 3f, new Color(0, 0, 0, 0.30f));
        DrawCircle(center, radius, WithAlpha(body, body.A * 0.66f));
        DrawCircle(center, radius, new Color("#F7F5EF"), false, 2f);
        DrawCircle(center + new Vector2(-radius * 0.35f, -radius * 0.12f), radius * 0.14f, new Color("#202A34"));
        DrawCircle(center + new Vector2(radius * 0.35f, -radius * 0.12f), radius * 0.14f, new Color("#202A34"));
        DrawLine(center + new Vector2(-radius * 0.32f, radius * 0.34f), center + new Vector2(radius * 0.32f, radius * 0.34f), new Color("#202A34"), 2f);

        string caption = $"SPAWN  {interval:0}s  x{burst:0}  max {maxActive:0}";
        DrawString(ThemeDB.FallbackFont, center + new Vector2(radius + 6f, 4f), caption, HorizontalAlignment.Left, 150f, 12, new Color("#FFF0A8"));
    }

    private void DrawSelectedWorldObjectHandles()
    {
        if (!EditorMode)
            return;

        if (_selectedWorldObjectIndex < 0 || _selectedWorldObjectIndex >= _placedWorldObjects.Count)
            return;

        WorldObject selected = _placedWorldObjects[_selectedWorldObjectIndex];
        if (selected.IsEditorOnly && !ShowEditorOnlyObjects)
            return;

        Vector2 start = selected.Start;
        Vector2 end = selected.End;
        DrawLine(start, end, new Color("#FF2BD6", 0.75f), 2.5f);
        DrawMoveGrip(selected.Center);
        DrawHandle(start, "A");
        DrawHandle(end, "B");
    }

    private void DrawMoveGrip(Vector2 point)
    {
        float radius = Mathf.Max(5f, TextUnitPixels * 0.65f);
        DrawCircle(point + new Vector2(2, 3), radius + 2f, new Color(0, 0, 0, 0.28f));
        DrawCircle(point, radius, new Color("#5CB8A7"));
        DrawCircle(point, radius + 1.5f, new Color("#F7F5EF"), false, 1.5f);
        DrawLine(point - new Vector2(radius * 1.8f, 0), point + new Vector2(radius * 1.8f, 0), new Color("#F7F5EF"), 1.5f);
        DrawLine(point - new Vector2(0, radius * 1.8f), point + new Vector2(0, radius * 1.8f), new Color("#F7F5EF"), 1.5f);
    }

    private void DrawHandle(Vector2 point, string label)
    {
        float radius = Mathf.Max(7f, TextUnitPixels * 0.9f);
        Rect2 rect = new(point - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));
        DrawRect(rect.Grow(2f), new Color("#101820", 0.72f), true);
        DrawRect(rect, new Color("#FFF0A8"), true);
        DrawRect(rect, new Color("#FF2BD6"), false, 2f);
        DrawString(ThemeDB.FallbackFont, point + new Vector2(radius + 3f, -radius - 2f), label, HorizontalAlignment.Left, 28f, 11, new Color("#FFF0A8"));
    }

    private static Texture2D? LoadPng(string resourcePath)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return null;

        Image image = Image.LoadFromFile(filePath);
        return image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }
}
