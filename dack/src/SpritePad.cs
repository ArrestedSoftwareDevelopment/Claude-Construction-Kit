using Godot;

namespace Dack;

public partial class SpritePad : Control
{
    private EditableSpriteModel? _model;
    private Vector2I _lastCell = new(-1, -1);

    public float DisplaySize { get; set; } = 320f;
    public Color PaintColor { get; set; } = new("#181A1F");
    public bool Erasing { get; set; }

    public EditableSpriteModel Model
    {
        get => _model!;
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

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(DisplaySize, DisplaySize);
        MouseDefaultCursorShape = CursorShape.Cross;
    }

    public override void _ExitTree()
    {
        if (_model is not null)
            _model.Changed -= OnModelChanged;
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex is MouseButton.Left or MouseButton.Right)
            {
                if (mouseButton.Pressed)
                    ApplyAt(mouseButton.Position, mouseButton.ButtonIndex == MouseButton.Right);
                else
                    _lastCell = new Vector2I(-1, -1);

                AcceptEvent();
            }
        }
        else if (inputEvent is InputEventMouseMotion motion
                 && (motion.ButtonMask & MouseButtonMask.Left) != 0)
        {
            ApplyAt(motion.Position, false);
            AcceptEvent();
        }
        else if (inputEvent is InputEventMouseMotion eraseMotion
                 && (eraseMotion.ButtonMask & MouseButtonMask.Right) != 0)
        {
            ApplyAt(eraseMotion.Position, true);
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        if (_model is null)
            return;

        float padSize = Mathf.Min(Size.X, Size.Y);
        float cellSize = padSize / EditableSpriteModel.CanvasSize;
        Color checkerA = new("#F6F3EC");
        Color checkerB = new("#E5E1D8");

        for (int y = 0; y < EditableSpriteModel.CanvasSize; y++)
        {
            for (int x = 0; x < EditableSpriteModel.CanvasSize; x++)
            {
                Rect2 cell = new(
                    new Vector2(x * cellSize, y * cellSize),
                    new Vector2(cellSize, cellSize)
                );
                DrawRect(cell, (x + y) % 2 == 0 ? checkerA : checkerB, true);

                Color pixel = _model.Pixels.GetPixel(x, y);
                if (pixel.A > 0.001f)
                    DrawRect(cell, pixel, true);
            }
        }

        Color grid = new(0.13f, 0.16f, 0.19f, 0.12f);
        for (int i = 0; i <= EditableSpriteModel.CanvasSize; i++)
        {
            float offset = i * cellSize;
            DrawLine(new Vector2(offset, 0), new Vector2(offset, padSize), grid);
            DrawLine(new Vector2(0, offset), new Vector2(padSize, offset), grid);
        }

        DrawRect(new Rect2(Vector2.Zero, new Vector2(padSize, padSize)), new Color("#30343B"), false, 2f);
    }

    private void ApplyAt(Vector2 localPosition, bool forceErase)
    {
        float padSize = Mathf.Min(Size.X, Size.Y);
        if (_model is null || localPosition.X < 0 || localPosition.Y < 0
            || localPosition.X >= padSize || localPosition.Y >= padSize)
            return;

        float cellSize = padSize / EditableSpriteModel.CanvasSize;
        Vector2I cell = new(
            Mathf.FloorToInt(localPosition.X / cellSize),
            Mathf.FloorToInt(localPosition.Y / cellSize)
        );

        if (cell == _lastCell)
            return;

        _lastCell = cell;
        _model.SetPixel(cell, forceErase || Erasing ? Colors.Transparent : PaintColor);
    }

    private void OnModelChanged() => QueueRedraw();
}
