namespace FluentSeeding.Tests.Common;

public sealed class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // n-n relationship with Post (through PostTag join table)
    public List<PostTag> PostTags { get; set; } = [];
}
