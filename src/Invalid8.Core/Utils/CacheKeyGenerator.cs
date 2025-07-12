using System.Reflection.Metadata.Ecma335;

namespace Invalid8.Core.Utils;

public class CacheKeyGenerator : IGenerateKey
{
    public string Generate(string[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length == 0 || key.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Key cannot be empty or contain whitespace", nameof(key));

        return string.Join(":", key);
    }

    public string Generate(string[] key, CancellationToken ct = default)
    {
        // Early cancellation check
        ct.ThrowIfCancellationRequested();

        return Generate(key);
    }
}