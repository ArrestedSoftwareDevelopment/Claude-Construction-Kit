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
    private CapturedPageFrame? _capturedPage;
    private double _elapsed;

    public float TextUnitPixels { get; set; } = 7f;
    public PlatformerMode Mode { get; set; } = PlatformerMode.Horizontal;
    public bool HasCapturedPage => _capturedPage is not null;
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
            return;

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

    public Rect2[] GetBrickRegions()
    {
        return GetTextObjectRegions(TextObjectGranularity.Letter);
    }

    public Rect2[] GetTextObjectRegions(TextObjectGranularity granularity)
    {
        if (_capturedPage is not null)
        {
            return granularity switch
            {
                TextObjectGranularity.Word => GetCapturedRects(_capturedPage, _capturedPage.TextWords),
                TextObjectGranularity.Line => GetCapturedRects(_capturedPage, _capturedPage.TextLines),
                _ => GetCapturedRects(_capturedPage, _capturedPage.TextBricks)
            };
        }

        return GetPlatforms();
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

        Rect2 sourceRegion = DisplayToSourceRect(_capturedPage, region).Grow(1.5f);
        Color fill = SampleBackgroundColor(_capturedPage.OriginalImage, sourceRegion);
        int minX = Mathf.Clamp(Mathf.FloorToInt(sourceRegion.Position.X), 0, _capturedPage.Image.GetWidth() - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(sourceRegion.Position.Y), 0, _capturedPage.Image.GetHeight() - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(sourceRegion.End.X), 0, _capturedPage.Image.GetWidth() - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(sourceRegion.End.Y), 0, _capturedPage.Image.GetHeight() - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Color original = _capturedPage.OriginalImage.GetPixel(x, y);
                Color current = _capturedPage.Image.GetPixel(x, y);
                if (IsLikelyTextPixel(original) || IsLikelyInkEdge(original, fill) || IsLikelyTextPixel(current))
                    _capturedPage.Image.SetPixel(x, y, fill);
            }
        }

        if (_capturedPage.Texture is ImageTexture imageTexture)
            imageTexture.Update(_capturedPage.Image);

        QueueRedraw();
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
        DrawTextureRect(frame.Texture, GetCapturedPageDrawRect(frame), false);
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

    private Rect2[] GetCapturedRects(CapturedPageFrame frame, Rect2[] sourceRects)
    {
        Rect2 imageRect = GetCapturedPageDrawRect(frame);
        Vector2 sourceSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        float scale = imageRect.Size.X / sourceSize.X;
        Rect2[] mapped = new Rect2[sourceRects.Length];

        for (int i = 0; i < sourceRects.Length; i++)
        {
            Rect2 source = sourceRects[i];
            mapped[i] = new Rect2(
                imageRect.Position + source.Position * scale,
                source.Size * scale
            );
        }

        return mapped;
    }

    private Rect2 GetCapturedPageDrawRect(CapturedPageFrame frame)
    {
        Vector2 sourceSize = new(frame.PixelSize.X, frame.PixelSize.Y);
        if (sourceSize.X <= 0 || sourceSize.Y <= 0 || Size.X <= 0 || Size.Y <= 0)
            return new Rect2(Vector2.Zero, sourceSize);

        float scale = Mathf.Max(Size.X / sourceSize.X, Size.Y / sourceSize.Y);
        Vector2 drawSize = sourceSize * scale;
        return new Rect2((Size - drawSize) * 0.5f, drawSize);
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
        return luminance < 0.38f;
    }

    private static bool IsLikelyInkEdge(Color pixel, Color background)
    {
        float luminance = pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
        float backgroundLuminance = background.R * 0.2126f + background.G * 0.7152f + background.B * 0.0722f;
        return luminance < backgroundLuminance - 0.08f;
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
