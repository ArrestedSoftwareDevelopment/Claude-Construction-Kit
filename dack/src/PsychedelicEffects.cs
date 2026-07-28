using Godot;
using System.Collections.Generic;

namespace Dack;

public sealed class PsychedelicEffects
{
    private readonly List<IVisualEffect> _effects = [];
    private readonly RandomNumberGenerator _random = new();
    private const float EffectOpacity = 0.5f;
    private static readonly Color[] ArcadePalette =
    [
        new("#FF2BD6"),
        new("#5CB8FF"),
        new("#FFF0A8"),
        new("#5CB8A7"),
        new("#FF5C35"),
        new("#B6FF3B"),
        new("#8A5CFF"),
        new("#FF3B7A"),
        new("#38FFDA"),
        new("#FF8C00"),
        new("#E9FF3B"),
        new("#FFFFFF")
    ];

    public PsychedelicEffects()
    {
        _random.Randomize();
    }

    public void Clear()
    {
        _effects.Clear();
    }

    public void Update(float delta)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].Update(delta);
            if (!_effects[i].Alive)
                _effects.RemoveAt(i);
        }
    }

    public void Draw(Control canvas)
    {
        foreach (IVisualEffect effect in _effects)
            effect.Draw(canvas, EffectOpacity);
    }

    public bool HasActiveEffects => _effects.Count > 0;

    public void TextHit(Vector2 position, int points, bool wordTarget, string? targetText = null)
    {
        Color color = RandomArcadeColor();
        NeonText($"+{points}", position, color, (wordTarget ? 1.45f : 1.1f) * _random.RandfRange(0.85f, 1.85f), 1.35f);
        if (!string.IsNullOrWhiteSpace(targetText))
            NeonText(targetText, position + new Vector2(0, -20f), color, wordTarget ? 1.35f : 0.95f, 1.05f);

        ImpactBurst(position, color, wordTarget ? 1.6f : 1.15f);

        if (wordTarget)
            ExplodeWord(string.IsNullOrWhiteSpace(targetText) ? "TEXT" : targetText, position, color, 1.45f);
    }

    public void PaddleSpark(Vector2 position)
    {
        Color color = RandomArcadeColor();
        AddRing(position, 10f, 34f, color, 0.34f, 3f);
        AddStarburst(position, color, 10, 0.55f, 44f);
    }

    public void Multiball(Vector2 position)
    {
        Color color = RandomArcadeColor();
        NeonText("MULTIBALL", position + new Vector2(0, -18), color, _random.RandfRange(1.75f, 3.1f), 1.9f);
        AddRing(position, 12f, 92f, color, 0.85f, 5f);
        AddRing(position, 2f, 124f, color, 1.1f, 2.5f);
        AddStarburst(position, color, 28, 1.25f, 120f);
        ExplodeWord("MULTI", position, color, 1.7f);
    }

    public void LaserArmed(Vector2 position, int strength)
    {
        Color color = RandomArcadeColor();
        NeonText($"LASER {strength * 10}%", position + new Vector2(0, -18), color, 1.75f, 1.8f);
        AddRing(position, 8f, 78f, color, 0.75f, 4f);
        AddStarburst(position, color, 18, 0.8f, 76f);
    }

    public void LaserColumn(Rect2 beam, Vector2 captionPosition, int strength, int destroyed, int points)
    {
        _effects.Add(new BeamEffect(beam, strength));
        Color color = RandomArcadeColor();
        string text = destroyed > 0 ? $"COLUMN BURN +{points}" : $"LASER {strength * 10}% MISS";
        NeonText(text, captionPosition, color, destroyed > 0 ? _random.RandfRange(1.95f, 3.25f) : _random.RandfRange(1.35f, 2.35f), 1.65f);
        AddStarburst(captionPosition, color, destroyed > 0 ? 34 : 14, destroyed > 0 ? 1.3f : 0.65f, destroyed > 0 ? 140f : 70f);
        if (destroyed > 0)
            ExplodeWord("BURN", captionPosition, color, 1.8f);
    }

    public void RoundBanner(Vector2 center, string message, Color color)
    {
        Color eventColor = RandomArcadeColor();
        NeonText(message, center, eventColor, 2.5f, 2.4f);
        NeonText("PRESS RESET FOR NEW GAME", center + new Vector2(0, 44f), eventColor, 1.35f, 2.2f);
        AddRing(center, 20f, 180f, eventColor, 1.5f, 6f);
        AddRing(center, 38f, 260f, eventColor, 1.9f, 2.5f);
        AddStarburst(center, eventColor, 48, 1.7f, 210f);
    }

    public void NeonText(string text, Vector2 position, Color color, float scale, float life)
    {
        Vector2 launchDirection = Vector2.FromAngle(_random.RandfRange(-Mathf.Pi, 0f));
        float launchSpeed = _random.RandfRange(70f, 260f);
        _effects.Add(new NeonTextEffect(
            text,
            position,
            color,
            life,
            scale,
            launchDirection * launchSpeed + new Vector2(_random.RandfRange(-80f, 80f), _random.RandfRange(-70f, 20f)),
            _random.RandfRange(-1.25f, 1.25f),
            _random.RandfRange(-5.5f, 5.5f)
        ));
    }

    public void ExplodeWord(string word, Vector2 position, Color color, float intensity)
    {
        string cleanWord = string.IsNullOrWhiteSpace(word) ? "TEXT" : word.Trim().ToUpperInvariant();
        if (cleanWord.Length > 12)
            cleanWord = cleanWord[..12];

        float letterSpacing = _random.RandfRange(14f, 26f) * intensity;
        float centerOffset = (cleanWord.Length - 1) * 0.5f;
        AddRing(position, 8f, _random.RandfRange(110f, 230f) * intensity, color, 1.25f, 4f);

        for (int i = 0; i < cleanWord.Length; i++)
        {
            char character = cleanWord[i];
            if (char.IsWhiteSpace(character))
                continue;

            Vector2 start = position + new Vector2((i - centerOffset) * letterSpacing, _random.RandfRange(-5f, 5f));
            float baseAngle = -Mathf.Pi / 2f + (i - centerOffset) * 0.28f + _random.RandfRange(-1.1f, 1.1f);
            float distance = _random.RandfRange(130f, 360f) * intensity;
            Vector2 end = start + Vector2.FromAngle(baseAngle) * distance;
            Vector2 chord = end - start;
            Vector2 normal = new(-chord.Y, chord.X);
            if (normal.LengthSquared() > 0.01f)
                normal = normal.Normalized();

            Vector2 control = start + chord * _random.RandfRange(0.24f, 0.72f) + normal * _random.RandfRange(-180f, 180f) * intensity;
            _effects.Add(new LetterShardEffect(
                character.ToString(),
                start,
                control,
                end,
                color,
                _random.RandfRange(1.0f, 2.1f),
                _random.RandfRange(1.15f, 3.2f) * intensity,
                _random.RandfRange(-Mathf.Pi, Mathf.Pi),
                _random.RandfRange(-9.5f, 9.5f)
            ));
        }
    }

    public void ImpactBurst(Vector2 position, Color color, float intensity)
    {
        AddRing(position, 5f, _random.RandfRange(46f, 90f) * intensity, color, 0.5f, 3.5f);
        AddRing(position, 12f, _random.RandfRange(86f, 180f) * intensity, color.Lightened(0.35f), 0.72f, 1.8f);
        AddStarburst(position, color, Mathf.RoundToInt(_random.RandfRange(16, 32) * intensity), 0.68f, _random.RandfRange(94f, 220f) * intensity);

        int sparks = Mathf.RoundToInt(_random.RandfRange(20, 42) * intensity);
        for (int i = 0; i < sparks; i++)
        {
            float angle = _random.RandfRange(0f, Mathf.Tau);
            float speed = _random.RandfRange(95f, 390f) * intensity;
            _effects.Add(new SparkEffect(
                position,
                Vector2.FromAngle(angle) * speed,
                color,
                _random.RandfRange(0.32f, 1.25f),
                _random.RandfRange(2.4f, 8.8f)
            ));
        }
    }

    private Color RandomArcadeColor()
    {
        Color color = ArcadePalette[_random.RandiRange(0, ArcadePalette.Length - 1)];
        return color.Lerp(new Color("#FFFFFF"), _random.RandfRange(0f, 0.18f));
    }

    private void AddRing(Vector2 position, float startRadius, float endRadius, Color color, float life, float width)
    {
        _effects.Add(new RingEffect(position, startRadius, endRadius, color, life, width));
    }

    private void AddStarburst(Vector2 position, Color color, int rays, float life, float length)
    {
        _effects.Add(new StarburstEffect(position, color, rays, life, length, _random.RandfRange(0f, Mathf.Tau)));
    }

    private interface IVisualEffect
    {
        bool Alive { get; }
        void Update(float delta);
        void Draw(Control canvas, float opacity);
    }

    private sealed class NeonTextEffect : IVisualEffect
    {
        private readonly string _text;
        private readonly Color _color;
        private readonly float _life;
        private readonly Vector2 _velocity;
        private readonly float _spin;
        private Vector2 _position;
        private float _age;
        private float _scale;
        private float _rotation;

        public NeonTextEffect(string text, Vector2 position, Color color, float life, float scale, Vector2 velocity, float rotation, float spin)
        {
            _text = text;
            _position = position;
            _color = color;
            _life = life;
            _scale = scale;
            _velocity = velocity;
            _rotation = rotation;
            _spin = spin;
        }

        public bool Alive => _age < _life;

        public void Update(float delta)
        {
            _age += delta;
            _position += _velocity * delta;
            _rotation += _spin * delta;
            _scale += delta * 0.42f;
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / _life, 0f, 1f);
            float alpha = (1f - t) * opacity;
            float strobe = 0.55f + 0.45f * Mathf.Pow(Mathf.Sin(_age * 28f), 2f);
            Color hot = _color.Lightened(0.25f + 0.35f * strobe) with { A = alpha };
            Color glow = _color with { A = alpha * 0.28f };
            Color dark = new("#101820", alpha * 0.78f);

            canvas.DrawSetTransform(_position, _rotation, new Vector2(_scale, _scale));
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-80, 7), _text, HorizontalAlignment.Center, 160, 18, dark);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-84, 2), _text, HorizontalAlignment.Center, 168, 18, glow);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-76, -2), _text, HorizontalAlignment.Center, 152, 18, glow);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-80, 0), _text, HorizontalAlignment.Center, 160, 18, hot);
            canvas.DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        }
    }

    private sealed class RingEffect : IVisualEffect
    {
        private readonly Vector2 _position;
        private readonly float _startRadius;
        private readonly float _endRadius;
        private readonly Color _color;
        private readonly float _life;
        private readonly float _width;
        private float _age;

        public RingEffect(Vector2 position, float startRadius, float endRadius, Color color, float life, float width)
        {
            _position = position;
            _startRadius = startRadius;
            _endRadius = endRadius;
            _color = color;
            _life = life;
            _width = width;
        }

        public bool Alive => _age < _life;

        public void Update(float delta)
        {
            _age += delta;
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / _life, 0f, 1f);
            float radius = Mathf.Lerp(_startRadius, _endRadius, t);
            Color color = _color with { A = (1f - t) * 0.85f * opacity };
            canvas.DrawArc(_position, radius, 0f, Mathf.Tau, 96, color, _width * (1f - t * 0.45f));
            canvas.DrawArc(_position, radius * 0.72f, Mathf.Pi * 0.14f, Mathf.Tau * 0.86f, 72, color.Lightened(0.35f) with { A = color.A * 0.62f }, Mathf.Max(1f, _width * 0.45f));
        }
    }

    private sealed class StarburstEffect : IVisualEffect
    {
        private readonly Vector2 _position;
        private readonly Color _color;
        private readonly int _rays;
        private readonly float _life;
        private readonly float _length;
        private readonly float _phase;
        private float _age;

        public StarburstEffect(Vector2 position, Color color, int rays, float life, float length, float phase)
        {
            _position = position;
            _color = color;
            _rays = Mathf.Max(3, rays);
            _life = life;
            _length = length;
            _phase = phase;
        }

        public bool Alive => _age < _life;

        public void Update(float delta)
        {
            _age += delta;
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / _life, 0f, 1f);
            float alpha = (1f - t) * opacity;
            float spin = _phase + _age * 4.3f;
            for (int i = 0; i < _rays; i++)
            {
                float wobble = Mathf.Sin(_age * 17f + i * 0.71f) * 0.13f;
                Vector2 direction = Vector2.FromAngle(spin + Mathf.Tau * i / _rays + wobble);
                float inner = Mathf.Lerp(2f, _length * 0.18f, t);
                float outer = Mathf.Lerp(_length * 0.25f, _length, t);
                Color color = (i % 2 == 0 ? _color : _color.Lightened(0.42f)) with { A = alpha * 0.72f };
                canvas.DrawLine(_position + direction * inner, _position + direction * outer, color, i % 3 == 0 ? 3f : 1.5f);
            }
        }
    }

    private sealed class SparkEffect : IVisualEffect
    {
        private readonly Color _color;
        private readonly float _life;
        private readonly float _radius;
        private Vector2 _position;
        private Vector2 _velocity;
        private float _age;

        public SparkEffect(Vector2 position, Vector2 velocity, Color color, float life, float radius)
        {
            _position = position;
            _velocity = velocity;
            _color = color;
            _life = life;
            _radius = radius;
        }

        public bool Alive => _age < _life;

        public void Update(float delta)
        {
            _age += delta;
            _position += _velocity * delta;
            _velocity = _velocity.MoveToward(Vector2.Zero, 110f * delta);
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / _life, 0f, 1f);
            Color color = _color with { A = (1f - t) * opacity };
            canvas.DrawCircle(_position, _radius * (1f - t * 0.55f), color);
            if (_velocity.LengthSquared() > 0.01f)
                canvas.DrawLine(_position, _position - _velocity.Normalized() * _radius * 4f, color with { A = color.A * 0.6f }, Mathf.Max(1f, _radius * 0.45f));
        }
    }

    private sealed class LetterShardEffect : IVisualEffect
    {
        private readonly string _letter;
        private readonly Vector2 _start;
        private readonly Vector2 _control;
        private readonly Vector2 _end;
        private readonly Color _color;
        private readonly float _life;
        private readonly float _baseScale;
        private readonly float _spin;
        private float _age;
        private float _rotation;

        public LetterShardEffect(string letter, Vector2 start, Vector2 control, Vector2 end, Color color, float life, float baseScale, float rotation, float spin)
        {
            _letter = letter;
            _start = start;
            _control = control;
            _end = end;
            _color = color;
            _life = life;
            _baseScale = baseScale;
            _rotation = rotation;
            _spin = spin;
        }

        public bool Alive => _age < _life;

        public void Update(float delta)
        {
            _age += delta;
            _rotation += _spin * delta;
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / _life, 0f, 1f);
            float eased = 1f - Mathf.Pow(1f - t, 2.35f);
            Vector2 position = Quadratic(_start, _control, _end, eased);
            Vector2 tangent = Quadratic(_start, _control, _end, Mathf.Clamp(eased + 0.01f, 0f, 1f)) - position;
            float tangentAngle = tangent.LengthSquared() > 0.01f ? tangent.Angle() : 0f;
            float alpha = (1f - t) * opacity;
            float pulse = 0.72f + 0.42f * Mathf.Sin(_age * 31f) * Mathf.Sin(_age * 31f);
            float scale = _baseScale * (1f + t * 0.9f) * pulse;
            Color glow = _color with { A = alpha * 0.34f };
            Color hot = _color.Lightened(0.45f) with { A = alpha };
            Color dark = new("#101820", alpha * 0.8f);

            canvas.DrawSetTransform(position, _rotation + tangentAngle * 0.32f, new Vector2(scale, scale));
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-16, 7), _letter, HorizontalAlignment.Center, 32, 24, dark);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-18, 2), _letter, HorizontalAlignment.Center, 36, 24, glow);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-14, -2), _letter, HorizontalAlignment.Center, 28, 24, glow);
            canvas.DrawString(ThemeDB.FallbackFont, new Vector2(-16, 0), _letter, HorizontalAlignment.Center, 32, 24, hot);
            canvas.DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        }

        private static Vector2 Quadratic(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float inv = 1f - t;
            return a * inv * inv + b * 2f * inv * t + c * t * t;
        }
    }

    private sealed class BeamEffect : IVisualEffect
    {
        private readonly Rect2 _beam;
        private readonly int _strength;
        private float _age;
        private const float Life = 0.56f;

        public BeamEffect(Rect2 beam, int strength)
        {
            _beam = beam;
            _strength = strength;
        }

        public bool Alive => _age < Life;

        public void Update(float delta)
        {
            _age += delta;
        }

        public void Draw(Control canvas, float opacity)
        {
            float t = Mathf.Clamp(_age / Life, 0f, 1f);
            float pulse = Mathf.Pow(Mathf.Sin(_age * 48f), 2f);
            float alpha = (1f - t) * opacity;
            float scale = Mathf.Lerp(1.4f, 0.15f, t);
            Color magenta = new("#FF2BD6", alpha * (0.25f + 0.45f * pulse));
            Color cyan = new("#5CB8FF", alpha * 0.24f);
            Color white = new("#FFF0A8", alpha * 0.92f);

            canvas.DrawRect(_beam.Grow(10f + _strength * 1.4f * scale), cyan, true);
            canvas.DrawRect(_beam.Grow(5f + _strength * 0.65f * scale), magenta, true);
            canvas.DrawRect(_beam.Grow(1.5f), white, true);
        }
    }
}
