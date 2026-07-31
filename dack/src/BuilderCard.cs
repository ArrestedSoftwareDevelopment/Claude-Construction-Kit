using Godot;
using Godot.Collections;
using System;

namespace Dack;

public partial class BuilderCard : PanelContainer
{
    public string CardKind { get; }
    public string CardId { get; }
    public string CardTitle { get; }
    public string CardSubtitle { get; }

    public event Action<BuilderCard>? Activated;

    public BuilderCard(string cardKind, string cardId, string title, string subtitle, string details)
    {
        CardKind = cardKind;
        CardId = cardId;
        CardTitle = title;
        CardSubtitle = subtitle;
        TooltipText = details;
        MouseFilter = MouseFilterEnum.Stop;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddThemeStyleboxOverride("panel", CardStyle("#FFF8E8", "#5CB8A7"));

        MarginContainer margin = new()
        {
            OffsetLeft = 10,
            OffsetTop = 8,
            OffsetRight = -10,
            OffsetBottom = -8
        };
        AddChild(margin);

        VBoxContainer root = new();
        root.AddThemeConstantOverride("separation", 5);
        margin.AddChild(root);

        HBoxContainer top = new();
        top.AddThemeConstantOverride("separation", 6);
        root.AddChild(top);

        Label badge = Text("CARD", "#202A34", 10);
        badge.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        top.AddChild(badge);

        Label titleLabel = Text(title, "#202A34", 13);
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        top.AddChild(titleLabel);

        Button use = new()
        {
            Text = "Use",
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(54, 26),
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
        use.Pressed += () => Activated?.Invoke(this);
        top.AddChild(use);

        Label subtitleLabel = Text(subtitle, "#46515C", 11);
        root.AddChild(subtitleLabel);

        Label detailsLabel = Text(details, "#6C7782", 10);
        detailsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        root.AddChild(detailsLabel);
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        PanelContainer preview = new();
        preview.CustomMinimumSize = new Vector2(210, 58);
        preview.AddThemeStyleboxOverride("panel", CardStyle("#FFF8E8", "#FF5C35"));
        MarginContainer margin = new()
        {
            OffsetLeft = 10,
            OffsetTop = 8,
            OffsetRight = -10,
            OffsetBottom = -8
        };
        preview.AddChild(margin);
        margin.AddChild(Text($"Dragging: {CardTitle}", "#202A34", 13));
        SetDragPreview(preview);

        Dictionary data = new()
        {
            ["dackCardKind"] = CardKind,
            ["dackCardId"] = CardId,
            ["dackCardTitle"] = CardTitle
        };
        return data;
    }

    private static Label Text(string value, string color, int size)
    {
        Label label = new()
        {
            Text = value,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", new Color(color));
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static StyleBoxFlat CardStyle(string fill, string border)
    {
        StyleBoxFlat style = new()
        {
            BgColor = new Color(fill),
            BorderColor = new Color(border),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ShadowColor = new Color(0, 0, 0, 0.20f),
            ShadowSize = 4,
            ShadowOffset = new Vector2(2, 3)
        };
        return style;
    }
}
