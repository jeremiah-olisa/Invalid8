using System.Text.RegularExpressions;
using Invalid8.Core.Exceptions;

namespace Invalid8.Core;

/// <summary>
/// Generates and validates cache keys with sensible defaults
/// </summary>
public partial interface IKeyGenerator
{
    // Properties with default values
    int MaxKeyLength => 256;
    IReadOnlyList<string> ForbiddenPatterns => ["..", "\\", "/", ":", "*", "?", "\"", "<", ">", "|"];
    string KeySeparator => ":";
    
    /// <summary>
    /// Generates a cache key from parts (REQUIRED implementation)
    /// </summary>
    string Generate(string[] keyParts);
    
    /// <summary>
    /// Generates a cache key with validation (DEFAULT implementation)
    /// </summary>
    string GenerateValidated(string[] keyParts, out string[] validationErrors)
    {
        validationErrors = ValidateKeyParts(keyParts).ToArray();
        return Generate(keyParts);
    }
    
    /// <summary>
    /// Validates key parts for security and correctness (DEFAULT implementation)
    /// </summary>
    IEnumerable<string> ValidateKeyParts(string[] keyParts)
    {
        if (keyParts.Length == 0)
        {
            yield return "Key parts cannot be null or empty";
            yield break;
        }
        
        for (var i = 0; i < keyParts.Length; i++)
        {
            var part = keyParts[i];
            
            if (string.IsNullOrWhiteSpace(part))
            {
                yield return $"Key part at index {i} cannot be null or empty";
                continue;
            }
            
            if (part.Length > MaxKeyLength / 2) // Allow room for other parts
            {
                yield return $"Key part '{part}' exceeds maximum length of {MaxKeyLength / 2} characters";
            }
            
            foreach (var forbiddenPattern in ForbiddenPatterns)
            {
                if (part.Contains(forbiddenPattern))
                {
                    yield return $"Key part '{part}' contains forbidden pattern '{forbiddenPattern}'";
                }
            }
            
            // Validate against common injection patterns
            if (ContainsPotentialInjection(part))
            {
                yield return $"Key part '{part}' contains potential injection pattern";
            }
        }
        
        var fullKey = Generate(keyParts);
        if (fullKey.Length > MaxKeyLength)
        {
            yield return $"Generated key exceeds maximum length of {MaxKeyLength} characters";
        }
    }
    
    /// <summary>
    /// Generates a tag-based key for invalidation (DEFAULT implementation)
    /// </summary>
    string GenerateTagKey(string tag) => Generate(["tag", SanitizeTag(tag)]);
    
    /// <summary>
    /// Sanitizes a tag name (DEFAULT implementation)
    /// </summary>
    protected string SanitizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return "unknown";
            
        // Remove invalid characters from tags
        var sanitized = InvalidCharsRegex().Replace(tag, "_");
        return sanitized.ToLowerInvariant();
    }
    
    /// <summary>
    /// Checks for potential injection patterns (DEFAULT implementation)
    /// </summary>
    protected bool ContainsPotentialInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
            
        // Check for common injection patterns
        var patterns = new[]
        {
            @"(?i)select\s.+from",
            @"(?i)insert\s.+into",
            @"(?i)update\s.+set",
            @"(?i)delete\s.+from",
            @"(?i)drop\s.+table",
            @"(?i)exec(\s|\()",
            @"(?i)execute\s",
            @"(?i)union\s.+select",
            @"(?i);\s*--",
            @"(?i)/\*.*\*/"
        };
        
        return patterns.Any(pattern => Regex.IsMatch(input, pattern));
    }
    
    /// <summary>
    /// Validates key parts and throws on error (DEFAULT implementation)
    /// </summary>
    string GenerateOrThrow(string[] keyParts)
    {
        var errors = ValidateKeyParts(keyParts).ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidCacheKeyException(
                $"Invalid cache key parts: {string.Join("; ", errors)}", 
                keyParts, 
                errors);
        }
        return Generate(keyParts);
    }

    [GeneratedRegex(@"[^\w\-\.]")]
    public static partial Regex InvalidCharsRegex();
}