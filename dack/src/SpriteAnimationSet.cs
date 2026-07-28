using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record SpriteFrame(Texture2D Texture, Rect2 SourceRegion, Vector2 DisplaySize);
public readonly record struct AnimationFrameRange(int Start, int End);

public sealed class SpriteAnimationSet
{
    private const string AssetRoot = "res://assets/third_party/stickman-pack-v0.1";
    private const string SunnyDragonRelativePath = "raw base assets/Legacy Collection/Legacy Collection/Assets/Misc/Characters/sunny-dragon/spritesheets/sunny-dragon-fly.png";
    private const string TgcPlatformerRuntimePath = "res://assets/project/game-creators-pack/platformer-spritesheet.png";
    private const string TgcShooterBossRuntimePath = "res://assets/project/game-creators-pack/shooter-boss-sprite.png";
    private const string TgcShooterRuntimePath = "res://assets/project/game-creators-pack/shooter-spritesheet.png";
    private static readonly AnimationFrameRange DefaultGameCreatorIdle = new(0, 2);
    private static readonly AnimationFrameRange DefaultGameCreatorRun = new(3, 14);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpUp = new(15, 15);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpDown = new(16, 16);
    private static readonly AnimationFrameRange DefaultGameCreatorFall = new(16, 16);
    private static readonly AnimationFrameRange DefaultGameCreatorRunShoot = new(9, 10);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpShoot = new(11, 12);
    private static readonly AnimationFrameRange DefaultGameCreatorDeath = new(16, 17);

    private readonly Dictionary<ActorMotionState, SpriteFrame[]> _frames;

    private SpriteAnimationSet(Dictionary<ActorMotionState, SpriteFrame[]> frames)
    {
        _frames = frames;
    }

