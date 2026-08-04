using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dack;

public enum SoundVariantSelectionMode
{
    Fixed,
    Sequential,
    RandomNoRepeat,
    Shuffle
}

public enum SoundVoiceOverflowPolicy
{
    DropNewest,
    RestartOldest
}

public sealed record SoundVariantDefinition(
    string ResourcePath,
    string DisplayName
);

public sealed record SoundCardDefinition(
    string Id,
    string DisplayName,
    string Family,
    string Description,
    IReadOnlyList<SoundVariantDefinition> Variants
)
{
    public IReadOnlyList<string> Tags { get; init; } = [];
    public SoundVariantSelectionMode SelectionMode { get; init; } = SoundVariantSelectionMode.RandomNoRepeat;
    public SoundVoiceOverflowPolicy OverflowPolicy { get; init; } = SoundVoiceOverflowPolicy.RestartOldest;
    public float VolumeDb { get; init; } = -10f;
    public float PitchMin { get; init; } = 1f;
    public float PitchMax { get; init; } = 1f;
    public float CooldownSeconds { get; init; }
    public int MaxVoices { get; init; } = 2;
    public bool Loop { get; init; }
    public string Bus { get; init; } = "Master";
    public string ProvenanceId { get; init; } = "kenney-all-in-one-3.6.0";
}

public sealed partial class SoundCardPlayer : Node
{
    private sealed record RuntimeVariant(SoundVariantDefinition Definition, AudioStream Stream);

    private sealed class Voice
    {
        public required AudioStreamPlayer Player { get; init; }
        public ulong StartedAtMilliseconds { get; set; }
    }

    private sealed class RuntimeCard
    {
        public required SoundCardDefinition Definition { get; init; }
        public required List<RuntimeVariant> Variants { get; init; }
        public List<Voice> Voices { get; } = [];
        public List<int> ShuffleBag { get; } = [];
        public int SequentialIndex { get; set; }
        public int LastVariantIndex { get; set; } = -1;
        public ulong LastStartedAtMilliseconds { get; set; }
    }

    private readonly Dictionary<string, RuntimeCard> _cards = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SoundCardDefinition> _definitions = [];
    private readonly RandomNumberGenerator _random = new();

    public IReadOnlyList<SoundCardDefinition> Cards => _definitions;

    public override void _Ready()
    {
        _random.Randomize();
    }

