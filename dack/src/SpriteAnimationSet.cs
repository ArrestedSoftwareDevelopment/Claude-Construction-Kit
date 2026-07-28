using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record SpriteFrame(Texture2D Texture, Rect2 SourceRegion);
public readonly record struct AnimationFrameRange(int Start, int End);

public sealed class SpriteAnimationSet
{
    private const string AssetRoot = "res://assets/third_party/stickman-pack-v0.1";
    private static readonly AnimationFrameRange DefaultGameCreatorIdle = new(0, 2);
    private static readonly AnimationFrameRange DefaultGameCreatorRun = new(3, 14);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpUp = new(15, 15);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpDown = new(16, 16);

    private readonly Dictionary<ActorMotionState, SpriteFrame[]> _frames;

    private SpriteAnimationSet(Dictionary<ActorMotionState, SpriteFrame[]> frames)
    {
        _frames = frames;
    }

    public static SpriteAnimationSet? TryLoadStickman()
    {
        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        AddFrames(frames, ActorMotionState.Idle, $"{AssetRoot}/thin-idle-sheet.png");
        AddFrames(frames, ActorMotionState.Run, $"{AssetRoot}/thin-run-sheet.png");
        AddFrames(frames, ActorMotionState.JumpUp, $"{AssetRoot}/thin-jump-up.png");
        AddFrames(frames, ActorMotionState.JumpDown, $"{AssetRoot}/thin-jump-down.png");

        if (!frames.ContainsKey(ActorMotionState.Idle))
            return null;

        if (!frames.ContainsKey(ActorMotionState.Run))
            frames[ActorMotionState.Run] = frames[ActorMotionState.Idle];

        if (!frames.ContainsKey(ActorMotionState.Crawl))
            frames[ActorMotionState.Crawl] = frames[ActorMotionState.Run];

        if (!frames.ContainsKey(ActorMotionState.JumpUp))
            frames[ActorMotionState.JumpUp] = frames[ActorMotionState.Idle];

        if (!frames.ContainsKey(ActorMotionState.JumpDown))
            frames[ActorMotionState.JumpDown] = frames[ActorMotionState.JumpUp];

        return new SpriteAnimationSet(frames);
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer()
    {
        return TryLoadGameCreatorPlayer(
            DefaultGameCreatorIdle,
            DefaultGameCreatorRun,
            DefaultGameCreatorJumpUp,
            DefaultGameCreatorJumpDown
        );
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown
    )
    {
        return TryLoadGameCreatorPlayer(idle, run, jumpUp, jumpDown, false, false, false, false);
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong
    )
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string filePath = Path.GetFullPath(Path.Combine(
            projectRoot,
            "..",
            "raw base assets",
            "The Game Creator's Pack",
            "The Game Creator's Pack",
            "Graphic Pack",
            "Player_DarkOutline.png"
        ));

        if (!File.Exists(filePath))
            return null;

        Image strip = Image.LoadFromFile(filePath);
        if (strip.IsEmpty())
            return null;

        strip.Convert(Image.Format.Rgba8);
        Rect2[] detectedFrames = DetectBlobFrames(strip);
        if (detectedFrames.Length == 0)
            return null;

        ImageTexture texture = ImageTexture.CreateFromImage(strip);
        SpriteFrame[] all = new SpriteFrame[detectedFrames.Length];
        for (int i = 0; i < detectedFrames.Length; i++)
            all[i] = new SpriteFrame(texture, detectedFrames[i]);

        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        frames[ActorMotionState.Idle] = SliceFrames(all, idle, idlePingPong);
        frames[ActorMotionState.Run] = SliceFrames(all, run, runPingPong);
        frames[ActorMotionState.Crawl] = frames[ActorMotionState.Run];
        frames[ActorMotionState.JumpUp] = SliceFrames(all, jumpUp, jumpUpPingPong);
        frames[ActorMotionState.JumpDown] = SliceFrames(all, jumpDown, jumpDownPingPong);

        return new SpriteAnimationSet(frames);
    }

    public static int GetGameCreatorPlayerFrameCount()
    {
        string filePath = GameCreatorPlayerPath();

        if (!File.Exists(filePath))
            return 0;

        Image strip = Image.LoadFromFile(filePath);
        if (strip.IsEmpty())
            return 0;

        strip.Convert(Image.Format.Rgba8);
        return DetectBlobFrames(strip).Length;
    }

    public static bool TryLoadGameCreatorPlayerFramePreview(out Texture2D? texture, out Rect2[] frames)
    {
        texture = null;
        frames = [];
        string filePath = GameCreatorPlayerPath();

        if (!File.Exists(filePath))
            return false;

        Image strip = Image.LoadFromFile(filePath);
        if (strip.IsEmpty())
            return false;

        strip.Convert(Image.Format.Rgba8);
        frames = DetectBlobFrames(strip);
        if (frames.Length == 0)
            return false;

        texture = ImageTexture.CreateFromImage(strip);
        return true;
    }

