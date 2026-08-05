using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dack;

/// <summary>
/// Searchable two-level catalog: choose a category, then apply or drag a card.
/// The shelf keeps definitions separate from placed instances.
/// </summary>
public partial class CardShelf : VBoxContainer
{
    private readonly List<CardDefinition> _definitions;
    private readonly OptionButton _categoryPicker = new();
    private readonly LineEdit _search = new();
    private readonly VBoxContainer _cards = new();
    private int _forkSequence;

    public event Action<CardDefinition>? Activated;
    public event Action<CardDefinition>? Forked;

    public CardShelf(string title, IEnumerable<CardDefinition> definitions)
    {
        _definitions = definitions.ToList();
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", 7);

        Label heading = new() { Text = title.ToUpperInvariant() };
        heading.AddThemeColorOverride("font_color", new Color("#FF5C35"));
        heading.AddThemeFontSizeOverride("font_size", 12);
        AddChild(heading);

        HBoxContainer filters = new();
        filters.AddThemeConstantOverride("separation", 6);
        AddChild(filters);

        _categoryPicker.CustomMinimumSize = new Vector2(156, 34);
        _categoryPicker.FocusMode = FocusModeEnum.None;
        filters.AddChild(_categoryPicker);

        _search.PlaceholderText = "Search cards";
        _search.ClearButtonEnabled = true;
        _search.CustomMinimumSize = new Vector2(150, 34);
        _search.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        filters.AddChild(_search);

        _cards.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _cards.AddThemeConstantOverride("separation", 7);
        AddChild(_cards);

        _categoryPicker.ItemSelected += _ => RefreshCards();
        _search.TextChanged += _ => RefreshCards();
        RefreshCategories();
        RefreshCards();
    }

    public void AddDefinition(CardDefinition definition)
    {
        _definitions.Add(definition);
        RefreshCategories();
        SelectCategory("Project-Created");
        RefreshCards();
    }

    private void RefreshCategories()
    {
        string selected = _categoryPicker.Selected >= 0
            ? _categoryPicker.GetItemText(_categoryPicker.Selected)
            : "All Cards";
        _categoryPicker.Clear();
        _categoryPicker.AddItem("All Cards");
        foreach (string category in _definitions.Select(definition => definition.Category).Distinct().OrderBy(value => value))
            _categoryPicker.AddItem(category);
        SelectCategory(selected);
    }

    private void SelectCategory(string category)
    {
        for (int i = 0; i < _categoryPicker.ItemCount; i++)
        {
            if (_categoryPicker.GetItemText(i).Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                _categoryPicker.Select(i);
                return;
            }
        }

        _categoryPicker.Select(0);
    }

    private void RefreshCards()
    {
        foreach (Node child in _cards.GetChildren())
            child.QueueFree();

        string category = _categoryPicker.Selected >= 0 ? _categoryPicker.GetItemText(_categoryPicker.Selected) : "All Cards";
        string query = _search.Text.Trim();
        IEnumerable<CardDefinition> visible = _definitions.Where(definition =>
            (category == "All Cards" || definition.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            && (query.Length == 0
                || definition.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || definition.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)
                || definition.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase))));

        int count = 0;
        foreach (CardDefinition definition in visible)
        {
            BuilderCard card = new(definition);
            card.Activated += _ => Activated?.Invoke(definition);
            card.DuplicateRequested += _ => Activated?.Invoke(definition);
            card.ForkRequested += _ => ForkDefinition(definition);
            _cards.AddChild(card);
            count++;
        }

        if (count == 0)
        {
            Label empty = new() { Text = "No cards match this category/search." };
            empty.AddThemeColorOverride("font_color", new Color("#6C7782"));
            empty.AddThemeFontSizeOverride("font_size", 11);
            _cards.AddChild(empty);
        }
    }

    private void ForkDefinition(CardDefinition source)
    {
        _forkSequence++;
        CardDefinition fork = source with
        {
            Id = $"{source.EffectiveId}-fork-{_forkSequence}",
            Title = $"{source.Title} Fork {_forkSequence}",
            Subtitle = "Project-local editable fork",
            Category = "Project-Created",
            Provenance = $"Project fork of {source.Title}",
            ExportStatus = "Local override",
            Tags = [.. source.Tags, "local", "fork"],
            SourceCardId = source.EffectiveId
        };
        AddDefinition(fork);
        Forked?.Invoke(fork);
    }
}
