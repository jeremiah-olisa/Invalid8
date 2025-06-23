using Invalid8.Core;

namespace Invalid8.Providers.Base;

/// <summary>
/// Default implementation of IKeyGenerator with sensible defaults,
/// </summary>
public sealed class CacheKeyGenerator(string? keySeparator = null) : IKeyGenerator
{
    private readonly string _keySeparator = keySeparator ?? KeySeparator;
    private const string KeySeparator = ";";

    /// <summary>
    /// Generates a cache key from parts using the configured separator
    /// </summary>
    public string Generate(string[] keyParts)
    {
        if (keyParts == null || keyParts.Length == 0)
            throw new ArgumentException("Key parts cannot be null or empty", nameof(keyParts));

        // Filter out null/empty parts and trim each part
        var validParts = keyParts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim())
            .ToArray();

        if (validParts.Length == 0)
            throw new ArgumentException("No valid key parts provided", nameof(keyParts));

        return string.Join(_keySeparator, validParts);
    }

    // All other methods use the default interface implementations
    // Users can override any specific behavior by implementing IKeyGenerator themselves
}