    public int RegisterCard(SoundCardDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Id))
            throw new ArgumentException("Sound Card ID cannot be empty.", nameof(definition));

        if (_cards.TryGetValue(definition.Id, out RuntimeCard? existing))
        {
            foreach (Voice voice in existing.Voices)
                voice.Player.QueueFree();
            _definitions.RemoveAll(card => card.Id.Equals(definition.Id, StringComparison.OrdinalIgnoreCase));
        }

        List<RuntimeVariant> variants = [];
        foreach (SoundVariantDefinition variant in definition.Variants)
        {
            AudioStream? stream = GD.Load<AudioStream>(variant.ResourcePath);
            if (stream is null)
                continue;

            if (definition.Loop && stream is AudioStreamOggVorbis ogg)
            {
                if (ogg.Duplicate() is AudioStreamOggVorbis looped)
                {
                    looped.Loop = true;
                    stream = looped;
                }
            }

            variants.Add(new RuntimeVariant(variant, stream));
        }

        RuntimeCard runtime = new()
        {
            Definition = definition with
            {
                MaxVoices = Math.Clamp(definition.MaxVoices, 1, 8),
                PitchMin = Mathf.Clamp(Mathf.Min(definition.PitchMin, definition.PitchMax), 0.25f, 4f),
                PitchMax = Mathf.Clamp(Mathf.Max(definition.PitchMin, definition.PitchMax), 0.25f, 4f),
                CooldownSeconds = Mathf.Max(0f, definition.CooldownSeconds)
            },
            Variants = variants
        };

        _cards[definition.Id] = runtime;
        _definitions.Add(runtime.Definition);
        return variants.Count;
    }

    public bool TryPlayCard(string cardId, out string variantDisplayName, int? explicitVariantIndex = null)
    {
        variantDisplayName = "Unavailable";
        if (!_cards.TryGetValue(cardId, out RuntimeCard? card) || card.Variants.Count == 0)
            return false;

        ulong now = Time.GetTicksMsec();
        ulong cooldownMilliseconds = (ulong)Mathf.RoundToInt(card.Definition.CooldownSeconds * 1000f);
        if (cooldownMilliseconds > 0
            && card.LastStartedAtMilliseconds > 0
            && now - card.LastStartedAtMilliseconds < cooldownMilliseconds)
        {
            variantDisplayName = "Cooling down";
            return false;
        }

        int variantIndex = explicitVariantIndex is int requested
            ? Mathf.PosMod(requested, card.Variants.Count)
            : SelectVariant(card);
        RuntimeVariant variant = card.Variants[variantIndex];

        Voice? voice = card.Voices.FirstOrDefault(candidate => !candidate.Player.Playing);
        if (voice is null && card.Voices.Count < card.Definition.MaxVoices)
        {
            AudioStreamPlayer player = new()
            {
                Name = $"{SanitizeNodeName(card.Definition.Id)}Voice{card.Voices.Count + 1}"
            };
            AddChild(player);
            voice = new Voice { Player = player };
            card.Voices.Add(voice);
        }

        if (voice is null)
        {
            if (card.Definition.OverflowPolicy == SoundVoiceOverflowPolicy.DropNewest)
            {
                variantDisplayName = "Voice limit reached";
                return false;
            }

            voice = card.Voices.OrderBy(candidate => candidate.StartedAtMilliseconds).First();
            voice.Player.Stop();
        }

        voice.Player.Stream = variant.Stream;
        voice.Player.VolumeDb = card.Definition.VolumeDb;
        voice.Player.PitchScale = Mathf.IsEqualApprox(card.Definition.PitchMin, card.Definition.PitchMax)
            ? card.Definition.PitchMin
            : _random.RandfRange(card.Definition.PitchMin, card.Definition.PitchMax);
        voice.Player.Bus = card.Definition.Bus;
        voice.Player.Play();
        voice.StartedAtMilliseconds = now;
        card.LastStartedAtMilliseconds = now;
        card.LastVariantIndex = variantIndex;
        variantDisplayName = variant.Definition.DisplayName;
        return true;
    }

    public void StopCard(string cardId)
    {
        if (!_cards.TryGetValue(cardId, out RuntimeCard? card))
            return;

        foreach (Voice voice in card.Voices)
            voice.Player.Stop();
    }

    public void StopAll()
    {
        foreach (RuntimeCard card in _cards.Values)
        {
            foreach (Voice voice in card.Voices)
                voice.Player.Stop();
        }
    }

    public int AvailableVariantCount(string cardId)
    {
        return _cards.TryGetValue(cardId, out RuntimeCard? card) ? card.Variants.Count : 0;
    }

    private int SelectVariant(RuntimeCard card)
    {
        int count = card.Variants.Count;
        if (count <= 1)
            return 0;

        switch (card.Definition.SelectionMode)
        {
            case SoundVariantSelectionMode.Fixed:
                return 0;
            case SoundVariantSelectionMode.Sequential:
            {
                int index = card.SequentialIndex % count;
                card.SequentialIndex = (card.SequentialIndex + 1) % count;
                return index;
            }
            case SoundVariantSelectionMode.Shuffle:
            {
                if (card.ShuffleBag.Count == 0)
                {
                    for (int i = 0; i < count; i++)
                        card.ShuffleBag.Add(i);
                    for (int i = card.ShuffleBag.Count - 1; i > 0; i--)
                    {
                        int swap = _random.RandiRange(0, i);
                        (card.ShuffleBag[i], card.ShuffleBag[swap]) = (card.ShuffleBag[swap], card.ShuffleBag[i]);
                    }
                }

                int selected = card.ShuffleBag[^1];
                card.ShuffleBag.RemoveAt(card.ShuffleBag.Count - 1);
                return selected;
            }
            default:
            {
                int selected;
                do
                {
                    selected = _random.RandiRange(0, count - 1);
                } while (selected == card.LastVariantIndex);
                return selected;
            }
        }
    }

    private static string SanitizeNodeName(string value)
    {
        char[] safe = value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray();
        return new string(safe);
    }
}
