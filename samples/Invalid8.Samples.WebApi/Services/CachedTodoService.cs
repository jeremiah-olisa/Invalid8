using Invalid8.Core;
using Invalid8.Samples.WebApi.Models;

namespace Invalid8.Samples.WebApi.Services;

public class CachedTodoService(
    TodoService todoService,
    IQueryClient queryClient,
    ILogger<CachedTodoService> logger)
{
    public async Task<IEnumerable<Todo>> GetTodosAsync()
    {
        var result = await queryClient.QueryAsync(
            key: ["todos", "all"],
            queryFunc: async () =>
            {
                logger.LogInformation("🚀 Cache miss! Fetching todos from database...");
                return await todoService.GetTodosAsync();
            },
            options: new QueryOptions
            {
                StaleTime = TimeSpan.FromMinutes(5),
                CacheTime = TimeSpan.FromMinutes(30)
            });

        if (result.IsFromCache)
            logger.LogInformation("✅ Cache hit! Returning {Count} todos from cache", result.Data?.Count() ?? 0);


        return result.Data ?? [];
    }

    public async Task<Todo?> GetTodoAsync(int id)
    {
        var result = await queryClient.QueryAsync(
            key: ["todos", id.ToString()],
            queryFunc: async () =>
            {
                logger.LogInformation("🚀 Cache miss! Fetching todo {Id} from database...", id);
                return await todoService.GetTodoAsync(id);
            },
            options: new QueryOptions
            {
                StaleTime = TimeSpan.FromMinutes(2),
                CacheTime = TimeSpan.FromMinutes(15)
            });

        if (result.IsFromCache)
            logger.LogInformation("✅ Cache hit! Returning todo {Id} from cache", id);
        else if (result.Data == null)
            logger.LogWarning("❌ Todo {Id} not found in database", id);

        return result.Data;
    }

    public async Task<Todo> AddTodoAsync(Todo todo)
    {
        var result = await queryClient.MutateAsync(
            mutationFunc: async () => await todoService.AddTodoAsync(todo),
            options: new MutationOptions
            {
                InvalidateQueries = [["todos", "all"]]
            });

        await queryClient.SetQueryDataAsync(["todos", result.Id.ToString()], result);
        logger.LogInformation("🔄 Added todo {Id}, invalidated all todos cache", result.Id);

        return result;
    }

    public async Task<bool> UpdateTodoAsync(int id, Todo todo)
    {
        var success = await queryClient.MutateAsync(
            mutationFunc: async () => await todoService.UpdateTodoAsync(id, todo),
            options: new MutationOptions
            {
                InvalidateQueries =
                [
                    ["todos", "all"],
                    ["todos", id.ToString()]
                ]
            });

        if (success)
        {
            logger.LogInformation("🔄 Updated todo {Id}, invalidated todo and todos cache", id);
        }

        return success;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var success = await queryClient.MutateAsync(
            mutationFunc: async () => await todoService.DeleteTodoAsync(id),
            options: new MutationOptions
            {
                InvalidateQueries =
                [
                    ["todos", "all"],
                    ["todos", id.ToString()]
                ]
            });

        if (success)
        {
            logger.LogInformation("🗑️ Deleted todo {Id}, invalidated todo and todos cache", id);
        }

        return success;
    }

    // Additional cache management methods
    public async Task RefreshTodosCacheAsync()
    {
        await queryClient.RefetchQueriesAsync(["todos", "all"]);
        logger.LogInformation("🔄 Manually refreshed todos cache");
    }

    public async Task InvalidateTodoCacheAsync(int id)
    {
        await queryClient.InvalidateQueriesAsync(["todos", id.ToString()]);
        logger.LogInformation("🗑️ Manually invalidated todo {Id} cache", id);
    }

    public async Task InvalidateAllTodosCacheAsync()
    {
        await queryClient.InvalidateQueriesAsync(["todos"]);
        logger.LogInformation("🗑️ Manually invalidated all todos cache");
    }
}