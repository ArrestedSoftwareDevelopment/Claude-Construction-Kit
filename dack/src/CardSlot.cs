using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace Dack;

/// <summary>
/// A character/object binding target. Compatible cards can be dropped here;
/// the catalog definition remains separate from the selected actor instance.
/// </summary>
public partial class CardSlot : PanelContainer
{
    private readonly string[] _acceptedKinds;
    private readonly Label _currentLabel;

    public event Action<CardSlot>? Activated;
    public event Action<CardSlot, Dictionary>? CardDropped;

    public string SlotName { get; }

    public CardSlot(string slotName, string current, string description, params string[] acceptedKinds)
    {
        SlotName = slotName;
        _acceptedKinds = acceptedKinds;
        TooltipText = description;
        MouseFilter = MouseFilterEnum.Stop;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;

        StyleBoxFlat style = new()
        {
            BgColor = new Color("#EEF4F3"),
            BorderColor = new Color("#5CB8A7"),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 7,
            CornerRadiusTopRight = 7,
            CornerRadiusBottomLeft = 7,
            CornerRadiusBottomRight = 7
        };
        AddThemeStyleboxOverride("panel", style);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 7);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 7);
        AddChild(margin);

        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 8);
        margin.AddChild(row);

        VBoxContainer labels = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        row.AddChild(labels);
        Label title = new() { Text = slotName };
        title.AddThemeColorOverride("font_color", new Color("#202A34"));
        title.AddThemeFontSizeOverride("font_size", 12);
        labels.AddChild(title);
        _currentLabel = new Label { Text = current };
        _currentLabel.AddThemeColorOverride("font_color", new Color("#5D6975"));
        _currentLabel.AddThemeFontSizeOverride("font_size", 10);
        labels.AddChild(_currentLabel);

        Button open = new()
        {
            Text = "Open",
            FocusMode = FocusModeEnum.None,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
        open.Pressed += () => Activated?.Invoke(this);
        row.AddChild(open);
    }

    public void SetCurrent(string value)
    {
        _currentLabel.Text = value;
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return false;
        Dictionary card = data.AsGodotDictionary();
        if (!card.ContainsKey("dackCardKind"))
            return false;
        string kind = card["dackCardKind"].AsString();
        return _acceptedKinds.Any(accepted => accepted.Equals(kind, StringComparison.OrdinalIgnoreCase));
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (!_CanDropData(atPosition, data))
            return;
        CardDropped?.Invoke(this, data.AsGodotDictionary());
    }
}
