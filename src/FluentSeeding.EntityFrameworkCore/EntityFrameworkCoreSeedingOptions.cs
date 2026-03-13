namespace FluentSeeding.EntityFrameworkCore;

/// <summary>
/// Options for configuring the EF Core persistence layer.
/// </summary>
public sealed class EntityFrameworkCoreSeedingOptions
{
    /// <summary>
    /// Controls how the persistence layer handles entities whose primary key already exists in the database.
    /// Defaults to <see cref="ConflictBehavior.Insert"/>.
    /// </summary>
    public ConflictBehavior ConflictBehavior { get; set; } = ConflictBehavior.Insert;
}
