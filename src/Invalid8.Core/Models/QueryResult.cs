namespace Invalid8.Core;

public class QueryResult<T>
{
    public QueryResult()
    {
    }

    public QueryResult(T data, bool isFromCache = false, bool isStale = false)
    {
        Data = data;
        IsFromCache = isFromCache;
        IsStale = isStale;
    }

    public T? Data { get; set; }
    public bool IsFromCache { get; set; }
    public bool IsStale { get; set; }

    public T GetData() => Data ?? throw new ArgumentNullException(nameof(Data));
}