using Godot;
using System.IO;

namespace Dack;

public partial class PlayfieldSurface : Control
{
    private Texture2D? _brick;
    private Texture2D? _platform;
    private Texture2D? _window;
    private CapturedPageFrame? _capturedPage;
    private double _elapsed;

    public float TextUnitPixels { get; set; } = 2f;
    public PlatformerMode Mode { get; set; } = PlatformerMode.Horizontal;
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
        MouseFilter = MouseFilterEnum.Ignore;
        _brick = LoadPng("res://assets/third_party/8-bit-dungeon/brick-solid.png");
        _platform = LoadPng("res://assets/third_party/8-bit-dungeon/platform.png");
        _window = LoadPng("res://assets/third_party/8-bit-dungeon/window-2.png");
        _capturedPage = CapturedPageImportModule.TryLoadDefault();
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        DrawBackground();

        if (_capturedPage is not null)
        {
            DrawRect(new Rect2(0, 0, Size.X, 30), new Color("#26313C"), true);
            DrawRect(new Rect2(14, 9, 12, 12), new Color("#FF5C35"), true);
            DrawRect(new Rect2(34, 9, 12, 12), new Color("#F4C95D"), true);
            DrawRect(new Rect2(54, 9, 12, 12), new Color("#5CB8A7"), true);
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

        if (Mode == PlatformerMode.Vertical)
        {
            foreach (WorldObject ladder in GetLadders())
                DrawLadder(ladder);
        }

        foreach (WorldObject ramp in GetRamps())
            DrawRamp(ramp);

        foreach (WorldObject conveyor in GetConveyors())
            DrawConveyor(conveyor);

        foreach (WorldObject elevator in GetElevators())
            DrawElevator(elevator);

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

    public WorldObject[] GetLadders()
    {
        if (_capturedPage is not null)
            return [];

        float unit = TextUnitPixels;
        return
        [
            new WorldObject(WorldObjectKind.Ladder, new Vector2(Size.X * 0.29f, Size.Y * 0.72f), new Vector2(Size.X * 0.29f, Size.Y * 0.36f), 1.2f),
            new WorldObject(WorldObjectKind.Ladder, new Vector2(Size.X * 0.62f, Size.Y * 0.88f), new Vector2(Size.X * 0.62f, Size.Y * 0.54f), 1.2f)
        ];
    }

    public WorldObject[] GetRamps()
    {
        if (_capturedPage is not null)
            return [];

        float unit = TextUnitPixels;
        if (Mode == PlatformerMode.Vertical)
        {
            return
            [
                new WorldObject(WorldObjectKind.Ramp, new Vector2(Size.X * 0.42f, Size.Y * 0.56f), new Vector2(Size.X * 0.58f, Size.Y * 0.39f), 0.9f)
            ];
        }

        return
        [
            new WorldObject(WorldObjectKind.Ramp, new Vector2(Size.X * 0.39f, Size.Y * 0.66f), new Vector2(Size.X * 0.55f, Size.Y * 0.56f), 0.9f)
        ];
    }

    public WorldObject[] GetConveyors()
    {
        if (_capturedPage is not null)
            return [];

        return
        [
            new WorldObject(WorldObjectKind.Conveyor, new Vector2(Size.X * 0.66f, Size.Y * 0.78f), new Vector2(Size.X * 0.85f, Size.Y * 0.78f), 0.9f, Mode == PlatformerMode.Horizontal ? -6f : 6f)
        ];
    }

    public WorldObject[] GetElevators()
    {
        if (_capturedPage is not null)
            return [];

        return
        [
            new WorldObject(WorldObjectKind.Elevator, new Vector2(Size.X * 0.74f, Size.Y * 0.40f), new Vector2(Size.X * 0.86f, Size.Y * 0.40f), 0.9f, 1.6f, 0.25f)
        ];
    }

    public bool IsTouchingLadder(Rect2 actorBounds)
    {
        if (Mode != PlatformerMode.Vertical)
            return false;

        foreach (WorldObject ladder in GetLadders())
        {
            if (ladder.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
                return true;
        }

        return false;
    }

    public bool IsTouchingRamp(Rect2 actorBounds)
    {
        foreach (WorldObject ramp in GetRamps())
        {
            if (ramp.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
                return true;
        }

        return false;
    }

    public Vector2 GetConveyorVelocity(Rect2 actorBounds)
    {
        foreach (WorldObject conveyor in GetConveyors())
        {
            if (conveyor.Bounds(TextUnitPixels, ElapsedSeconds).Intersects(actorBounds, true))
                return conveyor.Direction(TextUnitPixels, ElapsedSeconds) * conveyor.SpeedUnits * TextUnitPixels;
        }

        return Vector2.Zero;
    }

    public Vector2 GetSpawnPosition(Vector2 actorSize)
    {
        foreach (Rect2 platform in GetPlatforms())
        {
            if (platform.Size.X < actorSize.X * 1.5f || platform.Position.Y < 48f)
                continue;

            float x = Mathf.Clamp(platform.Position.X, 0, Mathf.Max(0, Size.X - actorSize.X));
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
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("#D9E3EA"), true);

        if (_capturedPage is not null)
        {
            DrawCapturedPage(_capturedPage);
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
        Vector2 imageSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = Mathf.Max(Size.X / imageSize.X, Size.Y / imageSize.Y);
        Vector2 drawSize = imageSize * scale;
        Vector2 drawPosition = (Size - drawSize) * 0.5f;
        DrawTextureRect(frame.Texture, new Rect2(drawPosition, drawSize), false);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.08f), true);
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

    private Rect2 GetCapturedPageDrawRect(CapturedPageFrame frame)
    {
        Vector2 imageSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = Mathf.Max(Size.X / imageSize.X, Size.Y / imageSize.Y);
        Vector2 drawSize = imageSize * scale;
        Vector2 drawPosition = (Size - drawSize) * 0.5f;
        return new Rect2(drawPosition, drawSize);
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
        Rect2 rect = ladder.Bounds(TextUnitPixels, ElapsedSeconds);
        Color rail = new("#8A5A37");
        DrawRect(new Rect2(rect.Position, new Vector2(3, rect.Size.Y)), rail, true);
        DrawRect(new Rect2(rect.Position + new Vector2(rect.Size.X - 3, 0), new Vector2(3, rect.Size.Y)), rail, true);

        for (float y = rect.Position.Y + TextUnitPixels; y < rect.End.Y; y += TextUnitPixels)
            DrawRect(new Rect2(rect.Position.X, y, rect.Size.X, 3), rail, true);
    }

    private void DrawRamp(WorldObject ramp)
    {
        Vector2 start = ramp.ResolvePoint(ramp.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = ramp.ResolvePoint(ramp.End, TextUnitPixels, ElapsedSeconds);
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.45f);
        DrawLine(start, end, new Color("#5CB8A7"), TextUnitPixels * 0.42f);
        DrawLine(start, end, new Color("#F7F5EF"), 2f);
    }

    private void DrawConveyor(WorldObject conveyor)
    {
        Vector2 start = conveyor.ResolvePoint(conveyor.Start, TextUnitPixels, ElapsedSeconds);
        Vector2 end = conveyor.ResolvePoint(conveyor.End, TextUnitPixels, ElapsedSeconds);
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.55f);
        DrawLine(start, end, new Color("#4378B8"), TextUnitPixels * 0.5f);

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
        DrawLine(start + new Vector2(3, 5), end + new Vector2(3, 5), new Color(0, 0, 0, 0.18f), TextUnitPixels * 0.65f);
        DrawLine(start, end, new Color("#F4C95D"), TextUnitPixels * 0.6f);
        DrawLine(start, end, new Color("#202A34"), 2f);
    }

    private static Texture2D? LoadPng(string resourcePath)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return null;

        Image image = Image.LoadFromFile(filePath);
        return image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
    }
}