    public static SpriteAnimationSet? TryLoadStickman()
    {
        if (!TryLoadStickmanFrames(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(6, 14),
            new AnimationFrameRange(15, 15),
            new AnimationFrameRange(16, 16),
            new AnimationFrameRange(16, 16),
            new AnimationFrameRange(6, 14),
            new AnimationFrameRange(15, 15),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadStickman(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadStickmanFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                idle,
                run,
                jumpUp,
                jumpDown,
                fall,
                runShoot,
                jumpShoot,
                death,
                idlePingPong,
                runPingPong,
                jumpUpPingPong,
                jumpDownPingPong,
                fallPingPong,
                runShootPingPong,
                jumpShootPingPong,
                deathPingPong
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer()
    {
        return TryLoadGameCreatorPlayer(
            DefaultGameCreatorIdle,
            DefaultGameCreatorRun,
            DefaultGameCreatorJumpUp,
            DefaultGameCreatorJumpDown,
            DefaultGameCreatorFall,
            DefaultGameCreatorRunShoot,
            DefaultGameCreatorJumpShoot,
            DefaultGameCreatorDeath,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcPlatformerEnemy(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange crawl
    )
    {
        if (!TryLoadBlobFrames(TgcPlatformerRuntimePath, out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            idle,
            idle,
            idle,
            run,
            idle,
            idle,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        ).WithCrawl(all, crawl);
    }

    public static SpriteAnimationSet? TryLoadTgcShooterBoss()
    {
        return TryLoadSingleSprite(TgcShooterBossRuntimePath);
    }

    public static SpriteAnimationSet? TryLoadTgcShooterFleet()
    {
        return TryLoadBlobFrames(TgcShooterRuntimePath, out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadSunnyDragon()
    {
        return TryLoadSunnyDragonFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadSunnyDragon(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadSunnyDragonFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                idle,
                run,
                jumpUp,
                jumpDown,
                fall,
                runShoot,
                jumpShoot,
                death,
                idlePingPong,
                runPingPong,
                jumpUpPingPong,
                jumpDownPingPong,
                fallPingPong,
                runShootPingPong,
                jumpShootPingPong,
                deathPingPong
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown
    )
    {
        return TryLoadGameCreatorPlayer(
            idle,
            run,
            jumpUp,
            jumpDown,
            DefaultGameCreatorFall,
            DefaultGameCreatorRunShoot,
            DefaultGameCreatorJumpShoot,
            DefaultGameCreatorDeath,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        if (!TryLoadGameCreatorPlayerFrames(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    private static SpriteAnimationSet BuildAnimationSetFromFrames(
        SpriteFrame[] all,
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        frames[ActorMotionState.Idle] = SliceFrames(all, idle, idlePingPong);
        frames[ActorMotionState.Run] = SliceFrames(all, run, runPingPong);
        frames[ActorMotionState.Crawl] = frames[ActorMotionState.Run];
        frames[ActorMotionState.JumpUp] = SliceFrames(all, jumpUp, jumpUpPingPong);
        frames[ActorMotionState.JumpDown] = SliceFrames(all, jumpDown, jumpDownPingPong);
        frames[ActorMotionState.Fall] = SliceFrames(all, fall, fallPingPong);
        frames[ActorMotionState.RunShoot] = SliceFrames(all, runShoot, runShootPingPong);
        frames[ActorMotionState.JumpShoot] = SliceFrames(all, jumpShoot, jumpShootPingPong);
        frames[ActorMotionState.Death] = SliceFrames(all, death, deathPingPong);

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

    public static bool TryLoadGameCreatorPlayerFrames(out SpriteFrame[] frames)
    {
        frames = [];
        if (!TryLoadGameCreatorPlayerFramePreview(out Texture2D? texture, out Rect2[] rects) || texture is null)
            return false;

        Vector2 displaySize = GetCommonDisplaySize(rects);
        frames = new SpriteFrame[rects.Length];
        for (int i = 0; i < rects.Length; i++)
            frames[i] = new SpriteFrame(texture, rects[i], displaySize);

        return frames.Length > 0;
    }

    public static bool TryLoadTgcPlatformerFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobFrames(TgcPlatformerRuntimePath, out frames);
    }

    public static bool TryLoadStickmanFrames(out SpriteFrame[] frames)
    {
        List<SpriteFrame> loaded = [];
        AppendFrames(loaded, $"{AssetRoot}/thin-idle-sheet.png");
        AppendFrames(loaded, $"{AssetRoot}/thin-run-sheet.png");
        AppendFrames(loaded, $"{AssetRoot}/thin-jump-up.png");
        AppendFrames(loaded, $"{AssetRoot}/thin-jump-down.png");
        frames = loaded.ToArray();
        return frames.Length > 0;
    }

    public static bool TryLoadSunnyDragonFrames(out SpriteFrame[] frames)
    {
        frames = [];
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string filePath = Path.GetFullPath(Path.Combine(projectRoot, "..", SunnyDragonRelativePath));
        if (!File.Exists(filePath))
            return false;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return false;

        sheet.Convert(Image.Format.Rgba8);
        int frameCount = 9;
        int frameWidth = sheet.GetWidth() / frameCount;
        int frameHeight = sheet.GetHeight();
        if (frameWidth <= 0 || frameHeight <= 0)
            return false;

        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        frames = new SpriteFrame[frameCount];
        Vector2 displaySize = new(frameWidth, frameHeight);
        for (int i = 0; i < frameCount; i++)
            frames[i] = new SpriteFrame(texture, new Rect2(i * frameWidth, 0, frameWidth, frameHeight), displaySize);

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

    private SpriteAnimationSet WithCrawl(SpriteFrame[] all, AnimationFrameRange crawl)
    {
        _frames[ActorMotionState.Crawl] = SliceFrames(all, crawl);
        return this;
    }

    private static SpriteAnimationSet? TryLoadSingleSprite(string resourcePath)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return null;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return null;

        image.Convert(Image.Format.Rgba8);
        ImageTexture texture = ImageTexture.CreateFromImage(image);
        SpriteFrame frame = new(texture, new Rect2(0, 0, image.GetWidth(), image.GetHeight()), new Vector2(image.GetWidth(), image.GetHeight()));
        SpriteFrame[] frames = [frame];
        Dictionary<ActorMotionState, SpriteFrame[]> states = [];
        foreach (ActorMotionState state in Enum.GetValues<ActorMotionState>())
            states[state] = frames;

        return new SpriteAnimationSet(states);
    }

    private static bool TryLoadBlobFrames(string resourcePath, out SpriteFrame[] frames)
    {
        frames = [];
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        Rect2[] rects = DetectBlobFrames(image);
        if (rects.Length == 0)
            return false;

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        Vector2 displaySize = GetCommonDisplaySize(rects);
        frames = new SpriteFrame[rects.Length];
        for (int i = 0; i < rects.Length; i++)
            frames[i] = new SpriteFrame(texture, rects[i], displaySize);

        return true;
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
        ThickenOpaquePixels(sheet, 1);

        int frameSize = sheet.GetHeight();
        int frameCount = Mathf.Max(1, sheet.GetWidth() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        SpriteFrame[] loaded = new SpriteFrame[frameCount];

        for (int i = 0; i < frameCount; i++)
            loaded[i] = new SpriteFrame(texture, new Rect2(i * frameSize, 0, frameSize, frameSize), new Vector2(frameSize, frameSize));

        frames[state] = loaded;
    }

    private static void AppendFrames(List<SpriteFrame> frames, string resourcePath)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return;

        sheet.Convert(Image.Format.Rgba8);
        MakeNearWhiteTransparent(sheet);
        ThickenOpaquePixels(sheet, 1);

        int frameSize = sheet.GetHeight();
        int frameCount = Mathf.Max(1, sheet.GetWidth() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);

        for (int i = 0; i < frameCount; i++)
            frames.Add(new SpriteFrame(texture, new Rect2(i * frameSize, 0, frameSize, frameSize), new Vector2(frameSize, frameSize)));
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

    private static Vector2 GetCommonDisplaySize(Rect2[] frames)
    {
        float maxWidth = 1f;
        float maxHeight = 1f;
        foreach (Rect2 frame in frames)
        {
            maxWidth = Mathf.Max(maxWidth, frame.Size.X);
            maxHeight = Mathf.Max(maxHeight, frame.Size.Y);
        }

        return new Vector2(maxWidth, maxHeight);
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

        bool likelyHorizontalStrip = width > height * 4;
        frames.Sort((a, b) =>
        {
            if (likelyHorizontalStrip)
            {
                int xCompare = a.X.CompareTo(b.X);
                return xCompare != 0 ? xCompare : a.Y.CompareTo(b.Y);
            }

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

    private static void ThickenOpaquePixels(Image image, int radius)
    {
        if (radius <= 0)
            return;

        int width = image.GetWidth();
        int height = image.GetHeight();
        Image source = (Image)image.Duplicate();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = source.GetPixel(x, y);
                if (pixel.A <= 0.03f)
                    continue;

                for (int oy = -radius; oy <= radius; oy++)
                {
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        int targetX = x + ox;
                        int targetY = y + oy;
                        if (targetX < 0 || targetY < 0 || targetX >= width || targetY >= height)
                            continue;

                        Color target = image.GetPixel(targetX, targetY);
                        if (target.A < pixel.A)
                            image.SetPixel(targetX, targetY, pixel);
                    }
                }
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
