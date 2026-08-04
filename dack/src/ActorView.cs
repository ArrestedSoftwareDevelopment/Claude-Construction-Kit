using Godot;
using System;

namespace Dack;

public partial class ActorView : Control
{
    private EditableSpriteModel? _model;
    private bool _selected;
    private bool _isPlayable;
    private bool _facingRight = true;
    private ActorMotionState _motionState = ActorMotionState.Idle;
    private bool _dragging;
    private static readonly Color SpriteShadowModulate = new(0.08f, 0.09f, 0.10f, 0.24f);

    public string ActorName { get; set; } = "Actor";
    public string AnimationSourceId { get; set; } = "";
    public SpriteAnimationSet? AnimationSet { get; set; }
    public ActorMotionState MotionState
    {
        get => _motionState;
        set
        {
            if (_motionState == value)
                return;

            _motionState = value;
            AnimationClock = 0;
            QueueRedraw();
        }
    }
    public int? DirectionFrameIndex { get; set; }
    public double AnimationClock { get; set; }
    public bool StrobeEnabled { get; set; }
    public int StrobeCount { get; set; }
    public bool ManualPlacement { get; set; }
    public bool CanDragPlayableInEditor { get; set; }
    public Vector2 HomePosition { get; set; }
    public bool EditorMode { get; set; } = true;
    public bool CanFireProjectiles { get; set; }
    public int ShotToughness { get; set; } = 1;
    public float RadarRangeUnits { get; set; } = 28f;

    public bool IsPlayable
    {
        get => _isPlayable;
        set
        {
            _isPlayable = value;
            QueueRedraw();
        }
    }

    public bool FacingRight
    {
        get => _facingRight;
        set
        {
            _facingRight = value;
            QueueRedraw();
        }
    }

