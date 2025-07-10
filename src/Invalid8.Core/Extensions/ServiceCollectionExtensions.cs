using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvalid8(this IServiceCollection services)
    {
        services.AddSingleton<IQueryClient, QueryClient>();
        return services;
    }
}
