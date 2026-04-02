namespace FluentSeeding.Tests.Common;

public sealed class Author
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // 1-1 relationship with Profile
    public AuthorProfile? Profile { get; set; }

    // 1-n relationship with Post
    public List<Post> Posts { get; set; } = [];
}
