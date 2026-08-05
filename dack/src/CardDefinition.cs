namespace Dack;

public sealed record CardDefinition(
    string Kind,
    string Id,
    string Title,
    string Subtitle,
    string Details,
    string Category,
    string Provenance,
    string License,
    string ExportStatus,
    string[] Tags,
    string SourceCardId = "",
    string PrimaryAction = "Apply"
)
{
    public string EffectiveId => string.IsNullOrWhiteSpace(SourceCardId) ? Id : SourceCardId;
    public bool IsFork => !string.IsNullOrWhiteSpace(SourceCardId);
}
