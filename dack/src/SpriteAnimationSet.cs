using Godot;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record SpriteFrame(Texture2D Texture, Rect2 SourceRegion);

public sealed class SpriteAnimationSet
{
    private const string AssetRoot = "res://assets/third_party/stickman-pack-v0.1";

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
}
