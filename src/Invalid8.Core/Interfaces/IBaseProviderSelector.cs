namespace Invalid8.Core;

public interface IBaseProviderSelector<T> where T : class
{
    T SelectProvider(string[] key);
    Task<T> SelectProviderAsync(string[] key, CancellationToken ct = default);
}
