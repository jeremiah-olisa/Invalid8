using Invalid8.Samples.WebApi.Services;
using Microsoft.EntityFrameworkCore;

namespace Invalid8.Samples.WebApi.Extensions;

using Invalid8.Extensions;

public static class WebApiServiceCollectionExtensions
{
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext with SQLite
        return services.AddDbContextPool<TodoContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Database") ?? "Data Source=todos.db"));
    }

    public static IServiceCollection AddDependencyResolvers(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services
            .AddInvalid8()
            .AddInvalid8WithRedisCache(options =>
                options.Configuration = configuration.GetConnectionString("Redis"));

        services.AddScoped<TodoService>();
        services.AddScoped<CachedTodoService>();
        services.AddScoped<BenchmarkService>();

        return services;
    }
}