namespace FluentSeeding.Tests.Common;

public sealed class PostTag
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public Post? Post { get; set; }
    public Tag? Tag { get; set; }
}
