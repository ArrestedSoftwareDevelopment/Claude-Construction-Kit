using Godot;
using System.Collections.Generic;

namespace Dack;

public partial class CombatFxOverlay : Control
{
    private readonly Texture2D? _fireballExplosion = GD.Load<Texture2D>("res://assets/project/effects/fireball-impact-explosion.png");
    private EffectVisual[] _impactEffects = [];

    public void SetImpactEffects(IReadOnlyList<EffectVisual> effects)
    {
        if (effects.Count == 0)
        {
            _impactEffects = [];
            QueueRedraw();
            return;
        }

        EffectVisual[] copy = new EffectVisual[effects.Count];
        for (int i = 0; i < effects.Count; i++)
            copy[i] = effects[i];

        _impactEffects = copy;
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (EffectVisual effect in _impactEffects)
            DrawProjectileFrame(effect.Position, effect.Frame, 1.08f);
    }

    private void DrawProjectileFrame(Vector2 position, int frame, float scale)
    {
        if (_fireballExplosion is null)
        {
            DrawCircle(position, 15f, new Color("#FF2BD6", 0.72f));
            DrawCircle(position, 6f, new Color("#FFF0A8", 0.9f));
            return;
        }

        const int frameWidth = 80;
        const int frameHeight = 48;
        frame = Mathf.Clamp(frame, 0, 12);
        Rect2 source = new(frame * frameWidth, 0, frameWidth, frameHeight);
        Vector2 size = new Vector2(frameWidth, frameHeight) * scale;
        DrawTextureRectRegion(_fireballExplosion, new Rect2(position - size * 0.5f, size), source);
    }
}
