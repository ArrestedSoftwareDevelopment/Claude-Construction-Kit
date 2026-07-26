using Godot;
using System;

namespace Dack;

public partial class ActorView : Control
{
    private EditableSpriteModel? _model;
    private bool _selected;
    private bool _isPlayable;
    private bool _facingRight = true;

    public string ActorName { get; set; } = "Actor";
    public SpriteAnimationSet? AnimationSet { get; set; }
    public ActorMotionState MotionState { get; set; } = ActorMotionState.Idle;
    public double AnimationClock { get; set; }

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
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        if (IsPlayable)
        {
            DrawPlayable();
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

    private void DrawPlayable()
    {
        if (_model is null)
            return;

        if (AnimationSet is not null)
        {
            SpriteFrame frame = AnimationSet.GetFrame(MotionState, AnimationClock);
            DrawSpriteFrame(frame.Texture, frame.SourceRegion);
        }
        else
        {
            DrawSpriteFrame(_model.Texture, _model.GetOpaqueBounds(2));
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
    }

    private void DrawSpriteFrame(Texture2D texture, Rect2 sourceRegion)
    {
        if (!FacingRight)
            DrawSetTransform(new Vector2(Size.X, 0), 0, new Vector2(-1, 1));

        DrawTextureRectRegion(texture, new Rect2(Vector2.Zero, Size), sourceRegion);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    private void OnModelChanged() => QueueRedraw();
}
