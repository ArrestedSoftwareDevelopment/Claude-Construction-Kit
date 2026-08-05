using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace Dack;

public partial class BuilderCard : PanelContainer
{
    public CardDefinition Definition { get; }
    public string CardKind { get; }
    public string CardId { get; }
    public string CardTitle { get; }
    public string CardSubtitle { get; }

    public event Action<BuilderCard>? Activated;
    public event Action<BuilderCard>? DuplicateRequested;
    public event Action<BuilderCard>? ForkRequested;
    public event Action<BuilderCard>? FavoriteRequested;

    public BuilderCard(string cardKind, string cardId, string title, string subtitle, string details)
        : this(new CardDefinition(cardKind, cardId, title, subtitle, details, "Cards", "Project catalog", "Review", "Local", [], PrimaryAction: "Apply"))
    {
    }

    public BuilderCard(CardDefinition definition)
    {
        Definition = definition;
        CardKind = definition.Kind;
        CardId = definition.Id;
        CardTitle = definition.Title;
        CardSubtitle = definition.Subtitle;
        TooltipText = definition.Details;
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

        Label badge = Text(definition.Kind.Replace('-', ' ').ToUpperInvariant(), "#202A34", 9);
        badge.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        top.AddChild(badge);

        Label titleLabel = Text(definition.Title, "#202A34", 13);
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        top.AddChild(titleLabel);

        Button use = new()
        {
            Text = definition.PrimaryAction,
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(54, 26),
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
        use.Pressed += () => Activated?.Invoke(this);
        top.AddChild(use);

        Label subtitleLabel = Text(definition.Subtitle, "#46515C", 11);
        root.AddChild(subtitleLabel);

        Label provenance = Text($"{definition.Provenance}  |  {definition.License}  |  {definition.ExportStatus}", "#5A6570", 9);
        provenance.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        root.AddChild(provenance);

        if (definition.Tags.Length > 0)
            root.AddChild(Text(string.Join("  ", definition.Tags.Select(tag => $"#{tag}")), "#3D6F6A", 9));

        Label detailsLabel = Text(definition.Details, "#6C7782", 10);
        detailsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        root.AddChild(detailsLabel);

        HBoxContainer actions = new();
        actions.AddThemeConstantOverride("separation", 5);
        root.AddChild(actions);

        Button duplicate = SmallButton("Duplicate");
        duplicate.Disabled = definition.Kind is "player-character" or "projectile" or "effect";
        duplicate.TooltipText = duplicate.Disabled
            ? "This card binds to a unique slot; use Apply or Fork."
            : "Create another independent instance from this definition.";
        duplicate.Pressed += () => DuplicateRequested?.Invoke(this);
        actions.AddChild(duplicate);

        Button fork = SmallButton(definition.IsFork ? "Fork Again" : "Fork");
        fork.TooltipText = "Create a project-local editable card while preserving the shared source definition.";
        fork.Pressed += () => ForkRequested?.Invoke(this);
        actions.AddChild(fork);

        Button favorite = SmallButton("Favorite");
        favorite.TooltipText = "Toggle this definition in the Favorites shelf.";
        favorite.Pressed += () => FavoriteRequested?.Invoke(this);
        actions.AddChild(favorite);
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
            ["dackCardSourceId"] = Definition.EffectiveId,
            ["dackCardTitle"] = CardTitle
        };
        return data;
    }

    private static Button SmallButton(string text)
    {
        return new Button
        {
            Text = text,
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(0, 26),
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };
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
