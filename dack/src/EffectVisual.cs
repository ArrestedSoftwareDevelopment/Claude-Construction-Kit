using Godot;

namespace Dack;

public enum EffectVisualKind
{
    FireballImpact,
    LegacyEnemyDeath
}

public readonly record struct EffectVisual(Vector2 Position, int Frame, EffectVisualKind Kind)
{
    public EffectVisual(Vector2 position, int frame)
        : this(position, frame, EffectVisualKind.FireballImpact)
    {
    }
}
