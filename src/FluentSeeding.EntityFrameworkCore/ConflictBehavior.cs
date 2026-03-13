namespace FluentSeeding.EntityFrameworkCore;

/// <summary>
/// Controls how the EF Core persistence layer handles entities whose primary key already exists in the database.
/// </summary>
public enum ConflictBehavior
{
    /// <summary>
    /// Always inserts all seeded entities. Throws if a primary key collision occurs.
    /// This is the default behavior.
    /// </summary>
    Insert,

    /// <summary>
    /// Skips entities whose primary key already exists in the database.
    /// Useful for idempotent seed scripts that should not duplicate data on re-runs.
    /// </summary>
    Skip,

    /// <summary>
    /// Updates entities whose primary key already exists, and inserts those that do not.
    /// Useful for keeping seed data in sync without removing existing rows.
    /// </summary>
    Update,
}