    public EditableSpriteModel Model
    {
        get => _model ?? throw new InvalidOperationException("Actor has no sprite model.");
        set
        {
            if (_model == value)
                return;

            if (_model is not null)
                _model.Changed -= OnModelChanged;

            _model = value;
            _model.Changed += OnModelChanged;
            QueueRedraw();
        }
    }

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            QueueRedraw();
        }
    }

    public event Action<ActorView>? SelectionRequested;

    public override void _Ready()
    {
        MouseDefaultCursorShape = CursorShape.PointingHand;
        TooltipText = $"Select {ActorName}";
    }

    public override void _Process(double delta)
    {
        if (AnimationSet is null)
            return;

        AnimationClock += delta;
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (_model is not null)
            _model.Changed -= OnModelChanged;
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseButton
            && mouseButton.ButtonIndex == MouseButton.Left
            && mouseButton.Pressed)
        {
            SelectionRequested?.Invoke(this);
            if (CanDragInEditor)
            {
                _dragging = true;
                ManualPlacement = true;
            }

            AcceptEvent();
        }
        else if (inputEvent is InputEventMouseButton release
                 && release.ButtonIndex == MouseButton.Left
                 && !release.Pressed)
        {
            _dragging = false;
            if (CanDragInEditor)
                HomePosition = Position;
            AcceptEvent();
        }
        else if (inputEvent is InputEventMouseMotion motion && _dragging && CanDragInEditor)
        {
            Position += motion.Relative;
            AcceptEvent();
        }
    }

    private bool CanDragInEditor => EditorMode && (!IsPlayable || CanDragPlayableInEditor);

    public override void _Draw()
    {
        if (IsPlayable || AnimationSet is not null)
        {
            DrawAnimatedActor();
            return;
        }

        Rect2 card = new(new Vector2(4, 4), Size - new Vector2(8, 12));
        DrawRect(card, new Color("#F7F5EF"), true);
        DrawRect(
            card,
            Selected ? new Color("#FF5C35") : new Color("#52606D"),
            false,
            Selected ? 4f : 2f
        );

        if (_model is not null)
        {
            Rect2 spriteArea = new(new Vector2(16, 12), new Vector2(72, 72));
            DrawTextureRect(_model.Texture, spriteArea, false);
        }

        DrawRect(
            new Rect2(new Vector2(14, 91), new Vector2(76, 7)),
            new Color("#D9DEE5"),
            true
        );
        DrawRect(
            new Rect2(new Vector2(14, 91), new Vector2(Selected ? 60 : 48, 7)),
            Selected ? new Color("#FF5C35") : new Color("#5CB8A7"),
            true
        );
    }

    private void DrawAnimatedActor()
    {
        if (_model is null)
            return;

        if (AnimationSet is not null)
        {
            SpriteFrame frame = DirectionFrameIndex is int directionFrame
                ? AnimationSet.GetFrame(directionFrame)
                : AnimationSet.GetFrame(MotionState, AnimationClock);
            DrawSpriteFrame(frame.Texture, frame.SourceRegion, frame.DisplaySize);
        }
        else
        {
            Rect2 sourceRegion = _model.GetOpaqueBounds(2);
            DrawSpriteFrame(_model.Texture, sourceRegion, sourceRegion.Size);
        }

        if (Selected)
        {
            DrawArc(
                new Vector2(Size.X * 0.5f, Size.Y * 0.92f),
                Size.X * 0.30f,
                0,
                Mathf.Tau,
                32,
                new Color("#FF5C35"),
                2f
            );
        }

        if (StrobeEnabled && StrobeCount > 0)
        {
            float pulse = Mathf.Sin((float)AnimationClock * Mathf.Max(1, StrobeCount) * Mathf.Tau);
            if (pulse > 0f)
            {
                DrawRect(new Rect2(Vector2.Zero, Size), new Color("#F7F5EF", 0.42f), true);
                DrawRect(new Rect2(Vector2.Zero, Size), new Color("#FF2BD6", 0.62f), false, 2.5f);
            }
        }
    }

    private void DrawSpriteFrame(Texture2D texture, Rect2 sourceRegion, Vector2 displaySize)
    {
        Vector2 stableDisplay = new(Mathf.Max(1f, displaySize.X), Mathf.Max(1f, displaySize.Y));
        float scale = Mathf.Min(Size.X / stableDisplay.X, Size.Y / stableDisplay.Y);
        Vector2 drawSize = sourceRegion.Size * scale;
        Vector2 drawPosition = new(
            (Size.X - drawSize.X) * 0.5f,
            Size.Y - drawSize.Y
        );

        DrawSpriteShadow(texture, sourceRegion, drawPosition, drawSize);

        if (!FacingRight)
            DrawSetTransform(new Vector2(Size.X, 0), 0, new Vector2(-1, 1));

        DrawTextureRectRegion(texture, new Rect2(drawPosition, drawSize), sourceRegion);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    private void DrawSpriteShadow(Texture2D texture, Rect2 sourceRegion, Vector2 drawPosition, Vector2 drawSize)
    {
        if (drawSize.X <= 0 || drawSize.Y <= 0)
            return;

        Vector2 baseCenter = drawPosition + new Vector2(drawSize.X * 0.5f, drawSize.Y * 0.94f);
        Vector2 shadowSize = new(drawSize.X * 0.82f, Mathf.Max(3f, drawSize.Y * 0.22f));
        Vector2 shadowOffset = new(drawSize.X * 0.10f, Mathf.Max(4f, drawSize.Y * 0.08f));
        Rect2 shadowRect = new(new Vector2(-shadowSize.X * 0.5f, -shadowSize.Y * 0.5f), shadowSize);

        DrawSetTransform(baseCenter + shadowOffset, Mathf.DegToRad(-7f), new Vector2(1.08f, 0.72f));
        DrawTextureRectRegion(texture, shadowRect, sourceRegion, SpriteShadowModulate);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    private void OnModelChanged() => QueueRedraw();
}
