using Godot;

namespace Dack;

/// <summary>
/// Shared family-page grammar. Every game family exposes the same ordered
/// sections while choosing which sections begin expanded.
/// </summary>
public partial class FamilyPageShell : VBoxContainer
{
    public FamilyPageShell(string family, string preset)
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", 8);

        Label familyLabel = new()
        {
            Text = family.ToUpperInvariant(),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        familyLabel.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        familyLabel.AddThemeFontSizeOverride("font_size", 15);
        AddChild(familyLabel);

        Label presetLabel = new()
        {
            Text = $"Preset: {preset}",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        presetLabel.AddThemeColorOverride("font_color", new Color("#5D6975"));
        presetLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(presetLabel);
    }

    public void AddSection(string title, string summary, Control content, bool expanded = false)
    {
        VBoxContainer section = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        section.AddThemeConstantOverride("separation", 4);
        AddChild(section);

        Button header = new()
        {
            Text = SectionTitle(expanded, title, summary),
            Alignment = HorizontalAlignment.Left,
            FocusMode = FocusModeEnum.None,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 34)
        };
        header.AddThemeFontSizeOverride("font_size", 11);
        header.AddThemeColorOverride("font_color", new Color("#F7F5EF"));
        header.AddThemeColorOverride("font_hover_color", Colors.White);
        header.AddThemeStyleboxOverride("normal", FlatStyle("#344454"));
        header.AddThemeStyleboxOverride("hover", FlatStyle("#40556A"));
        header.AddThemeStyleboxOverride("pressed", FlatStyle("#263746"));
        section.AddChild(header);

        PanelContainer contentPanel = new()
        {
            Visible = expanded,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        contentPanel.AddThemeStyleboxOverride("panel", FlatStyle("#F7F5EF"));
        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        contentPanel.AddChild(margin);
        content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        margin.AddChild(content);
        section.AddChild(contentPanel);

        bool isExpanded = expanded;
        header.Pressed += () =>
        {
            isExpanded = !isExpanded;
            contentPanel.Visible = isExpanded;
            header.Text = SectionTitle(isExpanded, title, summary);
        };
    }

    private static string SectionTitle(bool expanded, string title, string summary)
    {
        string marker = expanded ? "-" : "+";
        return string.IsNullOrWhiteSpace(summary)
            ? $"{marker}  {title}"
            : $"{marker}  {title}  //  {summary}";
    }

    private static StyleBoxFlat FlatStyle(string hex)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(hex),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 6,
            ContentMarginBottom = 6
        };
    }
}
