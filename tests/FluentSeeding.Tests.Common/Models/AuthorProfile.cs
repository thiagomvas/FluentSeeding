namespace FluentSeeding.Tests.Common;

public sealed class AuthorProfile
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Bio { get; set; } = string.Empty;

    // 1-1 relationship with Author
    public Author? Author { get; set; }
}
