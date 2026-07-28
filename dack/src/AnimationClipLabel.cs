using Godot;

namespace Dack;

public sealed record AnimationClipLabel(
    string Name,
    AnimationFrameRange Range,
    Color Color,
    bool PingPong = false,
    bool Strobe = false,
    int StrobeCount = 0
);
