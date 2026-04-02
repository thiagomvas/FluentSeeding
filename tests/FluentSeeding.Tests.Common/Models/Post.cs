namespace FluentSeeding.Tests.Common;

public sealed class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // n-1 relationship with Author
    public Author? Author { get; set; }

    // n-n relationship with Tag (through PostTag join table)
    public List<PostTag> PostTags { get; set; } = [];
}
