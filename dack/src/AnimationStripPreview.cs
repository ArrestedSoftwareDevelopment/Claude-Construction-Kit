using Godot;
using System.Collections.Generic;

namespace Dack;

public partial class AnimationStripPreview : Control
{
    private SpriteFrame[] _frames = [];
    private readonly List<AnimationClipLabel> _labels = [];
    private bool _facingRight = true;

    public int Columns { get; set; } = 8;
    public int NumberBase { get; set; }
    /// <summary>
    /// Keeps the contact strip in the same facing as the selected actor preview.
    /// Source sheets are kept in their authoring orientation; this is only a
    /// display transform and never rewrites the imported frame.
    /// </summary>
    public bool FacingRight
    {
        get => _facingRight;
        set
        {
            if (_facingRight == value)
                return;

            _facingRight = value;
            QueueRedraw();
        }
    }
    public int FrameCount => _frames.Length;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(0, 190);
        if (SpriteAnimationSet.TryLoadGameCreatorPlayerFrames(out SpriteFrame[] frames))
            SetFrames(frames);
    }

    public void SetFrames(SpriteFrame[] frames)
    {
        _frames = frames;
        CustomMinimumSize = new Vector2(0, Mathf.Max(1, Mathf.Ceil(frames.Length / (float)Mathf.Max(1, Columns))) * 46f + 8f);
        QueueRedraw();
    }

    public void SetLabels(IEnumerable<AnimationClipLabel> labels)
    {
        _labels.Clear();
        _labels.AddRange(labels);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("#F7F5EF", 0.82f), true);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("#D9DEE5"), false, 1f);

        if (_frames.Length == 0)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(10, 24), "No animation frames detected", HorizontalAlignment.Left, Size.X - 20, 12, new Color("#52606D"));
            return;
        }

        int columns = Mathf.Max(1, Columns);
        float gutter = 4f;
        float cellWidth = Mathf.Max(32f, (Size.X - gutter * (columns + 1)) / columns);
        float cellHeight = 42f;

        for (int i = 0; i < _frames.Length; i++)
        {
            int column = i % columns;
            int row = i / columns;
            Rect2 cell = new(
                new Vector2(gutter + column * (cellWidth + gutter), gutter + row * (cellHeight + gutter)),
                new Vector2(cellWidth, cellHeight)
            );

            Color rangeColor = RangeColorForFrame(i);
            DrawRect(cell, rangeColor.A > 0 ? rangeColor : new Color("#E8EDF2"), true);
            DrawRect(cell, new Color("#52606D", 0.45f), false, 1f);

            SpriteFrame frame = _frames[i];
            Rect2 source = frame.SourceRegion;
            Vector2 displaySize = frame.DisplaySize;
            float scale = Mathf.Min((cell.Size.X - 8f) / displaySize.X, 25f / displaySize.Y);
            Vector2 drawSize = displaySize * scale;
            Vector2 drawPosition = cell.Position + new Vector2((cell.Size.X - drawSize.X) * 0.5f, 4f);
            DrawFrame(frame.Texture, source, drawPosition, drawSize);

            DrawString(
                ThemeDB.FallbackFont,
                cell.Position + new Vector2(3f, cell.Size.Y - 6f),
                (i + NumberBase).ToString(),
                HorizontalAlignment.Left,
                cell.Size.X - 6f,
                10,
                new Color("#202A34")
            );

            if (IsPingPongFrame(i))
            {
                DrawString(
                    ThemeDB.FallbackFont,
                    cell.Position + new Vector2(cell.Size.X - 15f, cell.Size.Y - 6f),
                    "↔",
                    HorizontalAlignment.Left,
                    12f,
                    10,
                    new Color("#202A34")
                );
            }
        }
    }

    private void DrawFrame(Texture2D texture, Rect2 source, Vector2 drawPosition, Vector2 drawSize)
    {
        if (FacingRight)
        {
            DrawTextureRectRegion(texture, new Rect2(drawPosition, drawSize), source);
            return;
        }

        // Mirror only the sprite pixels around the frame cell's center. The
        // cell, frame number, and range overlays must remain in reading order.
        DrawSetTransform(new Vector2(drawPosition.X + drawSize.X, drawPosition.Y), 0f, new Vector2(-1f, 1f));
        DrawTextureRectRegion(texture, new Rect2(Vector2.Zero, drawSize), source);
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }

    private Color RangeColorForFrame(int frame)
    {
        foreach (AnimationClipLabel label in _labels)
        {
            if (!Contains(label.Range, frame))
                continue;

            return new Color(label.Color.R, label.Color.G, label.Color.B, 0.38f);
        }

        return Colors.Transparent;
    }

    private bool IsPingPongFrame(int frame)
    {
        foreach (AnimationClipLabel label in _labels)
        {
            if (label.PingPong && Contains(label.Range, frame))
                return true;
        }

        return false;
    }

    private static bool Contains(AnimationFrameRange range, int frame)
    {
        int start = Mathf.Min(range.Start, range.End);
        int end = Mathf.Max(range.Start, range.End);
        return frame >= start && frame <= end;
    }
}
