namespace Invalid8.Core.Exceptions;

public class InvalidCacheKeyException(string message, string[] keyParts, string[] validationErrors)
    : Exception(message)
{
    public string[] KeyParts { get; } = keyParts;
    public string[] ValidationErrors { get; } = validationErrors;

    public override string ToString() => 
        $"{Message} Key: [{string.Join(", ", KeyParts)}]. Errors: [{string.Join("; ", ValidationErrors)}]";
}