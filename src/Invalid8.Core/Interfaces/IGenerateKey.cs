namespace Invalid8.Core;

public interface IGenerateKey
{
    string Generate(string[] key);
    string Generate(string[] key, CancellationToken ct = default);
}