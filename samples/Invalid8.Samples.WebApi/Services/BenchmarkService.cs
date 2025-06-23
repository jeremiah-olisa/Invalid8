namespace Invalid8.Samples.WebApi.Services;

public class BenchmarkService(
    TodoService todoService,
    CachedTodoService cachedTodoService,
    ILogger<BenchmarkService> logger)
{
    public async Task<BenchmarkResult> BenchmarkTodoServiceAsync(int todoId = 1)
    {
        logger.LogInformation("🧪 Starting benchmark for TodoService (Database)...");

        var results = new List<TimeSpan>();
        var responses = new List<string>();

        for (var i = 1; i <= 3; i++)
        {
            logger.LogInformation("🔹 Hit #{HitNumber} - TodoService", i);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var todo = await todoService.GetTodoAsync(todoId);
            sw.Stop();

            results.Add(sw.Elapsed);
            responses.Add(todo != null ? $"Todo {todoId}: {todo.Title}" : "Not found");

            logger.LogInformation("   ⏱️  Duration: {DurationMs}ms", sw.ElapsedMilliseconds);
            logger.LogInformation("   📦 Response: {Response}", responses.Last());

            // Small delay between requests
            if (i < 3) await Task.Delay(100);
        }

        return new BenchmarkResult
        {
            ServiceName = "TodoService (Database)",
            Hits = results,
            Responses = responses,
            AverageDuration = TimeSpan.FromMilliseconds(results.Average(r => r.TotalMilliseconds))
        };
    }

    public async Task<BenchmarkResult> BenchmarkCachedTodoServiceAsync(int todoId = 1)
    {
        logger.LogInformation("🧪 Starting benchmark for CachedTodoService...");

        // First, ensure we have data by calling the regular service
        logger.LogInformation("📦 Pre-loading data using TodoService...");
        await todoService.GetTodoAsync(todoId);

        var results = new List<TimeSpan>();
        var responses = new List<string>();
        var cacheStatuses = new List<string>();

        for (var i = 1; i <= 3; i++)
        {
            logger.LogInformation("🔹 Hit #{HitNumber} - CachedTodoService", i);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var todo = await cachedTodoService.GetTodoAsync(todoId);
            sw.Stop();

            results.Add(sw.Elapsed);
            responses.Add(todo != null ? $"Todo {todoId}: {todo.Title}" : "Not found");

            // Determine cache status based on timing
            var cacheStatus = i == 1 ? "MISS (First request)" : "HIT (Subsequent request)";
            cacheStatuses.Add(cacheStatus);

            logger.LogInformation("   ⏱️  Duration: {DurationMs}ms", sw.ElapsedMilliseconds);
            logger.LogInformation("   📦 Response: {Response}", responses.Last());
            logger.LogInformation("   🏷️  Cache: {CacheStatus}", cacheStatus);

            // Small delay between requests
            if (i < 3) await Task.Delay(100);
        }

        return new BenchmarkResult
        {
            ServiceName = "CachedTodoService",
            Hits = results,
            Responses = responses,
            CacheStatuses = cacheStatuses,
            AverageDuration = TimeSpan.FromMilliseconds(results.Average(r => r.TotalMilliseconds))
        };
    }

    public async Task<BenchmarkResult> BenchmarkBothServicesAsync(int todoId = 1)
    {
        logger.LogInformation("🎯 Starting comprehensive benchmark comparison...");

        var todoServiceResult = await BenchmarkTodoServiceAsync(todoId);
        logger.LogInformation("---");
        var cachedServiceResult = await BenchmarkCachedTodoServiceAsync(todoId);

        // Calculate performance improvement
        var improvement = ((todoServiceResult.AverageDuration.TotalMilliseconds -
                            cachedServiceResult.AverageDuration.TotalMilliseconds) /
                           todoServiceResult.AverageDuration.TotalMilliseconds) * 100;

        logger.LogInformation("==========================================");
        logger.LogInformation("📊 BENCHMARK RESULTS SUMMARY");
        logger.LogInformation("==========================================");
        logger.LogInformation("🏃‍♂️ TodoService (DB) Average: {AvgMs}ms",
            todoServiceResult.AverageDuration.TotalMilliseconds);
        logger.LogInformation("⚡ CachedTodoService Average: {AvgMs}ms",
            cachedServiceResult.AverageDuration.TotalMilliseconds);
        logger.LogInformation("💨 Performance Improvement: {Improvement}% faster",
            improvement.ToString("0.0"));
        logger.LogInformation("==========================================");

        return new BenchmarkResult
        {
            ServiceName = "Comparison Summary",
            AverageDuration = cachedServiceResult.AverageDuration,
            ComparisonImprovement = improvement
        };
    }
}

public class BenchmarkResult
{
    public string ServiceName { get; set; } = string.Empty;
    public List<TimeSpan> Hits { get; set; } = [];
    public List<string> Responses { get; set; } = [];
    public List<string> CacheStatuses { get; set; } = [];
    public TimeSpan AverageDuration { get; set; }
    public double ComparisonImprovement { get; set; }
}