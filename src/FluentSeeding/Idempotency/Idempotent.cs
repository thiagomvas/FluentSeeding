using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FluentSeeding;

/// <summary>
/// Generates deterministic, stable values from an entity type name and index.
/// The same inputs always produce the same output across runs and machines.
/// </summary>
public static class Idempotent
{
    /// <summary>
    /// RFC 4122 DNS namespace used for name-based UUID generation. Using a fixed namespace ensures that the same entity type and index
    /// will always produce the same GUID regardless of where or when the code is run.
    /// </summary>
    private static readonly Guid Namespace =
        new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    /// <inheritdoc cref="Guid(Type, int, string?)"/>
    public static Guid Guid<T>(int index, string? seed = null) =>
        Guid(typeof(T), index, seed);

    /// <summary>
    /// Generates a deterministic UUID v5 (SHA-1 name-based) <see cref="Guid"/> from
    /// <paramref name="entityType"/>'s name and <paramref name="index"/>.
    /// The same inputs always produce the same value across processes, machines, and time.
    /// </summary>
    /// <param name="entityType">The entity type whose name forms part of the hash key.</param>
    /// <param name="index">
    /// A position within the seeded collection (e.g. 0, 1, 2 …). Each distinct value produces
    /// a different GUID for the same type and seed.
    /// </param>
    /// <param name="seed">
    /// An optional discriminator included in the hash key. Use this when two seeders target the
    /// same <paramref name="entityType"/> and must produce non-overlapping GUIDs. Defaults to
    /// <paramref name="entityType"/>.Name when <see langword="null"/>.
    /// </param>
    /// <returns>A stable UUID v5 GUID uniquely identifying this type/index/seed combination.</returns>
    public static Guid Guid(Type entityType, int index, string? seed = null)
    {
        var key = seed != null
            ? $"{seed}:{index}"
            : $"{entityType.Name}:{index}";
        return V5Uuid(Namespace, key);
    }

    /// <inheritdoc cref="Int(Type, int, string?)"/>
    public static int Int<T>(int index, string? seed = null) =>
        Int(typeof(T), index, seed);

    /// <summary>
    /// Generates a deterministic <see cref="int"/> from <paramref name="entityType"/>'s name and
    /// <paramref name="index"/> by XOR-folding the four 32-bit words of the underlying UUID v5 GUID.
    /// The result may be negative and covers the full <see cref="int"/> range.
    /// </summary>
    /// <param name="entityType">The entity type whose name forms part of the hash key.</param>
    /// <param name="index">
    /// A position within the seeded collection. Each distinct value produces a different integer
    /// for the same type and seed.
    /// </param>
    /// <param name="seed">
    /// An optional discriminator. See <see cref="Guid(Type, int, string?)"/> for details.
    /// </param>
    /// <returns>
    /// A stable integer derived by XOR-folding the underlying GUID. May be negative.
    /// </returns>
    public static int Int(Type entityType, int index, string? seed = null)
    {
        var guid = Guid(entityType, index, seed);
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        return MemoryMarshal.Read<int>(bytes[..4])
               ^ MemoryMarshal.Read<int>(bytes[4..8])
               ^ MemoryMarshal.Read<int>(bytes[8..12])
               ^ MemoryMarshal.Read<int>(bytes[12..]);
    }

    /// <inheritdoc cref="Long(Type, int, string?)"/>
    public static long Long<T>(int index, string? seed = null) =>
        Long(typeof(T), index, seed);

    /// <summary>
    /// Generates a deterministic <see cref="long"/> from <paramref name="entityType"/>'s name and
    /// <paramref name="index"/> by XOR-folding the two 64-bit halves of the underlying UUID v5 GUID.
    /// The result may be negative and covers the full <see cref="long"/> range.
    /// </summary>
    /// <param name="entityType">The entity type whose name forms part of the hash key.</param>
    /// <param name="index">
    /// A position within the seeded collection. Each distinct value produces a different long
    /// for the same type and seed.
    /// </param>
    /// <param name="seed">
    /// An optional discriminator. See <see cref="Guid(Type, int, string?)"/> for details.
    /// </param>
    /// <returns>
    /// A stable long derived by XOR-folding the two 64-bit halves of the underlying GUID. May be negative.
    /// </returns>
    public static long Long(Type entityType, int index, string? seed = null)
    {
        var guid = Guid(entityType, index, seed);
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        return MemoryMarshal.Read<long>(bytes[..8])
               ^ MemoryMarshal.Read<long>(bytes[8..]);
    }

    /// <inheritdoc cref="Slug(Type, int, string?)"/>
    public static string Slug<T>(int index, string? prefix = null) =>
        Slug(typeof(T), index, prefix);

    /// <summary>
    /// Generates a URL-safe slug of the form <c>"{prefix}-{index}"</c>.
    /// Unlike the hash-based methods on this class, the slug is a simple formatted string not
    /// derived from a cryptographic hash making it human-readable and predictable.
    /// </summary>
    /// <param name="entityType">
    /// Used to derive the default prefix via kebab-case conversion of the type name
    /// (e.g. <c>ProductCategory</c> -> <c>"product-category"</c>).
    /// </param>
    /// <param name="index">Appended as the numeric suffix of the slug.</param>
    /// <param name="prefix">
    /// Overrides the auto-generated kebab-case prefix. When <see langword="null"/>, the prefix is
    /// derived from <paramref name="entityType"/>.Name.
    /// </param>
    /// <returns>A slug in the form <c>"{prefix}-{index}"</c>, e.g. <c>"product-category-1"</c>.</returns>
    public static string Slug(Type entityType, int index, string? prefix = null)
    {
        var p = prefix ?? ToKebabCase(entityType.Name);
        return $"{p}-{index}";
    }

    private static Guid V5Uuid(Guid namespaceid, string name)
    {
        Span<byte> nsBytes = stackalloc byte[16];
        namespaceid.TryWriteBytes(nsBytes);
        SwapEndianness(nsBytes);

        var nameBytes = Encoding.UTF8.GetBytes(name);

        Span<byte> hash = stackalloc byte[20];
        using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        sha1.AppendData(nsBytes);
        sha1.AppendData(nameBytes);
        sha1.GetCurrentHash(hash);

        Span<byte> result = stackalloc byte[16];
        hash[..16].CopyTo(result);
        result[6] = (byte)((result[6] & 0x0F) | 0x50); // version 5
        result[8] = (byte)((result[8] & 0x3F) | 0x80); // RFC 4122 variant
        SwapEndianness(result);

        return new Guid(result);
    }

    private static void SwapEndianness(Span<byte> b)
    {
        (b[0], b[3]) = (b[3], b[0]);
        (b[1], b[2]) = (b[2], b[1]);
        (b[4], b[5]) = (b[5], b[4]);
        (b[6], b[7]) = (b[7], b[6]);
    }

    private static string ToKebabCase(string pascal)
    {
        var sb = new StringBuilder(pascal.Length + 4);
        for (var i = 0; i < pascal.Length; i++)
        {
            if (char.IsUpper(pascal[i]) && i > 0) sb.Append('-');
            sb.Append(char.ToLowerInvariant(pascal[i]));
        }

        return sb.ToString();
    }
}