    public SpriteFrame GetFrame(ActorMotionState state, double clock)
    {
        SpriteFrame[] frames = _frames.TryGetValue(state, out SpriteFrame[]? stateFrames)
            ? stateFrames
            : _frames[ActorMotionState.Idle];

        float framesPerSecond = state is ActorMotionState.Run or ActorMotionState.Crawl ? 12f : 6f;
        int index = Mathf.FloorToInt((float)clock * framesPerSecond) % frames.Length;
        return frames[index];
    }

    private static void AddFrames(
        Dictionary<ActorMotionState, SpriteFrame[]> frames,
        ActorMotionState state,
        string resourcePath
    )
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return;

        sheet.Convert(Image.Format.Rgba8);
        MakeNearWhiteTransparent(sheet);

        int frameSize = sheet.GetHeight();
        int frameCount = Mathf.Max(1, sheet.GetWidth() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        SpriteFrame[] loaded = new SpriteFrame[frameCount];

        for (int i = 0; i < frameCount; i++)
            loaded[i] = new SpriteFrame(texture, new Rect2(i * frameSize, 0, frameSize, frameSize));

        frames[state] = loaded;
    }

    private static string GameCreatorPlayerPath()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(
            projectRoot,
            "..",
            "raw base assets",
            "The Game Creator's Pack",
            "The Game Creator's Pack",
            "Graphic Pack",
            "Player_DarkOutline.png"
        ));
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, int start, int count)
    {
        if (frames.Length == 0)
            return [];

        start = Mathf.Clamp(start, 0, frames.Length - 1);
        count = Mathf.Clamp(count, 1, frames.Length - start);
        SpriteFrame[] slice = new SpriteFrame[count];
        Array.Copy(frames, start, slice, 0, count);
        return slice;
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, AnimationFrameRange range)
    {
        return SliceFrames(frames, range, false);
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, AnimationFrameRange range, bool pingPong)
    {
        int start = Mathf.Min(range.Start, range.End);
        int end = Mathf.Max(range.Start, range.End);
        SpriteFrame[] forward = SliceFrames(frames, start, end - start + 1);
        if (!pingPong || forward.Length <= 1)
            return forward;

        SpriteFrame[] expanded = new SpriteFrame[forward.Length * 2 - 1];
        Array.Copy(forward, expanded, forward.Length);
        for (int i = 1; i < forward.Length; i++)
            expanded[forward.Length + i - 1] = forward[forward.Length - 1 - i];

        return expanded;
    }

    private static Rect2[] DetectBlobFrames(Image image)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        bool[,] visited = new bool[width, height];
        List<DetectedFrame> frames = [];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (visited[x, y] || image.GetPixel(x, y).A <= 0.03f)
                    continue;

                DetectedFrame frame = FloodFrame(image, visited, x, y);
                if (frame.OpaquePixels < 180 || frame.Height < 24)
                    continue;

                frames.Add(frame.Grow(1, width, height));
            }
        }

        frames.Sort((a, b) =>
        {
            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        Rect2[] rects = new Rect2[frames.Count];
        for (int i = 0; i < frames.Count; i++)
            rects[i] = new Rect2(frames[i].X, frames[i].Y, frames[i].Width, frames[i].Height);

        return rects;
    }

    private static DetectedFrame FloodFrame(Image image, bool[,] visited, int startX, int startY)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        Stack<Vector2I> stack = new();
        stack.Push(new Vector2I(startX, startY));
        visited[startX, startY] = true;
        int minX = startX;
        int maxX = startX;
        int minY = startY;
        int maxY = startY;
        int opaquePixels = 0;

        while (stack.Count > 0)
        {
            Vector2I point = stack.Pop();
            opaquePixels++;
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);

            TryPush(image, visited, stack, point.X + 1, point.Y, width, height);
            TryPush(image, visited, stack, point.X - 1, point.Y, width, height);
            TryPush(image, visited, stack, point.X, point.Y + 1, width, height);
            TryPush(image, visited, stack, point.X, point.Y - 1, width, height);
        }

        return new DetectedFrame(minX, minY, maxX - minX + 1, maxY - minY + 1, opaquePixels);
    }

    private static void TryPush(Image image, bool[,] visited, Stack<Vector2I> stack, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x >= width || y >= height || visited[x, y])
            return;

        if (image.GetPixel(x, y).A <= 0.03f)
            return;

        visited[x, y] = true;
        stack.Push(new Vector2I(x, y));
    }

    private static void MakeNearWhiteTransparent(Image image)
    {
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.R > 0.95f && pixel.G > 0.95f && pixel.B > 0.95f)
                    image.SetPixel(x, y, Colors.Transparent);
            }
        }
    }

    private readonly record struct DetectedFrame(int X, int Y, int Width, int Height, int OpaquePixels)
    {
        public DetectedFrame Grow(int pixels, int maxWidth, int maxHeight)
        {
            int x = Math.Max(0, X - pixels);
            int y = Math.Max(0, Y - pixels);
            int endX = Math.Min(maxWidth, X + Width + pixels);
            int endY = Math.Min(maxHeight, Y + Height + pixels);
            return new DetectedFrame(x, y, endX - x, endY - y, OpaquePixels);
        }
    }
}
