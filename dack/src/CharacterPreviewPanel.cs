using Godot;

namespace Dack;

public partial class CharacterPreviewPanel : Control
{
    private ActorView? _actor;
    private SpriteFrame[]? _previewFrames;
    private string _previewTitle = "Idle preview";
    private double _clock;

    public Color BackgroundColor { get; set; } = new("#101820");
    public Color BorderColor { get; set; } = new("#5CB8FF", 0.55f);
    public Color TitleColor { get; set; } = new("#FFF0A8");
    public Color TextColor { get; set; } = new("#F7F5EF");
    public Color MutedTextColor { get; set; } = new("#AAB7C4");

    public ActorView? Actor
    {
        get => _actor;
        set
        {
            _actor = value;
            _previewFrames = null;
            _previewTitle = "Idle preview";
            _clock = 0;
            QueueRedraw();
        }
    }

    public void ShowFrames(SpriteFrame[] frames, string title)
    {
        _previewFrames = frames.Length == 0 ? null : frames;
        _previewTitle = string.IsNullOrWhiteSpace(title) ? "Animation preview" : title;
        _clock = 0;
        QueueRedraw();
    }

    public override void _Ready()
    {
        if (CustomMinimumSize.Y < 1f)
            CustomMinimumSize = new Vector2(0, 170);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Process(double delta)
    {
        _clock += delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        Rect2 panel = new(Vector2.Zero, Size);
        Color background = BackgroundColor;
        if (background.A <= 0.001f)
            background.A = 1f;
        DrawRect(panel, background, true);
        DrawRect(panel, BorderColor, false, 1.5f);

        if ((_actor is null || _actor.AnimationSet is null) && _previewFrames is null)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(16, 34), "Select a character", HorizontalAlignment.Left, Size.X - 32, 16, TextColor);
            DrawString(ThemeDB.FallbackFont, new Vector2(16, 60), "Sprite Studio preview will show animation here.", HorizontalAlignment.Left, Size.X - 32, 13, MutedTextColor);
            return;
        }

        SpriteFrame frame = _previewFrames is { Length: > 0 } frames
            ? frames[Mathf.PosMod(Mathf.FloorToInt((float)_clock * 8), frames.Length)]
            : _actor!.AnimationSet!.GetFrame(ActorMotionState.Idle, _clock);
        Vector2 stableDisplay = new(Mathf.Max(1f, frame.DisplaySize.X), Mathf.Max(1f, frame.DisplaySize.Y));
        float availableWidth = Mathf.Max(1f, Size.X - 36f);
        float availableHeight = Mathf.Max(1f, Size.Y - 52f);
        float scale = Mathf.Min(availableWidth / stableDisplay.X, availableHeight / stableDisplay.Y);
        scale = Mathf.Min(scale, 5.5f);
        Vector2 drawSize = frame.SourceRegion.Size * scale;
        Vector2 drawPosition = new(
            (Size.X - drawSize.X) * 0.5f,
            Mathf.Max(34f, Size.Y - drawSize.Y - 14f)
        );

        string actorName = _actor?.ActorName ?? "Sprite";
        DrawString(ThemeDB.FallbackFont, new Vector2(14, 22), $"{actorName}  //  {_previewTitle}", HorizontalAlignment.Left, Size.X - 28f, 14, TitleColor);
        DrawLine(new Vector2(18, Size.Y - 14f), new Vector2(Size.X - 18f, Size.Y - 14f), new Color("#202A34", 0.20f), 1.2f);
        DrawSpriteShadow(frame.Texture, frame.SourceRegion, drawPosition, drawSize);

        if (_actor is not null && !_actor.FacingRight)
            DrawSetTransform(new Vector2(Size.X, 0), 0, new Vector2(-1, 1));

        DrawTextureRectRegion(frame.Texture, new Rect2(drawPosition, drawSize), frame.SourceRegion);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    private void DrawSpriteShadow(Texture2D texture, Rect2 sourceRegion, Vector2 drawPosition, Vector2 drawSize)
    {
        Vector2 baseCenter = drawPosition + new Vector2(drawSize.X * 0.5f, drawSize.Y * 0.94f);
        Vector2 shadowSize = new(drawSize.X * 0.82f, Mathf.Max(4f, drawSize.Y * 0.20f));
        Rect2 shadowRect = new(new Vector2(-shadowSize.X * 0.5f, -shadowSize.Y * 0.5f), shadowSize);
        DrawSetTransform(baseCenter + new Vector2(drawSize.X * 0.10f, Mathf.Max(5f, drawSize.Y * 0.08f)), Mathf.DegToRad(-7f), new Vector2(1.08f, 0.72f));
        DrawTextureRectRegion(texture, shadowRect, sourceRegion, new Color(0.08f, 0.09f, 0.10f, 0.26f));
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }
}
