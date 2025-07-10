namespace Invalid8.Core;

public class QueryResult<T>
{
    public T? Data { get; set; }
    public bool IsFromCache { get; set; }
    public bool IsStale { get; set; }
}